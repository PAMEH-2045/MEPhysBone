using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using VRC.Dynamics.ManagedTypes;

namespace VRC.Dynamics
{
	// Token: 0x0200001A RID: 26
	internal class VRCConstraintOffsetBaker
	{
		// Token: 0x060000E9 RID: 233 RVA: 0x00007E66 File Offset: 0x00006066
		public VRCConstraintOffsetBaker(Transform targetTransform, float weightSum, VRCConstraintBase.BakeOptions bakeOptions)
		{
			this._targetTransform = targetTransform;
			this._weightSum = weightSum;
			this._bakeOptions = bakeOptions;
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000EA RID: 234 RVA: 0x00007E83 File Offset: 0x00006083
		private bool ShouldBakeAtRest
		{
			get
			{
				return (this._bakeOptions & VRCConstraintBase.BakeOptions.BakeAtRest) > (VRCConstraintBase.BakeOptions)0;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000EB RID: 235 RVA: 0x00007E90 File Offset: 0x00006090
		private bool ShouldBakeOffsets
		{
			get
			{
				return (this._bakeOptions & VRCConstraintBase.BakeOptions.BakeOffsets) > (VRCConstraintBase.BakeOptions)0;
			}
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00007E9D File Offset: 0x0000609D
		public void Bake(VRCPositionConstraintBase positionConstraint)
		{
			if (this.ShouldBakeAtRest)
			{
				positionConstraint.PositionAtRest = this._targetTransform.localPosition;
			}
			if (this.ShouldBakeOffsets)
			{
				positionConstraint.PositionOffset = this.CalculateBakedPositionOffset(ref positionConstraint.Sources, positionConstraint.SolveInLocalSpace, false);
			}
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00007EDC File Offset: 0x000060DC
		public void Bake(VRCRotationConstraintBase rotationConstraint)
		{
			if (this.ShouldBakeAtRest)
			{
				rotationConstraint.RotationAtRest = this.AsSignedEulers(this._targetTransform.localRotation);
			}
			if (this.ShouldBakeOffsets)
			{
				rotationConstraint.RotationOffset = this.CalculateBakedRotationOffset(ref rotationConstraint.Sources, rotationConstraint.SolveInLocalSpace, false);
			}
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00007F29 File Offset: 0x00006129
		public void Bake(VRCScaleConstraintBase scaleConstraint)
		{
			if (this.ShouldBakeAtRest)
			{
				scaleConstraint.ScaleAtRest = this._targetTransform.localScale;
			}
			if (this.ShouldBakeOffsets)
			{
				scaleConstraint.ScaleOffset = this.CalculateBakedScaleOffset(ref scaleConstraint.Sources, scaleConstraint.SolveInLocalSpace);
			}
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00007F64 File Offset: 0x00006164
		public void Bake(VRCParentConstraintBase parentConstraint)
		{
			if (this.ShouldBakeAtRest)
			{
				parentConstraint.PositionAtRest = this._targetTransform.localPosition;
			}
			if (this.ShouldBakeOffsets)
			{
				Vector3 parentPositionOffset = this.CalculateBakedPositionOffset(ref parentConstraint.Sources, parentConstraint.SolveInLocalSpace, true);
				for (int i = 0; i < parentConstraint.Sources.Count; i++)
				{
					VRCConstraintSource value = parentConstraint.Sources[i];
					value.ParentPositionOffset = parentPositionOffset;
					parentConstraint.Sources[i] = value;
				}
			}
			if (this.ShouldBakeAtRest)
			{
				parentConstraint.RotationAtRest = this.AsSignedEulers(this._targetTransform.localRotation);
			}
			if (this.ShouldBakeOffsets)
			{
				this.CalculateBakedRotationOffset(ref parentConstraint.Sources, parentConstraint.SolveInLocalSpace, true);
			}
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x0000801C File Offset: 0x0000621C
		public void Bake(VRCWorldUpConstraintBase worldUpConstraint)
		{
			if (this.ShouldBakeAtRest)
			{
				worldUpConstraint.RotationAtRest = this.AsSignedEulers(this._targetTransform.localRotation);
			}
			if (this.ShouldBakeOffsets)
			{
				if (this._weightSum != 0f)
				{
					Vector3 vector = Vector3.zero;
					for (int i = 0; i < worldUpConstraint.Sources.Count; i++)
					{
						if (worldUpConstraint.Sources[i].SourceTransform != null)
						{
							float num = (worldUpConstraint.Sources.Count > 1) ? (worldUpConstraint.Sources[i].Weight / this._weightSum) : worldUpConstraint.Sources[i].Weight;
							Vector3 vector2;
							if (worldUpConstraint.SolveInLocalSpace && this._targetTransform.parent != null)
							{
								vector2 = this._targetTransform.parent.localToWorldMatrix.MultiplyPoint3x4(worldUpConstraint.Sources[i].SourceTransform.localPosition);
							}
							else
							{
								vector2 = worldUpConstraint.Sources[i].SourceTransform.position;
							}
							vector += vector2 * num;
						}
					}
					quaternion quaternion = worldUpConstraint.GenerateForwardLook(vector);
					quaternion quaternion2 = quaternion.identity;
					for (int j = 0; j < worldUpConstraint.Sources.Count; j++)
					{
						Quaternion rotation = this._targetTransform.rotation;
						quaternion quaternion3 = math.mul(math.inverse(quaternion), rotation);
						if (j == 0)
						{
							quaternion2 = quaternion3;
						}
						else
						{
							quaternion2 = math.nlerp(quaternion2, quaternion3, worldUpConstraint.Sources[j].Weight);
						}
					}
					worldUpConstraint.RotationOffset = this.AsSignedEulers(quaternion2);
					return;
				}
				worldUpConstraint.RotationOffset = Vector3.zero;
			}
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x000081E0 File Offset: 0x000063E0
		private Vector3 CalculateBakedPositionOffset(ref VRCConstraintSourceKeyableList sources, bool solveInLocalSpace, bool parentConstraintContext)
		{
			if (this._weightSum <= 0f)
			{
				return Vector3.zero;
			}
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < sources.Count; i++)
			{
				if (sources[i].SourceTransform != null)
				{
					float num = (sources.Count > 1) ? (sources[i].Weight / this._weightSum) : sources[i].Weight;
					Vector3 vector2 = (solveInLocalSpace ? sources[i].SourceTransform.localPosition : sources[i].SourceTransform.position) * num;
					vector += vector2;
				}
			}
			Vector3 vector3 = (solveInLocalSpace ? this._targetTransform.localPosition : this._targetTransform.position) - vector;
			if (parentConstraintContext)
			{
				return Quaternion.Inverse(this._targetTransform.rotation) * vector3;
			}
			if (!solveInLocalSpace && this._targetTransform.parent != null)
			{
				return this._targetTransform.parent.worldToLocalMatrix.MultiplyVector(vector3);
			}
			return vector3;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00008304 File Offset: 0x00006504
		private Vector3 CalculateBakedRotationOffset(ref VRCConstraintSourceKeyableList sources, bool solveInLocalSpace, bool setPerSourceParentOffsets)
		{
			if (this._weightSum > 0f)
			{
				quaternion quaternion = quaternion.identity;
				quaternion quaternion2 = solveInLocalSpace ? this._targetTransform.localRotation : this._targetTransform.rotation;
				for (int i = 0; i < sources.Count; i++)
				{
					if (sources[i].SourceTransform != null)
					{
						float num = (sources.Count > 1) ? (sources[i].Weight / this._weightSum) : sources[i].Weight;
						quaternion quaternion3 = math.mul(math.inverse(solveInLocalSpace ? sources[i].SourceTransform.localRotation : sources[i].SourceTransform.rotation), quaternion2);
						if (i == 0)
						{
							quaternion = quaternion3;
						}
						else
						{
							quaternion = math.nlerp(quaternion, quaternion3, num);
						}
						if (setPerSourceParentOffsets)
						{
							VRCConstraintSource value = sources[i];
							value.ParentRotationOffset = this.AsSignedEulers(quaternion3);
							value.ParentPositionOffset = math.mul(quaternion3, sources[i].ParentPositionOffset);
							sources[i] = value;
						}
					}
				}
				return this.AsSignedEulers(quaternion);
			}
			return Vector3.zero;
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0000844C File Offset: 0x0000664C
		private Vector3 CalculateBakedScaleOffset(ref VRCConstraintSourceKeyableList sources, bool solveInLocalSpace)
		{
			if (this._weightSum > 0f)
			{
				Vector3 vector = Vector3.one;
				for (int i = 0; i < sources.Count; i++)
				{
					if (sources[i].SourceTransform != null)
					{
						float num = (sources.Count > 1) ? (sources[i].Weight / this._weightSum) : sources[i].Weight;
						Vector3 vector2 = solveInLocalSpace ? this._targetTransform.localScale : this._targetTransform.lossyScale;
						float3 @float = math.sign(vector2);
						float3 float2 = @float * math.pow(@float * vector2, num);
						float3 float3 = solveInLocalSpace ? sources[i].SourceTransform.localScale : sources[i].SourceTransform.lossyScale;
						float3 float4 = new((float3.x != 0f) ? (float2.x / float3.x) : 0f, (float3.y != 0f) ? (float2.y / float3.y) : 0f, (float3.z != 0f) ? (float2.z / float3.z) : 0f);
						vector *= float4;
					}
				}
				return vector;
			}
			return Vector3.one;
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x000085C8 File Offset: 0x000067C8
		private Vector3 AsSignedEulers(Quaternion q)
		{
			if (!Mathf.Approximately(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w, 1f))
			{
				return Vector3.zero;
			}
			Vector3 eulerAngles = q.eulerAngles;
            TreatEuler(ref eulerAngles.x);
            TreatEuler(ref eulerAngles.y);
            TreatEuler(ref eulerAngles.z);
			return eulerAngles;

            // Token: 0x060000F5 RID: 245 RVA: 0x0000864C File Offset: 0x0000684C
            void TreatEuler(ref float angleDegrees)
			{
                while (angleDegrees > 180f)
                {
                    angleDegrees -= 360f;
                }
                float num = Mathf.Round(angleDegrees);
                if ((double)Mathf.Abs(angleDegrees - num) < 0.0001)
                {
                    angleDegrees = num;
                }
            }
        }

		

		// Token: 0x040000AD RID: 173
		private readonly Transform _targetTransform;

		// Token: 0x040000AE RID: 174
		private readonly float _weightSum;

		// Token: 0x040000AF RID: 175
		private readonly VRCConstraintBase.BakeOptions _bakeOptions;
	}
}
