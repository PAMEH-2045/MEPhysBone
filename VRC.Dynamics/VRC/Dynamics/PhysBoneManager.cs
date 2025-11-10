using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;
using static VRC.Dynamics.PhysBoneManager;

namespace VRC.Dynamics
{
	// Token: 0x02000031 RID: 49
	[AddComponentMenu("")]
	public class PhysBoneManager : MonoBehaviour
	{
		// Token: 0x060001A8 RID: 424 RVA: 0x0000B81C File Offset: 0x00009A1C
		public void AddPhysBone(VRCPhysBoneBase comp)
		{
			if (comp.chainId == ChainId.Null)
			{
				return;
			}
			this.compsToRemove.RemoveAll((PhysBoneManager.ToRemoveData x) => x.comp == comp);
			if (!this.compsToAdd.Contains(comp))
			{
				this.compsToAdd.Add(comp);
			}
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x0000B88C File Offset: 0x00009A8C
		public void RemovePhysBone(VRCPhysBoneBase comp)
		{
			if (comp.chainId == ChainId.Null)
			{
				return;
			}
			this.compsToAdd.Remove(comp);
			if (!Enumerable.Any<PhysBoneManager.ToRemoveData>(this.compsToRemove, (PhysBoneManager.ToRemoveData x) => x.comp == comp))
			{
				this.compsToRemove.Add(new PhysBoneManager.ToRemoveData(comp));
			}
		}

		// Token: 0x060001AA RID: 426 RVA: 0x0000B8FF File Offset: 0x00009AFF
		public bool HasPhysBone(VRCPhysBoneBase physBoneComp)
		{
			return this.compsToAdd.Contains(physBoneComp) || this.FindChainIndex(physBoneComp.chainId) >= 0;
		}

		// Token: 0x060001AB RID: 427 RVA: 0x0000B924 File Offset: 0x00009B24
		public VRCPhysBoneBase FindPhysBone(ChainId chainId)
		{
			int num = this.FindChainIndex(chainId);
			if (num < 0)
			{
				return null;
			}
			return this.chainBuffer.comps[num];
		}

		// Token: 0x060001AC RID: 428 RVA: 0x0000B950 File Offset: 0x00009B50
		internal void MarkRootDirty(PhysBoneRoot root)
		{
			if (!this.rootsToUpdate.Contains(root))
			{
				this.rootsToUpdate.Add(root);
			}
		}

		// Token: 0x060001AD RID: 429 RVA: 0x0000B96C File Offset: 0x00009B6C
		private void UpdateRoots()
		{
			for (int i = 0; i < this.rootsToUpdate.Count; i++)
			{
				PhysBoneRoot physBoneRoot = this.rootsToUpdate[i];
				if (physBoneRoot.bufferIndex >= 0)
				{
					PhysBoneManager.ChainRoot chainRoot = this.rootBuffer.roots[physBoneRoot.bufferIndex];
					chainRoot.useFixedTime = physBoneRoot.useFixedUpdate;
					this.rootBuffer.roots[physBoneRoot.bufferIndex] = chainRoot;
				}
			}
			this.rootsToUpdate.Clear();
		}

		// Token: 0x060001AE RID: 430 RVA: 0x0000B9EC File Offset: 0x00009BEC
		private void RemoveChains()
		{
			if (this.compsToRemove.Count == 0)
			{
				return;
			}
			int length = this.chainBuffer.chains.Length;
			foreach (PhysBoneManager.ToRemoveData toRemoveData in this.compsToRemove)
			{
				int num = this.FindChainIndex(toRemoveData.chainId);
				if (num >= 0)
				{
					if (toRemoveData.shape != null)
					{
						this.collision.RemoveShape(toRemoveData.shape);
					}
					if (num >= 0 && num < length)
					{
						this.chainBuffer.chains[num].Dispose();
					}
					PhysBoneGroup executionGroup = this.GetExecutionGroup(toRemoveData.executionGroup);
					if (executionGroup != null)
					{
						executionGroup.RemovePhysBone(toRemoveData.chainId);
					}
					for (int i = 0; i < 8; i++)
					{
						if (this.executionGroups[i].chainIds.Contains(toRemoveData.chainId))
						{
							PhysBoneManager.ReportCriticalError(PhysBoneManager.CriticalErrorType.ExecutionGroupsStillContainsId, null);
							break;
						}
					}
					this.buffer.Release(toRemoveData.chainId);
					if (toRemoveData.comp != null)
					{
						toRemoveData.comp.shape = null;
						if (toRemoveData.comp.resetWhenDisabled)
						{
							toRemoveData.comp.ResetTransformsToRestPosition();
						}
					}
				}
			}
			this.compsToRemove.Clear();
			this.buffer.Compact(false);
		}

		// Token: 0x060001AF RID: 431 RVA: 0x0000BB58 File Offset: 0x00009D58
		private void AddChains()
		{
			if (this.compsToAdd.Count == 0)
			{
				return;
			}
			Chain chain;
            foreach (VRCPhysBoneBase vrcphysBoneBase in this.compsToAdd)
			{
				if (!(vrcphysBoneBase == null))
				{
					vrcphysBoneBase.InitTransforms(false);
					if (vrcphysBoneBase.bones.Count < 1 || vrcphysBoneBase.bones.Count > 256)
					{
						Debug.LogWarning(string.Format("Invalid bone count for chain, Name:{0} ID:{1} Count:{2}", vrcphysBoneBase.name, vrcphysBoneBase.chainId, vrcphysBoneBase.bones.Count));
					}
					else if (this.FindChainIndex(vrcphysBoneBase.chainId) < 0)
					{
						int num = this.buffer.Request(vrcphysBoneBase.chainId, vrcphysBoneBase.bones.Count + 2);
						if (num < 0)
						{
							Debug.LogWarning(string.Format("Unable to request buffer for, Name:{0} ID:{1}", vrcphysBoneBase.name, vrcphysBoneBase.chainId));
						}
						else
						{
							this.chainBuffer.comps[num] = vrcphysBoneBase;
							if (vrcphysBoneBase.ExecutionGroup < 0)
							{
								PhysBoneManager.UpdateExecutionGroupsForRoot(vrcphysBoneBase.transform.root);
							}
							PhysBoneGroup executionGroup = this.GetExecutionGroup(vrcphysBoneBase.ExecutionGroup);
							if (executionGroup != null)
							{
								executionGroup.AddPhysBone(vrcphysBoneBase.chainId);
							}
							if (vrcphysBoneBase.root == null)
							{
								vrcphysBoneBase.root = vrcphysBoneBase.transform.root.gameObject.GetComponent<PhysBoneRoot>();
								if (vrcphysBoneBase.root == null)
								{
									vrcphysBoneBase.root = vrcphysBoneBase.transform.root.gameObject.AddComponent<PhysBoneRoot>();
								}
							}
							this.rootBuffer.Add(vrcphysBoneBase.root);
							Transform rootTransform = vrcphysBoneBase.GetRootTransform();
							Transform parent = rootTransform.parent;
							chain = this.chainBuffer.chains[num];
							chain.Init();
							chain.rootIndex = vrcphysBoneBase.root.bufferIndex;
							chain.boneCount = vrcphysBoneBase.bones.Count;
							chain.shapeId = CollisionScene.Shape.NullId;
							chain.grabBone = -1;
							chain.version = vrcphysBoneBase.version;
							chain.isAnimated = vrcphysBoneBase.isAnimated;
							chain.integrationType = vrcphysBoneBase.integrationType;
							chain.immobileType = vrcphysBoneBase.immobileType;
							chain.limitType = vrcphysBoneBase.limitType;
							chain.staticFreezeAxis = vrcphysBoneBase.staticFreezeAxis;
							chain.grabMovement = vrcphysBoneBase.grabMovement;
							chain.lastRootParentState = ((parent != null) ? new PhysBoneManager.TransformState(parent.position, parent.rotation) : PhysBoneManager.TransformState.identity);
							chain.lastSceneRootState = new PhysBoneManager.TransformState(rootTransform.root.position, Quaternion.identity);
							chain.renderBounds = new Bounds(rootTransform.position, Vector3.zero);
							vrcphysBoneBase.shape = new CollisionScene.Shape();
							vrcphysBoneBase.shape.shapeType = CollisionScene.ShapeType.AABB;
							vrcphysBoneBase.shape.maxSize = 10f;
							vrcphysBoneBase.shape.isReceiver = true;
							vrcphysBoneBase.shape.OnEnter = new Action<CollisionScene.Shape>(vrcphysBoneBase.OnShapeEnter);
							vrcphysBoneBase.shape.OnExit = new Action<CollisionScene.Shape>(vrcphysBoneBase.OnShapeExit);
							vrcphysBoneBase.shape.component = vrcphysBoneBase;
							CollisionScene.Shape shape = vrcphysBoneBase.shape;
							shape.OnIdUpdated = (Action)Delegate.Combine(shape.OnIdUpdated, new Action(vrcphysBoneBase.OnCollidersUpdated));
							vrcphysBoneBase.collidersHaveUpdated = true;
							this.collision.AddShape(vrcphysBoneBase.shape);
							this.chainBuffer.chains[num] = chain;
							vrcphysBoneBase.ResetTransformsToRestPosition();
							ReserveExtraTransform(rootTransform.parent, 0);
							ReserveExtraTransform((rootTransform.root != rootTransform) ? rootTransform.root : null, 1);
							VRCPhysBoneBase.ImmobileType immobileType = chain.immobileType;
							Matrix4x4 matrix4x;
							if (immobileType == VRCPhysBoneBase.ImmobileType.AllMotion || immobileType != VRCPhysBoneBase.ImmobileType.World)
							{
								matrix4x = ((rootTransform.parent != null) ? rootTransform.parent.localToWorldMatrix : Matrix4x4.identity);
							}
							else
							{
								matrix4x = Matrix4x4.TRS(rootTransform.root.position, Quaternion.identity, Vector3.one);
							}
							Matrix4x4 matrix4x2 = PhysBoneManager.SafeInverse(matrix4x);
							for (int i = 0; i < vrcphysBoneBase.bones.Count; i++)
							{
								VRCPhysBoneBase.Bone bone = vrcphysBoneBase.bones[i];
								PhysBoneManager.Bone bone2 = default(PhysBoneManager.Bone);
								Transform transform = bone.transform;
								int num2 = chain.boneOffset + i;
								bone2.childIndex = -1;
								bone2.parentIndex = bone.parentIndex;
								bone2.isEndBone = bone.isEndBone;
								bone2.boneChainIndex = bone.boneChainIndex;
								bone2.simulatedType = PhysBoneManager.Bone.SimulatedType.None;
								if (bone.childCount == 1 || (bone.childCount > 1 && vrcphysBoneBase.multiChildType == VRCPhysBoneBase.MultiChildType.First))
								{
									Transform transform2 = vrcphysBoneBase.bones[bone.childIndex].transform;
									bone2.childIndex = bone.childIndex;
									bone2.simulatedType = PhysBoneManager.Bone.SimulatedType.Child;
									bone2.localBoneVector = transform2.localPosition;
								}
								else if (bone2.isEndBone)
								{
									bone2.simulatedType = PhysBoneManager.Bone.SimulatedType.Endpoint;
									bone2.localBoneVector = vrcphysBoneBase.endpointPosition;
								}
								else if (vrcphysBoneBase.multiChildType != VRCPhysBoneBase.MultiChildType.Ignore)
								{
									bone2.simulatedType = PhysBoneManager.Bone.SimulatedType.Endpoint;
									VRCPhysBoneBase.MultiChildType multiChildType = vrcphysBoneBase.multiChildType;
									if (multiChildType != VRCPhysBoneBase.MultiChildType.First)
									{
										if (multiChildType == VRCPhysBoneBase.MultiChildType.Average)
										{
											bone2.localBoneVector = vrcphysBoneBase.bones[i].averageChildPos;
										}
									}
									else
									{
										bone2.localBoneVector = vrcphysBoneBase.bones[i].averageChildPos;
									}
								}
								if (bone2.simulatedType == PhysBoneManager.Bone.SimulatedType.Endpoint && !math.any(bone2.localBoneVector))
								{
									bone2.simulatedType = PhysBoneManager.Bone.SimulatedType.None;
								}
								float t = vrcphysBoneBase.CalcTransformRatio(bone2.boneChainIndex);
								float t2 = vrcphysBoneBase.CalcTransformRatio(bone2.boneChainIndex + 1);
								bone2.radiusBegin = (bone.sphereCollision ? 0f : vrcphysBoneBase.CalcRadius(t));
								bone2.radiusEnd = vrcphysBoneBase.CalcRadius(t2);
								float t3 = vrcphysBoneBase.CalcBoneRatio(bone2.boneChainIndex);
								bone2.pull = vrcphysBoneBase.CalcPull(t3);
								bone2.spring = vrcphysBoneBase.CalcSpring(t3);
								bone2.stiffness = vrcphysBoneBase.CalcStiffness(t3);
								bone2.immobile = vrcphysBoneBase.CalcImmobile(t3);
								bone2.gravity = vrcphysBoneBase.CalcGravity(t3);
								bone2.gravityFalloff = vrcphysBoneBase.CalcGravityFalloff(t3);
								bone2.maxAngle = vrcphysBoneBase.CalcMaxAngle(t3);
								bone2.beginPoint = transform.position;
								bone2.endPoint = transform.TransformPoint(bone2.localBoneVector);
								bone2.prevEndPoint = bone2.endPoint;
								bone2.prevVector = bone2.endPoint - bone2.beginPoint;
								bone2.prevLocalRotation = transform.localRotation;
								bone2.localPoseBoneVector = bone2.localBoneVector;
								bone2.localPoseRotation = transform.localRotation;
								bone2.originalLocalPosition = transform.localPosition;
								bone2.originalLocalRotation = transform.localRotation;
								bone2.originalLocalVector = bone2.localBoneVector;
								bone2.originalLocalBoneLength = math.length(bone2.localBoneVector);
								bone2.originalRootEndpoint = math.transform((parent != null) ? math.inverse(parent.localToWorldMatrix) : float4x4.identity, bone2.endPoint);
								bone2.originalLocalGravityNormal = bone.localGravityDirection;
								bone2.immobileEndpoint = math.transform(matrix4x2, bone2.endPoint);
								bone2.stretchMotion = vrcphysBoneBase.CalcStretchMotion(t3);
								bone2.squish = 1f - vrcphysBoneBase.CalcMaxSquish(t3);
								bone2.stretch = 1f + vrcphysBoneBase.CalcMaxStretch(t3);
								bone2.limitRotation = vrcphysBoneBase.CalcLimitRotation(t3);
								PhysBoneManager.CalcLimitAxis(bone2.originalLocalVector, bone2.limitRotation, out bone2.limitAxisX, out bone2.limitAxisY);
								if (math.any(chain.staticFreezeAxis))
								{
									bone2.limitAxisX = chain.staticFreezeAxis;
								}
								this.boneBuffer.bones[num2] = bone2;
								this.boneBuffer.transformArray[num2] = transform;
								this.boneBuffer.transformData[num2] = new PhysBoneManager.TransformData(transform);
							}
						}
					}
				}
			}
			this.compsToAdd.Clear();

            // Token: 0x060001F2 RID: 498 RVA: 0x0000EAC8 File Offset: 0x0000CCC8
            void ReserveExtraTransform(Transform transform, int offset)
			{
                int num = chain.boneOffset + chain.boneCount + offset;
                this.boneBuffer.transformArray[num] = transform;
                this.boneBuffer.transformData[num] = new PhysBoneManager.TransformData(transform);
            }
        }

		// Token: 0x060001B0 RID: 432 RVA: 0x0000C494 File Offset: 0x0000A694
		internal void RemoveRoot(PhysBoneRoot root)
		{
			if (!this.rootsToRemove.Contains(root.bufferIndex))
			{
				this.rootsToRemove.Add(root.bufferIndex);
			}
			root.bufferIndex = -1;
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x0000C4C4 File Offset: 0x0000A6C4
		private void RemoveRoots()
		{
			foreach (int index in this.rootsToRemove)
			{
				this.rootBuffer.Remove(index);
			}
			this.rootsToRemove.Clear();
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0000C528 File Offset: 0x0000A728
		public void AddCollider(VRCPhysBoneColliderBase collider)
		{
			this.collidersToRemove.RemoveAll((PhysBoneManager.ColliderToRemoveData x) => x.comp == collider);
			this.collidersToAdd.Add(collider);
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x0000C56B File Offset: 0x0000A76B
		public void RemoveCollider(VRCPhysBoneColliderBase collider)
		{
			this.collidersToAdd.Remove(collider);
			this.collidersToRemove.Add(new PhysBoneManager.ColliderToRemoveData(collider));
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x0000C58C File Offset: 0x0000A78C
		private void RemoveColliders()
		{
			foreach (PhysBoneManager.ColliderToRemoveData colliderToRemoveData in this.collidersToRemove)
			{
				if (colliderToRemoveData.shape != null)
				{
					PhysBoneGroup executionGroup = this.GetExecutionGroup(colliderToRemoveData.executionGroup);
					if (executionGroup != null)
					{
						executionGroup.RemoveShape(colliderToRemoveData.shape.id);
					}
					this.collision.RemoveShape(colliderToRemoveData.shape);
				}
			}
			this.collidersToRemove.Clear();
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000C620 File Offset: 0x0000A820
		private void AddColliders()
		{
			if (this.collidersToAdd.Count == 0)
			{
				return;
			}
			foreach (VRCPhysBoneColliderBase vrcphysBoneColliderBase in this.collidersToAdd)
			{
				if (!(vrcphysBoneColliderBase == null))
				{
					if (vrcphysBoneColliderBase.shape == null)
					{
						vrcphysBoneColliderBase.InitShape();
					}
					this.collision.AddShape(vrcphysBoneColliderBase.shape);
				}
			}
			this.collision.SyncShapesNow();
			foreach (VRCPhysBoneColliderBase vrcphysBoneColliderBase2 in this.collidersToAdd)
			{
				if (vrcphysBoneColliderBase2 == null || vrcphysBoneColliderBase2.shape.id == CollisionScene.Shape.NullId)
				{
					PhysBoneManager.ReportCriticalError(PhysBoneManager.CriticalErrorType.MaxShapes, string.Format("Active:{0} Max:{1}", this.collision.activeShapes.Length, 8192));
				}
				else
				{
					if (vrcphysBoneColliderBase2.ExecutionGroup < 0)
					{
						PhysBoneManager.UpdateExecutionGroupsForRoot(vrcphysBoneColliderBase2.transform.root);
					}
					PhysBoneGroup executionGroup = this.GetExecutionGroup(vrcphysBoneColliderBase2.ExecutionGroup);
					if (executionGroup != null)
					{
						executionGroup.AddShape(vrcphysBoneColliderBase2.shape.id);
					}
				}
			}
			this.collidersToAdd.Clear();
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x0000C784 File Offset: 0x0000A984
		public IEnumerator<PhysBoneManager.Chain> GetChains()
		{
			int num;
			for (int i = 0; i < this.chainBuffer.Length; i = num + 1)
			{
				yield return this.chainBuffer.chains[i];
				num = i;
			}
			yield break;
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x0000C793 File Offset: 0x0000A993
		public int FindChainIndex(ChainId id)
		{
			return this.buffer.FindIndex(id);
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x0000C7A1 File Offset: 0x0000A9A1
		public PhysBoneManager.Chain GetChain(int index)
		{
			return this.chainBuffer.chains[index];
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x0000C7B4 File Offset: 0x0000A9B4
		public VRCPhysBoneBase GetChainComponent(int index)
		{
			return this.chainBuffer.comps[index];
		}

		// Token: 0x060001BA RID: 442 RVA: 0x0000C7C7 File Offset: 0x0000A9C7
		public void SetChain(int index, PhysBoneManager.Chain chain)
		{
			this.chainBuffer.chains[index] = chain;
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0000C7DB File Offset: 0x0000A9DB
		public PhysBoneManager.Bone GetBone(int index)
		{
			return this.boneBuffer.bones[index];
		}

		// Token: 0x060001BC RID: 444 RVA: 0x0000C7EE File Offset: 0x0000A9EE
		public void SetBone(int index, PhysBoneManager.Bone bone)
		{
			this.boneBuffer.bones[index] = bone;
		}

		// Token: 0x060001BD RID: 445 RVA: 0x0000C802 File Offset: 0x0000AA02
		public PhysBoneManager.TransformData GetTransformData(int index)
		{
			return this.boneBuffer.transformData[index];
		}

		// Token: 0x060001BE RID: 446 RVA: 0x0000C818 File Offset: 0x0000AA18
		public static void CalcLimitAxis(Vector3 boneVector, Vector3 limitRotation, out Vector3 limitAxisX, out Vector3 limitAxisY)
		{
			float3 @float;
			float3 float2;
			PhysBoneManager.CalcLimitAxis(boneVector, limitRotation, out @float, out float2);
			limitAxisX = @float;
			limitAxisY = float2;
		}

		// Token: 0x060001BF RID: 447 RVA: 0x0000C854 File Offset: 0x0000AA54
		//private static void CalcLimitAxis(float3 boneVector, float3 limitRotation, out float3 limitAxisX, out float3 limitAxisY)
		//{
		//	float3 @float = math.normalizesafe(boneVector, default(float3));
		//	Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, @float) * math.EulerXYZ(limitRotation * 0.017453292f);
		//	//Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, @float) * Quaternion.Euler(limitRotation * 0.017453292f);
		//	limitAxisX = math.rotate(quaternion, Vector3.right);
		//	limitAxisY = math.rotate(quaternion, Vector3.up);
		//}
		private static void CalcLimitAxis(float3 boneVector, float3 limitRotation, out float3 limitAxisX, out float3 limitAxisY)
		{
			// normalize bone vector, fallback to up if zero
			float3 dir = math.normalizesafe(boneVector, new float3(0f, 1f, 0f));

			// Build a from->to rotation as a Unity.Mathematics.quaternion (don't mix UnityEngine.Quaternion here)
			float3 from = new float3(0f, 1f, 0f);
			float dot = math.dot(from, dir);
			quaternion fromTo;

			if (dot >= 1f)
			{
				fromTo = quaternion.identity;
			}
			else if (dot > -1f)
			{
				fromTo = math.normalizesafe(new quaternion(new float4(math.cross(from, dir), dot + 1f)));
			}
			else
			{
				// 180 degree case: pick an arbitrary axis perpendicular to `from`
				float3 axis = math.cross(from, new float3(1f, 0f, 0f));
				if (math.lengthsq(axis) > 0f)
				{
					fromTo = quaternion.AxisAngle(math.normalize(axis), 3.1415927f);
				}
				else
				{
					fromTo = quaternion.RotateY(3.1415927f);
				}
			}

			// Apply the limit rotation (convert degrees to radians)
			quaternion limitQ = quaternion.Euler(limitRotation * 0.017453292f, (math.RotationOrder)4);

			quaternion finalQ = math.mul(fromTo, limitQ);

			limitAxisX = math.rotate(finalQ, new float3(1f, 0f, 0f));
			limitAxisY = math.rotate(finalQ, new float3(0f, 1f, 0f));
		}
            // Token: 0x060001C0 RID: 448 RVA: 0x0000C8D2 File Offset: 0x0000AAD2
            [MethodImpl(256)]
		private static bool AlmostEquals(float3 a, float3 b, float tolerance = 0.0001f)
		{
			return PhysBoneManager.AlmostEquals(a.x, b.x, tolerance) && PhysBoneManager.AlmostEquals(a.y, b.y, tolerance) && PhysBoneManager.AlmostEquals(a.z, b.z, tolerance);
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000C910 File Offset: 0x0000AB10
		[MethodImpl(256)]
		private static bool AlmostEquals(float a, float b, float tolerance = 0.0001f)
		{
			return math.abs(a - b) <= tolerance;
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000C920 File Offset: 0x0000AB20
		[MethodImpl(256)]
		private static bool HasChanged(float3 prev, float3 next)
		{
			return math.distance(prev, next) > math.clamp(math.length(prev) * 0.001f, float.Epsilon, 0.01f);
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x0000C948 File Offset: 0x0000AB48
		[MethodImpl(256)]
		private static bool HasChanged(quaternion prev, quaternion next)
		{
			return !PhysBoneManager.AlmostEquals(prev.value.x, next.value.x, 0.0001f) || !PhysBoneManager.AlmostEquals(prev.value.y, next.value.y, 0.0001f) || !PhysBoneManager.AlmostEquals(prev.value.z, next.value.z, 0.0001f) || !PhysBoneManager.AlmostEquals(prev.value.w, next.value.w, 0.0001f);
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x0000C9E0 File Offset: 0x0000ABE0
		private static float4x4 SafeInverse(float4x4 value)
		{
			if (math.abs(math.determinant(value)) < 1.1754944E-38f)
			{
				return float4x4.zero;
			}
			return math.inverse(value);
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000CA00 File Offset: 0x0000AC00
		private void Awake()
		{
			if (PhysBoneManager.Inst == null)
			{
				PhysBoneManager.Inst = this;
			}
			else
			{
				base.enabled = false;
			}
			this.Init();
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000CA24 File Offset: 0x0000AC24
		private void OnDestroy()
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			if (this.buffer != null)
			{
				this.buffer.Dispose();
			}
			if (this.collision != null)
			{
				this.collision.Dispose();
			}
			if (this.rootBuffer != null)
			{
				this.rootBuffer.Dispose();
			}
			NativeArray<PhysBoneManager.CriticalErrorType> nativeArray = this.errorBuffer;
			this.errorBuffer.Dispose();
			this.buffer = null;
			this.collision = null;
			this.chainBuffer = null;
			this.boneBuffer = null;
			this.rootBuffer = null;
			if (this.executionGroups != null)
			{
				for (int i = 0; i < this.executionGroups.Length; i++)
				{
					this.executionGroups[i].Dispose();
				}
			}
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x0000CACC File Offset: 0x0000ACCC
		public void Init()
		{
			if (this.hasInit)
			{
				return;
			}
			this.hasInit = true;
			this.chainBuffer = new ChainBuffer(256);
			this.boneBuffer = new BoneBuffer(2048);
			this.buffer = new MemoryBuffer(this.chainBuffer, this.boneBuffer);
			this.rootBuffer = new RootsBuffer(128);
			this.errorBuffer = new NativeArray<PhysBoneManager.CriticalErrorType>(1, (Allocator)4, (NativeArrayOptions)1);
			this.executionGroups = new PhysBoneGroup[8];
			for (int i = 0; i < this.executionGroups.Length; i++)
			{
				this.executionGroups[i] = new PhysBoneGroup(this, i);
			}
			this.InitCollision();
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x0000CB72 File Offset: 0x0000AD72
		private void FixedUpdate()
		{
			this.fixedTimeElapsed += Time.fixedDeltaTime;
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x0000CB86 File Offset: 0x0000AD86
		private void LateUpdate()
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			this.realTimeElapsed += Time.deltaTime;
		}

		// Token: 0x060001CA RID: 458 RVA: 0x0000CBA0 File Offset: 0x0000ADA0
		private void UpdateChains()
		{
			for (int i = 0; i < this.chainBuffer.Length; i++)
			{
				VRCPhysBoneBase vrcphysBoneBase = this.chainBuffer.comps[i];
				if (vrcphysBoneBase.collidersHaveUpdated)
				{
					this.UpdateCollidersForChain(i, vrcphysBoneBase);
					vrcphysBoneBase.collidersHaveUpdated = false;
				}
				if (!string.IsNullOrEmpty(vrcphysBoneBase.parameter))
				{
					PhysBoneManager.Chain chain = this.chainBuffer.chains[i];
					vrcphysBoneBase.param_AngleValue = chain.paramAngle;
					vrcphysBoneBase.param_StretchValue = chain.paramStretch;
					vrcphysBoneBase.param_SquishValue = chain.paramSquish;
					if (vrcphysBoneBase.param_Angle != null)
					{
						vrcphysBoneBase.param_Angle.floatVal = vrcphysBoneBase.param_AngleValue;
					}
					if (vrcphysBoneBase.param_Stretch != null)
					{
						vrcphysBoneBase.param_Stretch.floatVal = vrcphysBoneBase.param_StretchValue;
					}
					if (vrcphysBoneBase.param_Squish != null)
					{
						vrcphysBoneBase.param_Squish.floatVal = vrcphysBoneBase.param_SquishValue;
					}
				}
				if (vrcphysBoneBase.configHasUpdated)
				{
					vrcphysBoneBase.configHasUpdated = false;
					PhysBoneManager.Chain chain2 = this.chainBuffer.chains[i];
					chain2.grabMovement = vrcphysBoneBase.grabMovement;
					chain2.limitType = vrcphysBoneBase.limitType;
					chain2.version = vrcphysBoneBase.version;
					chain2.isAnimated = vrcphysBoneBase.isAnimated;
					chain2.integrationType = vrcphysBoneBase.integrationType;
					chain2.immobileType = vrcphysBoneBase.immobileType;
					for (int j = 0; j < chain2.boneCount; j++)
					{
						int num = chain2.boneOffset + j;
						PhysBoneManager.Bone bone = this.boneBuffer.bones[num];
						Transform transform = vrcphysBoneBase.bones[j].transform;
						this.boneBuffer.transformArray[num] = transform;
						bone.simulatedType = PhysBoneManager.Bone.SimulatedType.None;
						if (bone.childIndex > 0)
						{
							bone.simulatedType = PhysBoneManager.Bone.SimulatedType.Child;
						}
						else if (bone.isEndBone)
						{
							bone.simulatedType = PhysBoneManager.Bone.SimulatedType.Endpoint;
							bone.localBoneVector = vrcphysBoneBase.endpointPosition;
						}
						else if (vrcphysBoneBase.multiChildType == VRCPhysBoneBase.MultiChildType.Average)
						{
							bone.simulatedType = PhysBoneManager.Bone.SimulatedType.Endpoint;
							bone.localBoneVector = vrcphysBoneBase.bones[j].averageChildPos;
						}
						if (bone.simulatedType == PhysBoneManager.Bone.SimulatedType.Endpoint && !math.any(bone.localBoneVector))
						{
							bone.simulatedType = PhysBoneManager.Bone.SimulatedType.None;
						}
						float t = vrcphysBoneBase.CalcTransformRatio(bone.boneChainIndex);
						float t2 = vrcphysBoneBase.CalcTransformRatio(bone.boneChainIndex + 1);
						bone.radiusBegin = vrcphysBoneBase.CalcRadius(t);
						bone.radiusEnd = vrcphysBoneBase.CalcRadius(t2);
						float t3 = vrcphysBoneBase.CalcBoneRatio(bone.boneChainIndex);
						bone.pull = vrcphysBoneBase.CalcPull(t3);
						bone.spring = vrcphysBoneBase.CalcSpring(t3);
						bone.stiffness = vrcphysBoneBase.CalcStiffness(t3);
						bone.immobile = vrcphysBoneBase.CalcImmobile(t3);
						bone.gravity = vrcphysBoneBase.CalcGravity(t3);
						bone.gravityFalloff = vrcphysBoneBase.CalcGravityFalloff(t3);
						bone.maxAngle = vrcphysBoneBase.CalcMaxAngle(t3);
						bone.stretchMotion = vrcphysBoneBase.CalcStretchMotion(t3);
						bone.squish = 1f - vrcphysBoneBase.CalcMaxSquish(t3);
						bone.stretch = 1f + vrcphysBoneBase.CalcMaxStretch(t3);
						bone.limitRotation = vrcphysBoneBase.CalcLimitRotation(t3);
						PhysBoneManager.CalcLimitAxis(bone.originalLocalVector, bone.limitRotation, out bone.limitAxisX, out bone.limitAxisY);
						if (math.any(chain2.staticFreezeAxis))
						{
							bone.limitAxisX = chain2.staticFreezeAxis;
						}
						this.boneBuffer.bones[num] = bone;
					}
					this.chainBuffer.chains[i] = chain2;
				}
			}
		}

		// Token: 0x060001CB RID: 459 RVA: 0x0000CF50 File Offset: 0x0000B150
		private JobHandle ScheduleReadBoneJob(JobHandle dependsOn = default(JobHandle))
		{
			dependsOn = IJobParallelForTransformExtensions.Schedule<PhysBoneManager.ReadBoneJob>(new PhysBoneManager.ReadBoneJob
			{
				transformData = this.boneBuffer.transformAccess.AsArray()
			}, this.boneBuffer.transformArray, dependsOn);
			return dependsOn;
		}

		// Token: 0x060001CC RID: 460 RVA: 0x0000CF94 File Offset: 0x0000B194
		internal static void ReportCriticalError(PhysBoneManager.CriticalErrorType type, string message = null)
		{
			if (PhysBoneManager.hasReportedCriticalError)
			{
				return;
			}
			PhysBoneManager.hasReportedCriticalError = true;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(string.Format("Physbone Critical Error:{0}\n", type));
			if (message != null)
			{
				stringBuilder.Append("Details: " + message + "\n");
			}
			if (PhysBoneManager.Inst != null && PhysBoneManager.Inst.rootBuffer.roots.Length == 1)
			{
				PhysBoneRoot physBoneRoot = PhysBoneManager.Inst.rootBuffer.comps[0];
				stringBuilder.Append("Avatar:" + physBoneRoot.avatarId + "\n");
			}
			Debug.LogError(stringBuilder.ToString());
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0000D048 File Offset: 0x0000B248
		private void DumpObject(StringBuilder sb, object obj)
		{
			FieldInfo[] fields = obj.GetType().GetFields();
			sb.Append("{\n");
			foreach (FieldInfo fieldInfo in fields)
			{
				object value = fieldInfo.GetValue(obj);
				string text = (value != null) ? value.ToString() : null;
				sb.Append(string.Concat(new string[]
				{
					"  ",
					fieldInfo.Name,
					" = ",
					text,
					"\n"
				}));
			}
			sb.Append("}\n");
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000D0D8 File Offset: 0x0000B2D8
		internal JobHandle ScheduleExecutionJob(JobHandle dependsOn = default(JobHandle))
		{
			if (this.errorBuffer[0] != PhysBoneManager.CriticalErrorType.None)
			{
				PhysBoneManager.ReportCriticalError(this.errorBuffer[0], null);
				this.errorBuffer[0] = PhysBoneManager.CriticalErrorType.None;
			}
			this.fullFrameTimeElapsed += Time.deltaTime;
			this.executeShapeUpdates = (this.fullFrameTimeElapsed >= 0.016666668f);
			if (this.executeShapeUpdates)
			{
				this.fullFrameTimeElapsed = 0f;
			}
			this.executeShapeUpdates = true;
			this.RemoveRoots();
			this.RemoveChains();
			this.AddChains();
			this.RemoveColliders();
			this.AddColliders();
			this.UpdateRoots();
			dependsOn = this.UpdateAndScheduleColliders(dependsOn);
			dependsOn = this.ScheduleUpdateRootsJob(dependsOn);
			dependsOn = this.ScheduleReadBoneJob(dependsOn);
			if (this.executeShapeUpdates)
			{
				this.UpdateChains();
			}
			this.UpdateGrabs();
			for (int i = 0; i < 8; i++)
			{
				dependsOn = this.ScheduleExecutionGroupJob(i, dependsOn);
			}
			return dependsOn;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000D1C0 File Offset: 0x0000B3C0
		internal JobHandle ScheduleUpdateRootsJob(JobHandle dependsOn)
		{
			dependsOn = IJobParallelForExtensions.Schedule<PhysBoneManager.UpdateRootsJob>(new PhysBoneManager.UpdateRootsJob
			{
				fixedTime = this.fixedTimeElapsed,
				realTime = this.realTimeElapsed,
				roots = this.rootBuffer.roots.AsArray()
			}, this.rootBuffer.roots.Length, PhysBoneManager.THREAD_BATCH_SIZE, dependsOn);
			this.fixedTimeElapsed = 0f;
			this.realTimeElapsed = 0f;
			return dependsOn;
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x0000D23C File Offset: 0x0000B43C
		internal JobHandle ScheduleExecutionGroupJob(int groupIndex, JobHandle dependsOn = default(JobHandle))
		{
			PhysBoneGroup physBoneGroup = this.executionGroups[groupIndex];
			if (!this.executeShapeUpdates || groupIndex > 0)
			{
				dependsOn = this.collision.ScheduleUpdateShapePositions(physBoneGroup.GetShapes(), dependsOn);
			}
			NativeArray<int> chains = physBoneGroup.GetChains();
			if (chains.IsCreated && chains.Length > 0)
			{
				PhysBoneManager.PhysBoneJob physBoneJob = default(PhysBoneManager.PhysBoneJob);
				physBoneJob.currentTime = Time.time;
				physBoneJob.distanceCullOrigin = this.distanceCullOrigin;
				physBoneJob.chainIndices = chains;
				physBoneJob.chains = this.chainBuffer.chains.AsArray();
				physBoneJob.roots = this.rootBuffer.roots.AsArray();
				physBoneJob.shapeData = this.collision.shapeData;
				physBoneJob.bones = this.boneBuffer.bones.AsArray();
				physBoneJob.transformAccess = this.boneBuffer.transformAccess.AsArray();
				physBoneJob.transformData = this.boneBuffer.transformData.AsArray();
				physBoneJob.errorBuffer = this.errorBuffer;
				dependsOn = IJobParallelForExtensions.Schedule<PhysBoneManager.PhysBoneJob>(physBoneJob, physBoneJob.chainIndices.Length, PhysBoneManager.THREAD_BATCH_SIZE, dependsOn);
			}
			return dependsOn;
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x0000D36D File Offset: 0x0000B56D
		public static float CalcBoneScale(Vector3 scale)
		{
			return Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x0000D38B File Offset: 0x0000B58B
		public void CompleteJob()
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x0000D394 File Offset: 0x0000B594
		public void PrintDebug()
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			this.buffer.PrintDebug();
			Debug.Log(string.Format("VRCPhysBoneBaseManager Chains:{0} Bones:{1}", this.chainBuffer.Length, this.boneBuffer.Length));
			for (int i = 0; i < this.chainBuffer.Length; i++)
			{
				PhysBoneManager.Chain chain = this.chainBuffer.chains[i];
				VRCPhysBoneBase vrcphysBoneBase = this.chainBuffer.comps[i];
				Debug.Log(string.Format("Chain - Name:{0} Offset:{1} Count:{2}", vrcphysBoneBase.gameObject.name, chain.boneOffset, chain.boneCount));
			}
			for (int j = 0; j < this.boneBuffer.Length; j++)
			{
				Transform transform = this.boneBuffer.transformArray[j];
				PhysBoneManager.Bone bone = this.boneBuffer.bones[j];
				if (transform != null)
				{
					Debug.Log(string.Format("Bone {0} - Transform:{1} Parent:{2} Child:{3}", new object[]
					{
						j,
						transform.gameObject.name,
						bone.parentIndex,
						bone.childIndex
					}));
				}
				else
				{
					Debug.Log(string.Format("Bone {0} - Transform:NULL", j));
				}
			}
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0000D4F8 File Offset: 0x0000B6F8
		public void PrintDebug(ChainId chainId)
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			int num = this.FindChainIndex(chainId);
			if (num < 0)
			{
				return;
			}
			PhysBoneManager.Chain chain = this.GetChain(num);
			Debug.Log(string.Format("Chain - ID:{0} Bones:{1} IsAnimated:{2}", chainId, chain.boneCount, chain.isAnimated));
			if (this.FindPose(chainId) != null)
			{
				Debug.Log("Is Posed");
			}
			for (int i = 0; i < chain.boneCount; i++)
			{
				int index = chain.boneOffset + i;
				this.GetBone(index);
			}
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x0000D57F File Offset: 0x0000B77F
		public void OnDrawGizmos()
		{
			if (this.drawGizmos)
			{
				this.collision.DrawGizmos();
			}
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x0000D594 File Offset: 0x0000B794
		private static bool IsCyclicDependency(PhysBoneManager.SortingData parent, PhysBoneManager.SortingData child)
		{
			if (parent.parentDependency == child)
			{
				return false;
			}
			foreach (PhysBoneManager.SortingData sortingData in PhysBoneManager.sortingData)
			{
				sortingData.visited = false;
			}
			return RecusriveSearch(child);

            // Token: 0x060001F3 RID: 499 RVA: 0x0000EB18 File Offset: 0x0000CD18
            bool RecusriveSearch(PhysBoneManager.SortingData data)

        {
                foreach (PhysBoneManager.SortingData sortingData in data.dependencies)
                {
                    if (!sortingData.visited)
                    {
                        sortingData.visited = true;
                        if (sortingData == parent)
                        {
                            return true;
                        }
                        if (RecusriveSearch(sortingData))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

		// Token: 0x060001D7 RID: 471 RVA: 0x0000D604 File Offset: 0x0000B804
		private static void UpdateExecutionGroupsForRoot(Transform transform)
		{
			bool flag;
			bool flag2;
			PhysBoneManager.UpdateExecutionGroupsForRoot(transform, out flag, out flag2);
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000D61C File Offset: 0x0000B81C
		public static void UpdateExecutionGroupsForRoot(Transform transform, out bool hasUnassignedGroups, out bool hasCyclicDependencies)
		{
			Component root = transform.root;
			hasUnassignedGroups = false;
			hasCyclicDependencies = false;
			PhysBoneManager.IJobSortable[] componentsInChildren = root.GetComponentsInChildren<PhysBoneManager.IJobSortable>(true);
			PhysBoneManager.sortingData.Clear();
			foreach (PhysBoneManager.IJobSortable source in componentsInChildren)
			{
				PhysBoneManager.SortingData sortingData = new PhysBoneManager.SortingData();
				sortingData.source = source;
				PhysBoneManager.sortingData.Add(sortingData);
			}
			foreach (PhysBoneManager.SortingData sortingData2 in PhysBoneManager.sortingData)
			{
				sortingData2.parentDependency = FindParentDependency(sortingData2.source);
				if (sortingData2.parentDependency != null)
				{
					sortingData2.dependencies.Add(sortingData2.parentDependency);
				}
				List<PhysBoneManager.IJobSortable> list = new List<PhysBoneManager.IJobSortable>();
				sortingData2.source.GetKnownDependencies(list);
				foreach (PhysBoneManager.IJobSortable item in list)
				{
					PhysBoneManager.SortingData sortingData3 = FindSortingData(item);
					if (sortingData3 != null)
					{
						sortingData2.dependencies.Add(sortingData3);
					}
				}
			}
			List<ValueTuple<PhysBoneManager.SortingData, PhysBoneManager.SortingData>> list2 = new List<ValueTuple<PhysBoneManager.SortingData, PhysBoneManager.SortingData>>();
			foreach (PhysBoneManager.SortingData sortingData4 in PhysBoneManager.sortingData)
			{
				foreach (PhysBoneManager.SortingData sortingData5 in sortingData4.dependencies)
				{
					if (PhysBoneManager.IsCyclicDependency(sortingData4, sortingData5))
					{
						Debug.LogWarning(string.Concat(new string[]
						{
							"Cyclic dependency found for component ",
							sortingData4.source.GetType().Name,
							" on '",
							(sortingData4.source as MonoBehaviour).name,
							"'.  As a result some components may run out of order, usually meaning a collider being run one frame behind.  If the cyclic dependency is intentional you may ignore this warning, otherwise you should remove the offending dependency from the component."
						}));
						list2.Add(new ValueTuple<PhysBoneManager.SortingData, PhysBoneManager.SortingData>(sortingData4, sortingData5));
						hasCyclicDependencies = true;
					}
				}
			}
			foreach (ValueTuple<PhysBoneManager.SortingData, PhysBoneManager.SortingData> valueTuple in list2)
			{
				valueTuple.Item1.dependencies.Remove(valueTuple.Item2);
			}
			for (int j = 0; j < 8; j++)
			{
				for (int k = 0; k < PhysBoneManager.sortingData.Count; k++)
				{
					PhysBoneManager.SortingData sortingData6 = PhysBoneManager.sortingData[k];
					bool flag = true;
					foreach (PhysBoneManager.SortingData sortingData7 in sortingData6.dependencies)
					{
						if (sortingData7.executionGroup < 0 || sortingData7.executionGroup >= j)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						sortingData6.executionGroup = j;
						PhysBoneManager.UpdateExecutionGroup(sortingData6.source, j);
						ListExtensions.RemoveAtSwapBack<PhysBoneManager.SortingData>(PhysBoneManager.sortingData, k);
						k--;
					}
				}
				if (PhysBoneManager.sortingData.Count == 0)
				{
					break;
				}
			}
			for (int l = 0; l < PhysBoneManager.sortingData.Count; l++)
			{
				PhysBoneManager.SortingData sortingData8 = PhysBoneManager.sortingData[l];
				PhysBoneManager.UpdateExecutionGroup(sortingData8.source, 8);
				Debug.LogError(string.Concat(new string[]
				{
					"Component ",
					sortingData8.GetType().Name,
					" on '",
					(sortingData8.source as MonoBehaviour).name,
					"' has exceeded max dependency depth.  It will be ignored."
				}));
			}
			hasUnassignedGroups = (PhysBoneManager.sortingData.Count > 0);

			// Token: 0x060001F4 RID: 500 RVA: 0x0000EB90 File Offset: 0x0000CD90
			PhysBoneManager.SortingData FindSortingData(PhysBoneManager.IJobSortable item)
			{
				return PhysBoneManager.sortingData.Find((PhysBoneManager.SortingData a) => a.source == item);
			}

            // Token: 0x060001F5 RID: 501 RVA: 0x0000EBC0 File Offset: 0x0000CDC0
   //         PhysBoneManager.SortingData FindParentDependency(PhysBoneManager.IJobSortable item)
			//{
   //             Transform parent = item.SortingBaseTransform.parent;
   //             while (parent != null)
   //             {
   //                 List<PhysBoneManager.SortingData> list = PhysBoneManager.sortingData;
   //                 Predicate<PhysBoneManager.SortingData> predicate;
   //                 if ((predicate = <> 9__3) == null)
   //                 {
   //                     predicate = (<> 9__3 = ((PhysBoneManager.SortingData a) => a.source as VRCPhysBoneBase != null && a.source.SortingBaseTransform == parent));
   //                 }
   //                 int num = list.FindIndex(predicate);
   //                 if (num >= 0)
   //                 {
   //                     return PhysBoneManager.sortingData[num];
   //                 }
   //                 parent = parent.parent;
   //             }
   //             return null;
   //         }
            PhysBoneManager.SortingData FindParentDependency(PhysBoneManager.IJobSortable item)
            {
                Transform parent = item.SortingBaseTransform.parent;

                while (parent != null)
                {
                    int index = PhysBoneManager.sortingData.FindIndex(a =>
                        a.source is VRCPhysBoneBase && a.source.SortingBaseTransform == parent);

                    if (index >= 0)
                    {
                        return PhysBoneManager.sortingData[index];
                    }

                    parent = parent.parent;
                }

                return null;
            }

        }

        // Token: 0x060001D9 RID: 473 RVA: 0x0000D9F8 File Offset: 0x0000BBF8
        private static void UpdateExecutionGroup(PhysBoneManager.IJobSortable source, int ex)
		{
			if (source.ExecutionGroup == ex)
			{
				return;
			}
			int executionGroup = source.ExecutionGroup;
			bool flag = false;
			if (PhysBoneManager.Inst != null)
			{
				flag = PhysBoneManager.Inst.RemoveFromExecutionGroup(source);
			}
			source.ExecutionGroup = ex;
			if (PhysBoneManager.Inst != null && flag)
			{
				PhysBoneManager.Inst.AddToExecutionGroup(source);
			}
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000DA54 File Offset: 0x0000BC54
		private bool RemoveFromExecutionGroup(PhysBoneManager.IJobSortable source)
		{
			PhysBoneGroup executionGroup = this.GetExecutionGroup(source.ExecutionGroup);
			if (executionGroup != null)
			{
				VRCPhysBoneBase vrcphysBoneBase = source as VRCPhysBoneBase;
				if (vrcphysBoneBase != null)
				{
					return executionGroup.RemovePhysBone(vrcphysBoneBase.chainId);
				}
				VRCPhysBoneColliderBase vrcphysBoneColliderBase = source as VRCPhysBoneColliderBase;
				if (vrcphysBoneColliderBase != null)
				{
					return executionGroup.RemoveShape(vrcphysBoneColliderBase.shape.id);
				}
			}
			return false;
		}

		// Token: 0x060001DB RID: 475 RVA: 0x0000DAA8 File Offset: 0x0000BCA8
		private void AddToExecutionGroup(PhysBoneManager.IJobSortable source)
		{
			PhysBoneGroup executionGroup = this.GetExecutionGroup(source.ExecutionGroup);
			if (executionGroup != null)
			{
				VRCPhysBoneBase vrcphysBoneBase = source as VRCPhysBoneBase;
				if (vrcphysBoneBase != null)
				{
					executionGroup.AddPhysBone(vrcphysBoneBase.chainId);
					return;
				}
				VRCPhysBoneColliderBase vrcphysBoneColliderBase = source as VRCPhysBoneColliderBase;
				if (vrcphysBoneColliderBase == null)
				{
					return;
				}
				executionGroup.AddShape(vrcphysBoneColliderBase.shape.id);
			}
		}

		// Token: 0x060001DC RID: 476 RVA: 0x0000DAF8 File Offset: 0x0000BCF8
		public void MarkGroupListDirty(int index)
		{
			PhysBoneGroup executionGroup = this.GetExecutionGroup(index);
			if (executionGroup == null)
			{
				return;
			}
			executionGroup.MarkDirty();
		}

		// Token: 0x060001DD RID: 477 RVA: 0x0000DB0B File Offset: 0x0000BD0B
		private PhysBoneGroup GetExecutionGroup(int index)
		{
			if (index < 0 || index >= 8)
			{
				return null;
			}
			return this.executionGroups[index];
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000DB1F File Offset: 0x0000BD1F
		private void InitCollision()
		{
			this.collision = new CollisionScene();
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0000DB2C File Offset: 0x0000BD2C
		private JobHandle UpdateAndScheduleColliders(JobHandle dependsOn = default(JobHandle))
		{
			if (this.executeShapeUpdates)
			{
				return this.collision.UpdateAndSchedule(Time.deltaTime, false, dependsOn);
			}
			return this.collision.ScheduleReadTransforms(dependsOn);
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000DB58 File Offset: 0x0000BD58
		private void UpdateCollidersForChain(int chainIndex, VRCPhysBoneBase comp)
		{
			PhysBoneManager.Chain chain = this.chainBuffer.chains[chainIndex];
			chain.shapeId = comp.shape.id;
			chain.colliders.Clear();
			foreach (VRCPhysBoneColliderBase vrcphysBoneColliderBase in comp.colliders)
			{
				if (!(vrcphysBoneColliderBase == null) && vrcphysBoneColliderBase.shape != null && vrcphysBoneColliderBase.shape.id != CollisionScene.Shape.NullId && chain.colliders.Length < 32)
				{
					PhysBoneManager.Collider collider = new PhysBoneManager.Collider(vrcphysBoneColliderBase);
					chain.colliders.Add(in collider);
				}
			}
			foreach (VRCPhysBoneBase.CollisionRecord collisionRecord in comp.collisionRecords)
			{
				VRCPhysBoneColliderBase collider2 = collisionRecord.collider;
				if (!(collider2 == null) && collider2.shape != null && collider2.shape.id != CollisionScene.Shape.NullId && chain.colliders.Length < 32)
				{
					PhysBoneManager.Collider collider = new PhysBoneManager.Collider(collider2);
					chain.colliders.Add(in collider);
				}
			}
			this.chainBuffer.chains[chainIndex] = chain;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x0000DCBC File Offset: 0x0000BEBC
		public void UpdateGrabs()
		{
			foreach (PhysBoneManager.Grab grab in this.grabs)
			{
				int num = this.FindChainIndex(grab.chainId);
				if (num >= 0)
				{
					PhysBoneManager.Chain chain = this.chainBuffer.chains[num];
					chain.grabGlobalPosition = grab.globalPosition;
					this.chainBuffer.chains[num] = chain;
				}
			}
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000DD4C File Offset: 0x0000BF4C
		private bool InteractAllowed(int pid1, int pid2)
		{
			return VRCPhysBoneBase.OnVerifyCollision == null || VRCPhysBoneBase.OnVerifyCollision.Invoke(pid1, pid2);
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x0000DD64 File Offset: 0x0000BF64
		public PhysBoneManager.Grab AttemptGrab(int playerId, Ray ray, out Vector3 hitPoint)
		{
			float num = float.MaxValue;
			int num2 = -1;
			int num3 = -1;
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < this.chainBuffer.Length; i++)
			{
				PhysBoneManager.Chain chain = this.chainBuffer.chains[i];
				VRCPhysBoneBase vrcphysBoneBase = this.chainBuffer.comps[i];
				if (vrcphysBoneBase.radius > 0f && vrcphysBoneBase.IsGrabAllowed(playerId) && this.InteractAllowed(playerId, vrcphysBoneBase.playerId))
				{
					for (int j = 0; j < chain.boneCount; j++)
					{
						PhysBoneManager.Bone bone = this.boneBuffer.bones[chain.boneOffset + j];
						if (bone.isSimulated && bone.globalRadiusEnd > 0f)
						{
							float3 endPoint;
							float3 @float;
							if (bone.globalRadiusBegin > 0f)
							{
								MathUtil.ClosestPointsBetweenLineSegments(bone.beginPoint, bone.endPoint, ray.origin, ray.origin + ray.direction * 1000f, out endPoint, out @float);
							}
							else
							{
								endPoint = bone.endPoint;
								@float = MathUtil.ClosestPointOnLineSegment(ray.origin, ray.origin + ray.direction * 1000f, bone.endPoint);
							}
							float num4 = math.distance(bone.beginPoint, endPoint) / math.distance(bone.beginPoint, bone.endPoint);
							float num5 = math.lerp(bone.globalRadiusBegin, bone.globalRadiusEnd, num4);
							float num6 = math.distance(endPoint, @float);
							if (num6 <= num5 && num6 < num)
							{
								num = num6;
								num2 = i;
								num3 = j;
								vector = @float;
							}
						}
					}
				}
			}
			hitPoint = vector;
			if (num2 < 0)
			{
				return null;
			}
			PhysBoneManager.Chain chain2 = this.chainBuffer.chains[num2];
			VRCPhysBoneBase vrcphysBoneBase2 = this.chainBuffer.comps[num2];
			PhysBoneManager.Grab grab = new PhysBoneManager.Grab();
			grab.chainId = vrcphysBoneBase2.chainId;
			grab.bone = num3;
			grab.globalPosition = this.boneBuffer.bones[chain2.boneOffset + num3].endPoint;
			grab.localOffset = (vrcphysBoneBase2.snapToHand ? float3.zero : (grab.globalPosition - (float3)vector));
			if (!this.AddGrab(playerId, grab))
			{
				return null;
			}
			return grab;
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x0000DFFC File Offset: 0x0000C1FC
		public PhysBoneManager.Grab AttemptGrab(int playerId, Vector3 grabPosition, float grabRadius, Vector3 sortPosition)
		{
			CollisionShapes.Sphere sphere = default(CollisionShapes.Sphere);
			sphere.position = grabPosition;
			sphere.radius = grabRadius;
			this.GrabBuffer.Clear();
			this.collision.CastSphere(sphere, this.GrabBuffer);
			float num = float.MaxValue;
			int num2 = -1;
			int num3 = -1;
			foreach (CollisionScene.Shape shape in this.GrabBuffer)
			{
				if (shape.isReceiver)
				{
					VRCPhysBoneBase vrcphysBoneBase = shape.component as VRCPhysBoneBase;
					if (!(vrcphysBoneBase == null) && vrcphysBoneBase.radius > 0f && vrcphysBoneBase.IsGrabAllowed(playerId) && this.InteractAllowed(playerId, vrcphysBoneBase.playerId))
					{
						int num4 = this.FindChainIndex(vrcphysBoneBase.chainId);
						if (num4 >= 0)
						{
							PhysBoneManager.Chain chain = this.chainBuffer.chains[num4];
							for (int i = 0; i < chain.boneCount; i++)
							{
								PhysBoneManager.Bone bone = this.boneBuffer.bones[chain.boneOffset + i];
								if (bone.isSimulated && bone.globalRadiusEnd > 0f)
								{
									float num5 = bone.globalRadiusEnd;
									float3 @float = bone.endPoint;
									if (bone.globalRadiusBegin > 0f)
									{
										float num6 = MathUtil.ClosestPointOnLineSegment_Ratio(bone.beginPoint, bone.endPoint, grabPosition);
										@float = math.lerp(bone.beginPoint, bone.endPoint, num6);
										num5 = math.lerp(bone.globalRadiusBegin, bone.globalRadiusEnd, num6);
									}
									float num7 = Vector3.Distance(@float, grabPosition);
									if (num7 < num5 + grabRadius)
									{
										num7 = Vector3.Distance(@float, sortPosition);
										if (num7 < num)
										{
											num = num7;
											num2 = num4;
											num3 = i;
										}
									}
								}
							}
						}
					}
				}
			}
			if (num2 < 0)
			{
				return null;
			}
			PhysBoneManager.Chain chain2 = this.chainBuffer.chains[num2];
			VRCPhysBoneBase vrcphysBoneBase2 = this.chainBuffer.comps[num2];
			PhysBoneManager.Grab grab = new PhysBoneManager.Grab();
			grab.chainId = vrcphysBoneBase2.chainId;
			grab.bone = num3;
			grab.globalPosition = this.boneBuffer.bones[chain2.boneOffset + num3].endPoint;
			grab.localOffset = (vrcphysBoneBase2.snapToHand ? float3.zero : (grab.globalPosition - (float3)grabPosition));
			if (!this.AddGrab(playerId, grab))
			{
				return null;
			}
			return grab;
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x0000E2C0 File Offset: 0x0000C4C0
		public PhysBoneManager.Grab AttemptGrab(int grabberId, ChainId chainId, int bone)
		{
			PhysBoneManager.Grab grab = new PhysBoneManager.Grab();
			grab.chainId = chainId;
			grab.bone = bone;
			if (!this.AddGrab(grabberId, grab))
			{
				return null;
			}
			return grab;
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x0000E2F0 File Offset: 0x0000C4F0
		public bool IsChainGrabbed(ChainId chainId)
		{
			if (this.FindChainIndex(chainId) < 0)
			{
				return false;
			}
			using (List<PhysBoneManager.Grab>.Enumerator enumerator = this.grabs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.chainId == chainId)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x0000E35C File Offset: 0x0000C55C
		public void ReleaseGrab(ChainId chainId)
		{
			for (int i = 0; i < this.grabs.Count; i++)
			{
				PhysBoneManager.Grab grab = this.grabs[i];
				if (grab.chainId == chainId)
				{
					this.ReleaseGrab(grab, false);
					i--;
				}
			}
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000E3A8 File Offset: 0x0000C5A8
		public bool AddGrab(int grabberId, PhysBoneManager.Grab grab)
		{
			int num = this.FindChainIndex(grab.chainId);
			if (num < 0)
			{
				return false;
			}
			VRCPhysBoneBase vrcphysBoneBase = this.chainBuffer.comps[num];
			if (!vrcphysBoneBase.IsGrabAllowed(grabberId))
			{
				return false;
			}
			if (!this.InteractAllowed(grabberId, vrcphysBoneBase.playerId))
			{
				return false;
			}
			PhysBoneManager.Chain chain = this.chainBuffer.chains[num];
			if (grab.bone < 0 || grab.bone >= chain.boneCount)
			{
				return false;
			}
			using (List<PhysBoneManager.Grab>.Enumerator enumerator = this.grabs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.chainId == grab.chainId)
					{
						return false;
					}
				}
			}
			grab.pose = this.FindPose(grab.chainId);
			if (grab.pose == null)
			{
				grab.pose = this.CreatePose(this.chainBuffer.comps[num]);
			}
			PhysBoneManager.Bone bone;
			for (int i = grab.bone; i >= 0; i = bone.parentIndex)
			{
				int num2 = chain.boneOffset + i;
				bone = this.boneBuffer.bones[num2];
				bone.grabStatus = PhysBoneManager.Bone.Status.Grabbed;
				this.boneBuffer.bones[num2] = bone;
			}
			chain.grabBone = grab.bone;
			chain.grabIkSolved = 0f;
			this.chainBuffer.chains[num] = chain;
			vrcphysBoneBase.SetIsGrabbed(true);
			grab.playerId = grabberId;
			this.grabs.Add(grab);
			return true;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000E54C File Offset: 0x0000C74C
		public void ReleaseGrab(PhysBoneManager.Grab grab, bool createPose = false)
		{
			this.grabs.Remove(grab);
			int num = this.FindChainIndex(grab.chainId);
			if (num < 0)
			{
				return;
			}
			PhysBoneManager.Chain chain = this.chainBuffer.chains[num];
			chain.grabBone = -1;
			chain.grabIkSolved = 0f;
			this.chainBuffer.chains[num] = chain;
			VRCPhysBoneBase vrcphysBoneBase = this.chainBuffer.comps[num];
			vrcphysBoneBase.SetIsGrabbed(false);
			if (grab.pose != null)
			{
				if (createPose && vrcphysBoneBase.IsPoseAllowed(grab.playerId))
				{
					for (int i = 0; i < chain.boneCount; i++)
					{
						int num2 = chain.boneOffset + i;
						PhysBoneManager.Bone bone = this.boneBuffer.bones[num2];
						if (bone.grabStatus == PhysBoneManager.Bone.Status.Grabbed)
						{
							PhysBoneManager.TransformData transformData = this.boneBuffer.transformData[num2];
							bone.localPoseBoneVector = bone.localBoneVector;
							bone.localPoseRotation = transformData.localRotation;
							bone.grabStatus = PhysBoneManager.Bone.Status.Posed;
							this.boneBuffer.bones[num2] = bone;
						}
					}
					vrcphysBoneBase.SetIsPosed(true);
					Action onNeedsNetworkSync = vrcphysBoneBase.OnNeedsNetworkSync;
					if (onNeedsNetworkSync != null)
					{
						onNeedsNetworkSync.Invoke();
					}
				}
				else
				{
					this.RemovePose(grab.pose);
				}
			}
			grab.pose = null;
			grab.chainId = ChainId.Null;
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000E6AB File Offset: 0x0000C8AB
		public IEnumerable<PhysBoneManager.Grab> GetGrabs()
		{
			int num;
			for (int i = 0; i < this.grabs.Count; i = num + 1)
			{
				yield return this.grabs[i];
				num = i;
			}
			yield break;
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000E6BC File Offset: 0x0000C8BC
		private PhysBoneManager.Pose CreatePose(VRCPhysBoneBase comp)
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			this.RemovePoseForChain(comp.chainId, false);
			int num = this.FindChainIndex(comp.chainId);
			if (num < 0)
			{
				return null;
			}
			PhysBoneManager.Chain chain = this.chainBuffer.chains[num];
			PhysBoneManager.Pose pose = new PhysBoneManager.Pose();
			pose.chainId = comp.chainId;
			pose.prevIsAnimated = chain.isAnimated;
			pose.prevData.Capacity = chain.boneCount;
			for (int i = 0; i < chain.boneCount; i++)
			{
				PhysBoneManager.Bone bone = this.boneBuffer.bones[chain.boneOffset + i];
				PhysBoneManager.Pose.PoseData poseData = default(PhysBoneManager.Pose.PoseData);
				poseData.localPoseBoneVector = bone.localPoseBoneVector;
				poseData.localPoseRotation = bone.localPoseRotation;
				pose.prevData.Add(poseData);
			}
			this.poses.Add(pose);
			chain.isAnimated = false;
			this.chainBuffer.chains[num] = chain;
			Action onNeedsNetworkSync = comp.OnNeedsNetworkSync;
			if (onNeedsNetworkSync != null)
			{
				onNeedsNetworkSync.Invoke();
			}
			return pose;
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0000E7C4 File Offset: 0x0000C9C4
		public PhysBoneManager.Pose FindOrCreatePose(ChainId chainId)
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			PhysBoneManager.Pose pose = this.FindPose(chainId);
			if (pose == null)
			{
				int num = this.FindChainIndex(chainId);
				if (num < 0)
				{
					return null;
				}
				VRCPhysBoneBase comp = this.chainBuffer.comps[num];
				pose = this.CreatePose(comp);
			}
			return pose;
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000E80C File Offset: 0x0000CA0C
		public void RemovePoseForChain(ChainId chainId, bool checkIfGrabbed = true)
		{
			if (checkIfGrabbed && this.IsChainGrabbed(chainId))
			{
				return;
			}
			for (int i = 0; i < this.poses.Count; i++)
			{
				PhysBoneManager.Pose pose = this.poses[i];
				if (pose.chainId == chainId)
				{
					this.RemovePose(pose);
					i--;
				}
			}
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000E864 File Offset: 0x0000CA64
		public void RemovePose(PhysBoneManager.Pose pose)
		{
			this.poses.Remove(pose);
			int num = this.FindChainIndex(pose.chainId);
			if (num >= 0)
			{
				VRCAvatarDynamicsScheduler.FinalizeJob();
				PhysBoneManager.Chain chain = this.chainBuffer.chains[num];
				chain.isAnimated = pose.prevIsAnimated;
				this.chainBuffer.chains[num] = chain;
				for (int i = 0; i < chain.boneCount; i++)
				{
					int num2 = chain.boneOffset + i;
					PhysBoneManager.Bone bone = this.boneBuffer.bones[num2];
					if (i < pose.prevData.Count)
					{
						PhysBoneManager.Pose.PoseData poseData = pose.prevData[i];
						bone.localPoseBoneVector = poseData.localPoseBoneVector;
						bone.localPoseRotation = poseData.localPoseRotation;
					}
					bone.grabStatus = PhysBoneManager.Bone.Status.None;
					this.boneBuffer.bones[num2] = bone;
				}
				if (num < this.chainBuffer.comps.Count)
				{
					VRCPhysBoneBase vrcphysBoneBase = this.chainBuffer.comps[num];
					vrcphysBoneBase.SetIsPosed(false);
					Action onNeedsNetworkSync = vrcphysBoneBase.OnNeedsNetworkSync;
					if (onNeedsNetworkSync == null)
					{
						return;
					}
					onNeedsNetworkSync.Invoke();
				}
			}
		}

		// Token: 0x060001EF RID: 495 RVA: 0x0000E984 File Offset: 0x0000CB84
		public PhysBoneManager.Pose FindPose(ChainId chainId)
		{
			for (int i = 0; i < this.poses.Count; i++)
			{
				PhysBoneManager.Pose pose = this.poses[i];
				if (pose.chainId == chainId)
				{
					return pose;
				}
			}
			return null;
		}


		// Token: 0x04000126 RID: 294
		public static PhysBoneManager Inst;

		// Token: 0x04000127 RID: 295
		public Vector3 distanceCullOrigin;

		// Token: 0x04000128 RID: 296
		public bool IsSDK;

		// Token: 0x04000129 RID: 297
		private List<PhysBoneRoot> rootsToUpdate = new List<PhysBoneRoot>();

		// Token: 0x0400012A RID: 298
		private List<VRCPhysBoneBase> compsToAdd = new List<VRCPhysBoneBase>();

		// Token: 0x0400012B RID: 299
		private List<PhysBoneManager.ToRemoveData> compsToRemove = new List<PhysBoneManager.ToRemoveData>();

		// Token: 0x0400012C RID: 300
		private const float MAX_BOUNDS_SIZE = 10f;

		// Token: 0x0400012D RID: 301
		public const float MIN_BONE_LENGTH = 1E-05f;

		// Token: 0x0400012E RID: 302
		public const float MIN_SIMULATE_BONE = 8E-06f;

		// Token: 0x0400012F RID: 303
		private static float3 DebugLineAxis = new float3(0f, 1f, 0f);

		// Token: 0x04000130 RID: 304
		public static IPhysBoneDebugDrawer DebugDraw;

		// Token: 0x04000131 RID: 305
		private MemoryBuffer buffer;

		// Token: 0x04000132 RID: 306
		private ChainBuffer chainBuffer;

		// Token: 0x04000133 RID: 307
		private BoneBuffer boneBuffer;

		// Token: 0x04000134 RID: 308
		private RootsBuffer rootBuffer;

		// Token: 0x04000135 RID: 309
		[NonSerialized]
		public PhysBoneManager.EditorDebugInfo editorInfo;

		// Token: 0x04000136 RID: 310
		private NativeArray<PhysBoneManager.CriticalErrorType> errorBuffer;

		// Token: 0x04000137 RID: 311
		public static int THREAD_BATCH_SIZE = 8;

		// Token: 0x04000138 RID: 312
		public const int MAX_TRANSFORMS_PER_CHAIN = 256;

		// Token: 0x04000139 RID: 313
		public const float MAX_DELTA_TIME = 0.1f;

		// Token: 0x0400013A RID: 314
		public const int INITIAL_CAPACITY_ROOTS = 128;

		// Token: 0x0400013B RID: 315
		public const int INITIAL_CAPACITY_CHAINS = 256;

		// Token: 0x0400013C RID: 316
		public const int INITIAL_CAPACITY_BONES = 2048;

		// Token: 0x0400013D RID: 317
		private const float COLLISION_FRICTION = 0.25f;

		// Token: 0x0400013E RID: 318
		private const int EXTRA_TRANSFORM_COUNT = 2;

		// Token: 0x0400013F RID: 319
		private const int EXTRA_TRANSFORM_ROOT_PARENT = 0;

		// Token: 0x04000140 RID: 320
		private const int EXTRA_TRANSFORM_SCENE_ROOT = 1;

		// Token: 0x04000141 RID: 321
		private List<int> rootsToRemove = new List<int>();

		// Token: 0x04000142 RID: 322
		private List<VRCPhysBoneColliderBase> collidersToAdd = new List<VRCPhysBoneColliderBase>();

		// Token: 0x04000143 RID: 323
		private List<PhysBoneManager.ColliderToRemoveData> collidersToRemove = new List<PhysBoneManager.ColliderToRemoveData>();

		// Token: 0x04000144 RID: 324
		private const float CUSTOM_EPSILON = 0.0001f;

		// Token: 0x04000145 RID: 325
		private const float QUATERNION_EPSILON = 0.0001f;

		// Token: 0x04000146 RID: 326
		private const float MATRIX_EPSILON = 1.1754944E-38f;

		// Token: 0x04000147 RID: 327
		private bool hasInit;

		// Token: 0x04000148 RID: 328
		private static readonly ProfilerMarker Marker_UpdateRoots = new ProfilerMarker("PhysBoneManager.UpdateRoots");

		// Token: 0x04000149 RID: 329
		private static readonly ProfilerMarker Marker_AddRemoveChains = new ProfilerMarker("PhysBoneManager.AddRemoveChains");

		// Token: 0x0400014A RID: 330
		private static readonly ProfilerMarker Marker_AddRemoveColliders = new ProfilerMarker("PhysBoneManager.AddRemoveColliders");

		// Token: 0x0400014B RID: 331
		private static readonly ProfilerMarker Marker_UpdateChains = new ProfilerMarker("PhysBoneManager.UpdateChains");

		// Token: 0x0400014C RID: 332
		private ProfilerMarker Marker_JobsTotal = new ProfilerMarker("PhysBoneManager.JobsTotal");

		// Token: 0x0400014D RID: 333
		private const float FRAME_TIME = 0.016666668f;

		// Token: 0x0400014E RID: 334
		private const float MIN_FRAME_TIME_FOR_EXECUTION = 0.015833333f;

		// Token: 0x0400014F RID: 335
		private const float MAX_TIME_DELTA = 0.1f;

		// Token: 0x04000150 RID: 336
		private float fixedTimeElapsed;

		// Token: 0x04000151 RID: 337
		private float realTimeElapsed;

		// Token: 0x04000152 RID: 338
		private static bool hasReportedCriticalError = false;

		// Token: 0x04000153 RID: 339
		private float fullFrameTimeElapsed;

		// Token: 0x04000154 RID: 340
		private bool executeShapeUpdates;

		// Token: 0x04000155 RID: 341
		public bool drawGizmos;

		// Token: 0x04000156 RID: 342
		public const int MAX_EXECUTION_GROUPS = 8;

		// Token: 0x04000157 RID: 343
		private PhysBoneGroup[] executionGroups;

		// Token: 0x04000158 RID: 344
		private static List<PhysBoneManager.SortingData> sortingData = new List<PhysBoneManager.SortingData>();

		// Token: 0x04000159 RID: 345
		public CollisionScene collision;

		// Token: 0x0400015A RID: 346
		private const int MAX_COLLIDERS_PER_CHAIN = 32;

		// Token: 0x0400015B RID: 347
		private List<PhysBoneManager.Grab> grabs = new List<PhysBoneManager.Grab>();

		// Token: 0x0400015C RID: 348
		private const float GRAB_RAY_LENGTH = 1000f;

		// Token: 0x0400015D RID: 349
		private List<CollisionScene.Shape> GrabBuffer = new List<CollisionScene.Shape>();

		// Token: 0x0400015E RID: 350
		private List<PhysBoneManager.Pose> poses = new List<PhysBoneManager.Pose>();

		// Token: 0x02000064 RID: 100
		public struct Bone
		{
			// Token: 0x17000053 RID: 83
			// (get) Token: 0x060002C1 RID: 705 RVA: 0x000127DE File Offset: 0x000109DE
			public bool isSimulated
			{
				get
				{
					return this.simulatedType > PhysBoneManager.Bone.SimulatedType.None;
				}
			}

			// Token: 0x040002B1 RID: 689
			public int childIndex;

			// Token: 0x040002B2 RID: 690
			public int parentIndex;

			// Token: 0x040002B3 RID: 691
			public bool isEndBone;

			// Token: 0x040002B4 RID: 692
			public PhysBoneManager.Bone.SimulatedType simulatedType;

			// Token: 0x040002B5 RID: 693
			public int boneChainIndex;

			// Token: 0x040002B6 RID: 694
			public quaternion localPoseRotation;

			// Token: 0x040002B7 RID: 695
			public float3 localBoneVector;

			// Token: 0x040002B8 RID: 696
			public float3 localPoseBoneVector;

			// Token: 0x040002B9 RID: 697
			public float3 originalLocalPosition;

			// Token: 0x040002BA RID: 698
			public quaternion originalLocalRotation;

			// Token: 0x040002BB RID: 699
			public float3 originalLocalVector;

			// Token: 0x040002BC RID: 700
			public float originalLocalBoneLength;

			// Token: 0x040002BD RID: 701
			public float globalRestLength;

			// Token: 0x040002BE RID: 702
			public float3 originalRootEndpoint;

			// Token: 0x040002BF RID: 703
			public float3 originalLocalGravityNormal;

			// Token: 0x040002C0 RID: 704
			public float3 beginPoint;

			// Token: 0x040002C1 RID: 705
			public float3 endPoint;

			// Token: 0x040002C2 RID: 706
			public float3 prevVelocity;

			// Token: 0x040002C3 RID: 707
			public float3 immobileEndpoint;

			// Token: 0x040002C4 RID: 708
			public float3 prevEndPoint;

			// Token: 0x040002C5 RID: 709
			public float3 prevVector;

			// Token: 0x040002C6 RID: 710
			public quaternion prevLocalRotation;

			// Token: 0x040002C7 RID: 711
			public float totalRestLength;

			// Token: 0x040002C8 RID: 712
			public float totalLength;

			// Token: 0x040002C9 RID: 713
			public float totalMinRestLength;

			// Token: 0x040002CA RID: 714
			public float totalMaxRestLength;

			// Token: 0x040002CB RID: 715
			public PhysBoneManager.Bone.Status grabStatus;

			// Token: 0x040002CC RID: 716
			public float pull;

			// Token: 0x040002CD RID: 717
			public float spring;

			// Token: 0x040002CE RID: 718
			public float stiffness;

			// Token: 0x040002CF RID: 719
			public float gravity;

			// Token: 0x040002D0 RID: 720
			public float gravityFalloff;

			// Token: 0x040002D1 RID: 721
			public float immobile;

			// Token: 0x040002D2 RID: 722
			public float radiusBegin;

			// Token: 0x040002D3 RID: 723
			public float radiusEnd;

			// Token: 0x040002D4 RID: 724
			public float globalRadiusBegin;

			// Token: 0x040002D5 RID: 725
			public float globalRadiusEnd;

			// Token: 0x040002D6 RID: 726
			public float2 maxAngle;

			// Token: 0x040002D7 RID: 727
			public float3 limitAxisX;

			// Token: 0x040002D8 RID: 728
			public float3 limitAxisY;

			// Token: 0x040002D9 RID: 729
			public float3 limitRotation;

			// Token: 0x040002DA RID: 730
			public float stretchMotion;

			// Token: 0x040002DB RID: 731
			public float stretch;

			// Token: 0x040002DC RID: 732
			public float squish;

			// Token: 0x02000089 RID: 137
			public enum SimulatedType
			{
				// Token: 0x04000386 RID: 902
				None,
				// Token: 0x04000387 RID: 903
				Child,
				// Token: 0x04000388 RID: 904
				Endpoint
			}

			// Token: 0x0200008A RID: 138
			public enum Status : byte
			{
				// Token: 0x0400038A RID: 906
				None,
				// Token: 0x0400038B RID: 907
				Grabbed,
				// Token: 0x0400038C RID: 908
				Posed
			}
		}

		// Token: 0x02000065 RID: 101
		public struct Chain
		{
			// Token: 0x060002C2 RID: 706 RVA: 0x000127E9 File Offset: 0x000109E9
			public void Init()
			{
				this.colliders = new UnsafeList<PhysBoneManager.Collider>(0, (Allocator)4, 0);
			}

			// Token: 0x060002C3 RID: 707 RVA: 0x000127FE File Offset: 0x000109FE
			public void Dispose()
			{
				this.colliders.Dispose();
			}

			// Token: 0x040002DD RID: 733
			public int rootIndex;

			// Token: 0x040002DE RID: 734
			public int boneOffset;

			// Token: 0x040002DF RID: 735
			public int boneCount;

			// Token: 0x040002E0 RID: 736
			public int spanCount;

			// Token: 0x040002E1 RID: 737
			public bool hasInitialized;

			// Token: 0x040002E2 RID: 738
			public VRCPhysBoneBase.Version version;

			// Token: 0x040002E3 RID: 739
			public bool isAnimated;

			// Token: 0x040002E4 RID: 740
			public VRCPhysBoneBase.IntegrationType integrationType;

			// Token: 0x040002E5 RID: 741
			public UnsafeList<PhysBoneManager.Collider> colliders;

			// Token: 0x040002E6 RID: 742
			public int grabBone;

			// Token: 0x040002E7 RID: 743
			public float3 grabGlobalPosition;

			// Token: 0x040002E8 RID: 744
			public float grabMovement;

			// Token: 0x040002E9 RID: 745
			public float paramStretch;

			// Token: 0x040002EA RID: 746
			public float paramSquish;

			// Token: 0x040002EB RID: 747
			public float grabIkSolved;

			// Token: 0x040002EC RID: 748
			public VRCPhysBoneBase.LimitType limitType;

			// Token: 0x040002ED RID: 749
			public float3 staticFreezeAxis;

			// Token: 0x040002EE RID: 750
			public VRCPhysBoneBase.ImmobileType immobileType;

			// Token: 0x040002EF RID: 751
			public ushort shapeId;

			// Token: 0x040002F0 RID: 752
			public Bounds renderBounds;

			// Token: 0x040002F1 RID: 753
			public Bounds collisionBounds;

			// Token: 0x040002F2 RID: 754
			public float paramAngle;

			// Token: 0x040002F3 RID: 755
			public PhysBoneManager.TransformState lastRootParentState;

			// Token: 0x040002F4 RID: 756
			public PhysBoneManager.TransformState lastSceneRootState;
		}

		// Token: 0x02000066 RID: 102
		public struct ChainRoot
		{
			// Token: 0x040002F5 RID: 757
			public bool useFixedTime;

			// Token: 0x040002F6 RID: 758
			public float fixedTime;

			// Token: 0x040002F7 RID: 759
			public float realTime;

			// Token: 0x040002F8 RID: 760
			public bool isUsed;

			// Token: 0x040002F9 RID: 761
			public float executions;
		}

		// Token: 0x02000067 RID: 103
		public struct EditorDebugInfo
		{
			// Token: 0x040002FA RID: 762
			public int rootCount;

			// Token: 0x040002FB RID: 763
			public int rootCapacity;

			// Token: 0x040002FC RID: 764
			public int chainCount;

			// Token: 0x040002FD RID: 765
			public int chainCapacity;

			// Token: 0x040002FE RID: 766
			public int boneCount;

			// Token: 0x040002FF RID: 767
			public int boneCapacity;

			// Token: 0x04000300 RID: 768
			public int shapeCount;

			// Token: 0x04000301 RID: 769
			public int shapeCapacity;

			// Token: 0x04000302 RID: 770
			public long bytesUsed;
		}

		// Token: 0x02000068 RID: 104
		public enum CriticalErrorType
		{
			// Token: 0x04000304 RID: 772
			None,
			// Token: 0x04000305 RID: 773
			ChainIndexOutsideJob,
			// Token: 0x04000306 RID: 774
			ChainIndexInsideJob,
			// Token: 0x04000307 RID: 775
			ExecutionGroupMismatch,
			// Token: 0x04000308 RID: 776
			BoneIndex,
			// Token: 0x04000309 RID: 777
			RootIndex,
			// Token: 0x0400030A RID: 778
			MaxShapes,
			// Token: 0x0400030B RID: 779
			ExecutionGroupsStillContainsId
		}

		// Token: 0x02000069 RID: 105
		public struct TransformState
		{
			// Token: 0x060002C4 RID: 708 RVA: 0x0001280B File Offset: 0x00010A0B
			public TransformState(float3 p, quaternion q)
			{
				this.position = p;
				this.rotation = q;
			}

			// Token: 0x060002C5 RID: 709 RVA: 0x0001281B File Offset: 0x00010A1B
			public float4x4 ToMatrix()
			{
				return float4x4.TRS(this.position, this.rotation, new float3(1f, 1f, 1f));
			}

			// Token: 0x060002C6 RID: 710 RVA: 0x00012842 File Offset: 0x00010A42
			public static PhysBoneManager.TransformState Lerp(PhysBoneManager.TransformState a, PhysBoneManager.TransformState b, float t)
			{
				return new PhysBoneManager.TransformState(math.lerp(a.position, b.position, t), math.slerp(a.rotation, b.rotation, t));
			}

			// Token: 0x0400030C RID: 780
			public static PhysBoneManager.TransformState identity = new PhysBoneManager.TransformState(float3.zero, quaternion.identity);

			// Token: 0x0400030D RID: 781
			public float3 position;

			// Token: 0x0400030E RID: 782
			public quaternion rotation;
		}

		// Token: 0x0200006A RID: 106
		public struct TransformData
		{
			// Token: 0x060002C8 RID: 712 RVA: 0x00012884 File Offset: 0x00010A84
			public TransformData(Transform transform)
			{
				if (transform != null)
				{
					this.position = transform.position;
					this.rotation = transform.rotation;
					this.localToWorld = transform.localToWorldMatrix;
					this.localPosition = transform.localPosition;
					this.localRotation = transform.localRotation;
					this.localScale = transform.localScale;
				}
				else
				{
					this.position = Vector3.zero;
					this.rotation = Quaternion.identity;
					this.localToWorld = float4x4.identity;
					this.localPosition = Vector3.zero;
					this.localRotation = Quaternion.identity;
					this.localScale = Vector3.one;
				}
				this.prevLocalPosition = this.localPosition;
				this.prevLocalRotation = this.localRotation;
			}

			// Token: 0x060002C9 RID: 713 RVA: 0x00012978 File Offset: 0x00010B78
			public void UpdateFromUnityTransform(TransformAccess access)
			{
				if (access.isValid)
				{
					this.localToWorld = access.localToWorldMatrix;
					this.position = access.position;
					this.rotation = access.rotation;
					this.localPosition = access.localPosition;
					this.localRotation = access.localRotation;
					this.localScale = access.localScale;
				}
			}

			// Token: 0x060002CA RID: 714 RVA: 0x000129FC File Offset: 0x00010BFC
			public void UpdateGlobalTransform(float4x4 parentMatrix)
			{
				this.localToWorld = math.mul(parentMatrix, float4x4.TRS(this.localPosition, this.localRotation, this.localScale));
				this.position = this.localToWorld.c3.xyz;
				this.rotation = new quaternion(this.localToWorld);
			}

			// Token: 0x060002CB RID: 715 RVA: 0x00012A53 File Offset: 0x00010C53
			public void UpdateUnityTransform(TransformAccess access)
			{
				if (access.isValid)
				{
					access.SetLocalPositionAndRotation(this.localPosition, this.localRotation);
				}
			}

			// Token: 0x17000054 RID: 84
			// (get) Token: 0x060002CC RID: 716 RVA: 0x00012A7C File Offset: 0x00010C7C
			public float3 lossyScale
			{
				get
				{
					return new float3(math.length(this.localToWorld.c0.xyz), math.length(this.localToWorld.c1.xyz), math.length(this.localToWorld.c2.xyz));
				}
			}

			// Token: 0x0400030F RID: 783
			public float4x4 localToWorld;

			// Token: 0x04000310 RID: 784
			public float3 position;

			// Token: 0x04000311 RID: 785
			public quaternion rotation;

			// Token: 0x04000312 RID: 786
			public float3 localPosition;

			// Token: 0x04000313 RID: 787
			public quaternion localRotation;

			// Token: 0x04000314 RID: 788
			public float3 localScale;

			// Token: 0x04000315 RID: 789
			public float3 prevLocalPosition;

			// Token: 0x04000316 RID: 790
			public quaternion prevLocalRotation;
		}

		// Token: 0x0200006B RID: 107
		private struct ToRemoveData
		{
			// Token: 0x060002CD RID: 717 RVA: 0x00012ACD File Offset: 0x00010CCD
			public ToRemoveData(VRCPhysBoneBase comp)
			{
				this.comp = comp;
				this.chainId = comp.chainId;
				this.shape = comp.shape;
				this.executionGroup = comp.ExecutionGroup;
			}

			// Token: 0x04000317 RID: 791
			public VRCPhysBoneBase comp;

			// Token: 0x04000318 RID: 792
			public ChainId chainId;

			// Token: 0x04000319 RID: 793
			public int executionGroup;

			// Token: 0x0400031A RID: 794
			public CollisionScene.Shape shape;
		}

		// Token: 0x0200006C RID: 108
		private struct ColliderToRemoveData
		{
			// Token: 0x060002CE RID: 718 RVA: 0x00012AFA File Offset: 0x00010CFA
			public ColliderToRemoveData(VRCPhysBoneColliderBase comp)
			{
				this.comp = comp;
				this.shape = comp.shape;
				this.executionGroup = comp.ExecutionGroup;
			}

			// Token: 0x0400031B RID: 795
			public VRCPhysBoneColliderBase comp;

			// Token: 0x0400031C RID: 796
			public int executionGroup;

			// Token: 0x0400031D RID: 797
			public CollisionScene.Shape shape;
		}

		// Token: 0x0200006D RID: 109
		public struct PhysBoneJob : IJobParallelFor
		{
			// Token: 0x060002CF RID: 719 RVA: 0x00012B1C File Offset: 0x00010D1C
			public void Execute(int index)
			{
				int num = this.chainIndices[index];
				if (num < 0 || num >= this.chains.Length)
				{
					this.errorBuffer[0] = PhysBoneManager.CriticalErrorType.ChainIndexInsideJob;
					return;
				}
				PhysBoneManager.Chain chain = this.chains[num];
				if (chain.rootIndex < 0 || chain.rootIndex >= this.roots.Length)
				{
					this.errorBuffer[0] = PhysBoneManager.CriticalErrorType.RootIndex;
					return;
				}
				if (chain.boneOffset < 0 || chain.boneOffset >= this.bones.Length || chain.boneCount < 0 || chain.boneOffset + chain.boneCount > this.bones.Length)
				{
					this.errorBuffer[0] = PhysBoneManager.CriticalErrorType.BoneIndex;
					return;
				}
				PhysBoneManager.ChainRoot chainRoot = this.roots[chain.rootIndex];
				if (!chain.hasInitialized)
				{
					this.InitializeChain(ref chain);
					chain.hasInitialized = true;
				}
				int num2 = chain.boneCount + 2;
				for (int i = 0; i < num2; i++)
				{
					int num3 = chain.boneOffset + i;
					PhysBoneManager.TransformData transformData = this.transformData[num3];
					transformData.UpdateFromUnityTransform(this.transformAccess[num3]);
					this.transformData[num3] = transformData;
				}
				PhysBoneManager.TransformData transformData2 = this.transformData[chain.boneOffset + chain.boneCount];
				PhysBoneManager.TransformState transformState = new PhysBoneManager.TransformState(transformData2.position, transformData2.rotation);
				PhysBoneManager.TransformData transformData3 = this.transformData[chain.boneOffset + chain.boneCount + 1];
				PhysBoneManager.TransformState transformState2 = new PhysBoneManager.TransformState(transformData3.position, quaternion.identity);
				this.UpdateColliders(ref chain);
				if (chain.isAnimated)
				{
					this.SolveAnimation(ref chain, transformData2.localToWorld);
				}
				int num4 = (int)math.ceil(chainRoot.executions);
				if (num4 <= 1)
				{
					float executions = chainRoot.executions;
					this.SolveChain(ref chain, transformData2.localToWorld, (chain.immobileType == VRCPhysBoneBase.ImmobileType.AllMotion) ? transformData2.localToWorld : transformState2.ToMatrix(), executions);
					if (executions >= 1f)
					{
						chain.lastRootParentState = transformState;
						chain.lastSceneRootState = transformState2;
					}
				}
				else if (num4 > 1)
				{
					PhysBoneManager.TransformState lastRootParentState = chain.lastRootParentState;
					PhysBoneManager.TransformState lastSceneRootState = chain.lastSceneRootState;
					float4x4 float4x = math.mul(PhysBoneManager.SafeInverse(float4x4.TRS(transformData2.position, transformData2.rotation, Vector3.one)), transformData2.localToWorld);
					for (int j = 0; j < num4; j++)
					{
						float num5 = math.min((float)j + 1f, chainRoot.executions) / chainRoot.executions;
						float num6 = math.min(1f, chainRoot.executions - (float)j);
						PhysBoneManager.TransformState lastRootParentState2 = PhysBoneManager.TransformState.Lerp(lastRootParentState, transformState, num5);
						PhysBoneManager.TransformState lastSceneRootState2 = PhysBoneManager.TransformState.Lerp(lastSceneRootState, transformState2, num5);
						float4x4 float4x2 = math.mul(lastRootParentState2.ToMatrix(), float4x);
						float4x4 immobileMatrix = (chain.immobileType == VRCPhysBoneBase.ImmobileType.AllMotion) ? float4x2 : lastSceneRootState2.ToMatrix();
						this.LerpColliders(ref chain, num5);
						this.SolveChain(ref chain, float4x2, immobileMatrix, num6);
						if (num6 >= 1f)
						{
							chain.lastRootParentState = lastRootParentState2;
							chain.lastSceneRootState = lastSceneRootState2;
						}
					}
				}
				for (int k = 0; k < chain.boneCount; k++)
				{
					int num7 = chain.boneOffset + k;
					this.transformData[num7].UpdateUnityTransform(this.transformAccess[num7]);
				}
				if (chain.grabBone >= 0)
				{
					this.SolveGrabIK(ref chain, transformData2.localToWorld);
				}
				if (chain.shapeId != CollisionScene.Shape.NullId)
				{
					CollisionScene.ShapeData shapeData = this.shapeData[(int)chain.shapeId];
					shapeData.bounds = chain.collisionBounds;
					shapeData.outPos0 = shapeData.bounds.min;
					shapeData.outPos1 = shapeData.bounds.max;
					this.shapeData[(int)chain.shapeId] = shapeData;
				}
				this.chains[num] = chain;
			}

			// Token: 0x060002D0 RID: 720 RVA: 0x00012F1C File Offset: 0x0001111C
			private void InitializeChain(ref PhysBoneManager.Chain chain)
			{
				for (int i = 0; i < chain.boneCount; i++)
				{
					int num = chain.boneOffset + i;
					PhysBoneManager.Bone bone = this.bones[num];
					PhysBoneManager.TransformData transformData = this.transformData[num];
					float4x4 float4x;
					if (bone.parentIndex >= 0)
					{
						float4x = this.transformData[chain.boneOffset + bone.parentIndex].localToWorld;
					}
					else
					{
						float4x = float4x4.identity;
					}
					transformData.localToWorld = math.mul(float4x, float4x4.TRS(bone.originalLocalPosition, bone.originalLocalRotation, transformData.localScale));
					if (bone.isEndBone)
					{
						bone.originalRootEndpoint = math.transform(transformData.localToWorld, bone.originalLocalVector);
					}
					this.bones[num] = bone;
					this.transformData[num] = transformData;
				}
			}

			// Token: 0x060002D1 RID: 721 RVA: 0x00012FF4 File Offset: 0x000111F4
			private void SolveAnimation(ref PhysBoneManager.Chain chain, float4x4 rootParentMatrix)
			{
				for (int i = 0; i < chain.boneCount; i++)
				{
					int num = chain.boneOffset + i;
					PhysBoneManager.Bone bone = this.bones[num];
					PhysBoneManager.TransformData transformData = this.transformData[num];
					if (bone.grabStatus == PhysBoneManager.Bone.Status.None)
					{
						if (PhysBoneManager.HasChanged(transformData.prevLocalRotation, transformData.localRotation))
						{
							bone.localPoseRotation = transformData.localRotation;
							bone.originalLocalRotation = transformData.localRotation;
						}
						if (PhysBoneManager.HasChanged(transformData.prevLocalPosition, transformData.localPosition))
						{
							bone.originalLocalPosition = transformData.localPosition;
						}
						if (bone.childIndex >= 0)
						{
							PhysBoneManager.TransformData transformData2 = this.transformData[chain.boneOffset + bone.childIndex];
							if (PhysBoneManager.HasChanged(transformData2.prevLocalPosition, transformData2.localPosition))
							{
								bone.localPoseBoneVector = transformData2.localPosition;
								bone.originalLocalVector = transformData2.localPosition;
								bone.originalLocalBoneLength = math.length(bone.originalLocalVector);
								PhysBoneManager.CalcLimitAxis(bone.originalLocalVector, bone.limitRotation, out bone.limitAxisX, out bone.limitAxisY);
								if (math.any(chain.staticFreezeAxis))
								{
									bone.limitAxisX = chain.staticFreezeAxis;
								}
							}
						}
					}
					float4x4 float4x;
					if (bone.parentIndex >= 0)
					{
						float4x = this.transformData[chain.boneOffset + bone.parentIndex].localToWorld;
					}
					else
					{
						float4x = float4x4.identity;
					}
					transformData.localToWorld = math.mul(float4x, float4x4.TRS(bone.originalLocalPosition, bone.originalLocalRotation, transformData.localScale));
					if (bone.isEndBone)
					{
						bone.originalRootEndpoint = math.transform(transformData.localToWorld, bone.originalLocalVector);
					}
					this.bones[num] = bone;
					this.transformData[num] = transformData;
				}
			}

			// Token: 0x060002D2 RID: 722 RVA: 0x000131C8 File Offset: 0x000113C8
			private void SolveChain(ref PhysBoneManager.Chain chain, float4x4 rootParentMatrix, float4x4 immobileMatrix, float frameRatio)
			{
				PhysBoneManager.TransformData transformData = this.transformData[chain.boneOffset];
				float num = 0f;
				float num2 = 0f;
				float num3 = 0f;
				chain.renderBounds = new Bounds(transformData.position, Vector3.zero);
				bool flag = false;
				chain.collisionBounds = new Bounds(transformData.position, Vector3.zero);
				Matrix4x4 matrix4x = PhysBoneManager.SafeInverse(immobileMatrix);
				float3 @float = new(0f, -1f, 0f);
				for (int i = 0; i < chain.boneCount; i++)
				{
					int num4 = chain.boneOffset + i;
					PhysBoneManager.Bone bone = this.bones[num4];
					PhysBoneManager.TransformData transformData2 = this.transformData[num4];
					float4x4 float4x;
					if (bone.parentIndex < 0)
					{
						float4x = rootParentMatrix;
						transformData2.localPosition = bone.originalLocalPosition;
						bone.totalRestLength = 0f;
						bone.totalLength = 0f;
						bone.totalMinRestLength = 0f;
						bone.totalMaxRestLength = 0f;
					}
					else
					{
						PhysBoneManager.Bone bone2 = this.bones[chain.boneOffset + bone.parentIndex];
						if (bone2.isSimulated)
						{
							if (bone2.childIndex == i)
							{
								transformData2.localPosition = bone2.localBoneVector;
							}
							else
							{
								transformData2.localPosition = bone2.localBoneVector + (bone.originalLocalPosition - bone2.originalLocalVector);
							}
						}
						else
						{
							transformData2.localPosition = bone.originalLocalPosition;
						}
						float4x = this.transformData[chain.boneOffset + bone.parentIndex].localToWorld;
						bone.totalRestLength = bone2.totalRestLength;
						bone.totalLength = bone2.totalLength;
						bone.totalMinRestLength = bone2.totalMinRestLength;
						bone.totalMaxRestLength = bone2.totalMaxRestLength;
					}
					if (bone.isSimulated)
					{
						transformData2.localRotation = transformData2.prevLocalRotation;
					}
					transformData2.UpdateGlobalTransform(float4x);
					float3 lossyScale = transformData2.lossyScale;
					float num5 = math.cmax(lossyScale);
					bone.globalRadiusBegin = bone.radiusBegin * num5;
					bone.beginPoint = transformData2.position;
					bone.endPoint = bone.prevEndPoint;
					float3 prevVector = bone.prevVector;
					float3 float2 = bone.prevEndPoint;
					float num6 = ((bone.grabStatus == PhysBoneManager.Bone.Status.Grabbed) ? chain.grabMovement : 0f) * chain.grabIkSolved;
					float3 float3 = float3.zero;
					if (bone.isSimulated)
					{
						if (bone.immobile > 0f)
						{
							float3 float4 = (math.transform(immobileMatrix, bone.immobileEndpoint) - bone.endPoint) * bone.immobile;
							bone.endPoint += float4;
							float2 = bone.endPoint;
						}
						if (bone.childIndex < 0)
						{
							bone.globalRadiusEnd = bone.radiusEnd * num5;
						}
						else
						{
							PhysBoneManager.TransformData transformData3 = this.transformData[chain.boneOffset + bone.childIndex];
							float3 float5 = lossyScale * transformData3.localScale;
							float num7 = math.max(math.max(float5.x, float5.y), float5.z);
							bone.globalRadiusEnd = bone.radiusEnd * num7;
						}
						float3 float6 = math.transform(transformData2.localToWorld, bone.localBoneVector) - transformData2.position;
						float num8 = math.length(float6);
						if (num8 >= 8E-06f)
						{
							float4x4 float4x2 = math.mul(float4x, float4x4.TRS(transformData2.localPosition, bone.localPoseRotation, transformData2.localScale));
							float3 float7 = math.transform(float4x2, bone.localPoseBoneVector);
							float num9 = math.max(math.length(float7 - bone.beginPoint), 8E-06f);
							bone.globalRestLength = math.distance(transformData2.position, math.transform(float4x2, bone.originalLocalVector));
							if (chain.version == VRCPhysBoneBase.Version.Version_1_0)
							{
								float num10 = (num9 - num8) * bone.pull * 0.5f * frameRatio;
								num8 += num10;
								bone.endPoint += math.normalizesafe(bone.endPoint - bone.beginPoint, default(float3)) * num10;
								if (chain.integrationType == VRCPhysBoneBase.IntegrationType.Simplified)
								{
									float3 float8 = (float7 - bone.endPoint) * bone.pull;
									if (bone.gravity != 0f)
									{
										float num11 = math.min(bone.gravity, 1f) * num8 * ((bone.grabStatus == PhysBoneManager.Bone.Status.None) ? 1f : 0f);
										if (bone.gravityFalloff > 0f)
										{
											float3 float9 = math.normalize(math.rotate(transformData2.localToWorld, bone.originalLocalGravityNormal));
											float num12 = math.min(1f, 1f - math.dot(float9, @float));
											num11 *= math.lerp(1f - bone.gravityFalloff, 1f, num12);
										}
										float3 float10 = math.normalizesafe(bone.endPoint - bone.beginPoint, default(float3));
										float3 float11 = math.rotate(quaternion.AxisAngle(math.normalizesafe(math.cross(float10, @float), default(float3)), 1.5707964f), float10);
										float8 += float11 * num11 * math.dot(float11, @float);
									}
									float3 = math.lerp(float8, bone.prevVelocity, math.min(1f, math.lerp(0f, 0.99f, bone.spring)));
									if (frameRatio >= 1f)
									{
										bone.prevVelocity = float3;
									}
								}
								else
								{
									float3 = bone.prevVelocity * bone.spring;
									float3 += (float7 - (bone.endPoint + float3)) * bone.pull;
									if (bone.gravity != 0f)
									{
										float num13 = bone.gravity * num8 * ((bone.grabStatus == PhysBoneManager.Bone.Status.None) ? 1f : 0f);
										if (bone.gravityFalloff > 0f)
										{
											float3 float12 = math.normalize(math.rotate(transformData2.localToWorld, bone.originalLocalGravityNormal));
											float num14 = math.min(1f, 1f - math.dot(float12, @float));
											num13 *= math.lerp(1f - bone.gravityFalloff, 1f, num14);
										}
										float3 += @float * num13;
									}
									float3 += prevVector * bone.stiffness;
								}
								float3 *= frameRatio;
								float3 = bone.beginPoint + math.normalizesafe(bone.endPoint + float3 - bone.beginPoint, default(float3)) * num8 - bone.endPoint;
							}
							else
							{
								if (bone.gravity != 0f && bone.grabStatus == PhysBoneManager.Bone.Status.None)
								{
									float num15 = math.abs(bone.gravity);
									if (bone.gravityFalloff > 0f)
									{
										float3 float13 = math.normalize(math.rotate(transformData2.localToWorld, bone.originalLocalGravityNormal));
										float num16 = math.min(1f, 1f - math.dot(float13, @float));
										num15 *= math.lerp(1f - bone.gravityFalloff, 1f, num16);
									}
									float3 float14 = float7 - bone.beginPoint;
									float3 float15 = @float * math.sign(bone.gravity) * num9;
									float7 = bone.beginPoint + math.normalizesafe(math.lerp(float14, float15, num15), default(float3)) * num9;
								}
								if (chain.integrationType == VRCPhysBoneBase.IntegrationType.Simplified)
								{
									float3 = math.lerp((float7 - bone.endPoint) * bone.pull, bone.prevVelocity, math.min(1f, math.lerp(0f, 0.99f, bone.spring)));
									if (frameRatio >= 1f)
									{
										bone.prevVelocity = float3;
									}
								}
								else
								{
									float3 = bone.prevVelocity * bone.spring;
									float3 += (float7 - (bone.endPoint + float3)) * bone.pull;
									if (bone.stiffness > 0f)
									{
										float3 float16 = bone.endPoint + float3 - bone.beginPoint;
										float3 += (prevVector - float16) * bone.stiffness;
									}
								}
								float3 *= frameRatio;
								if (bone.stretchMotion < 1f)
								{
									float3 float17 = bone.endPoint + float3 - bone.beginPoint;
									float num17 = math.length(float17);
									float num18 = math.length(prevVector);
									float num19 = (num18 < num9) ? math.clamp(num17, num18, num9) : math.clamp(num17, num9, num18);
									float3 = bone.beginPoint + math.normalizesafe(float17, default(float3)) * math.lerp(num19, num17, bone.stretchMotion) - bone.endPoint;
								}
							}
							float3 float18 = (float7 - bone.endPoint) * frameRatio;
							float3 = math.lerp(float3, float18, num6);
							bone.endPoint += float3;
							float minLength = bone.globalRestLength * bone.squish;
							float maxLength = bone.globalRestLength * bone.stretch;
							bone.endPoint = bone.beginPoint + this.ClampGlobalBoneLength(bone.endPoint - bone.beginPoint, minLength, maxLength, float6);
							num8 = math.distance(bone.beginPoint, bone.endPoint);
							this.SolveCollisions(ref chain, ref bone, ref bone.endPoint, minLength, num8, frameRatio);
							num8 = math.distance(bone.beginPoint, bone.endPoint);
							float num20 = math.length(bone.localPoseBoneVector) * (num8 / num9);
							bone.localBoneVector = math.normalizesafe(bone.originalLocalVector, default(float3)) * num20;
							float3 float19 = math.normalizesafe(bone.localBoneVector, default(float3));
							float3 float20 = math.normalizesafe(math.transform(PhysBoneManager.SafeInverse(math.mul(float4x, float4x4.TRS(transformData2.localPosition, bone.originalLocalRotation, transformData2.localScale))), bone.endPoint), float19);
							float20 = this.ApplyAngleLimits(chain.limitType, bone.limitAxisX, bone.limitAxisY, float20, bone.maxAngle);
							transformData2.localRotation = math.mul(bone.originalLocalRotation, Quaternion.FromToRotation(float19, float20));
							if (frameRatio < 1f)
							{
								transformData2.localRotation = math.slerp(bone.prevLocalRotation, transformData2.localRotation, frameRatio);
							}
							bone.totalLength += num8;
							bone.totalRestLength += bone.globalRestLength;
							bone.totalMinRestLength += bone.globalRestLength * bone.squish;
							bone.totalMaxRestLength += bone.globalRestLength * bone.stretch;
						}
						Bounds bounds = new(bone.beginPoint, Vector3.one * bone.globalRadiusBegin * 2f);
						Bounds bounds2 = new(bone.endPoint, Vector3.one * bone.globalRadiusEnd * 2f);
						chain.renderBounds.Encapsulate(bounds);
						chain.renderBounds.Encapsulate(bounds2);
						if (bone.globalRadiusBegin > 0f)
						{
							if (!flag)
							{
								flag = true;
								chain.collisionBounds = bounds;
							}
							else
							{
								chain.collisionBounds.Encapsulate(bounds);
							}
						}
						if (bone.globalRadiusEnd > 0f)
						{
							if (!flag)
							{
								flag = true;
								chain.collisionBounds = bounds2;
							}
							else
							{
								chain.collisionBounds.Encapsulate(bounds2);
							}
						}
					}
					transformData2.UpdateGlobalTransform(float4x);
					bone.endPoint = math.transform(transformData2.localToWorld, bone.localBoneVector);
					if (frameRatio >= 1f)
					{
						bone.immobileEndpoint = math.transform(matrix4x, bone.endPoint);
						bone.prevEndPoint = bone.endPoint;
						bone.prevVector = bone.endPoint - bone.beginPoint;
						bone.prevLocalRotation = transformData2.localRotation;
						if (chain.integrationType == VRCPhysBoneBase.IntegrationType.Advanced)
						{
							bone.prevVelocity = bone.endPoint - float2;
						}
					}
					transformData2.prevLocalPosition = transformData2.localPosition;
					transformData2.prevLocalRotation = transformData2.localRotation;
					this.transformData[num4] = transformData2;
					this.bones[num4] = bone;
					if (bone.isEndBone)
					{
						float3 float21 = math.normalizesafe(math.transform(rootParentMatrix, bone.originalRootEndpoint) - transformData.position, default(float3));
						float3 float22 = math.normalizesafe(bone.endPoint - transformData.position, default(float3));
						float num21 = PhysBoneManager.AlmostEquals(float21, float22, 0.0001f) ? 0f : math.acos(math.dot(float21, float22));
						num = math.max(num, num21);
						float num22 = bone.totalLength - bone.totalRestLength;
						if (!PhysBoneManager.AlmostEquals(num22, 0f, 0.0001f))
						{
							if (num22 < 0f)
							{
								num22 = math.saturate(-num22 / (bone.totalRestLength - bone.totalMinRestLength));
								num3 = math.max(num3, num22);
							}
							else
							{
								num22 = math.saturate(num22 / (bone.totalMaxRestLength - bone.totalRestLength));
								num2 = math.max(num2, num22);
							}
						}
					}
				}
				chain.paramAngle = num / 3.1415927f;
				chain.paramStretch = num2;
				chain.paramSquish = num3;
			}

			// Token: 0x060002D3 RID: 723 RVA: 0x00014078 File Offset: 0x00012278
			private void UpdateColliders(ref PhysBoneManager.Chain chain)
			{
				for (int i = 0; i < chain.colliders.Length; i++)
				{
					PhysBoneManager.Collider collider = chain.colliders[i];
					CollisionScene.ShapeData shapeData = this.shapeData[collider.shapeId];
					collider.prevPos0 = collider.nextPos0;
					collider.prevPos1 = collider.nextPos1;
					collider.pos0 = shapeData.outPos0;
					collider.pos1 = shapeData.outPos1;
					collider.nextPos0 = shapeData.outPos0;
					collider.nextPos1 = shapeData.outPos1;
					if (!collider.hasUpdated)
					{
						collider.hasUpdated = true;
						collider.prevPos0 = shapeData.outPos0;
						collider.prevPos1 = shapeData.outPos1;
					}
					collider.radius = shapeData.outRadius;
					switch (shapeData.shapeType)
					{
					case CollisionScene.ShapeType.Sphere:
						collider.shapeType = PhysBoneManager.Collider.ShapeType.Sphere;
						break;
					case CollisionScene.ShapeType.Capsule:
					case CollisionScene.ShapeType.Finger:
						collider.shapeType = PhysBoneManager.Collider.ShapeType.Capsule;
						break;
					case CollisionScene.ShapeType.Plane:
						collider.shapeType = PhysBoneManager.Collider.ShapeType.Plane;
						break;
					}
					chain.colliders[i] = collider;
				}
			}

			// Token: 0x060002D4 RID: 724 RVA: 0x00014190 File Offset: 0x00012390
			private void LerpColliders(ref PhysBoneManager.Chain chain, float time)
			{
				for (int i = 0; i < chain.colliders.Length; i++)
				{
					PhysBoneManager.Collider collider = chain.colliders[i];
					collider.Lerp(time);
					chain.colliders[i] = collider;
				}
			}

			// Token: 0x060002D5 RID: 725 RVA: 0x000141D8 File Offset: 0x000123D8
			private void SolveCollisions(ref PhysBoneManager.Chain chain, ref PhysBoneManager.Bone bone, ref float3 currentEndpoint, float minLength, float maxLength, float frameRatio)
			{
				if (chain.colliders.Length == 0)
				{
					return;
				}
				float3 @float = bone.endPoint - bone.beginPoint;
				float num = math.length(@float);
				float num2 = 1f - math.max(0f, (num - bone.globalRadiusBegin) / num);
				float num3 = 0f;
				for (int i = 0; i < chain.colliders.Length; i++)
				{
					PhysBoneManager.Collider collider = chain.colliders[i];
					PhysBoneManager.Collider.ShapeType shapeType = collider.shapeType;
					if (shapeType <= PhysBoneManager.Collider.ShapeType.Capsule || shapeType != PhysBoneManager.Collider.ShapeType.Plane)
					{
						float3 float2;
						if (collider.shapeType == PhysBoneManager.Collider.ShapeType.Sphere)
						{
							float2 = collider.pos0;
						}
						else
						{
							float2 = MathUtil.ClosestPointOnLineSegment(collider.pos0, collider.pos1, currentEndpoint);
						}
						if (collider.insideBounds)
						{
							float3 float3 = currentEndpoint;
							float globalRadiusEnd = bone.globalRadiusEnd;
							float num4 = math.length(float3 - float2) + globalRadiusEnd - collider.radius;
							if (num4 > 0f)
							{
								float3 float4 = math.normalizesafe(float2 - float3, default(float3));
								currentEndpoint += float4 * num4;
								currentEndpoint = bone.beginPoint + this.ClampGlobalBoneLength(currentEndpoint - bone.beginPoint, minLength, maxLength, @float);
								num3 = 1f;
							}
						}
						else if (bone.globalRadiusEnd > 0f)
						{
							float3 float5 = currentEndpoint;
							float num5 = bone.globalRadiusEnd;
							if (bone.globalRadiusBegin > 0f && !collider.bonesAsSpheres)
							{
								float num6 = MathUtil.ClosestPointOnLineSegment_Ratio(bone.beginPoint, currentEndpoint, float2);
								num6 = math.max(num6, num2);
								float5 = math.lerp(bone.beginPoint, currentEndpoint, num6);
								num5 = math.lerp(bone.globalRadiusBegin, bone.globalRadiusEnd, num6);
							}
							float num7 = math.length(float5 - float2) - (num5 + collider.radius);
							if (num7 < 0f)
							{
								float3 float6 = math.normalizesafe(float2 - float5, default(float3));
								currentEndpoint += float6 * num7;
								currentEndpoint = bone.beginPoint + this.ClampGlobalBoneLength(currentEndpoint - bone.beginPoint, minLength, maxLength, @float);
								num3 = 1f;
							}
						}
					}
					else
					{
						float num8 = MathUtil.DistancePointToPlane(collider.pos0, collider.pos1, currentEndpoint) + bone.globalRadiusEnd;
						if (num8 > 0f)
						{
							currentEndpoint += collider.pos1 * num8;
							currentEndpoint = bone.beginPoint + this.ClampGlobalBoneLength(currentEndpoint - bone.beginPoint, minLength, maxLength, @float);
							num3 = 1f;
						}
					}
				}
				if (chain.integrationType == VRCPhysBoneBase.IntegrationType.Simplified && frameRatio >= 1f)
				{
					bone.prevVelocity = math.lerp(bone.prevVelocity, Vector3.zero, 0.25f * num3);
				}
			}

			// Token: 0x060002D6 RID: 726 RVA: 0x00014524 File Offset: 0x00012724
			public float3 ApplyAngleLimits(VRCPhysBoneBase.LimitType type, float3 limitAxisX, float3 limitAxisY, float3 axis, float2 maxAngle)
			{
				switch (type)
				{
				case VRCPhysBoneBase.LimitType.Angle:
					if (MathUtil.AngleBetweenTwoNormals(limitAxisY, axis) > maxAngle.x)
					{
						return math.mul(quaternion.AxisAngle(math.normalizesafe(math.cross(limitAxisY, axis), limitAxisX), maxAngle.x * 0.017453292f), limitAxisY);
					}
					return axis;
				case VRCPhysBoneBase.LimitType.Hinge:
				{
					float num = MathUtil.DistancePointToPlane(float3.zero, limitAxisX, axis);
					axis += limitAxisX * num;
					axis = math.normalizesafe(axis, default(float3));
					if (MathUtil.AngleBetweenTwoNormals(limitAxisY, axis) > maxAngle.x)
					{
						axis = math.mul(quaternion.AxisAngle(math.normalizesafe(math.cross(limitAxisY, axis), limitAxisX), maxAngle.x * 0.017453292f), limitAxisY);
					}
					return axis;
				}
				case VRCPhysBoneBase.LimitType.Polar:
				{
					float3 @float = math.normalize(math.cross(limitAxisX, limitAxisY));
					float3x3 float3x = new float3x3(@float, limitAxisX, limitAxisY);
					float3 float2 = math.normalize(math.mul(math.inverse(float3x), axis));
					float2 float3 = MathUtil.CartesianToPolar(float2);
					float3.x = math.clamp(float3.x, -maxAngle.x, maxAngle.x);
					float3.y = math.clamp(float3.y, -maxAngle.y, maxAngle.y);
					float2 = MathUtil.PolarToCartesian(float3);
					return math.normalize(math.mul(float3x, float2));
				}
				default:
					return axis;
				}
			}

			// Token: 0x060002D7 RID: 727 RVA: 0x00014688 File Offset: 0x00012888
			public float3 ClampGlobalBoneLength(float3 vector, float minLength, float maxLength, Vector3 fallbackVector)
			{
				minLength = math.max(minLength, 1E-05f);
				float num = math.length(vector);
				if (num <= 1E-45f)
				{
					return math.normalize(fallbackVector) * minLength;
				}
				if (num < minLength || num > maxLength)
				{
					return vector * (math.clamp(num, minLength, maxLength) / num);
				}
				return vector;
			}

			// Token: 0x060002D8 RID: 728 RVA: 0x000146E0 File Offset: 0x000128E0
			public void SolveGrabIK(ref PhysBoneManager.Chain chain, float4x4 rootParentMatrix)
			{
				PhysBoneManager.TransformData ptr = this.transformData[chain.boneOffset];
				chain.grabIkSolved = 1f;
				int num = 0;
				int grabBone = chain.grabBone;
				float3 position = ptr.position;
				float3 grabGlobalPosition = chain.grabGlobalPosition;
				PhysBoneManager.Bone bone = this.bones[chain.boneOffset + grabBone];
				if (!bone.isSimulated)
				{
					return;
				}
				NativeArray<PhysBoneManager.PhysBoneJob.IKBone> nativeArray = new NativeArray<PhysBoneManager.PhysBoneJob.IKBone>(256, (Allocator)2, (NativeArrayOptions)1);
				int num2 = 0;
				int i = grabBone;
				float3 @float = bone.endPoint;
				float num3 = 0f;
				float num4 = 0f;
				while (i >= 0)
				{
					int num5 = chain.boneOffset + i;
					PhysBoneManager.Bone bone2 = this.bones[num5];
					PhysBoneManager.TransformData transformData = this.transformData[num5];
					PhysBoneManager.PhysBoneJob.IKBone ikbone = default(PhysBoneManager.PhysBoneJob.IKBone);
					ikbone.boneIndex = num5;
					if (bone2.childIndex >= 0 || bone2.isEndBone)
					{
						ikbone.isSimulated = true;
						ikbone.position = transformData.position;
						ikbone.endPosition = @float;
						ikbone.length = math.distance(ikbone.position, ikbone.endPosition);
						ikbone.localAxis = math.normalize(math.transform(PhysBoneManager.SafeInverse(transformData.localToWorld), @float));
						num3 += bone2.globalRestLength;
						num4 += ikbone.length - math.distance(bone2.beginPoint, bone2.endPoint);
						ikbone.rotationOffset = Quaternion.FromToRotation(ikbone.localAxis, bone2.localPoseBoneVector);
						@float = transformData.position;
						num = i;
					}
					else
					{
						ikbone.isSimulated = false;
					}
					nativeArray[num2] = ikbone;
					num2++;
					i = bone2.parentIndex;
				}
				position = this.transformData[chain.boneOffset + num].position;
				float num6 = math.distance(position, grabGlobalPosition) - num4 - num3;
				PhysBoneManager.Bone bone3;
				for (i = grabBone; i >= 0; i = bone3.parentIndex)
				{
					int num7 = chain.boneOffset + i;
					bone3 = this.bones[num7];
					PhysBoneManager.TransformData transformData2 = this.transformData[num7];
					if (bone3.isSimulated)
					{
						float num8 = bone3.globalRestLength + num6 * (bone3.globalRestLength / num3);
						num8 = Mathf.Clamp(num8, Mathf.Max(bone3.globalRestLength * bone3.squish, 1E-05f), bone3.globalRestLength * bone3.stretch);
						float num9 = math.length(transformData2.position - math.transform(transformData2.localToWorld, bone3.originalLocalVector));
						float num10 = bone3.originalLocalBoneLength * (num8 / num9);
						bone3.localPoseBoneVector = math.normalizesafe(bone3.localPoseBoneVector, default(float3)) * num10;
						this.bones[num7] = bone3;
					}
				}
				for (int j = 0; j < 16; j++)
				{
					float3 float2 = grabGlobalPosition;
					for (int k = 0; k < num2; k++)
					{
						PhysBoneManager.PhysBoneJob.IKBone ikbone2 = nativeArray[k];
						if (ikbone2.isSimulated)
						{
							ikbone2.endPosition = float2;
							float3 float3 = math.normalizesafe(ikbone2.position - float2, default(float3));
							float2 += float3 * ikbone2.length;
							ikbone2.position = float2;
							nativeArray[k] = ikbone2;
						}
					}
					float3x3 float3x = new(rootParentMatrix);
					float3 float4 = position;
					for (int l = num2 - 1; l >= 0; l--)
					{
						PhysBoneManager.PhysBoneJob.IKBone ikbone3 = nativeArray[l];
						PhysBoneManager.Bone bone4 = this.bones[ikbone3.boneIndex];
						PhysBoneManager.TransformData transformData3 = this.transformData[ikbone3.boneIndex];
						if (!ikbone3.isSimulated)
						{
							float3x = math.mul(float3x, new float3x3(transformData3.localRotation));
						}
						else
						{
							ikbone3.position = float4;
							float3 float5 = math.normalizesafe(ikbone3.endPosition - float4, default(float3));
							ikbone3.endPosition = float4 + float5 * ikbone3.length;
							float3x3 float3x2 = math.mul(float3x, new float3x3(bone4.originalLocalRotation));
							float3 float6 = math.normalizesafe(bone4.localBoneVector, default(float3));
							float3 float7 = math.normalizesafe(math.mul(math.inverse(float3x2), ikbone3.endPosition - ikbone3.position), float6);
							float7 = this.ApplyAngleLimits(chain.limitType, bone4.limitAxisX, bone4.limitAxisY, float7, bone4.maxAngle);
							ikbone3.endPosition = ikbone3.position + math.normalizesafe(math.mul(float3x2, float7), default(float3)) * ikbone3.length;
							float3 localAxis = ikbone3.localAxis;
							float3 float8 = math.normalizesafe(math.mul(math.inverse(float3x2), ikbone3.endPosition - ikbone3.position), default(float3));
							if (math.length(float8) > 0f)
							{
								float3x3 float3x3 = new(math.mul(bone4.originalLocalRotation, Quaternion.FromToRotation(localAxis, float8)));
								float3x = math.mul(float3x, float3x3);
							}
							float4 = ikbone3.endPosition;
							nativeArray[l] = ikbone3;
						}
					}
					if (math.distance(grabGlobalPosition, nativeArray[0].endPosition) <= 0.01f)
					{
						break;
					}
				}
				float4x4 float4x = rootParentMatrix;
				for (int m = num2 - 1; m >= 0; m--)
				{
					PhysBoneManager.PhysBoneJob.IKBone ikbone4 = nativeArray[m];
					int boneIndex = ikbone4.boneIndex;
					PhysBoneManager.Bone bone5 = this.bones[boneIndex];
					PhysBoneManager.TransformData transformData4 = this.transformData[boneIndex];
					if (ikbone4.isSimulated)
					{
						float3 localAxis2 = ikbone4.localAxis;
						float3 float9 = math.normalizesafe(math.transform(math.inverse(math.mul(float4x, float4x4.TRS(transformData4.localPosition, bone5.originalLocalRotation, transformData4.localScale))), ikbone4.endPosition), default(float3));
						if (math.length(float9) > 0f)
						{
							bone5.localPoseRotation = math.mul(bone5.originalLocalRotation, Quaternion.FromToRotation(localAxis2, float9));
						}
						this.bones[boneIndex] = bone5;
						float4x = math.mul(float4x, float4x4.TRS(transformData4.localPosition, bone5.localPoseRotation, transformData4.localScale));
					}
					else
					{
						float4x = math.mul(float4x, float4x4.TRS(transformData4.localPosition, transformData4.localRotation, transformData4.localScale));
					}
				}
				nativeArray.Dispose();
			}

			// Token: 0x0400031E RID: 798
			[ReadOnly]
			public float currentTime;

			// Token: 0x0400031F RID: 799
			[ReadOnly]
			public float3 distanceCullOrigin;

			// Token: 0x04000320 RID: 800
			[ReadOnly]
			public NativeArray<int> chainIndices;

			// Token: 0x04000321 RID: 801
			[ReadOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<PhysBoneManager.ChainRoot> roots;

			// Token: 0x04000322 RID: 802
			[NativeDisableParallelForRestriction]
			public NativeArray<PhysBoneManager.Chain> chains;

			// Token: 0x04000323 RID: 803
			[NativeDisableParallelForRestriction]
			public NativeArray<PhysBoneManager.Bone> bones;

			// Token: 0x04000324 RID: 804
			[NativeDisableParallelForRestriction]
			public NativeArray<TransformAccess> transformAccess;

			// Token: 0x04000325 RID: 805
			[NativeDisableParallelForRestriction]
			public NativeArray<PhysBoneManager.TransformData> transformData;

			// Token: 0x04000326 RID: 806
			[NativeDisableParallelForRestriction]
			public NativeArray<CollisionScene.ShapeData> shapeData;

			// Token: 0x04000327 RID: 807
			[NativeDisableParallelForRestriction]
			public NativeArray<PhysBoneManager.CriticalErrorType> errorBuffer;

			// Token: 0x04000328 RID: 808
			private const int FABRIK_MAX_ITERATIONS = 16;

			// Token: 0x04000329 RID: 809
			private const float FABRIK_SOLVED_MARGIN = 0.01f;

			// Token: 0x0200008B RID: 139
			private struct IKBone
			{
				// Token: 0x0400038D RID: 909
				public bool isSimulated;

				// Token: 0x0400038E RID: 910
				public int boneIndex;

				// Token: 0x0400038F RID: 911
				public float3 position;

				// Token: 0x04000390 RID: 912
				public float3 endPosition;

				// Token: 0x04000391 RID: 913
				public float length;

				// Token: 0x04000392 RID: 914
				public float3 localAxis;

				// Token: 0x04000393 RID: 915
				public Quaternion rotationOffset;
			}
		}

		// Token: 0x0200006E RID: 110
		public struct ReadBoneJob : IJobParallelForTransform
		{
			// Token: 0x060002D9 RID: 729 RVA: 0x00014DAD File Offset: 0x00012FAD
			public void Execute(int index, TransformAccess transform)
			{
				this.transformData[index] = transform;
			}

			// Token: 0x0400032A RID: 810
			public NativeArray<TransformAccess> transformData;
		}

		// Token: 0x0200006F RID: 111
		public struct UpdateRootsJob : IJobParallelFor
		{
			// Token: 0x060002DA RID: 730 RVA: 0x00014DBC File Offset: 0x00012FBC
			public void Execute(int index)
			{
				PhysBoneManager.ChainRoot chainRoot = this.roots[index];
				if (!chainRoot.isUsed)
				{
					return;
				}
				chainRoot.fixedTime += this.fixedTime;
				chainRoot.realTime += this.realTime;
				float num = chainRoot.useFixedTime ? chainRoot.fixedTime : chainRoot.realTime;
				chainRoot.executions = math.min(num / 0.016666668f, 4f);
				if (chainRoot.executions >= 1f)
				{
					chainRoot.realTime = math.frac(chainRoot.executions) * 0.016666668f;
					chainRoot.fixedTime = math.frac(chainRoot.executions) * 0.016666668f;
				}
				this.roots[index] = chainRoot;
			}

			// Token: 0x0400032B RID: 811
			private const int MAX_EXECUTIONS = 4;

			// Token: 0x0400032C RID: 812
			public float realTime;

			// Token: 0x0400032D RID: 813
			public float fixedTime;

			// Token: 0x0400032E RID: 814
			public NativeArray<PhysBoneManager.ChainRoot> roots;
		}

		// Token: 0x02000070 RID: 112
		public interface IJobSortable
		{
			// Token: 0x17000055 RID: 85
			// (get) Token: 0x060002DB RID: 731
			Transform SortingBaseTransform { get; }

			// Token: 0x060002DC RID: 732
			void GetKnownDependencies(List<PhysBoneManager.IJobSortable> dependencies);

			// Token: 0x17000056 RID: 86
			// (get) Token: 0x060002DD RID: 733
			// (set) Token: 0x060002DE RID: 734
			int ExecutionGroup { get; set; }
		}

		// Token: 0x02000071 RID: 113
		private class SortingData
		{
			// Token: 0x0400032F RID: 815
			public PhysBoneManager.IJobSortable source;

			// Token: 0x04000330 RID: 816
			public PhysBoneManager.SortingData parentDependency;

			// Token: 0x04000331 RID: 817
			public List<PhysBoneManager.SortingData> dependencies = new List<PhysBoneManager.SortingData>();

			// Token: 0x04000332 RID: 818
			public bool visited;

			// Token: 0x04000333 RID: 819
			public int executionGroup = -1;
		}

		// Token: 0x02000072 RID: 114
		public struct Collider
		{
			// Token: 0x060002E0 RID: 736 RVA: 0x00014E94 File Offset: 0x00013094
			public Collider(VRCPhysBoneColliderBase comp)
			{
				this.shapeId = (int)comp.shape.id;
				this.insideBounds = comp.insideBounds;
				this.bonesAsSpheres = comp.bonesAsSpheres;
				this.shapeType = PhysBoneManager.Collider.ShapeType.Sphere;
				this.hasUpdated = false;
				this.prevPos0 = float3.zero;
				this.prevPos1 = float3.zero;
				this.nextPos0 = float3.zero;
				this.nextPos1 = float3.zero;
				this.pos0 = float3.zero;
				this.pos1 = float3.zero;
				this.radius = 0f;
			}

			// Token: 0x060002E1 RID: 737 RVA: 0x00014F25 File Offset: 0x00013125
			public void Lerp(float time)
			{
				this.pos0 = math.lerp(this.prevPos0, this.nextPos0, time);
				this.pos1 = math.lerp(this.prevPos1, this.nextPos1, time);
			}

			// Token: 0x04000334 RID: 820
			public int shapeId;

			// Token: 0x04000335 RID: 821
			public bool hasUpdated;

			// Token: 0x04000336 RID: 822
			public float3 prevPos0;

			// Token: 0x04000337 RID: 823
			public float3 prevPos1;

			// Token: 0x04000338 RID: 824
			public float3 nextPos0;

			// Token: 0x04000339 RID: 825
			public float3 nextPos1;

			// Token: 0x0400033A RID: 826
			public PhysBoneManager.Collider.ShapeType shapeType;

			// Token: 0x0400033B RID: 827
			public float3 pos0;

			// Token: 0x0400033C RID: 828
			public float3 pos1;

			// Token: 0x0400033D RID: 829
			public float radius;

			// Token: 0x0400033E RID: 830
			public bool insideBounds;

			// Token: 0x0400033F RID: 831
			public bool bonesAsSpheres;

			// Token: 0x0200008C RID: 140
			public enum ShapeType : byte
			{
				// Token: 0x04000395 RID: 917
				Sphere,
				// Token: 0x04000396 RID: 918
				Capsule,
				// Token: 0x04000397 RID: 919
				Plane
			}
		}

		// Token: 0x02000073 RID: 115
		public class Grab
		{
			// Token: 0x04000340 RID: 832
			public int playerId;

			// Token: 0x04000341 RID: 833
			public ChainId chainId;

			// Token: 0x04000342 RID: 834
			public int bone;

			// Token: 0x04000343 RID: 835
			public float3 globalPosition;

			// Token: 0x04000344 RID: 836
			public float3 localOffset;

			// Token: 0x04000345 RID: 837
			public PhysBoneManager.Pose pose;
		}

		// Token: 0x02000074 RID: 116
		public class Pose
		{
			// Token: 0x04000346 RID: 838
			public ChainId chainId;

			// Token: 0x04000347 RID: 839
			public bool prevIsAnimated;

			// Token: 0x04000348 RID: 840
			public List<PhysBoneManager.Pose.PoseData> prevData = new List<PhysBoneManager.Pose.PoseData>();

			// Token: 0x0200008D RID: 141
			public struct PoseData
			{
				// Token: 0x04000398 RID: 920
				public float3 localPoseBoneVector;

				// Token: 0x04000399 RID: 921
				public quaternion localPoseRotation;
			}
		}
	}
}
