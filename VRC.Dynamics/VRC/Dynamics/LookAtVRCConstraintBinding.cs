using System;
using UnityEngine.Animations;
using VRC.Dynamics.ManagedTypes;

namespace VRC.Dynamics
{
	// Token: 0x02000015 RID: 21
	internal class LookAtVRCConstraintBinding : AbstractVRCConstraintBinding<LookAtConstraint, VRCLookAtConstraintBase>
	{
		// Token: 0x060000D0 RID: 208 RVA: 0x00006E59 File Offset: 0x00005059
		public LookAtVRCConstraintBinding(LookAtConstraint unityConstraint, VRCLookAtConstraintBase vrcConstraint) : base(unityConstraint, vrcConstraint)
		{
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00006E64 File Offset: 0x00005064
		protected override VRCConstraintSynchronizeResult SynchronizeInternal()
		{
			bool flag = false | AbstractVRCConstraintBinding<LookAtConstraint, VRCLookAtConstraintBase>.ChangeProperty(ref this.VrcConstraint.RotationAtRest, this.UnityConstraint.rotationAtRest) | AbstractVRCConstraintBinding<LookAtConstraint, VRCLookAtConstraintBase>.ChangeProperty(ref this.VrcConstraint.RotationOffset, this.UnityConstraint.rotationOffset) | AbstractVRCConstraintBinding<LookAtConstraint, VRCLookAtConstraintBase>.ChangeProperty(ref this.VrcConstraint.Roll, this.UnityConstraint.roll);
			bool flag2 = AbstractVRCConstraintBinding<LookAtConstraint, VRCLookAtConstraintBase>.ChangeProperty(ref this.VrcConstraint.WorldUpTransform, this.UnityConstraint.worldUpObject);
			if (!((flag || flag2) | AbstractVRCConstraintBinding<LookAtConstraint, VRCLookAtConstraintBase>.ChangeProperty(ref this.VrcConstraint.UseUpTransform, this.UnityConstraint.useUpObject)))
			{
				return VRCConstraintSynchronizeResult.NoChanges;
			}
			if (!flag2)
			{
				return VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation;
			}
			return VRCConstraintSynchronizeResult.DidReceiveChangesFullReallocation;
		}
	}
}
