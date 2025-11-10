using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace VRC.Dynamics
{
	// Token: 0x02000033 RID: 51
	[AddComponentMenu("")]
	public abstract class VRCPhysBoneBase : MonoBehaviour, IParameterSetup, INetworkID, PhysBoneManager.IJobSortable
	{
		// Token: 0x060001FB RID: 507 RVA: 0x0000EC8C File Offset: 0x0000CE8C
		public Transform GetRootTransform()
		{
			if (!(this.rootTransform != null))
			{
				return base.transform;
			}
			return this.rootTransform;
		}

		// Token: 0x060001FC RID: 508 RVA: 0x0000ECA9 File Offset: 0x0000CEA9
		public bool IsCollisionAllowed(int sourceId)
		{
			return this.collisionFilter.IsAllowed(this.allowCollision, sourceId, this.playerId);
		}

		// Token: 0x060001FD RID: 509 RVA: 0x0000ECC3 File Offset: 0x0000CEC3
		public bool IsGrabAllowed(int sourceId)
		{
			return !this.GetIsGrabbed() && this.grabFilter.IsAllowed(this.allowGrabbing, sourceId, this.playerId);
		}

		// Token: 0x060001FE RID: 510 RVA: 0x0000ECE7 File Offset: 0x0000CEE7
		public bool IsPoseAllowed(int sourceId)
		{
			return this.poseFilter.IsAllowed(this.allowPosing, sourceId, this.playerId);
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060001FF RID: 511 RVA: 0x0000ED01 File Offset: 0x0000CF01
		public Transform SortingBaseTransform
		{
			get
			{
				return this.GetRootTransform();
			}
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000ED0C File Offset: 0x0000CF0C
		public void GetKnownDependencies(List<PhysBoneManager.IJobSortable> dependencies)
		{
			foreach (VRCPhysBoneColliderBase vrcphysBoneColliderBase in this.colliders)
			{
				if (vrcphysBoneColliderBase != null)
				{
					dependencies.Add(vrcphysBoneColliderBase);
				}
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000201 RID: 513 RVA: 0x0000ED68 File Offset: 0x0000CF68
		// (set) Token: 0x06000202 RID: 514 RVA: 0x0000ED70 File Offset: 0x0000CF70
		public int ExecutionGroup { get; set; } = -1;

		// Token: 0x06000203 RID: 515 RVA: 0x0000ED79 File Offset: 0x0000CF79
		private void Reset()
		{
			this.version = VRCPhysBoneBase.LatestVersion;
			this.ignoreOtherPhysBones = true;
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000ED90 File Offset: 0x0000CF90
		private void Start()
		{
			if (PhysBoneManager.Inst.IsSDK)
			{
				byte[] array = Guid.NewGuid().ToByteArray();
				this.chainId = new ChainId(BitConverter.ToUInt64(array, 0), 0UL);
				PhysBoneManager.Inst.AddPhysBone(this);
			}
			this.InitParameters();
			this.InitColliders();
		}

		// Token: 0x06000205 RID: 517 RVA: 0x0000EDE2 File Offset: 0x0000CFE2
		private void OnEnable()
		{
			this.SetIsPosed(false);
			this.SetIsGrabbed(false);
			if (this.chainId != ChainId.Null)
			{
				PhysBoneManager.Inst.AddPhysBone(this);
			}
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000EE0F File Offset: 0x0000D00F
		private void OnDisable()
		{
			if (this.chainId != ChainId.Null)
			{
				PhysBoneManager.Inst.RemovePhysBone(this);
				this.collisionRecords.Clear();
				this.OnCollidersUpdated();
			}
		}

		// Token: 0x06000207 RID: 519 RVA: 0x0000EE3F File Offset: 0x0000D03F
		public void InitParameters()
		{
			if (this.hasInitParams)
			{
				return;
			}
			this.hasInitParams = true;
			Action<VRCPhysBoneBase> onInitialize = VRCPhysBoneBase.OnInitialize;
			if (onInitialize == null)
			{
				return;
			}
			onInitialize.Invoke(this);
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0000EE64 File Offset: 0x0000D064
		private void InitColliders()
		{
			foreach (VRCPhysBoneColliderBase vrcphysBoneColliderBase in this.colliders)
			{
				if (vrcphysBoneColliderBase != null)
				{
					if (vrcphysBoneColliderBase.shape == null)
					{
						vrcphysBoneColliderBase.InitShape();
					}
					CollisionScene.Shape shape = vrcphysBoneColliderBase.shape;
					shape.OnIdUpdated = (Action)Delegate.Combine(shape.OnIdUpdated, new Action(this.OnCollidersUpdated));
				}
			}
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000EEF0 File Offset: 0x0000D0F0
		public void InitTransforms(bool force = false)
		{
			if (this.hasInitTransform && !force)
			{
				return;
			}
			this.hasInitTransform = true;
			this.maxBoneChainIndex = 0;
			this.bones.Clear();
			Transform transform = this.GetRootTransform();
			VRCPhysBoneBase[] componentsInChildren = transform.root.GetComponentsInChildren<VRCPhysBoneBase>(true);
            List<Transform> otherRootTransforms = new List<Transform>(componentsInChildren.Length);
			foreach (VRCPhysBoneBase vrcphysBoneBase in componentsInChildren)
			{
				if (!(vrcphysBoneBase == this))
				{
					otherRootTransforms.Add(vrcphysBoneBase.GetRootTransform());
				}
			}
			GetTransforms(transform, -1, 0);

			// Token: 0x06000222 RID: 546 RVA: 0x0000F490 File Offset: 0x0000D690
			void GetTransforms(Transform transform, int parentIndex, int boneChainIndex)
			{
				int count = this.bones.Count;
				VRCPhysBoneBase.Bone bone = default(VRCPhysBoneBase.Bone);
				bone.transform = transform;
				bone.childIndex = -1;
				bone.parentIndex = parentIndex;
				bone.boneChainIndex = boneChainIndex;
				bone.restPosition = transform.localPosition;
				bone.restRotation = transform.localRotation;
				bone.restScale = transform.localScale;
				bone.localGravityDirection = transform.InverseTransformDirection(Vector3.down);
				if (boneChainIndex > this.maxBoneChainIndex)
				{
					this.maxBoneChainIndex = boneChainIndex;
				}
				List<int> list = new List<int>();
				for (int i = 0; i < transform.childCount; i++)
				{
					Transform child = transform.GetChild(i);
					if (!this.ignoreTransforms.Contains(child) && (!this.ignoreOtherPhysBones || !otherRootTransforms.Contains(child)))
					{
						list.Add(i);
						bone.averageChildPos += child.localPosition;
					}
				}
				bone.childCount = list.Count;
				bone.childIndex = ((list.Count > 0) ? (count + 1) : -1);
				if (bone.childCount > 1)
				{
					bone.averageChildPos /= (float)bone.childCount;
				}
				this.bones.Add(bone);
				foreach (int num in list)
				{
					GetTransforms(transform.GetChild(num), count, boneChainIndex + 1);
				}
			}
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000EF90 File Offset: 0x0000D190
		public void ResetTransformsToRestPosition()
		{
			foreach (VRCPhysBoneBase.Bone bone in this.bones)
			{
				bone.transform.localPosition = bone.restPosition;
				bone.transform.localRotation = bone.restRotation;
				bone.transform.localScale = bone.restScale;
			}
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000F010 File Offset: 0x0000D210
		public float CalcBoneRatio(int index)
		{
            //int num = this.maxBoneChainIndex + (((this.endpointPosition != Vector3.zero) > false) ? 1 : 0) - 1;
            int num = this.maxBoneChainIndex + ((this.endpointPosition != Vector3.zero) ? 1 : 0) - 1;
            if (num <= 0)
			{
				return 0f;
			}
			return Mathf.Clamp01((float)index / (float)num);
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000F04E File Offset: 0x0000D24E
		public float CalcTransformRatio(int index)
		{
            //return Mathf.Clamp01((float)index / (float)(this.maxBoneChainIndex + (((this.endpointPosition != Vector3.zero) > false) ? 1 : 0)));
            return Mathf.Clamp01((float)index / (float)(this.maxBoneChainIndex + ((this.endpointPosition != Vector3.zero) ? 1 : 0)));
        }

		// Token: 0x0600020D RID: 525 RVA: 0x0000F073 File Offset: 0x0000D273
		public float CalcRadius(float t)
		{
			return this.SafeEvaluate(this.radiusCurve, t) * this.radius;
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0000F089 File Offset: 0x0000D289
		public float CalcPull(float t)
		{
			return Mathf.Clamp01(this.SafeEvaluate(this.pullCurve, t) * this.pull);
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0000F0A4 File Offset: 0x0000D2A4
		public float CalcSpring(float t)
		{
			return Mathf.Clamp01(this.SafeEvaluate(this.springCurve, t) * this.spring);
		}

		// Token: 0x06000210 RID: 528 RVA: 0x0000F0BF File Offset: 0x0000D2BF
		public float CalcStiffness(float t)
		{
			return Mathf.Clamp01(this.SafeEvaluate(this.stiffnessCurve, t) * this.stiffness);
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0000F0DA File Offset: 0x0000D2DA
		public float CalcImmobile(float t)
		{
			return Mathf.Clamp01(this.SafeEvaluate(this.immobileCurve, t) * this.immobile);
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0000F0F8 File Offset: 0x0000D2F8
		public Vector2 CalcMaxAngle(float t)
		{
			return new Vector2(Mathf.Max(0f, this.SafeEvaluate(this.maxAngleXCurve, t) * this.maxAngleX), Mathf.Max(0f, this.SafeEvaluate(this.maxAngleZCurve, t) * this.maxAngleZ));
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0000F148 File Offset: 0x0000D348
		public Vector3 CalcLimitRotation(float t)
		{
			return new Vector3(this.SafeEvaluate(this.limitRotationXCurve, t) * this.limitRotation.x, this.SafeEvaluate(this.limitRotationYCurve, t) * this.limitRotation.y, this.SafeEvaluate(this.limitRotationZCurve, t) * this.limitRotation.z);
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0000F1A5 File Offset: 0x0000D3A5
		public float CalcMaxStretch(float t)
		{
			return Mathf.Max(this.SafeEvaluate(this.maxStretchCurve, t) * this.maxStretch, 0f);
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0000F1C5 File Offset: 0x0000D3C5
		public float CalcStretchMotion(float t)
		{
			return Mathf.Clamp01(this.SafeEvaluate(this.stretchMotionCurve, t) * this.stretchMotion);
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000F1E0 File Offset: 0x0000D3E0
		public float CalcMaxSquish(float t)
		{
			return Mathf.Clamp01(this.SafeEvaluate(this.maxSquishCurve, t) * this.maxSquish);
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000F1FB File Offset: 0x0000D3FB
		public float CalcGravity(float t)
		{
			return this.SafeEvaluate(this.gravityCurve, t) * this.gravity;
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0000F211 File Offset: 0x0000D411
		public float CalcGravityFalloff(float t)
		{
			return Mathf.Clamp01(this.SafeEvaluate(this.gravityFalloffCurve, t) * this.gravityFalloff);
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0000F22C File Offset: 0x0000D42C
		private float SafeEvaluate(AnimationCurve curve, float t)
		{
			if (curve != null && curve.length > 0)
			{
				return curve.Evaluate(t);
			}
			return 1f;
		}

		// Token: 0x0600021A RID: 538 RVA: 0x0000F248 File Offset: 0x0000D448
		public void OnShapeEnter(CollisionScene.Shape other)
		{
			VRCPhysBoneColliderBase vrcphysBoneColliderBase = other.component as VRCPhysBoneColliderBase;
			if (vrcphysBoneColliderBase != null)
			{
				if (!this.IsCollisionAllowed(vrcphysBoneColliderBase.playerId))
				{
					return;
				}
				if (VRCPhysBoneBase.OnVerifyCollision != null && !VRCPhysBoneBase.OnVerifyCollision.Invoke(this.playerId, vrcphysBoneColliderBase.playerId))
				{
					return;
				}
				VRCPhysBoneBase.CollisionRecord collisionRecord = default(VRCPhysBoneBase.CollisionRecord);
				collisionRecord.shape = other;
				collisionRecord.collider = vrcphysBoneColliderBase;
				this.collisionRecords.Add(collisionRecord);
				this.OnCollidersUpdated();
			}
		}

		// Token: 0x0600021B RID: 539 RVA: 0x0000F2C4 File Offset: 0x0000D4C4
		public void OnShapeExit(CollisionScene.Shape other)
		{
			for (int i = 0; i < this.collisionRecords.Count; i++)
			{
				if (this.collisionRecords[i].shape == other)
				{
					this.collisionRecords.RemoveAt(i);
					this.OnCollidersUpdated();
					return;
				}
			}
		}

		// Token: 0x0600021C RID: 540 RVA: 0x0000F30E File Offset: 0x0000D50E
		public void OnCollidersUpdated()
		{
			this.collidersHaveUpdated = true;
		}

		// Token: 0x0600021D RID: 541 RVA: 0x0000F317 File Offset: 0x0000D517
		public bool GetIsGrabbed()
		{
			return this.param_IsGrabbedValue;
		}

		// Token: 0x0600021E RID: 542 RVA: 0x0000F31F File Offset: 0x0000D51F
		public void SetIsGrabbed(bool value)
		{
			this.param_IsGrabbedValue = value;
			if (this.param_IsGrabbed != null)
			{
				this.param_IsGrabbed.boolVal = value;
			}
		}

		// Token: 0x0600021F RID: 543 RVA: 0x0000F33C File Offset: 0x0000D53C
		public void SetIsPosed(bool value)
		{
			this.param_IsPosedValue = value;
			if (this.param_IsPosed != null)
			{
				this.param_IsPosed.boolVal = value;
			}
		}

		

		// Token: 0x04000163 RID: 355
		public bool foldout_transforms = true;

		// Token: 0x04000164 RID: 356
		public bool foldout_forces = true;

		// Token: 0x04000165 RID: 357
		public bool foldout_collision = true;

		// Token: 0x04000166 RID: 358
		public bool foldout_stretchsquish = true;

		// Token: 0x04000167 RID: 359
		public bool foldout_limits = true;

		// Token: 0x04000168 RID: 360
		public bool foldout_grabpose = true;

		// Token: 0x04000169 RID: 361
		public bool foldout_options = true;

		// Token: 0x0400016A RID: 362
		public bool foldout_gizmos;

		// Token: 0x0400016B RID: 363
		public const string PARAM_ISGRABBED = "_IsGrabbed";

		// Token: 0x0400016C RID: 364
		public const string PARAM_ISPOSED = "_IsPosed";

		// Token: 0x0400016D RID: 365
		public const string PARAM_ANGLE = "_Angle";

		// Token: 0x0400016E RID: 366
		public const string PARAM_STRETCH = "_Stretch";

		// Token: 0x0400016F RID: 367
		public const string PARAM_SQUISH = "_Squish";

		// Token: 0x04000170 RID: 368
		public VRCPhysBoneBase.Version version;

		// Token: 0x04000171 RID: 369
		public static VRCPhysBoneBase.Version LatestVersion = VRCPhysBoneBase.Version.Version_1_1;

		// Token: 0x04000172 RID: 370
		[Tooltip("Determines how forces are applied.  Certain kinds of motion may require using a specific integration type.")]
		public VRCPhysBoneBase.IntegrationType integrationType;

		// Token: 0x04000173 RID: 371
		[Tooltip("The transform where this component begins.  If left blank, we assume we start at this game object.")]
		public Transform rootTransform;

		// Token: 0x04000174 RID: 372
		[Tooltip("List of ignored transforms that shouldn't be affected by this component.  Ignored transforms automatically include any of that transform's children.")]
		[NonReorderable]
		public List<Transform> ignoreTransforms = new List<Transform>();

		// Token: 0x04000175 RID: 373
		[Tooltip("When enabled, automatically ignore transforms targeted by another physbone component. These are determined at initialization and regardless if the components are enabled or disabled.")]
		public bool ignoreOtherPhysBones;

		// Token: 0x04000176 RID: 374
		[Tooltip("Vector used to create additional bones at each endpoint of the chain. Only used if the value is non-zero.")]
		public Vector3 endpointPosition = Vector3.zero;

		// Token: 0x04000177 RID: 375
		[Tooltip("Determines how transforms with multiple children are handled. By default those transforms are ignored.")]
		public VRCPhysBoneBase.MultiChildType multiChildType;

		// Token: 0x04000178 RID: 376
		[Tooltip("Amount of force used to return bones to their rest position.")]
		[Range(0f, 1f)]
		public float pull = 0.2f;

		// Token: 0x04000179 RID: 377
		public AnimationCurve pullCurve;

		// Token: 0x0400017A RID: 378
		[Tooltip("Amount bones will wobble when trying to reach their rest position.")]
		[Range(0f, 1f)]
		public float spring = 0.2f;

		// Token: 0x0400017B RID: 379
		public AnimationCurve springCurve;

		// Token: 0x0400017C RID: 380
		[Tooltip("Amount bones will try and stay at their current orientation.")]
		[Range(0f, 1f)]
		public float stiffness = 0.2f;

		// Token: 0x0400017D RID: 381
		public AnimationCurve stiffnessCurve;

		// Token: 0x0400017E RID: 382
		[Tooltip("Amount of gravity applied to bones.  Positive value pulls bones down, negative pulls upwards.")]
		[Range(-1f, 1f)]
		public float gravity;

		// Token: 0x0400017F RID: 383
		public AnimationCurve gravityCurve;

		// Token: 0x04000180 RID: 384
		[Tooltip("Reduces gravity while bones are at their rest orientation.  Gravity will increase as bones rotate away from their rest orientation, reaching full gravity at 90 degress from rest.")]
		[Range(0f, 1f)]
		public float gravityFalloff;

		// Token: 0x04000181 RID: 385
		public AnimationCurve gravityFalloffCurve;

		// Token: 0x04000182 RID: 386
		[Tooltip("Determines how immobile is calculated.\n\nAll Motion - Reduces any motion as calculated from the root transform's parent.World - Reduces positional movement from locomotion, any movement due to animations or IK still affect bones normally.\n\n")]
		public VRCPhysBoneBase.ImmobileType immobileType;

		// Token: 0x04000183 RID: 387
		[Tooltip("Reduces the effect movement has on bones. The greater the value the less motion affects the chain as determined by the Immobile Type.")]
		[Range(0f, 1f)]
		public float immobile;

		// Token: 0x04000184 RID: 388
		public AnimationCurve immobileCurve;

		// Token: 0x04000185 RID: 389
		[Tooltip("Allows collision with colliders other than the ones specified on this component.  Currently the only other colliders are each player's hands as defined by their avatar.")]
		public VRCPhysBoneBase.AdvancedBool allowCollision = VRCPhysBoneBase.AdvancedBool.True;

		// Token: 0x04000186 RID: 390
		public VRCPhysBoneBase.PermissionFilter collisionFilter = new VRCPhysBoneBase.PermissionFilter(true);

		// Token: 0x04000187 RID: 391
		[Tooltip("Collision radius around each bone.  Used for both collision and grabbing.")]
		public float radius;

		// Token: 0x04000188 RID: 392
		public AnimationCurve radiusCurve;

		// Token: 0x04000189 RID: 393
		[Tooltip("List of colliders that specifically collide with these bones.")]
		[NonReorderable]
		public List<VRCPhysBoneColliderBase> colliders = new List<VRCPhysBoneColliderBase>();

		// Token: 0x0400018A RID: 394
		[Tooltip("Type of angular limit applied to each bone.")]
		public VRCPhysBoneBase.LimitType limitType;

		// Token: 0x0400018B RID: 395
		[Tooltip("Maximum angle each bone can rotate from its rest position.")]
		[Range(0f, 180f)]
		public float maxAngleX = 45f;

		// Token: 0x0400018C RID: 396
		public AnimationCurve maxAngleXCurve;

		// Token: 0x0400018D RID: 397
		[Tooltip("Maximum angle each bone can rotate from its rest position.")]
		[Range(0f, 90f)]
		public float maxAngleZ = 45f;

		// Token: 0x0400018E RID: 398
		public AnimationCurve maxAngleZCurve;

		// Token: 0x0400018F RID: 399
		[Tooltip("Rotates the angular limits on each axis.")]
		public Vector3 limitRotation;

		// Token: 0x04000190 RID: 400
		public AnimationCurve limitRotationXCurve;

		// Token: 0x04000191 RID: 401
		public AnimationCurve limitRotationYCurve;

		// Token: 0x04000192 RID: 402
		public AnimationCurve limitRotationZCurve;

		// Token: 0x04000193 RID: 403
		[NonSerialized]
		public Vector3 staticFreezeAxis;

		// Token: 0x04000194 RID: 404
		[Tooltip("Allows players to grab the bones.")]
		[FormerlySerializedAs("isGrabbable")]
		public VRCPhysBoneBase.AdvancedBool allowGrabbing = VRCPhysBoneBase.AdvancedBool.True;

		// Token: 0x04000195 RID: 405
		public VRCPhysBoneBase.PermissionFilter grabFilter = new VRCPhysBoneBase.PermissionFilter(true);

		// Token: 0x04000196 RID: 406
		[Tooltip("Allows players to pose the bones after grabbing.")]
		[FormerlySerializedAs("isPoseable")]
		public VRCPhysBoneBase.AdvancedBool allowPosing = VRCPhysBoneBase.AdvancedBool.True;

		// Token: 0x04000197 RID: 407
		public VRCPhysBoneBase.PermissionFilter poseFilter = new VRCPhysBoneBase.PermissionFilter(true);

		// Token: 0x04000198 RID: 408
		[Tooltip("When a bone is grabbed it will snap to the hand grabbing it.")]
		public bool snapToHand;

		// Token: 0x04000199 RID: 409
		[Tooltip("Controls how grabbed bones move.\nA value of zero results in bones using pull & spring to reach the grabbed position.\nA value of one results in bones immediately moving to the grabbed position.")]
		[Range(0f, 1f)]
		public float grabMovement = 0.5f;

		// Token: 0x0400019A RID: 410
		[Tooltip("Maximum amount the bones can stretch.  This value is a multiple of the original bone length.")]
		public float maxStretch;

		// Token: 0x0400019B RID: 411
		public AnimationCurve maxStretchCurve;

		// Token: 0x0400019C RID: 412
		[Tooltip("Maximum amount the bones can shrink.  This value is a multiple of the original bone length.")]
		[Range(0f, 1f)]
		public float maxSquish;

		// Token: 0x0400019D RID: 413
		public AnimationCurve maxSquishCurve;

		// Token: 0x0400019E RID: 414
		[Tooltip("The amount motion will affect the stretch/squish of the bones.  A value of zero means bones will only stretch/squish as a result of grabbing or collisions.")]
		[Range(0f, 1f)]
		public float stretchMotion;

		// Token: 0x0400019F RID: 415
		public AnimationCurve stretchMotionCurve;

		// Token: 0x040001A0 RID: 416
		[Tooltip("Allows bone transforms to be animated.  Each frame bone rest position will be updated according to what was animated.")]
		public bool isAnimated;

		// Token: 0x040001A1 RID: 417
		[Tooltip("When this component becomes disabled, the bones will automatially reset to their default rest position.")]
		public bool resetWhenDisabled;

		// Token: 0x040001A2 RID: 418
		[Tooltip("Keyname used to provide multiple parameters to the avatar controller.")]
		public string parameter;

		// Token: 0x040001A3 RID: 419
		public bool showGizmos = true;

		// Token: 0x040001A4 RID: 420
		[Range(0f, 1f)]
		public float boneOpacity = 0.5f;

		// Token: 0x040001A5 RID: 421
		[Range(0f, 1f)]
		public float limitOpacity = 0.5f;

		// Token: 0x040001A6 RID: 422
		[NonSerialized]
		public bool configHasUpdated;

		// Token: 0x040001A7 RID: 423
		[NonSerialized]
		public List<VRCPhysBoneBase.Bone> bones = new List<VRCPhysBoneBase.Bone>();

		// Token: 0x040001A8 RID: 424
		[NonSerialized]
		public int maxBoneChainIndex;

		// Token: 0x040001A9 RID: 425
		[NonSerialized]
		public ChainId chainId = ChainId.Null;

		// Token: 0x040001AA RID: 426
		[NonSerialized]
		public Action OnNeedsNetworkSync;

		// Token: 0x040001AB RID: 427
		[NonSerialized]
		public int playerId = -1;

		// Token: 0x040001AC RID: 428
		[NonSerialized]
		public int netId;

		// Token: 0x040001AD RID: 429
		[NonSerialized]
		public int netSubId;

		// Token: 0x040001AE RID: 430
		[NonSerialized]
		public bool collidersHaveUpdated;

		// Token: 0x040001AF RID: 431
		[NonSerialized]
		public Transform worldImmobileTransform;

		// Token: 0x040001B0 RID: 432
		[NonSerialized]
		public PhysBoneManager.Grab grab;

		// Token: 0x040001B1 RID: 433
		[NonSerialized]
		internal PhysBoneRoot root;

		// Token: 0x040001B3 RID: 435
		[Obsolete]
		public Action OnPoseUpdated;

		// Token: 0x040001B4 RID: 436
		public static Action<VRCPhysBoneBase> OnInitialize;

		// Token: 0x040001B5 RID: 437
		private bool hasInitParams;

		// Token: 0x040001B6 RID: 438
		private bool hasInitTransform;

		// Token: 0x040001B7 RID: 439
		public static Func<int, int, bool> OnVerifyCollision;

		// Token: 0x040001B8 RID: 440
		[NonSerialized]
		public CollisionScene.Shape shape;

		// Token: 0x040001B9 RID: 441
		[NonSerialized]
		public List<VRCPhysBoneBase.CollisionRecord> collisionRecords = new List<VRCPhysBoneBase.CollisionRecord>();

		// Token: 0x040001BA RID: 442
		[NonSerialized]
		public bool param_IsGrabbedValue;

		// Token: 0x040001BB RID: 443
		[NonSerialized]
		public bool param_IsPosedValue;

		// Token: 0x040001BC RID: 444
		[NonSerialized]
		public float param_AngleValue;

		// Token: 0x040001BD RID: 445
		[NonSerialized]
		public float param_StretchValue;

		// Token: 0x040001BE RID: 446
		[NonSerialized]
		public float param_SquishValue;

		// Token: 0x040001BF RID: 447
		public IAnimParameterAccess param_IsGrabbed;

		// Token: 0x040001C0 RID: 448
		public IAnimParameterAccess param_IsPosed;

		// Token: 0x040001C1 RID: 449
		public IAnimParameterAccess param_Angle;

		// Token: 0x040001C2 RID: 450
		public IAnimParameterAccess param_Stretch;

		// Token: 0x040001C3 RID: 451
		public IAnimParameterAccess param_Squish;

		// Token: 0x040001C4 RID: 452
		[Obsolete]
		public const float MAX_STRETCH = 5f;

		// Token: 0x0200007E RID: 126
		public enum Version
		{
			// Token: 0x0400035B RID: 859
			[InspectorName("Version 1.0")]
			Version_1_0,
			// Token: 0x0400035C RID: 860
			[InspectorName("Version 1.1")]
			Version_1_1
		}

		// Token: 0x0200007F RID: 127
		public enum AdvancedBool
		{
			// Token: 0x0400035E RID: 862
			False,
			// Token: 0x0400035F RID: 863
			True,
			// Token: 0x04000360 RID: 864
			Other
		}

		// Token: 0x02000080 RID: 128
		[Serializable]
		public struct PermissionFilter
		{
			// Token: 0x060002FC RID: 764 RVA: 0x000151C3 File Offset: 0x000133C3
			public PermissionFilter(bool value)
			{
				this.allowSelf = value;
				this.allowOthers = value;
			}

			// Token: 0x060002FD RID: 765 RVA: 0x000151D3 File Offset: 0x000133D3
			public bool IsAllowed(VRCPhysBoneBase.AdvancedBool mainSetting, int idA, int idB)
			{
				switch (mainSetting)
				{
				default:
					return false;
				case VRCPhysBoneBase.AdvancedBool.True:
					return true;
				case VRCPhysBoneBase.AdvancedBool.Other:
					if (idA == idB)
					{
						return this.allowSelf;
					}
					return this.allowOthers;
				}
			}

			// Token: 0x04000361 RID: 865
			public bool allowSelf;

			// Token: 0x04000362 RID: 866
			public bool allowOthers;
		}

		// Token: 0x02000081 RID: 129
		public enum IntegrationType
		{
			// Token: 0x04000364 RID: 868
			Simplified,
			// Token: 0x04000365 RID: 869
			Advanced
		}

		// Token: 0x02000082 RID: 130
		public enum MultiChildType
		{
			// Token: 0x04000367 RID: 871
			Ignore,
			// Token: 0x04000368 RID: 872
			First,
			// Token: 0x04000369 RID: 873
			Average
		}

		// Token: 0x02000083 RID: 131
		public enum ImmobileType
		{
			// Token: 0x0400036B RID: 875
			[InspectorName("All Motion")]
			AllMotion,
			// Token: 0x0400036C RID: 876
			[InspectorName("World (Experimental)")]
			World
		}

		// Token: 0x02000084 RID: 132
		public enum LimitType
		{
			// Token: 0x0400036E RID: 878
			None,
			// Token: 0x0400036F RID: 879
			Angle,
			// Token: 0x04000370 RID: 880
			Hinge,
			// Token: 0x04000371 RID: 881
			Polar
		}

		// Token: 0x02000085 RID: 133
		public struct Bone
		{
			// Token: 0x1700005B RID: 91
			// (get) Token: 0x060002FE RID: 766 RVA: 0x000151FC File Offset: 0x000133FC
			public bool isEndBone
			{
				get
				{
					return this.childCount == 0;
				}
			}

			// Token: 0x04000372 RID: 882
			public Transform transform;

			// Token: 0x04000373 RID: 883
			public int parentIndex;

			// Token: 0x04000374 RID: 884
			public int childIndex;

			// Token: 0x04000375 RID: 885
			public int boneChainIndex;

			// Token: 0x04000376 RID: 886
			public int childCount;

			// Token: 0x04000377 RID: 887
			public Vector3 averageChildPos;

			// Token: 0x04000378 RID: 888
			public Vector3 restPosition;

			// Token: 0x04000379 RID: 889
			public Quaternion restRotation;

			// Token: 0x0400037A RID: 890
			public Vector3 restScale;

			// Token: 0x0400037B RID: 891
			public Vector3 localGravityDirection;

			// Token: 0x0400037C RID: 892
			public bool sphereCollision;
		}

		// Token: 0x02000086 RID: 134
		public struct CollisionRecord
		{
			// Token: 0x0400037D RID: 893
			public CollisionScene.Shape shape;

			// Token: 0x0400037E RID: 894
			public VRCPhysBoneColliderBase collider;
		}
	}
}
