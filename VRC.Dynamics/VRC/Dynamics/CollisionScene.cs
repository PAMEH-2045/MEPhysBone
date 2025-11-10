using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using VRC.Core.Burst;

namespace VRC.Dynamics
{
	// Token: 0x02000004 RID: 4
	public class CollisionScene : IDisposable
	{
		// Token: 0x06000018 RID: 24 RVA: 0x00002A8D File Offset: 0x00000C8D
		public CollisionScene() : this(new CollisionBroadphase_HybridSAP())
		{
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002A9C File Offset: 0x00000C9C
		public CollisionScene(ICollisionBroadphase broadphase)
		{
			this.SetBroadphase(broadphase);
			VRCAvatarDynamicsScheduler.OnFrameComplete += new Action(this.Complete);
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002B53 File Offset: 0x00000D53
		private void SetBroadphase(ICollisionBroadphase broadphase)
		{
			broadphase.scene = this;
			this.broadphase = broadphase;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002B63 File Offset: 0x00000D63
		public ICollisionBroadphase GetBroadphase()
		{
			return this.broadphase;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002B6B File Offset: 0x00000D6B
		public bool AddShape(CollisionScene.Shape shape)
		{
			bool result = this.shapesToRemove.Remove(shape);
			this.shapesToAdd.Add(shape);
			return result;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002B86 File Offset: 0x00000D86
		public void RemoveShape(CollisionScene.Shape shape)
		{
			this.shapesToAdd.Remove(shape);
			this.shapesToRemove.Add(shape);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002BA4 File Offset: 0x00000DA4
		public void UpdateShapeData(CollisionScene.Shape shape)
		{
			if (shape.id == CollisionScene.Shape.NullId)
			{
				return;
			}
			this.jobHandle.Complete();
			CollisionScene.ShapeData shapeData = this.shapeData[(int)shape.id];
			shapeData.shapeType = shape.shapeType;
			shapeData.radius = shape.radius;
			shapeData.height = shape.height;
			shapeData.center = shape.center;
			shapeData.axis = shape.axis;
			this.shapeData[(int)shape.id] = shapeData;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002C30 File Offset: 0x00000E30
		public CollisionScene.ShapeData GetShapeData(CollisionScene.Shape shape)
		{
			if (shape.id == CollisionScene.Shape.NullId)
			{
				return default(CollisionScene.ShapeData);
			}
			return this.shapeData[(int)shape.id];
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002C65 File Offset: 0x00000E65
		public IEnumerator<CollisionScene.ShapeData> GetShapeData()
		{
			int num2;
			for (int i = 0; i < this.activeShapes.Length; i = num2 + 1)
			{
				ushort num = this.activeShapes[i];
				yield return this.shapeData[(int)num];
				num2 = i;
			}
			yield break;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002C74 File Offset: 0x00000E74
		public int ShapeCount()
		{
			return this.activeShapes.Length;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002C84 File Offset: 0x00000E84
		public void CastSphere(CollisionShapes.Sphere sphere, List<CollisionScene.Shape> output)
		{
			this.jobHandle.Complete();
			CollisionScene.ShapeData shape = default(CollisionScene.ShapeData);
			shape.isReceiver = true;
			shape.isCollider = true;
			shape.shapeType = CollisionScene.ShapeType.Sphere;
			shape.outPos0 = sphere.position;
			shape.outRadius = sphere.radius;
			shape.bounds = new Bounds(sphere.position, Vector3.one * sphere.radius * 2f);
			CollisionScene.CastBuffer.Clear();
			this.broadphase.CastShape(shape, CollisionScene.CastBuffer);
			output.Clear();
			foreach (ushort num in CollisionScene.CastBuffer)
			{
				output.Add(this.shapes[(int)num]);
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002D74 File Offset: 0x00000F74
		internal void UpdateBounds(CollisionScene.Shape shape, Bounds bounds)
		{
			if (shape.id == CollisionScene.Shape.NullId)
			{
				return;
			}
			CollisionScene.ShapeData shapeData = this.shapeData[(int)shape.id];
			shapeData.bounds = bounds;
			this.shapeData[(int)shape.id] = shapeData;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002DBC File Offset: 0x00000FBC
		public JobHandle UpdateAndSchedule(float deltaTime, bool completeImmediately, JobHandle dependsOn = default(JobHandle))
		{
			this.RemoveShapes();
			this.AddShapes();
			dependsOn = this.ScheduleReadTransforms(dependsOn);
			dependsOn = this.broadphase.ScheduleJobs(deltaTime, dependsOn);
			this.jobHandle = new DisposableJobHandle(dependsOn);
			this.jobHandlePendingCompletion = true;
			if (completeImmediately)
			{
				this.Complete();
			}
			return dependsOn;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002E0C File Offset: 0x0000100C
		public JobHandle ScheduleReadTransforms(JobHandle dependsOn = default(JobHandle))
		{
			return IJobParallelForTransformExtensions.ScheduleReadOnly<CollisionScene.ReadTransformJob>(new CollisionScene.ReadTransformJob
			{
				transformData = this.transformData
			}, this.transforms.GetTransformAccessArray(), 8, dependsOn);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002E44 File Offset: 0x00001044
		public JobHandle ScheduleUpdateShapePositions(NativeList<ushort> activeShapes, JobHandle dependsOn = default(JobHandle))
		{
			if (activeShapes.Length > 0)
			{
				dependsOn = IJobParallelForExtensions.Schedule<CollisionScene.UpdateShapePositionsJob>(new CollisionScene.UpdateShapePositionsJob
				{
					activeShapes = activeShapes,
					transformData = this.transformData,
					transformLookup = this.transforms.GetLookupFromId(),
					shapeData = this.shapeData
				}, activeShapes.Length, CollisionScene.UpdateShapePositionsJob.SHAPE_BATCH_COUNT, dependsOn);
			}
			return dependsOn;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002EB0 File Offset: 0x000010B0
		private void Complete()
		{
			if (!this.jobHandlePendingCompletion)
			{
				return;
			}
			this.jobHandlePendingCompletion = false;
			this.jobHandle.Complete();
			this.broadphase.OnComplete();
			CollisionScene.CollisionEvent collisionEvent;
			while (this.collisionEvents.TryDequeue(out collisionEvent))
			{
				CollisionScene.Shape shape = this.shapes[collisionEvent.receiver];
				CollisionScene.Shape shape2 = this.shapes[collisionEvent.collider];
				if (collisionEvent.found)
				{
					Action<CollisionScene.Shape> onEnter = shape.OnEnter;
					if (onEnter != null)
					{
						onEnter.Invoke(shape2);
					}
				}
				else
				{
					Action<CollisionScene.Shape> onExit = shape.OnExit;
					if (onExit != null)
					{
						onExit.Invoke(shape2);
					}
				}
			}
			this.collisionEvents.Clear();
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002F4C File Offset: 0x0000114C
		public void Dispose()
		{
			VRCAvatarDynamicsScheduler.OnFrameComplete -= new Action(this.Complete);
			this.shapesToAdd.Clear();
			this.shapesToRemove.Clear();
			this.activeShapes.Dispose();
			this.shapeData.Dispose();
			this.collisionEvents.Dispose();
			this.transforms.Dispose();
			this.transformData.Dispose();
			this.broadphase.Dispose();
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002FC2 File Offset: 0x000011C2
		public void SyncShapesNow()
		{
			this.AddShapes();
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002FCC File Offset: 0x000011CC
		private void RemoveShapes()
		{
			foreach (CollisionScene.Shape shape in this.deadShapes)
			{
				this.shapes[(int)shape.id] = null;
				shape.id = CollisionScene.Shape.NullId;
			}
			this.deadShapes.Clear();
			foreach (CollisionScene.Shape shape2 in this.shapesToRemove)
			{
				if (shape2.id != CollisionScene.Shape.NullId)
				{
					for (int i = 0; i < this.activeShapes.Length; i++)
					{
						if (this.activeShapes[i] == shape2.id)
						{
							this.activeShapes.RemoveAtSwapBack(i);
							break;
						}
					}
					CollisionScene.ShapeData shapeData = this.shapeData[(int)shape2.id];
					shapeData.shapeType = CollisionScene.ShapeType.None;
					this.shapeData[(int)shape2.id] = shapeData;
					this.transforms.Remove((int)(shape2.id * 2));
					this.transforms.Remove((int)(shape2.id * 2 + 1));
					this.broadphase.RemoveShape(shapeData, shape2.id);
					this.deadShapes.Add(shape2);
					ContactReceiver contactReceiver = shape2.component as ContactReceiver;
					if (contactReceiver != null)
					{
						contactReceiver.OnDisableInCollisionScene();
					}
				}
			}
			this.shapesToRemove.Clear();
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003164 File Offset: 0x00001364
		private void AddShapes()
		{
			foreach (CollisionScene.Shape shape in this.shapesToAdd)
			{
				if (shape.id == CollisionScene.Shape.NullId)
				{
					if (this.activeShapes.Length >= 8192)
					{
						break;
					}
					shape.id = this.FindAvailableId();
					if (shape.id == CollisionScene.Shape.NullId)
					{
						break;
					}
					CollisionScene.ShapeData shapeData = default(CollisionScene.ShapeData);
					shapeData.shapeType = shape.shapeType;
					shapeData.radius = shape.radius;
					shapeData.height = shape.height;
					shapeData.center = shape.center;
					shapeData.axis = shape.axis;
					shapeData.maxSize = shape.maxSize;
					shapeData.isReceiver = shape.isReceiver;
					shapeData.isCollider = shape.isCollider;
					shapeData.boundsMin = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
					shapeData.boundsMax = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
					this.shapeData[(int)shape.id] = shapeData;
					if (shape.transform0 != null)
					{
						this.transforms.Add(shape.transform0, (int)(shape.id * 2));
					}
					if (shape.transform1 != null)
					{
						this.transforms.Add(shape.transform1, (int)(shape.id * 2 + 1));
					}
					ushort id = shape.id;
					this.activeShapes.Add(in id);
					this.shapes[(int)shape.id] = shape;
					this.broadphase.AddShape(shapeData, shape.id);
				}
			}
			this.shapesToAdd.Clear();
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003350 File Offset: 0x00001550
		private ushort FindAvailableId()
		{
			for (ushort num = 0; num < 8192; num += 1)
			{
				if (this.shapes[(int)num] == null)
				{
					return num;
				}
			}
			return CollisionScene.Shape.NullId;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x0000337F File Offset: 0x0000157F
		public void DrawGizmos()
		{
			this.jobHandle.Complete();
			this.broadphase.DrawGizmos();
		}

		// Token: 0x0400001D RID: 29
		private ICollisionBroadphase broadphase;

		// Token: 0x0400001E RID: 30
		private static HashSet<ushort> CastBuffer = new HashSet<ushort>();

		// Token: 0x0400001F RID: 31
		private List<CollisionScene.Shape> deadShapes = new List<CollisionScene.Shape>();

		// Token: 0x04000020 RID: 32
		internal const int MAX_SHAPES = 8192;

		// Token: 0x04000021 RID: 33
		internal const int MAX_TRANSFORMS = 16384;

		// Token: 0x04000022 RID: 34
		internal const int MAX_COLLISION_EVENTS = 4096;

		// Token: 0x04000023 RID: 35
		private CollisionScene.Shape[] shapes = new CollisionScene.Shape[8192];

		// Token: 0x04000024 RID: 36
		private HashSet<CollisionScene.Shape> shapesToAdd = new HashSet<CollisionScene.Shape>();

		// Token: 0x04000025 RID: 37
		private HashSet<CollisionScene.Shape> shapesToRemove = new HashSet<CollisionScene.Shape>();

		// Token: 0x04000026 RID: 38
		internal NativeList<ushort> activeShapes = new NativeList<ushort>(8192, (Allocator)4);

		// Token: 0x04000027 RID: 39
		internal NativeArray<CollisionScene.ShapeData> shapeData = new NativeArray<CollisionScene.ShapeData>(8192, (Allocator)4, (NativeArrayOptions)1);

		// Token: 0x04000028 RID: 40
		internal NativeQueue<CollisionScene.CollisionEvent> collisionEvents = new NativeQueue<CollisionScene.CollisionEvent>((Allocator)4);

		// Token: 0x04000029 RID: 41
		internal FixedTransformAccessArray transforms = new FixedTransformAccessArray(16384);

		// Token: 0x0400002A RID: 42
		internal NativeArray<TransformAccess> transformData = new NativeArray<TransformAccess>(16384, (Allocator)4, (NativeArrayOptions)1);

		// Token: 0x0400002B RID: 43
		private DisposableJobHandle jobHandle;

		// Token: 0x0400002C RID: 44
		private bool jobHandlePendingCompletion;

		// Token: 0x02000046 RID: 70
		public enum ShapeType
		{
			// Token: 0x04000225 RID: 549
			None,
			// Token: 0x04000226 RID: 550
			AABB,
			// Token: 0x04000227 RID: 551
			Sphere,
			// Token: 0x04000228 RID: 552
			Capsule,
			// Token: 0x04000229 RID: 553
			Finger,
			// Token: 0x0400022A RID: 554
			Plane
		}

		// Token: 0x02000047 RID: 71
		public class Shape
		{
			// Token: 0x1700004E RID: 78
			// (get) Token: 0x0600029A RID: 666 RVA: 0x00011A2C File Offset: 0x0000FC2C
			// (set) Token: 0x0600029B RID: 667 RVA: 0x00011A34 File Offset: 0x0000FC34
			public ushort id
			{
				get
				{
					return this._id;
				}
				set
				{
					this._id = value;
					Action onIdUpdated = this.OnIdUpdated;
					if (onIdUpdated == null)
					{
						return;
					}
					onIdUpdated.Invoke();
				}
			}

			// Token: 0x0400022B RID: 555
			public Transform transform0;

			// Token: 0x0400022C RID: 556
			public Transform transform1;

			// Token: 0x0400022D RID: 557
			public CollisionScene.ShapeType shapeType;

			// Token: 0x0400022E RID: 558
			public Vector3 center;

			// Token: 0x0400022F RID: 559
			public float radius;

			// Token: 0x04000230 RID: 560
			public float height;

			// Token: 0x04000231 RID: 561
			public Vector3 axis = Vector3.up;

			// Token: 0x04000232 RID: 562
			public float maxSize = float.MaxValue;

			// Token: 0x04000233 RID: 563
			public bool isReceiver;

			// Token: 0x04000234 RID: 564
			public bool isCollider;

			// Token: 0x04000235 RID: 565
			public Action<CollisionScene.Shape> OnEnter;

			// Token: 0x04000236 RID: 566
			public Action<CollisionScene.Shape> OnExit;

			// Token: 0x04000237 RID: 567
			public Action OnIdUpdated;

			// Token: 0x04000238 RID: 568
			private ushort _id = CollisionScene.Shape.NullId;

			// Token: 0x04000239 RID: 569
			public MonoBehaviour component;

			// Token: 0x0400023A RID: 570
			public static readonly ushort NullId = ushort.MaxValue;
		}

		// Token: 0x02000048 RID: 72
		[BurstCompile]
		public struct ShapeData
		{
			// Token: 0x0600029E RID: 670 RVA: 0x00011A84 File Offset: 0x0000FC84
			internal CollisionShapes.CollisionShape ToCollisionShape()
			{
				return new CollisionShapes.CollisionShape
				{
					shapeType = this.shapeType,
					pos0 = this.outPos0,
					pos1 = this.outPos1,
					radius = this.outRadius
				};
			}

			// Token: 0x0600029F RID: 671 RVA: 0x00011AD0 File Offset: 0x0000FCD0
			public float3 GetMidpoint()
			{
				CollisionScene.ShapeType shapeType = this.shapeType;
				if (shapeType == CollisionScene.ShapeType.AABB || shapeType - CollisionScene.ShapeType.Capsule <= 1)
				{
					return (this.outPos0 + this.outPos1) * 0.5f;
				}
				return this.outPos0;
			}

			// Token: 0x060002A0 RID: 672 RVA: 0x00011B10 File Offset: 0x0000FD10
			public float3 GetClosestPoint(float3 point)
			{
				switch (this.shapeType)
				{
				case CollisionScene.ShapeType.AABB:
					return this.ToCollisionShape().ToAABB().ClosestPoint(point);
				case CollisionScene.ShapeType.Sphere:
					return this.ToCollisionShape().ToSphere().ClosestPoint(point);
				case CollisionScene.ShapeType.Capsule:
				case CollisionScene.ShapeType.Finger:
					return this.ToCollisionShape().ToCapsule().ClosestPoint(point);
				case CollisionScene.ShapeType.Plane:
					return this.ToCollisionShape().ToPlane().ClosestPoint(point);
				default:
					return point;
				}
			}

			// Token: 0x060002A1 RID: 673 RVA: 0x00011BA8 File Offset: 0x0000FDA8
			public float3 CalcVelocityAtPoint(float3 point)
			{
				float3 @float = this.velocity;
				if (math.abs(this.angularSpeed) > 1E-45f && math.any(this.angularNormal))
				{
					float3 float2 = point - this.GetMidpoint();
					float3 float3 = math.cross(math.normalize(float2), this.angularNormal);
					@float += float3 * math.length(float2) * (this.angularSpeed * 0.017453292f);
				}
				return @float;
			}

			// Token: 0x060002A2 RID: 674 RVA: 0x00011C20 File Offset: 0x0000FE20
			[BurstCompile]
			public void UpdateShape(TransformAccess transform, TransformAccess parentTransform)
			{
				switch (this.shapeType)
				{
				case CollisionScene.ShapeType.AABB:
					this.bounds.size = Vector3.Min(this.bounds.size, new Vector3(this.maxSize, this.maxSize, this.maxSize));
					this.outPos0 = this.bounds.min;
					this.outPos1 = this.bounds.max;
					return;
				case CollisionScene.ShapeType.Sphere:
				{
					if (!transform.isValid)
					{
						return;
					}
					float num = math.cmax(transform.localToWorldMatrix.lossyScale);
					this.outPos0 = (float3)transform.position + math.rotate(transform.rotation, this.center * num);
					this.outRadius = math.min(this.radius * num, this.maxSize * 0.5f);
					this.bounds = new Bounds(this.outPos0, Vector3.one * this.outRadius * 2f);
					return;
				}
				case CollisionScene.ShapeType.Capsule:
				{
					if (!transform.isValid)
					{
						return;
					}
					float num2 = math.cmax(transform.localToWorldMatrix.lossyScale);
					Vector3 vector = this.center * num2;
					float num3 = math.min(this.radius * num2, this.maxSize * 0.5f);
					float num4 = math.min(this.height * num2, this.maxSize);
					Vector3 vector2 = this.axis * math.max(0f, num4 * 0.5f - num3);
					this.outPos0 = (float3)transform.position + math.rotate(transform.rotation, vector - vector2);
					this.outPos1 = (float3)transform.position + math.rotate(transform.rotation, vector + vector2);
					this.outRadius = num3;
					float3 @float = new float3(1f, 1f, 1f) * this.outRadius;
					float3 float2 = math.min(this.outPos0 - @float, this.outPos1 - @float);
					float3 float3 = math.max(this.outPos0 + @float, this.outPos1 + @float);
					this.bounds = new Bounds((float3 + float2) * 0.5f, float3 - float2);
					return;
				}
				case CollisionScene.ShapeType.Finger:
				{
					if (!transform.isValid || !parentTransform.isValid)
					{
						return;
					}
					float num5 = math.cmax(transform.localToWorldMatrix.lossyScale);
					this.outPos0 = parentTransform.position;
					this.outPos1 = (float3)transform.position + math.rotate(transform.rotation, this.center * num5);
					float num6 = math.min(math.distance(this.outPos0, this.outPos1), this.maxSize);
					this.outPos0 = this.outPos1 + math.normalizesafe(this.outPos0 - this.outPos1, default(float3)) * num6;
					this.outRadius = math.min(this.radius * num5, this.maxSize * 0.5f);
					float3 float4 = new float3(1f, 1f, 1f) * this.outRadius;
					float3 float5 = math.min(this.outPos0 - float4, this.outPos1 - float4);
					float3 float6 = math.max(this.outPos0 + float4, this.outPos1 + float4);
					this.bounds = new Bounds((float6 + float5) * 0.5f, float6 - float5);
					return;
				}
				case CollisionScene.ShapeType.Plane:
				{
					if (!transform.isValid)
					{
						return;
					}
					float num7 = math.cmax(transform.localToWorldMatrix.lossyScale);
					this.outPos0 = (float3)transform.position + math.rotate(transform.rotation, this.center * num7);
					this.outPos1 = math.mul(transform.rotation, this.axis);
					this.bounds = new Bounds(transform.position, Vector3.zero);
					return;
				}
				default:
					return;
				}
			}

			// Token: 0x060002A3 RID: 675 RVA: 0x00012108 File Offset: 0x00010308
			public void UpdateVelocity(TransformAccess transform, float deltaTime)
			{
				if (!transform.isValid)
				{
					return;
				}
				float num = 1f / deltaTime;
				float3 @float = ((float3)transform.position - this.lastPosition) * num;
				this.velocity = Vector3.Lerp(this.velocity, @float, deltaTime * 30f);
				float num2;
				Vector3 vector;
				Quaternion.FromToRotation(math.rotate(transform.rotation, new float3(0f, 1f, 0f)), math.rotate(this.lastRotation, new float3(0f, 1f, 0f))).ToAngleAxis(out num2, out vector);
				this.angularNormal = Vector3.Lerp(this.angularNormal, vector, deltaTime * 30f);
				this.angularSpeed = Mathf.Lerp(this.angularSpeed, num2 * num, deltaTime * 30f);
				this.lastPosition = transform.position;
				this.lastRotation = transform.rotation;
			}

			// Token: 0x0400023B RID: 571
			public CollisionScene.ShapeType shapeType;

			// Token: 0x0400023C RID: 572
			public Vector3 center;

			// Token: 0x0400023D RID: 573
			public float radius;

			// Token: 0x0400023E RID: 574
			public float height;

			// Token: 0x0400023F RID: 575
			public Vector3 axis;

			// Token: 0x04000240 RID: 576
			public float maxSize;

			// Token: 0x04000241 RID: 577
			public bool isReceiver;

			// Token: 0x04000242 RID: 578
			public bool isCollider;

			// Token: 0x04000243 RID: 579
			public Bounds bounds;

			// Token: 0x04000244 RID: 580
			public Vector3Int boundsMin;

			// Token: 0x04000245 RID: 581
			public Vector3Int boundsMax;

			// Token: 0x04000246 RID: 582
			public Vector3Int nextBoundsMin;

			// Token: 0x04000247 RID: 583
			public Vector3Int nextBoundsMax;

			// Token: 0x04000248 RID: 584
			public float3 velocity;

			// Token: 0x04000249 RID: 585
			public float3 angularNormal;

			// Token: 0x0400024A RID: 586
			public float angularSpeed;

			// Token: 0x0400024B RID: 587
			public float3 lastPosition;

			// Token: 0x0400024C RID: 588
			public quaternion lastRotation;

			// Token: 0x0400024D RID: 589
			public float3 outPos0;

			// Token: 0x0400024E RID: 590
			public float3 outPos1;

			// Token: 0x0400024F RID: 591
			public float outRadius;

			// Token: 0x04000250 RID: 592
			public int collisionCount;

			// Token: 0x04000251 RID: 593
			private const float VelocityLerpSpeed = 30f;
		}

		// Token: 0x02000049 RID: 73
		internal struct CollisionEvent
		{
			// Token: 0x04000252 RID: 594
			public bool found;

			// Token: 0x04000253 RID: 595
			public int receiver;

			// Token: 0x04000254 RID: 596
			public int collider;
		}

		// Token: 0x0200004A RID: 74
		private struct ReadTransformJob : IJobParallelForTransform
		{
			// Token: 0x060002A4 RID: 676 RVA: 0x0001222F File Offset: 0x0001042F
			public void Execute(int index, TransformAccess transform)
			{
				this.transformData[index] = transform;
			}

			// Token: 0x04000255 RID: 597
			public NativeArray<TransformAccess> transformData;
		}

		// Token: 0x0200004B RID: 75
		private struct UpdateShapePositionsJob : IJobParallelFor
		{
			// Token: 0x060002A5 RID: 677 RVA: 0x00012240 File Offset: 0x00010440
			public void Execute(int index)
			{
				ushort num = this.activeShapes[index];
				CollisionScene.ShapeData shapeData = this.shapeData[(int)num];
				TransformAccess transform = this.GetTransform((int)num, 0);
				TransformAccess transform2 = this.GetTransform((int)num, 1);
				shapeData.UpdateShape(transform, transform2);
				this.shapeData[(int)num] = shapeData;
			}

			// Token: 0x060002A6 RID: 678 RVA: 0x00012290 File Offset: 0x00010490
			private TransformAccess GetTransform(int id, int index)
			{
				int num = this.transformLookup[id * 2 + index];
				if (num >= 0)
				{
					return this.transformData[num];
				}
				return default(TransformAccess);
			}

			// Token: 0x04000256 RID: 598
			public static int SHAPE_BATCH_COUNT = 8;

			// Token: 0x04000257 RID: 599
			[ReadOnly]
			public NativeList<ushort> activeShapes;

			// Token: 0x04000258 RID: 600
			[NativeDisableParallelForRestriction]
			public NativeArray<CollisionScene.ShapeData> shapeData;

			// Token: 0x04000259 RID: 601
			[ReadOnly]
			public NativeArray<TransformAccess> transformData;

			// Token: 0x0400025A RID: 602
			[ReadOnly]
			public NativeArray<int> transformLookup;
		}
	}
}
