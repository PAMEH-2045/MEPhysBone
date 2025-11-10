using System;
using UnityEngine.Animations;
using VRC.Dynamics.ManagedTypes;

namespace VRC.Dynamics
{
	// Token: 0x02000013 RID: 19
	internal class ParentVRCConstraintBinding : AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>
	{
		// Token: 0x060000CB RID: 203 RVA: 0x00006B3E File Offset: 0x00004D3E
		public ParentVRCConstraintBinding(ParentConstraint unityConstraint, VRCParentConstraintBase vrcConstraint) : base(unityConstraint, vrcConstraint)
		{
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00006B48 File Offset: 0x00004D48
		protected override VRCConstraintSynchronizeResult SynchronizeInternal()
		{
			bool flag = false;
			flag |= AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref this.VrcConstraint.PositionAtRest, this.UnityConstraint.translationAtRest);
			flag |= AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref this.VrcConstraint.RotationAtRest, this.UnityConstraint.rotationAtRest);
			flag |= AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsPositionX, (this.UnityConstraint.translationAxis & (Axis)1) > 0);
			flag |= AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsPositionY, (this.UnityConstraint.translationAxis & (Axis)2) > 0);
			flag |= AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsPositionZ, (this.UnityConstraint.translationAxis & (Axis)4) > 0);
			flag |= AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationX, (this.UnityConstraint.rotationAxis & (Axis)1) > 0);
			flag |= AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationY, (this.UnityConstraint.rotationAxis & (Axis)2) > 0);
			flag |= AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref this.VrcConstraint.AffectsRotationZ, (this.UnityConstraint.rotationAxis & (Axis)4) > 0);
			for (int i = 0; i < this.VrcConstraint.Sources.Count; i++)
			{
				VRCConstraintSource value = this.VrcConstraint.Sources[i];
				if (AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref value.ParentPositionOffset, this.UnityConstraint.GetTranslationOffset(i)) | AbstractVRCConstraintBinding<ParentConstraint, VRCParentConstraintBase>.ChangeProperty(ref value.ParentRotationOffset, this.UnityConstraint.GetRotationOffset(i)))
				{
					this.VrcConstraint.Sources[i] = value;
					flag = true;
				}
			}
			if (!flag)
			{
				return VRCConstraintSynchronizeResult.NoChanges;
			}
			return VRCConstraintSynchronizeResult.DidReceiveChangesNoReallocation;
		}
	}
}
