using System;
using UnityEngine;

namespace VRC.Dynamics.ManagedTypes
{
	// Token: 0x02000039 RID: 57
	[AddComponentMenu("")]
	public class VRCRotationConstraintBase : VRCConstraintBase
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000258 RID: 600 RVA: 0x0001028F File Offset: 0x0000E48F
		protected override VRCConstraintPositionMode PositionMode
		{
			get
			{
				return VRCConstraintPositionMode.None;
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000259 RID: 601 RVA: 0x00010292 File Offset: 0x0000E492
		protected override VRCConstraintRotationMode RotationMode
		{
			get
			{
				return VRCConstraintRotationMode.MatchRotation;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x0600025A RID: 602 RVA: 0x00010295 File Offset: 0x0000E495
		protected override VRCConstraintScaleMode ScaleMode
		{
			get
			{
				return VRCConstraintScaleMode.None;
			}
		}

		// Token: 0x0600025B RID: 603 RVA: 0x00010298 File Offset: 0x0000E498
		protected internal override void UpdateTypeSpecificJobData(ref VRCConstraintJobData jobData)
		{
			jobData.RotationConfig.AtRest = this.RotationAtRest;
			jobData.RotationConfig.Offset = this.RotationOffset;
			jobData.RotationConfig.Axes = base.CreateAxisBitfield(this.AffectsRotationX, this.AffectsRotationY, this.AffectsRotationZ);
		}

		// Token: 0x0600025C RID: 604 RVA: 0x000102F4 File Offset: 0x0000E4F4
		protected override void ApplyZeroOffset()
		{
			this.RotationOffset = Vector3.zero;
		}

		// Token: 0x0600025D RID: 605 RVA: 0x00010301 File Offset: 0x0000E501
		internal override void AcceptOffsetBaker(VRCConstraintOffsetBaker baker)
		{
			baker.Bake(this);
		}

		// Token: 0x0600025E RID: 606 RVA: 0x0001030A File Offset: 0x0000E50A
		public sealed override bool AffectsAnyAxis()
		{
			return this.AffectsRotationX || this.AffectsRotationY || this.AffectsRotationZ;
		}

		// Token: 0x040001E7 RID: 487
		public Vector3 RotationAtRest = Vector3.zero;

		// Token: 0x040001E8 RID: 488
		public Vector3 RotationOffset = Vector3.zero;

		// Token: 0x040001E9 RID: 489
		public bool AffectsRotationX = true;

		// Token: 0x040001EA RID: 490
		public bool AffectsRotationY = true;

		// Token: 0x040001EB RID: 491
		public bool AffectsRotationZ = true;
	}
}
