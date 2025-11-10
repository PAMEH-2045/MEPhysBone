using System;
using UnityEngine;

namespace VRC.Dynamics.ManagedTypes
{
	// Token: 0x02000037 RID: 55
	[AddComponentMenu("")]
	public class VRCParentConstraintBase : VRCConstraintBase
	{
		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000247 RID: 583 RVA: 0x00010035 File Offset: 0x0000E235
		protected override VRCConstraintPositionMode PositionMode
		{
			get
			{
				return VRCConstraintPositionMode.ChildPosition;
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000248 RID: 584 RVA: 0x00010038 File Offset: 0x0000E238
		protected override VRCConstraintRotationMode RotationMode
		{
			get
			{
				return VRCConstraintRotationMode.MatchRotation;
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000249 RID: 585 RVA: 0x0001003B File Offset: 0x0000E23B
		protected override VRCConstraintScaleMode ScaleMode
		{
			get
			{
				return VRCConstraintScaleMode.None;
			}
		}

		// Token: 0x0600024A RID: 586 RVA: 0x00010040 File Offset: 0x0000E240
		protected internal override void UpdateTypeSpecificJobData(ref VRCConstraintJobData jobData)
		{
			jobData.PositionConfig.AtRest = this.PositionAtRest;
			jobData.PositionConfig.Axes = base.CreateAxisBitfield(this.AffectsPositionX, this.AffectsPositionY, this.AffectsPositionZ);
			jobData.RotationConfig.AtRest = this.RotationAtRest;
			jobData.RotationConfig.Axes = base.CreateAxisBitfield(this.AffectsRotationX, this.AffectsRotationY, this.AffectsRotationZ);
		}

		// Token: 0x0600024B RID: 587 RVA: 0x000100BF File Offset: 0x0000E2BF
		protected internal override void UpdateTypeSpecificSourceData(ref VRCConstraintJobData.ConstraintSourceData sourceData, VRCConstraintSource managedSource)
		{
			sourceData.ParentPositionOffset = managedSource.ParentPositionOffset;
			sourceData.ParentRotationOffset = managedSource.ParentRotationOffset;
		}

		// Token: 0x0600024C RID: 588 RVA: 0x000100E4 File Offset: 0x0000E2E4
		protected override void ApplyZeroOffset()
		{
			for (int i = 0; i < this.Sources.Count; i++)
			{
				VRCConstraintSource value = this.Sources[i];
				value.ParentPositionOffset = Vector3.zero;
				value.ParentRotationOffset = Vector3.zero;
				this.Sources[i] = value;
			}
		}

		// Token: 0x0600024D RID: 589 RVA: 0x00010139 File Offset: 0x0000E339
		internal override void AcceptOffsetBaker(VRCConstraintOffsetBaker baker)
		{
			baker.Bake(this);
		}

		// Token: 0x0600024E RID: 590 RVA: 0x00010142 File Offset: 0x0000E342
		public sealed override bool AffectsAnyAxis()
		{
			return this.AffectsPositionX || this.AffectsPositionY || this.AffectsPositionZ || this.AffectsRotationX || this.AffectsRotationY || this.AffectsRotationZ;
		}

		// Token: 0x040001DA RID: 474
		public Vector3 PositionAtRest = Vector3.zero;

		// Token: 0x040001DB RID: 475
		public bool AffectsPositionX = true;

		// Token: 0x040001DC RID: 476
		public bool AffectsPositionY = true;

		// Token: 0x040001DD RID: 477
		public bool AffectsPositionZ = true;

		// Token: 0x040001DE RID: 478
		public Vector3 RotationAtRest = Vector3.zero;

		// Token: 0x040001DF RID: 479
		public bool AffectsRotationX = true;

		// Token: 0x040001E0 RID: 480
		public bool AffectsRotationY = true;

		// Token: 0x040001E1 RID: 481
		public bool AffectsRotationZ = true;
	}
}
