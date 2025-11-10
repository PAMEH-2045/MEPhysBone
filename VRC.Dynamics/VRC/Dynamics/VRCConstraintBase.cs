using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase.Validation.Performance;
using static UnityEngine.GraphicsBuffer;

namespace VRC.Dynamics
{
	// Token: 0x0200000D RID: 13
	[ExecuteInEditMode]
	public abstract class VRCConstraintBase : MonoBehaviour, IDisposable, IVRCConstraint
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000070 RID: 112
		protected abstract VRCConstraintPositionMode PositionMode { get; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000071 RID: 113
		protected abstract VRCConstraintRotationMode RotationMode { get; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000072 RID: 114
		protected abstract VRCConstraintScaleMode ScaleMode { get; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000073 RID: 115 RVA: 0x000052E3 File Offset: 0x000034E3
		public bool AffectsPosition
		{
			get
			{
				return this.PositionMode > VRCConstraintPositionMode.None;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000074 RID: 116 RVA: 0x000052EE File Offset: 0x000034EE
		public bool AffectsRotation
		{
			get
			{
				return this.RotationMode > VRCConstraintRotationMode.None;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000075 RID: 117 RVA: 0x000052F9 File Offset: 0x000034F9
		public bool AffectsScale
		{
			get
			{
				return this.ScaleMode > VRCConstraintScaleMode.None;
			}
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00005304 File Offset: 0x00003504
		public bool IsLocalPostProcessIndependent()
		{
			return this.PositionMode == VRCConstraintPositionMode.None && (this.RotationMode == VRCConstraintRotationMode.None || this.RotationMode == VRCConstraintRotationMode.MatchRotation) && this.ScaleMode == VRCConstraintScaleMode.None;
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000077 RID: 119 RVA: 0x0000532A File Offset: 0x0000352A
		// (set) Token: 0x06000078 RID: 120 RVA: 0x00005332 File Offset: 0x00003532
		internal int NativeIndex { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000079 RID: 121 RVA: 0x0000533B File Offset: 0x0000353B
		public int CachedExecutionGroupIndex
		{
			get
			{
				return this.cachedExecutionGroupIndex;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600007A RID: 122 RVA: 0x00005343 File Offset: 0x00003543
		public int LatestValidExecutionGroupIndex
		{
			get
			{
				return this.latestValidExecutionGroupIndex;
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x0000534B File Offset: 0x0000354B
		internal void SetCachedExecutionGroupIndex(int executionGroupIndex)
		{
			this.cachedExecutionGroupIndex = executionGroupIndex;
			if (executionGroupIndex >= 0)
			{
				this.latestValidExecutionGroupIndex = executionGroupIndex;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600007C RID: 124 RVA: 0x0000535F File Offset: 0x0000355F
		internal Transform DependencyRoot
		{
			get
			{
				if (!(this._assignedDependencyRoot == null))
				{
					return this._assignedDependencyRoot;
				}
				return base.transform.root;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600007D RID: 125 RVA: 0x00005381 File Offset: 0x00003581
		// (set) Token: 0x0600007E RID: 126 RVA: 0x00005389 File Offset: 0x00003589
		public bool DependsOnLocalAvatarProcessing
		{
			get
			{
				return this._dependsOnLocalAvatarProcessing;
			}
			set
			{
				this._dependsOnLocalAvatarProcessing = value;
				this.InvalidatePlayerLoopStage();
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00005398 File Offset: 0x00003598
		public void ActivateConstraint()
		{
			this.Locked = false;
			this.IsActive = true;
			this.TryBakeCurrentOffsets(VRCConstraintBase.BakeOptions.BakeAll);
			this.Locked = true;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x000053B6 File Offset: 0x000035B6
		public void ZeroConstraint()
		{
			this.ApplyZeroOffset();
			this.IsActive = true;
			this.Locked = true;
			VRCAvatarDynamicsScheduler.UpdateConstraints(true);
			this.Locked = false;
			this.TryBakeCurrentOffsets(VRCConstraintBase.BakeOptions.BakeAtRest);
			this.Locked = true;
		}

		// Token: 0x06000081 RID: 129
		protected abstract void ApplyZeroOffset();

		// Token: 0x06000082 RID: 130 RVA: 0x000053E7 File Offset: 0x000035E7
		public void TryBakeCurrentOffsets(VRCConstraintBase.BakeOptions bakeOptions = VRCConstraintBase.BakeOptions.BakeAll)
		{
			if (this.Locked || Application.isPlaying)
			{
				return;
			}
			this.TryBakeCurrentOffsetsRuntime(bakeOptions);
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00005400 File Offset: 0x00003600
		private void TryBakeCurrentOffsetsRuntime(VRCConstraintBase.BakeOptions bakeOptions)
		{
			if (!base.enabled || !this.IsActive)
			{
				return;
			}
			if (this.Sources.Count == 0)
			{
				return;
			}
			Transform effectiveTargetTransform = this.GetEffectiveTargetTransform();
			if (effectiveTargetTransform == null)
			{
				return;
			}
			float num = 0f;
			for (int i = 0; i < this.Sources.Count; i++)
			{
				num += this.Sources[i].Weight;
			}
			VRCConstraintOffsetBaker baker = new VRCConstraintOffsetBaker(effectiveTargetTransform, num, bakeOptions);
			this.AcceptOffsetBaker(baker);
			foreach (Action<VRCConstraintBase> action in this._registeredBakeListeners)
			{
				if (action != null)
				{
					action.Invoke(this);
				}
			}
		}

		// Token: 0x06000084 RID: 132
		internal abstract void AcceptOffsetBaker(VRCConstraintOffsetBaker baker);

		// Token: 0x06000085 RID: 133 RVA: 0x000054C8 File Offset: 0x000036C8
		public void RegisterBakeListener(Action<VRCConstraintBase> listener)
		{
			this._registeredBakeListeners.Add(listener);
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000054D7 File Offset: 0x000036D7
		public void UnRegisterBakeListener(Action<VRCConstraintBase> listener)
		{
			this._registeredBakeListeners.Remove(listener);
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000054E8 File Offset: 0x000036E8
		internal void AssignBinding(IVRCConstraintBinding binding, bool keepBinding)
		{
			if (this._constraintBinding != null)
			{
				Debug.LogError("The VRChat constraint on " + base.name + " is already bound to a Unity constraint and can only be bound to one Unity constraint at a time.", base.gameObject);
				return;
			}
			if (binding != null && binding.ApplicationVrcConstraint != this)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"The subject of this binding (",
					(binding.ApplicationVrcConstraint != null) ? binding.ApplicationVrcConstraint.name : "<NULL!>",
					") does not match this constraint (",
					base.name,
					")"
				}));
				return;
			}
			this._constraintBinding = binding;
			IVRCConstraintBinding constraintBinding = this._constraintBinding;
			if (constraintBinding != null)
			{
				constraintBinding.Synchronize(keepBinding);
			}
			if (!keepBinding)
			{
				this._constraintBinding = null;
			}
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000055A8 File Offset: 0x000037A8
		public IConstraint GetBoundUnityConstraint()
		{
			if (this._constraintBinding == null)
			{
				return null;
			}
			return this._constraintBinding.ApplicationUnityConstraint;
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000055C0 File Offset: 0x000037C0
		internal bool AddToLocalGameObjectOrder()
		{
			if (this._isInLocalGameObjectOrder)
			{
				return false;
			}
			List<VRCConstraintBase> list;
			if (!VRCConstraintBase.OrderInfoPerGameObject.TryGetValue(base.gameObject, out list))
			{
				this._localGameObjectOrder = base.GetInstanceID();
				Dictionary<GameObject, List<VRCConstraintBase>> orderInfoPerGameObject = VRCConstraintBase.OrderInfoPerGameObject;
				GameObject gameObject = base.gameObject;
				List<VRCConstraintBase> list2 = new List<VRCConstraintBase>(3);
				list2.Add(this);
				orderInfoPerGameObject[gameObject] = list2;
				this._isInLocalGameObjectOrder = true;
				return false;
			}
			List<VRCConstraintBase> list3 = list;
			VRCConstraintBase vrcconstraintBase = list3[list3.Count - 1];
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			for (int i = 0; i < list.Count; i++)
			{
				VRCConstraintBase vrcconstraintBase2 = list[i];
				flag |= vrcconstraintBase2.AffectsPosition;
				flag2 |= vrcconstraintBase2.AffectsRotation;
				flag3 |= vrcconstraintBase2.AffectsScale;
			}
			bool flag4 = (this.AffectsPosition && flag) || (this.AffectsRotation && flag2) || (this.AffectsScale && flag3);
			this._localGameObjectOrder = vrcconstraintBase._localGameObjectOrder + 1;
			list.Add(this);
			VRCConstraintBase.OrderInfoPerGameObject[base.gameObject] = list;
			this._isInLocalGameObjectOrder = true;
			if (!flag4)
			{
				return false;
			}
			bool flag5;
			if (this.cachedExecutionGroupIndex >= 0 && vrcconstraintBase.cachedExecutionGroupIndex >= 0)
			{
				flag5 = (this.cachedExecutionGroupIndex > vrcconstraintBase.cachedExecutionGroupIndex);
			}
			else
			{
				flag5 = (this._dependencyTraversalHighestDepth > vrcconstraintBase._dependencyTraversalHighestDepth);
			}
			return !flag5;
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000056FC File Offset: 0x000038FC
		internal void RemoveFromLocalGameObjectOrder()
		{
			if (!this._isInLocalGameObjectOrder)
			{
				return;
			}
			List<VRCConstraintBase> list;
			if (VRCConstraintBase.OrderInfoPerGameObject.TryGetValue(base.gameObject, out list))
			{
				if (list.Remove(this) && list.Count == 0)
				{
					VRCConstraintBase.OrderInfoPerGameObject.Remove(base.gameObject);
				}
				else
				{
					VRCConstraintBase.OrderInfoPerGameObject[base.gameObject] = list;
				}
			}
			this._isInLocalGameObjectOrder = false;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00005764 File Offset: 0x00003964
		[MethodImpl(256)]
		private static VRCConstraintBase.ConstraintExecutionOrder CalculateOrder(VRCConstraintBase constraintA, VRCConstraintBase constraintB)
		{
			Transform effectiveTargetTransform = constraintA.GetEffectiveTargetTransform();
			Transform effectiveTargetTransform2 = constraintB.GetEffectiveTargetTransform();
			if (effectiveTargetTransform.GetHashCode() == effectiveTargetTransform2.GetHashCode())
			{
				if (constraintA.gameObject == constraintB.gameObject)
				{
					if (constraintA._localGameObjectOrder < constraintB._localGameObjectOrder)
					{
						return VRCConstraintBase.ConstraintExecutionOrder.AbeforeB;
					}
					if (constraintA._localGameObjectOrder > constraintB._localGameObjectOrder)
					{
						return VRCConstraintBase.ConstraintExecutionOrder.BbeforeA;
					}
				}
				return VRCConstraintBase.ConstraintExecutionOrder.Irrelevant;
			}
			if (effectiveTargetTransform2.IsChildOf(effectiveTargetTransform))
			{
				return VRCConstraintBase.ConstraintExecutionOrder.AbeforeB;
			}
			if (effectiveTargetTransform.IsChildOf(effectiveTargetTransform2))
			{
				return VRCConstraintBase.ConstraintExecutionOrder.BbeforeA;
			}
			if (constraintB.IsDependentOnTransform(effectiveTargetTransform))
			{
				return VRCConstraintBase.ConstraintExecutionOrder.AbeforeB;
			}
			if (constraintA.IsDependentOnTransform(effectiveTargetTransform2))
			{
				return VRCConstraintBase.ConstraintExecutionOrder.BbeforeA;
			}
			return VRCConstraintBase.ConstraintExecutionOrder.Irrelevant;
		}

		// Token: 0x0600008C RID: 140 RVA: 0x000057F0 File Offset: 0x000039F0
		protected internal virtual bool IsDependentOnTransform(Transform otherTransform)
		{
			for (int i = 0; i < this.Sources.Count; i++)
			{
				Transform sourceTransform = this.Sources[i].SourceTransform;
				if (sourceTransform != null && sourceTransform.IsChildOf(otherTransform))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600008D RID: 141 RVA: 0x0000583A File Offset: 0x00003A3A
		public void Setup(Transform dependencyRoot, VRCPhysBoneBase[] childPhysBones, Animator containingAnimator, bool isClone)
		{
			this._assignedDependencyRoot = dependencyRoot;
			this._rootChildPhysBones = childPhysBones;
			this._attachedToAvatarClone = isClone;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00005852 File Offset: 0x00003A52
		public Transform GetEffectiveTargetTransform()
		{
			if (this._isRuntimeTargetTransformAssigned)
			{
				return this._runtimeTargetTransform;
			}
			if (!(this.TargetTransform != null))
			{
				return base.transform;
			}
			return this.TargetTransform;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00005880 File Offset: 0x00003A80
		private void Awake()
		{
			Transform effectiveTargetTransform = this.GetEffectiveTargetTransform();
			if (Application.isPlaying)
			{
				this._runtimeTargetTransform = effectiveTargetTransform;
				this._isRuntimeTargetTransformAssigned = true;
				this._cachedTargetParentTransform = this._runtimeTargetTransform.parent;
			}
			else
			{
				this._runtimeTargetTransform = null;
				this._isRuntimeTargetTransformAssigned = false;
				this._cachedTargetParentTransform = effectiveTargetTransform.parent;
			}
			this._hasCachedTargetParentTransform = (this._cachedTargetParentTransform != null);
			if (this.latestValidExecutionGroupIndex < 0)
			{
				this.latestValidExecutionGroupIndex = this.cachedExecutionGroupIndex;
			}
		}

		// Token: 0x06000090 RID: 144 RVA: 0x000058FD File Offset: 0x00003AFD
		private void Start()
		{
			if (this._constraintBinding == null)
			{
				this.AddToLocalGameObjectOrder();
			}
			VRCConstraintManager.RegisterConstraint(this);
			this._initialRegistrationComplete = true;
		}

		// Token: 0x06000091 RID: 145 RVA: 0x0000591B File Offset: 0x00003B1B
		private void OnEnable()
		{
			this.RequestFullNativeUpdate();
			if (this.AddToLocalGameObjectOrder())
			{
				VRCConstraintManager.ReRegisterSameObjectConstraint(this);
			}
			if (Application.isPlaying)
			{
				return;
			}
			if (!this._initialRegistrationComplete)
			{
				return;
			}
			VRCConstraintManager.RegisterConstraint(this);
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00005948 File Offset: 0x00003B48
		private void OnDisable()
		{
			this.RequestFullNativeUpdate();
			this.RemoveFromLocalGameObjectOrder();
			if (Application.isPlaying)
			{
				return;
			}
			VRCConstraintManager.UnregisterConstraint(this);
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00005964 File Offset: 0x00003B64
		private void OnDestroy()
		{
			this.Dispose();
		}

		// Token: 0x06000094 RID: 148 RVA: 0x0000596C File Offset: 0x00003B6C
		public void Dispose()
		{
			if (this._constraintBinding != null)
			{
				this._constraintBinding.Dispose();
				this._constraintBinding = null;
			}
			this.RemoveFromLocalGameObjectOrder();
			VRCConstraintManager.UnregisterConstraint(this);
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00005994 File Offset: 0x00003B94
		internal int GetTransformCount(bool forceRecount)
		{
			if (this._cachedTransformCount < 0 || forceRecount)
			{
				this._cachedTransformCount = this.RecalculateTransformCount();
			}
			return this._cachedTransformCount;
		}

		// Token: 0x06000096 RID: 150 RVA: 0x000059B8 File Offset: 0x00003BB8
		protected internal virtual int RecalculateTransformCount()
		{
			if (this.GetEffectiveTargetTransform() == null)
			{
				return 0;
			}
			int num = 1;
			if (this._hasCachedTargetParentTransform)
			{
				num++;
			}
			for (int i = 0; i < this.Sources.Count; i++)
			{
				if (this.Sources[i].SourceTransform != null)
				{
					num++;
				}
			}
			return num;
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00005A18 File Offset: 0x00003C18
		internal virtual void GetTransforms(List<(Transform constraintTransform, bool isTarget)> results)
		{
			Transform effectiveTargetTransform = this.GetEffectiveTargetTransform();
			if (effectiveTargetTransform == null)
			{
				return;
			}
			results.Add(new ValueTuple<Transform, bool>(effectiveTargetTransform, true));
			if (this._hasCachedTargetParentTransform)
			{
				results.Add(new ValueTuple<Transform, bool>(this._cachedTargetParentTransform, false));
			}
			for (int i = 0; i < this.Sources.Count; i++)
			{
				Transform sourceTransform = this.Sources[i].SourceTransform;
				if (sourceTransform != null)
				{
					results.Add(new ValueTuple<Transform, bool>(sourceTransform, false));
				}
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00005A9B File Offset: 0x00003C9B
		protected internal virtual Transform GetManagedWorldUpTransform()
		{
			return null;
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00005AA0 File Offset: 0x00003CA0
		internal VRCConstraintJobData AllocateJobData(int transformStartIndex)
		{
			VRCConstraintJobData result = new VRCConstraintJobData
			{
				TransformStartIndex = transformStartIndex,
				PositionConstraintMode = this.PositionMode,
				RotationConstraintMode = this.RotationMode,
				ScaleConstraintMode = this.ScaleMode,
				Sources = new UnsafeList<VRCConstraintJobData.ConstraintSourceData>(this.Sources.Count, (Allocator)4, 0)
			};
			int num = this._hasCachedTargetParentTransform ? 2 : 1;
			for (int i = 0; i < this.Sources.Count; i++)
			{
				VRCConstraintJobData.ConstraintSourceData constraintSourceData = default(VRCConstraintJobData.ConstraintSourceData);
				if (this.Sources[i].SourceTransform != null)
				{
					constraintSourceData.SourceIndex = num++;
					constraintSourceData.SourceExists = true;
				}
				else
				{
					constraintSourceData.SourceIndex = -1;
					constraintSourceData.SourceExists = false;
				}
				result.Sources.Add(in constraintSourceData);
			}
			result.WorldUpTransformIndex = ((this.GetManagedWorldUpTransform() != null) ? num : -1);
			return result;
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00005B96 File Offset: 0x00003D96
		private void OnDidApplyAnimationProperties()
		{
			this.RequestFullNativeUpdate();
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00005BA0 File Offset: 0x00003DA0
		protected internal unsafe virtual bool RequiresReallocation(in VRCConstraintJobData jobData, out bool sameGameObjectOnly)
		{
			if (this._pendingReallocation != VRCConstraintBase.PendingReallocationType.None)
			{
				sameGameObjectOnly = (this._pendingReallocation == VRCConstraintBase.PendingReallocationType.GameObjectOnly);
				this._pendingReallocation = VRCConstraintBase.PendingReallocationType.None;
				return true;
			}
			sameGameObjectOnly = false;
			Transform effectiveTargetTransform = this.GetEffectiveTargetTransform();
			if (effectiveTargetTransform == null)
			{
				return true;
			}
			if (effectiveTargetTransform != this._cachedTargetTransform)
			{
				return true;
			}
			if (Application.isEditor && this._cachedTargetParentTransform != this._cachedTargetTransform.parent)
			{
				return true;
			}
			if (jobData.Sources.Length != this.Sources.Count)
			{
				return true;
			}
			VRCConstraintJobData.ConstraintSourceData* ptr = jobData.Sources.Ptr;
			for (int i = 0; i < this.Sources.Count; i++)
			{
				VRCConstraintSource vrcconstraintSource = this.Sources[i];
				Transform sourceTransform = vrcconstraintSource.SourceTransform;
				int sourceIndex = ptr[i].SourceIndex;
				if (VRCConstraintManager.GetBufferedTransform(this, sourceIndex) != sourceTransform)
				{
					if (sourceTransform != null && sourceTransform == null)
					{
						if (sourceIndex < 0)
						{
							goto IL_EC;
						}
						vrcconstraintSource.SourceTransform = null;
						this.Sources[i] = vrcconstraintSource;
					}
					return true;
				}
				IL_EC:;
			}
			return false;
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00005CAC File Offset: 0x00003EAC
		internal void SynchronizeWithBinding()
		{
			VRCConstraintSynchronizeResult vrcconstraintSynchronizeResult;
			if (this._constraintBinding == null)
			{
				vrcconstraintSynchronizeResult = VRCConstraintSynchronizeResult.NoChanges;
			}
			else
			{
				vrcconstraintSynchronizeResult = this._constraintBinding.Synchronize(true);
			}
			switch (vrcconstraintSynchronizeResult)
			{
			case VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation:
				this.RequestFullNativeUpdate();
				return;
			case VRCConstraintSynchronizeResult.DidReceiveChangesSameGameObjectReallocation:
				this._pendingReallocation = VRCConstraintBase.PendingReallocationType.GameObjectOnly;
				this.RequestFullNativeUpdate();
				return;
			case VRCConstraintSynchronizeResult.DidReceiveChangesFullReallocation:
				this._pendingReallocation = VRCConstraintBase.PendingReallocationType.Full;
				return;
			default:
				return;
			}
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00005D04 File Offset: 0x00003F04
		internal void CheckReallocation(in VRCConstraintJobData jobData)
		{
			bool flag;
			if (this.RequiresReallocation(jobData, out flag))
			{
				if (!flag)
				{
					VRCConstraintManager.ReRegisterConstraint(this);
					return;
				}
				VRCConstraintManager.ReRegisterSameObjectConstraint(this);
			}
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00005D2C File Offset: 0x00003F2C
		internal void EstablishTargetTransform()
		{
			UnityEngine.Object cachedTargetTransform = this._cachedTargetTransform;
			this._cachedTargetTransform = this.GetEffectiveTargetTransform();
			if (cachedTargetTransform != this._cachedTargetTransform || Application.isEditor)
			{
				this._cachedTargetParentTransform = ((this._cachedTargetTransform != null) ? this._cachedTargetTransform.parent : null);
				this._hasCachedTargetParentTransform = (this._cachedTargetParentTransform != null);
			}
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00005D94 File Offset: 0x00003F94
		internal void ReEvaluatePhysBoneOrder()
		{
			Transform effectiveTargetTransform = this.GetEffectiveTargetTransform();
			if (effectiveTargetTransform == null)
			{
				return;
			}
			this._physBoneDependency = VRCConstraintBase.ConstraintPhysBoneDependency.NoDependency;
			if (this._rootChildPhysBones == null)
			{
				this._rootChildPhysBones = this.DependencyRoot.GetComponentsInChildren<VRCPhysBoneBase>(true);
			}
			foreach (VRCPhysBoneBase physBone in this._rootChildPhysBones)
			{
				this._physBoneDependency |= this.DeterminePhysBoneDependency(effectiveTargetTransform, physBone);
				if (this._physBoneDependency == VRCConstraintBase.ConstraintPhysBoneDependency.MultiDependent)
				{
					break;
				}
			}
			this.InvalidatePlayerLoopStage();
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00005E10 File Offset: 0x00004010
		private VRCConstraintBase.ConstraintPhysBoneDependency DeterminePhysBoneDependency(Transform constraintEffectiveTarget, VRCPhysBoneBase physBone)
		{
			Transform rootTransform = physBone.GetRootTransform();
			if (constraintEffectiveTarget.IsChildOf(rootTransform))
			{
				return VRCConstraintBase.ConstraintPhysBoneDependency.DependsOnAnyPhysBones;
			}
			if (rootTransform.IsChildOf(constraintEffectiveTarget))
			{
				return VRCConstraintBase.ConstraintPhysBoneDependency.HasDependentPhysBones;
			}
			for (int i = 0; i < this.Sources.Count; i++)
			{
				Transform sourceTransform = this.Sources[i].SourceTransform;
				if (sourceTransform != null && sourceTransform.IsChildOf(rootTransform))
				{
					return VRCConstraintBase.ConstraintPhysBoneDependency.DependsOnAnyPhysBones;
				}
			}
			return VRCConstraintBase.ConstraintPhysBoneDependency.NoDependency;
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00005E78 File Offset: 0x00004078
		internal void EstablishPlayerLoopStage()
		{
			if (this._playerLoopStage != VRCConstraintPlayerLoopStage.Unassigned)
			{
				return;
			}
			VRCConstraintPlayerLoopStage newStage = VRCConstraintPlayerLoopStage.PostPhysBone;
			if (this.DependsOnLocalAvatarProcessing)
			{
				newStage = VRCConstraintPlayerLoopStage.PostLocalAvatarProcess;
			}
			else if (this._physBoneDependency == VRCConstraintBase.ConstraintPhysBoneDependency.HasDependentPhysBones)
			{
				newStage = VRCConstraintPlayerLoopStage.PrePhysBone;
			}
			this.AssignPlayerLoopStage(newStage, false);
			foreach (VRCConstraintBase vrcconstraintBase in this._dependents)
			{
				vrcconstraintBase.EstablishPlayerLoopStage();
				if (this._playerLoopStage > vrcconstraintBase._playerLoopStage)
				{
					if (this._physBoneDependency == VRCConstraintBase.ConstraintPhysBoneDependency.NoDependency && this._playerLoopStage <= VRCConstraintPlayerLoopStage.PostPhysBone)
					{
						this.AssignPlayerLoopStage(vrcconstraintBase._playerLoopStage, false);
					}
					else
					{
						vrcconstraintBase.AssignPlayerLoopStage(this._playerLoopStage, true);
					}
				}
			}
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00005F30 File Offset: 0x00004130
		private void AssignPlayerLoopStage(VRCConstraintPlayerLoopStage newStage, bool recursive)
		{
			if (this._playerLoopStage == newStage)
			{
				return;
			}
			this._playerLoopStage = newStage;
			if (recursive)
			{
				foreach (VRCConstraintBase vrcconstraintBase in this._dependents)
				{
					vrcconstraintBase.AssignPlayerLoopStage(newStage, true);
				}
			}
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00005F98 File Offset: 0x00004198
		private void InvalidatePlayerLoopStage()
		{
			this._playerLoopStage = VRCConstraintPlayerLoopStage.Unassigned;
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00005FA4 File Offset: 0x000041A4
		internal unsafe void UpdateJobData(ref VRCConstraintJobData jobData, bool passiveUpdate)
		{
			jobData.HasParentTransform = this._hasCachedTargetParentTransform;
			if (this._playerLoopStage == VRCConstraintPlayerLoopStage.Unassigned)
			{
				jobData.PlayerLoopStage = VRCConstraintPlayerLoopStage.PostPhysBone;
			}
			else
			{
				jobData.PlayerLoopStage = this._playerLoopStage;
			}
			jobData.AttachedToAvatarClone = this._attachedToAvatarClone;
			object obj = !passiveUpdate || this._fullNativeUpdatePending;
			this._fullNativeUpdatePending = false;
			object obj2 = obj;
			if (obj2 != null)
			{
				jobData.IsActive = (base.isActiveAndEnabled && this.IsActive);
				jobData.GlobalWeight = this.GlobalWeight;
				jobData.SolveInLocalSpace = this.SolveInLocalSpace;
				if (jobData.FreezeToWorld != this.FreezeToWorld && !this.FreezeToWorld && this.RebakeOffsetsWhenUnfrozen)
				{
					this.TryBakeCurrentOffsetsRuntime(VRCConstraintBase.BakeOptions.BakeOffsets);
				}
				jobData.FreezeToWorld = this.FreezeToWorld;
				jobData.Locked = (this.Locked || Application.isPlaying);
				this.UpdateTypeSpecificJobData(ref jobData);
			}
			if (obj2 != null)
			{
				float num = 0f;
				VRCConstraintJobData.ConstraintSourceData* ptr = jobData.Sources.Ptr;
				int num2 = 0;
				while (num2 < jobData.Sources.Length && num2 < this.Sources.Count)
				{
					VRCConstraintJobData.ConstraintSourceData constraintSourceData = ptr[num2];
					Transform sourceTransform = this.Sources[num2].SourceTransform;
					if (constraintSourceData.SourceIndex < 0)
					{
						constraintSourceData.Weight = 0f;
					}
					else
					{
						if (!passiveUpdate || !Application.isPlaying)
						{
							VRCConstraintManager.SetBufferedTransform(this, constraintSourceData.SourceIndex, sourceTransform);
						}
						constraintSourceData.Weight = this.Sources[num2].Weight;
						num += constraintSourceData.Weight;
						this.UpdateTypeSpecificSourceData(ref constraintSourceData, this.Sources[num2]);
					}
					ptr[num2] = constraintSourceData;
					num2++;
				}
				jobData.TotalValidSourceWeight = num;
			}
		}

		// Token: 0x060000A5 RID: 165
		protected internal abstract void UpdateTypeSpecificJobData(ref VRCConstraintJobData jobData);

		// Token: 0x060000A6 RID: 166 RVA: 0x0000616A File Offset: 0x0000436A
		protected internal virtual void UpdateTypeSpecificSourceData(ref VRCConstraintJobData.ConstraintSourceData sourceData, VRCConstraintSource managedSource)
		{
		}

		// Token: 0x060000A7 RID: 167
		public abstract bool AffectsAnyAxis();

		// Token: 0x060000A8 RID: 168 RVA: 0x0000616C File Offset: 0x0000436C
		internal void PostUpdateJobData()
		{
			IVRCConstraintBinding constraintBinding = this._constraintBinding;
			if (constraintBinding == null)
			{
				return;
			}
			constraintBinding.RestoreUnityConstraintEnabledState();
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00006180 File Offset: 0x00004380
		protected VRCConstraintBase.Axis CreateAxisBitfield(bool x, bool y, bool z)
		{
			VRCConstraintBase.Axis axis = VRCConstraintBase.Axis.None;
			if (x)
			{
				axis |= VRCConstraintBase.Axis.X;
			}
			if (y)
			{
				axis |= VRCConstraintBase.Axis.Y;
			}
			if (z)
			{
				axis |= VRCConstraintBase.Axis.Z;
			}
			return axis;
		}

		// Token: 0x060000AA RID: 170 RVA: 0x000061A5 File Offset: 0x000043A5
		public void RequestFullNativeUpdate()
		{
			this._fullNativeUpdatePending = true;
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000061B0 File Offset: 0x000043B0
		public Vector3[] GetPerSourcePositionOffsets()
		{
			Vector3[] array = new Vector3[this.Sources.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = this.Sources[i].ParentPositionOffset;
			}
			return array;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x000061F8 File Offset: 0x000043F8
		private void OnDrawGizmosSelected()
		{
			Transform targetTransform = this.TargetTransform;
			if (targetTransform != null && targetTransform != base.transform)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(targetTransform.position, 0.025f);
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060000AD RID: 173 RVA: 0x0000623D File Offset: 0x0000443D
		// (set) Token: 0x060000AE RID: 174 RVA: 0x00006245 File Offset: 0x00004445
		internal bool IsPendingUnprocessed { get; set; }

		// Token: 0x060000AF RID: 175 RVA: 0x00006250 File Offset: 0x00004450
		[MethodImpl(256)]
		private static void CalculateDependencies(VRCConstraintBase constraintA, in IReadOnlyList<VRCConstraintBase> searchSpace, int index)
		{
			while (index < searchSpace.Count)
			{
				VRCConstraintBase vrcconstraintBase = searchSpace[index];
				VRCConstraintBase.ConstraintExecutionOrder constraintExecutionOrder = VRCConstraintBase.CalculateOrder(constraintA, vrcconstraintBase);
				if (constraintExecutionOrder == VRCConstraintBase.ConstraintExecutionOrder.AbeforeB)
				{
					constraintA._dependents.Add(vrcconstraintBase);
					VRCConstraintBase._rootNodes.Remove(vrcconstraintBase);
				}
				else if (constraintExecutionOrder == VRCConstraintBase.ConstraintExecutionOrder.BbeforeA)
				{
					vrcconstraintBase._dependents.Add(constraintA);
					VRCConstraintBase._rootNodes.Remove(constraintA);
				}
				index++;
			}
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x000062BC File Offset: 0x000044BC
		private void TraverseDependencies(int depth = 0)
		{
			if (depth > 0 && depth <= this._dependencyTraversalHighestDepth)
			{
				return;
			}
			if (this._dependencyActiveMark)
			{
				this._dependencyTraversalHighestDepth = depth;
				return;
			}
			if (depth > 512)
			{
				return;
			}
			if (++VRCConstraintBase._dependencyTraversalTotalSteps > 32768)
			{
				return;
			}
			this._dependencyActiveMark = true;
			if (!VRCConstraintBase._dependencyTraversalMarks.Get(this._dependencyTraversalMarkIndex))
			{
				VRCConstraintBase._dependencyTraversalMarksCounter++;
			}
			VRCConstraintBase._dependencyTraversalMarks.Set(this._dependencyTraversalMarkIndex, true);
			this._dependencyTraversalHighestDepth = depth;
			foreach (VRCConstraintBase vrcconstraintBase in this._dependents)
			{
				vrcconstraintBase.TraverseDependencies(depth + 1);
			}
			this._dependencyActiveMark = false;
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00006390 File Offset: 0x00004590
		internal static void ReorganizeGroupsFastForRoot(IReadOnlyList<VRCConstraintBase> unprocessedConstraints, IDictionary<int, VRCConstraintGroup> executionGroups)
		{
			VRCConstraintBase._rootNodes.Clear();
			VRCConstraintBase._dependencyTraversalTotalSteps = 0;
			for (int i = 0; i < unprocessedConstraints.Count; i++)
			{
				VRCConstraintBase vrcconstraintBase = unprocessedConstraints[i];
				vrcconstraintBase._dependents.Clear();
				vrcconstraintBase._dependencyActiveMark = false;
				vrcconstraintBase._dependencyTraversalHighestDepth = 0;
				vrcconstraintBase._dependencyTraversalMarkIndex = i;
				VRCConstraintBase._rootNodes.Add(vrcconstraintBase);
				vrcconstraintBase.IsPendingUnprocessed = false;
				vrcconstraintBase.InvalidatePlayerLoopStage();
			}
			VRCConstraintBase._dependencyTraversalMarksCounter = 0;
			if (VRCConstraintBase._dependencyTraversalMarks.Count < unprocessedConstraints.Count)
			{
				VRCConstraintBase._dependencyTraversalMarks = new BitArray(math.ceilpow2(unprocessedConstraints.Count), false);
			}
			else
			{
				VRCConstraintBase._dependencyTraversalMarks.SetAll(false);
			}
			for (int j = 0; j < unprocessedConstraints.Count - 1; j++)
			{
				VRCConstraintBase.CalculateDependencies(unprocessedConstraints[j], unprocessedConstraints, j + 1);
			}
			foreach (VRCConstraintBase vrcconstraintBase2 in VRCConstraintBase._rootNodes)
			{
				vrcconstraintBase2.TraverseDependencies(0);
			}
			if (VRCConstraintBase._dependencyTraversalMarksCounter < unprocessedConstraints.Count)
			{
				for (int k = 0; k < unprocessedConstraints.Count; k++)
				{
					if (!VRCConstraintBase._dependencyTraversalMarks.Get(k))
					{
						unprocessedConstraints[k].TraverseDependencies(0);
					}
				}
			}
			for (int l = 0; l < unprocessedConstraints.Count; l++)
			{
				VRCConstraintBase vrcconstraintBase3 = unprocessedConstraints[l];
				int dependencyTraversalHighestDepth = vrcconstraintBase3._dependencyTraversalHighestDepth;
				VRCConstraintGroup vrcconstraintGroup;
				if (!executionGroups.TryGetValue(dependencyTraversalHighestDepth, out vrcconstraintGroup))
				{
					vrcconstraintGroup = new VRCConstraintGroup();
					executionGroups[dependencyTraversalHighestDepth] = vrcconstraintGroup;
				}
				vrcconstraintGroup.AddConstraint(vrcconstraintBase3, dependencyTraversalHighestDepth);
			}
		}

		// Token: 0x04000060 RID: 96
		public bool IsActive;

		// Token: 0x04000061 RID: 97
		public float GlobalWeight = 1f;

		// Token: 0x04000062 RID: 98
		[Tooltip("The transform this constraint will be applied to. Leave this blank to apply the constraint to the transform this component is attached to. This reference can only be changed in edit-mode.")]
		public Transform TargetTransform;

		// Token: 0x04000063 RID: 99
		[Tooltip("When set, solve this constraint in local space. Leave this unset to solve in world space.")]
		public bool SolveInLocalSpace;

		// Token: 0x04000064 RID: 100
		[Tooltip("When set, this constraint will ignore its sources and become locked to the world. Unset it to solve the constraint normally again.")]
		public bool FreezeToWorld;

		// Token: 0x04000065 RID: 101
		[Tooltip("When set, offsets will be recalculated as the constraint is unfrozen. Unset this to maintain the original offsets.")]
		public bool RebakeOffsetsWhenUnfrozen;

		// Token: 0x04000066 RID: 102
		[Tooltip("When set, evaluate with the current offsets. When not set, update the offsets based on the current transform. This has no effect in play-mode.")]
		public bool Locked;

		// Token: 0x04000067 RID: 103
		public VRCConstraintSourceKeyableList Sources;

		// Token: 0x04000069 RID: 105
		[SerializeField]
		[HideInInspector]
		[NotKeyable]
		private int cachedExecutionGroupIndex = -1;

		// Token: 0x0400006A RID: 106
		[SerializeField]
		[HideInInspector]
		[NotKeyable]
		private int latestValidExecutionGroupIndex = -1;

		// Token: 0x0400006B RID: 107
		private Transform _runtimeTargetTransform;

		// Token: 0x0400006C RID: 108
		private bool _isRuntimeTargetTransformAssigned;

		// Token: 0x0400006D RID: 109
		private Transform _cachedTargetTransform;

		// Token: 0x0400006E RID: 110
		private bool _hasCachedTargetParentTransform;

		// Token: 0x0400006F RID: 111
		private Transform _cachedTargetParentTransform;

		// Token: 0x04000070 RID: 112
		private Transform _assignedDependencyRoot;

		// Token: 0x04000071 RID: 113
		private bool _initialRegistrationComplete;

		// Token: 0x04000072 RID: 114
		private int _cachedTransformCount = -1;

		// Token: 0x04000073 RID: 115
		private IVRCConstraintBinding _constraintBinding;

		// Token: 0x04000074 RID: 116
		private bool _fullNativeUpdatePending;

		// Token: 0x04000075 RID: 117
		private VRCConstraintBase.PendingReallocationType _pendingReallocation;

		// Token: 0x04000076 RID: 118
		private VRCPhysBoneBase[] _rootChildPhysBones;

		// Token: 0x04000077 RID: 119
		private VRCConstraintPlayerLoopStage _playerLoopStage;

		// Token: 0x04000078 RID: 120
		private VRCConstraintBase.ConstraintPhysBoneDependency _physBoneDependency;

		// Token: 0x04000079 RID: 121
		private bool _dependsOnLocalAvatarProcessing;

		// Token: 0x0400007A RID: 122
		private bool _attachedToAvatarClone;

		// Token: 0x0400007B RID: 123
		private readonly HashSet<Action<VRCConstraintBase>> _registeredBakeListeners = new HashSet<Action<VRCConstraintBase>>();

		// Token: 0x0400007C RID: 124
		private static readonly Dictionary<GameObject, List<VRCConstraintBase>> OrderInfoPerGameObject = new Dictionary<GameObject, List<VRCConstraintBase>>();

		// Token: 0x0400007D RID: 125
		private int _localGameObjectOrder;

		// Token: 0x0400007E RID: 126
		private bool _isInLocalGameObjectOrder;

		// Token: 0x04000080 RID: 128
		private HashSet<VRCConstraintBase> _dependents = new HashSet<VRCConstraintBase>();

		// Token: 0x04000081 RID: 129
		private static HashSet<VRCConstraintBase> _rootNodes = new HashSet<VRCConstraintBase>();

		// Token: 0x04000082 RID: 130
		private static BitArray _dependencyTraversalMarks = new BitArray(1024, false);

		// Token: 0x04000083 RID: 131
		private static int _dependencyTraversalMarksCounter;

		// Token: 0x04000084 RID: 132
		private int _dependencyTraversalMarkIndex;

		// Token: 0x04000085 RID: 133
		private bool _dependencyActiveMark;

		// Token: 0x04000086 RID: 134
		private const int MAX_TRAVERSAL_STEPS = 32768;

		// Token: 0x04000087 RID: 135
		private static int _dependencyTraversalTotalSteps;

		// Token: 0x04000088 RID: 136
		private int _dependencyTraversalHighestDepth;

		// Token: 0x04000089 RID: 137
		private const int MAX_DEPENDENCY_DEPTH = 512;

		// Token: 0x02000054 RID: 84
		public enum WorldUpType
		{
			// Token: 0x04000275 RID: 629
			SceneUp,
			// Token: 0x04000276 RID: 630
			ObjectUp,
			// Token: 0x04000277 RID: 631
			ObjectRotationUp,
			// Token: 0x04000278 RID: 632
			Vector,
			// Token: 0x04000279 RID: 633
			None
		}

		// Token: 0x02000055 RID: 85
		[Flags]
		public enum Axis
		{
			// Token: 0x0400027B RID: 635
			None = 0,
			// Token: 0x0400027C RID: 636
			X = 1,
			// Token: 0x0400027D RID: 637
			Y = 2,
			// Token: 0x0400027E RID: 638
			Z = 4,
			// Token: 0x0400027F RID: 639
			All = -1
		}

		// Token: 0x02000056 RID: 86
		private enum PendingReallocationType
		{
			// Token: 0x04000281 RID: 641
			None,
			// Token: 0x04000282 RID: 642
			GameObjectOnly,
			// Token: 0x04000283 RID: 643
			Full
		}

		// Token: 0x02000057 RID: 87
		[Flags]
		private enum ConstraintPhysBoneDependency
		{
			// Token: 0x04000285 RID: 645
			NoDependency = 0,
			// Token: 0x04000286 RID: 646
			HasDependentPhysBones = 1,
			// Token: 0x04000287 RID: 647
			DependsOnAnyPhysBones = 2,
			// Token: 0x04000288 RID: 648
			MultiDependent = 3
		}

		// Token: 0x02000058 RID: 88
		[Flags]
		public enum BakeOptions
		{
			// Token: 0x0400028A RID: 650
			BakeAtRest = 1,
			// Token: 0x0400028B RID: 651
			BakeOffsets = 2,
			// Token: 0x0400028C RID: 652
			BakeAll = 3
		}

		// Token: 0x02000059 RID: 89
		private enum ConstraintExecutionOrder
		{
			// Token: 0x0400028E RID: 654
			AbeforeB,
			// Token: 0x0400028F RID: 655
			BbeforeA,
			// Token: 0x04000290 RID: 656
			Irrelevant
		}
	}
}
