using System;
using UnityEngine;

namespace VRC.Dynamics.ManagedTypes
{
	// Token: 0x02000038 RID: 56
	[AddComponentMenu("")]
	public class VRCPositionConstraintBase : VRCConstraintBase
	{
		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000250 RID: 592 RVA: 0x000101C7 File Offset: 0x0000E3C7
		protected override VRCConstraintPositionMode PositionMode
		{
			get
			{
				return VRCConstraintPositionMode.MatchPosition;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000251 RID: 593 RVA: 0x000101CA File Offset: 0x0000E3CA
		protected override VRCConstraintRotationMode RotationMode
		{
			get
			{
				return VRCConstraintRotationMode.None;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000252 RID: 594 RVA: 0x000101CD File Offset: 0x0000E3CD
		protected override VRCConstraintScaleMode ScaleMode
		{
			get
			{
				return VRCConstraintScaleMode.None;
			}
		}

		// Token: 0x06000253 RID: 595 RVA: 0x000101D0 File Offset: 0x0000E3D0
		protected internal override void UpdateTypeSpecificJobData(ref VRCConstraintJobData jobData)
		{
			jobData.PositionConfig.AtRest = this.PositionAtRest;
			jobData.PositionConfig.Offset = this.PositionOffset;
			jobData.PositionConfig.Axes = base.CreateAxisBitfield(this.AffectsPositionX, this.AffectsPositionY, this.AffectsPositionZ);
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0001022C File Offset: 0x0000E42C
		protected override void ApplyZeroOffset()
		{
			this.PositionOffset = Vector3.zero;
		}

		// Token: 0x06000255 RID: 597 RVA: 0x00010239 File Offset: 0x0000E439
		internal override void AcceptOffsetBaker(VRCConstraintOffsetBaker baker)
		{
			baker.Bake(this);
		}

		// Token: 0x06000256 RID: 598 RVA: 0x00010242 File Offset: 0x0000E442
		public sealed override bool AffectsAnyAxis()
		{
			return this.AffectsPositionX || this.AffectsPositionY || this.AffectsPositionZ;
		}

		// Token: 0x040001E2 RID: 482
		public Vector3 PositionAtRest = Vector3.zero;

		// Token: 0x040001E3 RID: 483
		public Vector3 PositionOffset = Vector3.zero;

		// Token: 0x040001E4 RID: 484
		public bool AffectsPositionX = true;

		// Token: 0x040001E5 RID: 485
		public bool AffectsPositionY = true;

		// Token: 0x040001E6 RID: 486
		public bool AffectsPositionZ = true;
	}
}
