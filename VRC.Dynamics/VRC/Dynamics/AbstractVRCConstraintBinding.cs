using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;

namespace VRC.Dynamics
{
	// Token: 0x0200000F RID: 15
	internal abstract class AbstractVRCConstraintBinding<TUnityConstraint, TVrcConstraint> : IVRCConstraintBinding, IDisposable where TUnityConstraint : Behaviour, IConstraint where TVrcConstraint : VRCConstraintBase
	{
		// Token: 0x17000015 RID: 21
		// (get) Token: 0x060000B8 RID: 184 RVA: 0x00006594 File Offset: 0x00004794
		public IConstraint ApplicationUnityConstraint
		{
			get
			{
				return this.UnityConstraint;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x000065A1 File Offset: 0x000047A1
		public VRCConstraintBase ApplicationVrcConstraint
		{
			get
			{
				return this.VrcConstraint;
			}
		}

		// Token: 0x060000BA RID: 186 RVA: 0x000065B0 File Offset: 0x000047B0
		protected AbstractVRCConstraintBinding(TUnityConstraint unityConstraint, TVrcConstraint vrcConstraint)
		{
			this.UnityConstraint = unityConstraint;
			this.VrcConstraint = vrcConstraint;
			this.VrcConstraint.Sources = new VRCConstraintSourceKeyableList(this.UnityConstraint.sourceCount);
			this._unityConstraintPendingReEnable = false;
			this._hasEverFullySynced = false;
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00006604 File Offset: 0x00004804
		public void Dispose()
		{
			UnityEngine.Object.Destroy(this.UnityConstraint);
			this._isDisposed = true;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00006620 File Offset: 0x00004820
		public VRCConstraintSynchronizeResult Synchronize(bool disableUnityConstraint)
		{
			if (this._isDisposed)
			{
				return VRCConstraintSynchronizeResult.NoChanges;
			}
			VRCConstraintSynchronizeResult result = VRCConstraintSynchronizeResult.NoChanges;
			bool enabled = this.UnityConstraint.enabled;
			if (disableUnityConstraint)
			{
				this.VrcConstraint.enabled = true;
				bool flag = AbstractVRCConstraintBinding<TUnityConstraint, TVrcConstraint>.ChangeProperty(ref this.VrcConstraint.IsActive, enabled && this.UnityConstraint.constraintActive);
				if (flag)
				{
                    PushResult(ref result, VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation);
				}
				if (flag)
				{
					if (this.VrcConstraint.IsActive)
					{
						if (this.VrcConstraint.AddToLocalGameObjectOrder())
						{
                            PushResult(ref result, VRCConstraintSynchronizeResult.DidReceiveChangesSameGameObjectReallocation);
						}
					}
					else
					{
						this.VrcConstraint.RemoveFromLocalGameObjectOrder();
					}
				}
				if (enabled)
				{
					this._unityConstraintPendingReEnable = true;
					this.UnityConstraint.enabled = false;
				}
				if (this._hasEverFullySynced && !this.VrcConstraint.IsActive)
				{
					return result;
				}
			}
			else
			{
				bool flag2 = this.VrcConstraint.enabled != enabled;
				this.VrcConstraint.enabled = enabled;
				if (flag2 | AbstractVRCConstraintBinding<TUnityConstraint, TVrcConstraint>.ChangeProperty(ref this.VrcConstraint.IsActive, this.UnityConstraint.constraintActive))
				{
                    PushResult(ref result, VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation);
				}
			}
			bool flag3 = AbstractVRCConstraintBinding<TUnityConstraint, TVrcConstraint>.ChangeProperty(ref this.VrcConstraint.Locked, this.UnityConstraint.locked);
			bool flag4 = AbstractVRCConstraintBinding<TUnityConstraint, TVrcConstraint>.ChangeProperty(ref this.VrcConstraint.GlobalWeight, this.UnityConstraint.weight);
			if (flag3 || flag4)
			{
                PushResult(ref result, VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation);
			}
			for (int i = 0; i < this.VrcConstraint.Sources.Count; i++)
			{
				VRCConstraintSource value = this.VrcConstraint.Sources[i];
				ConstraintSource source = this.UnityConstraint.GetSource(i);
				bool flag5 = AbstractVRCConstraintBinding<TUnityConstraint, TVrcConstraint>.ChangeProperty(ref value.SourceTransform, source.sourceTransform);
				if (flag5)
				{
                    PushResult(ref result, VRCConstraintSynchronizeResult.DidReceiveChangesFullReallocation);
				}
				bool flag6 = AbstractVRCConstraintBinding<TUnityConstraint, TVrcConstraint>.ChangeProperty(ref value.Weight, source.weight);
				if (flag6)
				{
                    PushResult(ref result, VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation);
				}
				if (flag5 || flag6)
				{
					this.VrcConstraint.Sources[i] = value;
				}
			}
			VRCConstraintSynchronizeResult newResult = this.SynchronizeInternal();
            PushResult(ref result, newResult);
			this._hasEverFullySynced = true;
			return result;

            // Token: 0x060000C4 RID: 196 RVA: 0x0000690A File Offset: 0x00004B0A
            void PushResult(ref VRCConstraintSynchronizeResult currentResult, VRCConstraintSynchronizeResult newResult)
			{
                if (currentResult < newResult)
                {
                    currentResult = newResult;
                }
            }
        }

		// Token: 0x060000BD RID: 189
		protected abstract VRCConstraintSynchronizeResult SynchronizeInternal();

		// Token: 0x060000BE RID: 190 RVA: 0x00006887 File Offset: 0x00004A87
		public void RestoreUnityConstraintEnabledState()
		{
			if (this._isDisposed)
			{
				return;
			}
			if (this._unityConstraintPendingReEnable)
			{
				this.UnityConstraint.enabled = true;
				this._unityConstraintPendingReEnable = false;
			}
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000068B2 File Offset: 0x00004AB2
		protected static bool ChangeProperty(ref bool property, bool newValue)
		{
			bool flag = property != newValue;
			if (flag)
			{
				property = newValue;
			}
			return flag;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000068C2 File Offset: 0x00004AC2
		protected static bool ChangeProperty(ref float property, float newValue)
		{
			bool flag = property != newValue;
			if (flag)
			{
				property = newValue;
			}
			return flag;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x000068D2 File Offset: 0x00004AD2
		protected static bool ChangeProperty(ref Vector3 property, Vector3 newValue)
		{
			bool flag = property != newValue;
			if (flag)
			{
				property = newValue;
			}
			return flag;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x000068EA File Offset: 0x00004AEA
		protected static bool ChangeProperty(ref VRCConstraintBase.WorldUpType property, VRCConstraintBase.WorldUpType newValue)
		{
			bool flag = property != newValue;
			if (flag)
			{
				property = newValue;
			}
			return flag;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x000068FA File Offset: 0x00004AFA
		protected static bool ChangeProperty(ref Transform property, Transform newValue)
		{
			bool flag = property != newValue;
			if (flag)
			{
				property = newValue;
			}
			return flag;
		}

		

		// Token: 0x0400008A RID: 138
		protected readonly TUnityConstraint UnityConstraint;

		// Token: 0x0400008B RID: 139
		protected readonly TVrcConstraint VrcConstraint;

		// Token: 0x0400008C RID: 140
		private bool _unityConstraintPendingReEnable;

		// Token: 0x0400008D RID: 141
		private bool _hasEverFullySynced;

		// Token: 0x0400008E RID: 142
		private bool _isDisposed;
	}
}
