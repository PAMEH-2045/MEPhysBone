using System;
using UnityEngine.Animations;
using VRC.Dynamics.ManagedTypes;

namespace VRC.Dynamics
{
	// Token: 0x02000011 RID: 17
	internal class RotationVRCConstraintBinding : AbstractVRCConstraintBinding<RotationConstraint, VRCRotationConstraintBase>
	{
		// Token: 0x060000C7 RID: 199 RVA: 0x000069CE File Offset: 0x00004BCE
		public RotationVRCConstraintBinding(RotationConstraint unityConstraint, VRCRotationConstraintBase vrcConstraint) : base(unityConstraint, vrcConstraint)
		{
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x000069D8 File Offset: 0x00004BD8
		protected override VRCConstraintSynchronizeResult SynchronizeInternal()
		{
			if (!(false | AbstractVRCConstraintBinding<RotationConstraint, VRCRotationConstraintBase>.ChangeProperty(ref this.VrcConstraint.RotationAtRest, this.UnityConstraint.rotationAtRest) | AbstractVRCConstraintBinding<RotationConstraint, VRCRotationConstraintBase>.ChangeProperty(ref this.VrcConstraint.RotationOffset, this.UnityConstraint.rotationOffset) | AbstractVRCConstraintBinding<RotationConstraint, VRCRotationConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationX, (this.UnityConstraint.rotationAxis & (Axis)1) > 0) | AbstractVRCConstraintBinding<RotationConstraint, VRCRotationConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationY, (this.UnityConstraint.rotationAxis & (Axis)2) > 0) | AbstractVRCConstraintBinding<RotationConstraint, VRCRotationConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationZ, (this.UnityConstraint.rotationAxis & (Axis)4) > 0)))
			{
				return VRCConstraintSynchronizeResult.NoChanges;
			}
			return VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation;
		}
	}
}
