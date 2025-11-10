using System;
using UnityEngine;

namespace VRC.Dynamics.ManagedTypes
{
	// Token: 0x0200003A RID: 58
	[AddComponentMenu("")]
	public class VRCScaleConstraintBase : VRCConstraintBase
	{
		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000260 RID: 608 RVA: 0x00010357 File Offset: 0x0000E557
		protected override VRCConstraintPositionMode PositionMode
		{
			get
			{
				return VRCConstraintPositionMode.None;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000261 RID: 609 RVA: 0x0001035A File Offset: 0x0000E55A
		protected override VRCConstraintRotationMode RotationMode
		{
			get
			{
				return VRCConstraintRotationMode.None;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000262 RID: 610 RVA: 0x0001035D File Offset: 0x0000E55D
		protected override VRCConstraintScaleMode ScaleMode
		{
			get
			{
				return VRCConstraintScaleMode.MatchScale;
			}
		}

		// Token: 0x06000263 RID: 611 RVA: 0x00010360 File Offset: 0x0000E560
		protected internal override void UpdateTypeSpecificJobData(ref VRCConstraintJobData jobData)
		{
			jobData.ScaleConfig.AtRest = this.ScaleAtRest;
			jobData.ScaleConfig.Offset = this.ScaleOffset;
			jobData.ScaleConfig.Axes = base.CreateAxisBitfield(this.AffectsScaleX, this.AffectsScaleY, this.AffectsScaleZ);
		}

		// Token: 0x06000264 RID: 612 RVA: 0x000103BC File Offset: 0x0000E5BC
		protected override void ApplyZeroOffset()
		{
			this.ScaleOffset = Vector3.one;
		}

		// Token: 0x06000265 RID: 613 RVA: 0x000103C9 File Offset: 0x0000E5C9
		internal override void AcceptOffsetBaker(VRCConstraintOffsetBaker baker)
		{
			baker.Bake(this);
		}

		// Token: 0x06000266 RID: 614 RVA: 0x000103D2 File Offset: 0x0000E5D2
		public sealed override bool AffectsAnyAxis()
		{
			return this.AffectsScaleX || this.AffectsScaleY || this.AffectsScaleZ;
		}

		// Token: 0x040001EC RID: 492
		public Vector3 ScaleAtRest = Vector3.one;

		// Token: 0x040001ED RID: 493
		public Vector3 ScaleOffset = Vector3.one;

		// Token: 0x040001EE RID: 494
		public bool AffectsScaleX = true;

		// Token: 0x040001EF RID: 495
		public bool AffectsScaleY = true;

		// Token: 0x040001F0 RID: 496
		public bool AffectsScaleZ = true;
	}
}
