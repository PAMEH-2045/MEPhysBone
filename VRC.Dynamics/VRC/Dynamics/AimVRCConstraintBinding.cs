using System;
using UnityEngine.Animations;
using VRC.Dynamics.ManagedTypes;

namespace VRC.Dynamics
{
	// Token: 0x02000014 RID: 20
	internal class AimVRCConstraintBinding : AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>
	{
		// Token: 0x060000CD RID: 205 RVA: 0x00006CDF File Offset: 0x00004EDF
		public AimVRCConstraintBinding(AimConstraint unityConstraint, VRCAimConstraintBase vrcConstraint) : base(unityConstraint, vrcConstraint)
		{
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00006CEC File Offset: 0x00004EEC
		protected override VRCConstraintSynchronizeResult SynchronizeInternal()
		{
			bool flag = false | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.RotationAtRest, this.UnityConstraint.rotationAtRest) | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.RotationOffset, this.UnityConstraint.rotationOffset) | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.AimAxis, this.UnityConstraint.aimVector) | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.UpAxis, this.UnityConstraint.upVector) | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.WorldUpVector, this.UnityConstraint.worldUpVector);
			bool flag2 = AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.WorldUpTransform, this.UnityConstraint.worldUpObject);
			if (!((flag || flag2) | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.WorldUp, AimVRCConstraintBinding.ToVrcWorldUpType(this.UnityConstraint.worldUpType)) | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationX, (this.UnityConstraint.rotationAxis & (Axis)1) > 0) | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationY, (this.UnityConstraint.rotationAxis & (Axis)2) > 0) | AbstractVRCConstraintBinding<AimConstraint, VRCAimConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationZ, (this.UnityConstraint.rotationAxis & (Axis)4) > 0)))
			{
				return VRCConstraintSynchronizeResult.NoChanges;
			}
			if (!flag2)
			{
				return VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation;
			}
			return VRCConstraintSynchronizeResult.DidReceiveChangesFullReallocation;
		}

		// Token: 0x060000CF RID: 207 RVA: 0x00006E32 File Offset: 0x00005032
		private static VRCConstraintBase.WorldUpType ToVrcWorldUpType(AimConstraint.WorldUpType unityUpType)
		{
			switch (unityUpType)
			{
			case (AimConstraint.WorldUpType)0:
				return VRCConstraintBase.WorldUpType.SceneUp;
			case (AimConstraint.WorldUpType)1:
				return VRCConstraintBase.WorldUpType.ObjectUp;
			case (AimConstraint.WorldUpType)2:
				return VRCConstraintBase.WorldUpType.ObjectRotationUp;
			case (AimConstraint.WorldUpType)3:
				return VRCConstraintBase.WorldUpType.Vector;
			}
			return VRCConstraintBase.WorldUpType.None;
		}
	}
}
