using System;
using Unity.Mathematics;
using UnityEngine;

namespace VRC.Dynamics.ManagedTypes
{
	// Token: 0x02000036 RID: 54
	[AddComponentMenu("")]
	public class VRCLookAtConstraintBase : VRCWorldUpConstraintBase
	{
		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600023C RID: 572 RVA: 0x0000FE72 File Offset: 0x0000E072
		protected override VRCConstraintPositionMode PositionMode
		{
			get
			{
				return VRCConstraintPositionMode.None;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600023D RID: 573 RVA: 0x0000FE75 File Offset: 0x0000E075
		protected override VRCConstraintRotationMode RotationMode
		{
			get
			{
				return VRCConstraintRotationMode.LookAtPosition;
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600023E RID: 574 RVA: 0x0000FE78 File Offset: 0x0000E078
		protected override VRCConstraintScaleMode ScaleMode
		{
			get
			{
				return VRCConstraintScaleMode.None;
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600023F RID: 575 RVA: 0x0000FE7B File Offset: 0x0000E07B
		protected override bool UsesWorldUpTransform
		{
			get
			{
				return this.UseUpTransform;
			}
		}

		// Token: 0x06000240 RID: 576 RVA: 0x0000FE84 File Offset: 0x0000E084
		protected internal override void UpdateTypeSpecificJobData(ref VRCConstraintJobData jobData)
		{
			jobData.RotationConfig.AtRest = this.RotationAtRest;
			jobData.RotationConfig.Offset = this.RotationOffset;
			jobData.RotationConfig.Axes = VRCConstraintBase.Axis.All;
			jobData.UseUpTransform = this.UseUpTransform;
			jobData.Roll = this.Roll;
			base.UpdateTypeSpecificJobData(ref jobData);
		}

		// Token: 0x06000241 RID: 577 RVA: 0x0000FEE8 File Offset: 0x0000E0E8
		protected override float3 DetermineUpVector(Transform targetTransform)
		{
			float3 float3;
			if (this.UseUpTransform)
			{
				float3 @float = (this.WorldUpTransform != null) ? (this.SolveInLocalSpace ? this.WorldUpTransform.localPosition : this.WorldUpTransform.position) : float3.zero;
				float3 float2 = (targetTransform != null) ? (this.SolveInLocalSpace ? targetTransform.localPosition : targetTransform.position) : float3.zero;
				float3 = @float - float2;
			}
			else
			{
				float3 = new(0f, 1f, 0f);
			}
			if (this.SolveInLocalSpace && targetTransform != null && targetTransform.parent != null)
			{
				float3 = targetTransform.parent.localToWorldMatrix.MultiplyVector(float3);
			}
			return float3;
		}

		// Token: 0x06000242 RID: 578 RVA: 0x0000FFC1 File Offset: 0x0000E1C1
		protected override bool ForwardLookShouldApplyIdentity(float3 toSource, float3 worldUpVector)
		{
			return math.lengthsq(toSource) == 0f;
		}

		// Token: 0x06000243 RID: 579 RVA: 0x0000FFD0 File Offset: 0x0000E1D0
		protected override void ForwardLookHandleZeroUp(float3 toSource, float3 worldUpVector, out quaternion look)
		{
			look = Quaternion.LookRotation(toSource, worldUpVector);
		}

		// Token: 0x06000244 RID: 580 RVA: 0x0000FFF0 File Offset: 0x0000E1F0
		protected override quaternion ReOrientateForwardLook(quaternion look)
		{
			quaternion quaternion = quaternion.AxisAngle(new float3(0f, 0f, 1f), math.radians(-this.Roll));
			return math.mul(look, quaternion);
		}

		// Token: 0x06000245 RID: 581 RVA: 0x0001002A File Offset: 0x0000E22A
		public sealed override bool AffectsAnyAxis()
		{
			return true;
		}

		// Token: 0x040001D8 RID: 472
		public float Roll;

		// Token: 0x040001D9 RID: 473
		public bool UseUpTransform;
	}
}
