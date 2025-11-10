using System;
using UnityEngine.Animations;
using VRC.Dynamics.ManagedTypes;

namespace VRC.Dynamics
{
	// Token: 0x02000010 RID: 16
	internal class PositionVRCConstraintBinding : AbstractVRCConstraintBinding<PositionConstraint, VRCPositionConstraintBase>
	{
		// Token: 0x060000C5 RID: 197 RVA: 0x00006914 File Offset: 0x00004B14
		public PositionVRCConstraintBinding(PositionConstraint unityConstraint, VRCPositionConstraintBase vrcConstraint) : base(unityConstraint, vrcConstraint)
		{
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00006920 File Offset: 0x00004B20
		protected override VRCConstraintSynchronizeResult SynchronizeInternal()
		{
			if (!(false | AbstractVRCConstraintBinding<PositionConstraint, VRCPositionConstraintBase>.ChangeProperty(ref this.VrcConstraint.PositionAtRest, this.UnityConstraint.translationAtRest) | AbstractVRCConstraintBinding<PositionConstraint, VRCPositionConstraintBase>.ChangeProperty(ref this.VrcConstraint.PositionOffset, this.UnityConstraint.translationOffset) | AbstractVRCConstraintBinding<PositionConstraint, VRCPositionConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsPositionX, (this.UnityConstraint.translationAxis & (Axis)1) > 0) | AbstractVRCConstraintBinding<PositionConstraint, VRCPositionConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsPositionY, (this.UnityConstraint.translationAxis & (Axis)2) > 0) | AbstractVRCConstraintBinding<PositionConstraint, VRCPositionConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsPositionZ, (this.UnityConstraint.translationAxis & (Axis)4) > 0)))
			{
				return VRCConstraintSynchronizeResult.NoChanges;
			}
			return VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation;
		}
	}
}
