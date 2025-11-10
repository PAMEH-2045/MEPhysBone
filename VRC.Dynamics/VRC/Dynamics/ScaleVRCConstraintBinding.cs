using System;
using UnityEngine.Animations;
using VRC.Dynamics.ManagedTypes;

namespace VRC.Dynamics
{
	// Token: 0x02000012 RID: 18
	internal class ScaleVRCConstraintBinding : AbstractVRCConstraintBinding<ScaleConstraint, VRCScaleConstraintBase>
	{
		// Token: 0x060000C9 RID: 201 RVA: 0x00006A86 File Offset: 0x00004C86
		public ScaleVRCConstraintBinding(ScaleConstraint unityConstraint, VRCScaleConstraintBase vrcConstraint) : base(unityConstraint, vrcConstraint)
		{
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00006A90 File Offset: 0x00004C90
		protected override VRCConstraintSynchronizeResult SynchronizeInternal()
		{
			if (!(false | AbstractVRCConstraintBinding<ScaleConstraint, VRCScaleConstraintBase>.ChangeProperty(ref this.VrcConstraint.ScaleAtRest, this.UnityConstraint.scaleAtRest) | AbstractVRCConstraintBinding<ScaleConstraint, VRCScaleConstraintBase>.ChangeProperty(ref this.VrcConstraint.ScaleOffset, this.UnityConstraint.scaleOffset) | AbstractVRCConstraintBinding<ScaleConstraint, VRCScaleConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsScaleX, (this.UnityConstraint.scalingAxis & (Axis)1) > 0) | AbstractVRCConstraintBinding<ScaleConstraint, VRCScaleConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsScaleY, (this.UnityConstraint.scalingAxis & (Axis)2) > 0) | AbstractVRCConstraintBinding<ScaleConstraint, VRCScaleConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsScaleZ, (this.UnityConstraint.scalingAxis & (Axis)4) > 0)))
			{
				return VRCConstraintSynchronizeResult.NoChanges;
			}
			return VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation;
		}
	}
}
