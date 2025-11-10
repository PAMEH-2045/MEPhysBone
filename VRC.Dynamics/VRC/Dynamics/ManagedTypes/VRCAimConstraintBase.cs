using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace VRC.Dynamics.ManagedTypes
{
	// Token: 0x02000035 RID: 53
	[AddComponentMenu("")]
	public class VRCAimConstraintBase : VRCWorldUpConstraintBase
	{
		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600022F RID: 559 RVA: 0x0000FA11 File Offset: 0x0000DC11
		protected override VRCConstraintPositionMode PositionMode
		{
			get
			{
				return VRCConstraintPositionMode.None;
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000230 RID: 560 RVA: 0x0000FA14 File Offset: 0x0000DC14
		protected override VRCConstraintRotationMode RotationMode
		{
			get
			{
				return VRCConstraintRotationMode.AimTowardsPosition;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000231 RID: 561 RVA: 0x0000FA17 File Offset: 0x0000DC17
		protected override VRCConstraintScaleMode ScaleMode
		{
			get
			{
				return VRCConstraintScaleMode.None;
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000232 RID: 562 RVA: 0x0000FA1A File Offset: 0x0000DC1A
		protected override bool UsesWorldUpTransform
		{
			get
			{
				return this.WorldUp == VRCConstraintBase.WorldUpType.ObjectUp || this.WorldUp == VRCConstraintBase.WorldUpType.ObjectRotationUp;
			}
		}

		// Token: 0x06000233 RID: 563 RVA: 0x0000FA30 File Offset: 0x0000DC30
		protected internal override void UpdateTypeSpecificJobData(ref VRCConstraintJobData jobData)
		{
			jobData.RotationConfig.AtRest = this.RotationAtRest;
			jobData.RotationConfig.Offset = this.RotationOffset;
			jobData.RotationConfig.Axes = base.CreateAxisBitfield(this.AffectsRotationX, this.AffectsRotationY, this.AffectsRotationZ);
			jobData.AimAxis = this.AimAxis;
			jobData.UpAxis = this.UpAxis;
			jobData.WorldUpType = this.WorldUp;
			jobData.WorldUpVector = this.WorldUpVector;
			base.UpdateTypeSpecificJobData(ref jobData);
		}

		// Token: 0x06000234 RID: 564 RVA: 0x0000FAD4 File Offset: 0x0000DCD4
		protected override float3 DetermineUpVector(Transform targetTransform)
		{
			float3 @float;
			switch (this.WorldUp)
			{
			case VRCConstraintBase.WorldUpType.SceneUp:
				@float = new(0f, 1f, 0f);
				goto IL_102;
			case VRCConstraintBase.WorldUpType.ObjectUp:
			{
				float3 float2 = (this.WorldUpTransform != null) ? (this.SolveInLocalSpace ? this.WorldUpTransform.localPosition : this.WorldUpTransform.position) : Unity.Mathematics.float3.zero;
				float3 float3 = (targetTransform != null) ? (this.SolveInLocalSpace ? targetTransform.localPosition : targetTransform.position) : float3.zero;
				@float = float2 - float3;
				goto IL_102;
			}
			case VRCConstraintBase.WorldUpType.ObjectRotationUp:
				@float = ((this.WorldUpTransform != null) ? math.mul(this.WorldUpTransform.rotation, this.WorldUpVector) : this.WorldUpVector);
				goto IL_102;
			case VRCConstraintBase.WorldUpType.Vector:
				@float = this.WorldUpVector;
				goto IL_102;
			}
			@float = float3.zero;
			IL_102:
			if (this.SolveInLocalSpace && targetTransform != null && targetTransform.parent != null)
			{
				@float = targetTransform.parent.localToWorldMatrix.MultiplyVector(@float);
			}
			return @float;
		}

		// Token: 0x06000235 RID: 565 RVA: 0x0000FC24 File Offset: 0x0000DE24
		protected override bool ForwardLookShouldApplyIdentity(float3 toSource, float3 worldUpVector)
		{
			return math.lengthsq(toSource) == 0f || math.lengthsq(this.AimAxis) == 0f || (math.lengthsq(this.UpAxis) == 0f && math.lengthsq(worldUpVector) > 0f);
		}

		// Token: 0x06000236 RID: 566 RVA: 0x0000FC80 File Offset: 0x0000DE80
		protected override void ForwardLookHandleZeroUp(float3 toSource, float3 worldUpVector, out quaternion look)
		{
			float3 @float = this.AimAxis.normalized;
			if (math.lengthsq(@float) == 0f)
			{
				@float = new(0f, 0f, 1f);
			}
			float3 float2 = math.normalize(toSource);
			VRCAimConstraintBase.FromToRotation(@float, float2, out look);
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0000FCD4 File Offset: 0x0000DED4
		protected override quaternion ReOrientateForwardLook(quaternion look)
		{
			float3 @float = this.AimAxis;
			float3 float2 = default(float3);
			float3 float3 = math.normalizesafe(@float, float2);
			float2 = Vector3.forward;
			quaternion quaternion;
			VRCAimConstraintBase.FromToRotation(float3, float2, out quaternion);
			float3 float4 = math.mul(quaternion, this.UpAxis);
			float num = math.atan2(float4.x, float4.y);
			quaternion = math.mul(quaternion, quaternion.AxisAngle(float3, num));
			return math.mul(look, quaternion);
		}

		// Token: 0x06000238 RID: 568 RVA: 0x0000FD4C File Offset: 0x0000DF4C
		private static void FromToRotation(in float3 fromNormalized, in float3 toNormalized, out quaternion fromTo)
		{
			float num = math.dot(fromNormalized, toNormalized);
			if (Approximately(num, 1f))
			{
				fromTo = quaternion.identity;
				return;
			}
			if (!Approximately(num, -1f))
			{
				fromTo = math.normalizesafe(new quaternion(new float4(math.cross(fromNormalized, toNormalized), num + 1f)));
				return;
			}
			float3 @float = math.cross(fromNormalized, Vector3.right);
			if (math.lengthsq(@float) > 0f)
			{
				fromTo = quaternion.AxisAngle(math.normalize(@float), 3.1415927f);
				return;
			}
			fromTo = quaternion.RotateY(3.1415927f);

            // Token: 0x0600023B RID: 571 RVA: 0x0000FE61 File Offset: 0x0000E061
            bool Approximately(float a, float b)
			{
                return math.abs(a - b) < 1E-06f;
            }
        }

		// Token: 0x06000239 RID: 569 RVA: 0x0000FE09 File Offset: 0x0000E009
		public sealed override bool AffectsAnyAxis()
		{
			return this.AffectsRotationX || this.AffectsRotationY || this.AffectsRotationZ;
		}

		

		// Token: 0x040001D1 RID: 465
		public bool AffectsRotationX = true;

		// Token: 0x040001D2 RID: 466
		public bool AffectsRotationY = true;

		// Token: 0x040001D3 RID: 467
		public bool AffectsRotationZ = true;

		// Token: 0x040001D4 RID: 468
		public Vector3 AimAxis = Vector3.forward;

		// Token: 0x040001D5 RID: 469
		public Vector3 UpAxis = Vector3.up;

		// Token: 0x040001D6 RID: 470
		public VRCConstraintBase.WorldUpType WorldUp;

		// Token: 0x040001D7 RID: 471
		public Vector3 WorldUpVector = Vector3.up;
	}
}
