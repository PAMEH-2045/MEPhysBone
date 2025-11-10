using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace VRC.Dynamics
{
	// Token: 0x0200000B RID: 11
	internal struct VRCConstraintJob : IJobParallelFor
	{
		// Token: 0x06000061 RID: 97 RVA: 0x00003E1C File Offset: 0x0000201C
		public unsafe void Execute(int index)
		{
			int num = this.targetConstraintIndices[index];
			VRCConstraintJobData vrcconstraintJobData = this.constraints[num];
			if (vrcconstraintJobData.PlayerLoopStage != this.playerLoopStage)
			{
				return;
			}
			if (vrcconstraintJobData.AttachedToAvatarClone && this.playerLoopStage != VRCConstraintPlayerLoopStage.PostLocalAvatarProcess)
			{
				return;
			}
			if (!vrcconstraintJobData.IsActive || !vrcconstraintJobData.Locked || (vrcconstraintJobData.Sources.Length == 0 && !vrcconstraintJobData.FreezeToWorld))
			{
				return;
			}
			TransformAccess transformAccess = this.transformDataBuffer[vrcconstraintJobData.TransformStartIndex];
			TransformAccess transformAccess2 = transformAccess;
			quaternion quaternion;
			float4x4 float4x;
			float3 float3One;
			if (vrcconstraintJobData.HasParentTransform)
			{
				TransformAccess transformAccess3 = this.transformDataBuffer[vrcconstraintJobData.TransformStartIndex + 1];
				quaternion = transformAccess3.rotation;
				float4x = transformAccess3.localToWorldMatrix;
				this.GetTransformLossyScale(float4x, quaternion, out float3One);
			}
			else
			{
				quaternion = quaternion.identity;
				float4x = float4x4.identity;
				float3One = VRCConstraintJob.Float3One;
			}
			float4x4 float4x2 = VRCConstraintJob.IsValidTrsMatrix(float4x) ? math.inverse(float4x) : float4x4.identity;
			bool flag = vrcconstraintJobData.PositionConstraintMode > VRCConstraintPositionMode.None;
			bool flag2 = vrcconstraintJobData.RotationConstraintMode > VRCConstraintRotationMode.None;
			bool flag3 = vrcconstraintJobData.ScaleConstraintMode > VRCConstraintScaleMode.None;
			if (!flag && !flag2 && !flag3)
			{
				return;
			}
			float3 @float = transformAccess.localPosition;
			quaternion quaternion2 = transformAccess.localRotation;
			float3 float2 = transformAccess.localScale;
			float3 float3 = flag ? vrcconstraintJobData.PositionConfig.AtRest : float3.zero;
			quaternion quaternion3 = flag2 ? quaternion.Euler(math.radians(vrcconstraintJobData.RotationConfig.AtRest), (math.RotationOrder)4) : quaternion.identity;
			float3 float4 = flag3 ? vrcconstraintJobData.ScaleConfig.AtRest : VRCConstraintJob.Float3One;
			float3 float5 = VRCConstraintJob.CalculateEulerZXY(quaternion2);
			if (vrcconstraintJobData.HasOriginalLocalEulersHint)
			{
				float3 originalLocalEulersHint = vrcconstraintJobData.OriginalLocalEulersHint;
				if (math.abs(float5.x - originalLocalEulersHint.x) < 0.00017453292f)
				{
					float5.x = originalLocalEulersHint.x;
				}
				if (math.abs(float5.y - originalLocalEulersHint.y) < 0.00017453292f)
				{
					float5.y = originalLocalEulersHint.y;
				}
				if (math.abs(float5.z - originalLocalEulersHint.z) < 0.00017453292f)
				{
					float5.z = originalLocalEulersHint.z;
				}
			}
			vrcconstraintJobData.OriginalLocalEulersHint = float5;
			vrcconstraintJobData.HasOriginalLocalEulersHint = true;
			if (vrcconstraintJobData.FreezeToWorld && !vrcconstraintJobData.FreezeToWorldHasTrs)
			{
				float3 frozenWorldPosition = transformAccess2.position;
				quaternion frozenWorldRotation = transformAccess2.rotation;
				float4x4 float4x3 = transformAccess2.localToWorldMatrix;
				float3 frozenWorldScale;
				this.GetTransformLossyScale(float4x3, frozenWorldRotation, out frozenWorldScale);
				VRCConstraintJob.CorrectQuaternion(ref frozenWorldRotation, frozenWorldScale);
				vrcconstraintJobData.FrozenWorldPosition = frozenWorldPosition;
				vrcconstraintJobData.FrozenWorldRotation = frozenWorldRotation;
				vrcconstraintJobData.FrozenWorldScale = frozenWorldScale;
			}
			vrcconstraintJobData.FreezeToWorldHasTrs = vrcconstraintJobData.FreezeToWorld;
			float3 float6 = flag ? float3 : @float;
			quaternion quaternion4 = flag2 ? quaternion3 : quaternion2;
			float3 float7 = flag3 ? float4 : float2;
			int num2 = 0;
			int num3 = 0;
			if (vrcconstraintJobData.FreezeToWorld)
			{
				VRCConstraintJob.TransformPoint(float4x2, vrcconstraintJobData.FrozenWorldPosition, out float6);
				quaternion quaternion5 = math.inverse(quaternion);
				VRCConstraintJob.CorrectQuaternion(ref quaternion5, float3One);
				quaternion4 = math.mul(quaternion5, vrcconstraintJobData.FrozenWorldRotation);
				if (!VRCConstraintJob.IsAnyAxisZero(float3One))
				{
					float7 = vrcconstraintJobData.FrozenWorldScale / float3One;
				}
			}
			else if (vrcconstraintJobData.TotalValidSourceWeight != 0f)
			{
				float3 float8 = float3.zero;
				quaternion quaternion6 = VRCConstraintJob.QuaternionZero;
				float3 float9 = VRCConstraintJob.Float3One;
				bool flag4 = false;
				VRCConstraintJobData.ConstraintSourceData* ptr = vrcconstraintJobData.Sources.Ptr;
				for (int i = 0; i < vrcconstraintJobData.Sources.Length; i++)
				{
					VRCConstraintJobData.ConstraintSourceData constraintSourceData = ptr[i];
					if (constraintSourceData.SourceExists)
					{
						num2++;
						if (!((vrcconstraintJobData.Sources.Length > 1) ? VRCConstraintJob.IsLowWeightSource(constraintSourceData, true) : (constraintSourceData.Weight == 0f)))
						{
							num3++;
							float3 float10;
							quaternion quaternion7;
							float3 float11;
							this.ProcessSource(vrcconstraintJobData, constraintSourceData, float4x, float4x2, quaternion, float3One, out float10, out quaternion7, out float11);
							float num4 = constraintSourceData.Weight / vrcconstraintJobData.TotalValidSourceWeight;
							float8 += float10 * num4;
							if (num4 > 0f)
							{
								if (!flag4)
								{
									quaternion6 = quaternion7;
									flag4 = true;
								}
								else
								{
									quaternion6 = math.nlerp(quaternion6, quaternion7, num4);
								}
							}
							float9 *= float11;
						}
					}
				}
				if (flag3)
				{
					this.FinalizeLocalScaleResult(vrcconstraintJobData, num3, float3One, ref float9);
				}
				float num5 = (num3 > 0) ? vrcconstraintJobData.GlobalWeight : 0f;
				if (num5 != 0f)
				{
					bool flag5 = vrcconstraintJobData.RotationConstraintMode == VRCConstraintRotationMode.AimTowardsPosition || vrcconstraintJobData.RotationConstraintMode == VRCConstraintRotationMode.LookAtPosition;
					if (flag || flag5)
					{
						if (num2 == 1)
						{
							float3 float12;
							if (flag5)
							{
								float12 = (vrcconstraintJobData.SolveInLocalSpace ? (-@float) : (-transformAccess.position));
							}
							else if (vrcconstraintJobData.SolveInLocalSpace)
							{
								float12 = vrcconstraintJobData.PositionConfig.Offset;
							}
							else
							{
								float3 float13 = math.mul(quaternion, vrcconstraintJobData.PositionConfig.Offset);
								VRCConstraintJob.TransformPoint(float4x2, float13, out float12);
							}
							float8 = math.lerp(float12, float8, vrcconstraintJobData.TotalValidSourceWeight);
						}
						float6 = math.lerp(float3, float8, num5);
					}
					if (flag5 && !VRCConstraintJob.ConstraintHasSingleLowWeightSource(vrcconstraintJobData))
					{
						this.PerformAimAt(vrcconstraintJobData, float6, quaternion, float3One, float4x, out quaternion6);
					}
					bool flag6 = false;
					if (flag2 && !flag5 && num3 == 1 && vrcconstraintJobData.TotalValidSourceWeight < 0f)
					{
						bool flag7 = vrcconstraintJobData.PositionConstraintMode == VRCConstraintPositionMode.ChildPosition;
						quaternion quaternion8 = flag7 ? quaternion.identity : quaternion.Euler(math.radians(vrcconstraintJobData.RotationConfig.Offset), (math.RotationOrder)4);
						if (!flag7)
						{
							VRCConstraintJob.CorrectQuaternion(ref quaternion, float3One);
						}
						quaternion6 = math.mul(math.inverse(quaternion), quaternion8);
						if (flag7)
						{
							VRCConstraintJob.CorrectQuaternion(ref quaternion6, float3One);
						}
						flag6 = true;
					}
					else
					{
						float num6 = math.lengthsq(quaternion6);
						if (flag2 && num6 > 0f)
						{
							if (math.abs(num6 - 1f) > 0.0001f)
							{
								quaternion6 = math.normalize(quaternion6);
							}
							flag6 = true;
						}
					}
					if (flag6)
					{
						if (num5 >= 0f && num5 <= 1f)
						{
							quaternion4 = math.nlerp(quaternion3, quaternion6, num5);
						}
						else
						{
							quaternion4 = math.slerp(quaternion3, quaternion6, num5);
						}
					}
					if (flag3)
					{
						float7 = math.lerp(float4, float9, num5);
					}
				}
			}
			if (num3 > 0 || vrcconstraintJobData.FreezeToWorld)
			{
				if (flag && vrcconstraintJobData.PositionConfig.Axes != VRCConstraintBase.Axis.All)
				{
					if ((vrcconstraintJobData.PositionConfig.Axes & VRCConstraintBase.Axis.X) == VRCConstraintBase.Axis.None)
					{
						float6.x = @float.x;
					}
					if ((vrcconstraintJobData.PositionConfig.Axes & VRCConstraintBase.Axis.Y) == VRCConstraintBase.Axis.None)
					{
						float6.y = @float.y;
					}
					if ((vrcconstraintJobData.PositionConfig.Axes & VRCConstraintBase.Axis.Z) == VRCConstraintBase.Axis.None)
					{
						float6.z = @float.z;
					}
				}
				if (flag2 && vrcconstraintJobData.RotationConfig.Axes != VRCConstraintBase.Axis.All)
				{
					float3 float14 = VRCConstraintJob.CalculateEulerZXY(quaternion4);
					if ((vrcconstraintJobData.RotationConfig.Axes & VRCConstraintBase.Axis.X) == VRCConstraintBase.Axis.None)
					{
						float14.x = float5.x;
					}
					if ((vrcconstraintJobData.RotationConfig.Axes & VRCConstraintBase.Axis.Y) == VRCConstraintBase.Axis.None)
					{
						float14.y = float5.y;
					}
					if ((vrcconstraintJobData.RotationConfig.Axes & VRCConstraintBase.Axis.Z) == VRCConstraintBase.Axis.None)
					{
						float14.z = float5.z;
					}
					quaternion4 = quaternion.EulerZXY(float14);
				}
				if (flag3 && vrcconstraintJobData.ScaleConfig.Axes != VRCConstraintBase.Axis.All)
				{
					if ((vrcconstraintJobData.ScaleConfig.Axes & VRCConstraintBase.Axis.X) == VRCConstraintBase.Axis.None)
					{
						float7.x = float2.x;
					}
					if ((vrcconstraintJobData.ScaleConfig.Axes & VRCConstraintBase.Axis.Y) == VRCConstraintBase.Axis.None)
					{
						float7.y = float2.y;
					}
					if ((vrcconstraintJobData.ScaleConfig.Axes & VRCConstraintBase.Axis.Z) == VRCConstraintBase.Axis.None)
					{
						float7.z = float2.z;
					}
				}
			}
			if (flag && flag2)
			{
				transformAccess2.SetLocalPositionAndRotation(float6, quaternion4);
			}
			else if (flag)
			{
				transformAccess2.localPosition = float6;
			}
			else if (flag2)
			{
				transformAccess2.localRotation = quaternion4;
			}
			if (flag3)
			{
				transformAccess2.localScale = float7;
			}
			this.constraints[num] = vrcconstraintJobData;
			this.transformDataBuffer[vrcconstraintJobData.TransformStartIndex] = transformAccess2;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00004628 File Offset: 0x00002828
		[BurstCompile]
		private void ProcessSource(in VRCConstraintJobData constraint, in VRCConstraintJobData.ConstraintSourceData source, in float4x4 originalParentMatrix, in float4x4 originalParentInverse, in quaternion parentWorldRotation, in float3 parentLossyScale, out float3 resultLocalPosition, out quaternion resultLocalRotation, out float3 resultLocalScale)
		{
			TransformAccess transformAccess = this.transformDataBuffer[constraint.TransformStartIndex];
			TransformAccess transformAccess2 = this.transformDataBuffer[constraint.TransformStartIndex + source.SourceIndex];
			resultLocalPosition = transformAccess.localPosition;
			resultLocalRotation = transformAccess.localRotation;
			resultLocalScale = transformAccess.localScale;
			if (!constraint.IsActive)
			{
				return;
			}
			if (constraint.PositionConstraintMode != VRCConstraintPositionMode.None && constraint.PositionConfig.Axes != VRCConstraintBase.Axis.None)
			{
				VRCConstraintPositionMode positionConstraintMode = constraint.PositionConstraintMode;
				if (positionConstraintMode != VRCConstraintPositionMode.MatchPosition)
				{
					if (positionConstraintMode == VRCConstraintPositionMode.ChildPosition)
					{
						float4x4 float4x;
						if (constraint.SolveInLocalSpace)
						{
							float4x = float4x4.TRS(transformAccess2.localPosition, transformAccess2.localRotation, VRCConstraintJob.Float3One);
						}
						else
						{
							float4x = float4x4.TRS(transformAccess2.position, transformAccess2.rotation, VRCConstraintJob.Float3One);
							float4x = math.mul(originalParentInverse, float4x);
						}
						resultLocalPosition = math.transform(float4x, source.ParentPositionOffset);
					}
				}
				else
				{
					float3 @float = constraint.SolveInLocalSpace ? transformAccess2.localPosition : transformAccess2.position;
					float3 float2;
					if (!constraint.SolveInLocalSpace)
					{
						VRCConstraintJob.TransformPoint(originalParentInverse, @float, out float2);
					}
					else
					{
						float2 = @float;
					}
					resultLocalPosition = float2 + constraint.PositionConfig.Offset;
				}
			}
			if (constraint.RotationConstraintMode != VRCConstraintRotationMode.None && constraint.RotationConfig.Axes != VRCConstraintBase.Axis.None)
			{
				VRCConstraintRotationMode rotationConstraintMode = constraint.RotationConstraintMode;
				if (rotationConstraintMode != VRCConstraintRotationMode.MatchRotation)
				{
					if (rotationConstraintMode - VRCConstraintRotationMode.AimTowardsPosition <= 1)
					{
						float3 float3;
						if (!constraint.SolveInLocalSpace)
						{
							float3 = transformAccess2.position - transformAccess.position;
						}
						else
						{
							float3 = transformAccess2.localPosition - transformAccess.localPosition;
							float3x3 float3x = new(originalParentMatrix);
							VRCConstraintJob.TransformPoint(float3x, float3, out float3);
						}
						resultLocalPosition = float3;
					}
				}
				else
				{
					quaternion quaternion;
					if (VRCConstraintJob.ConstraintHasSingleLowWeightSource(constraint))
					{
						quaternion = quaternion.identity;
					}
					else
					{
						quaternion = (constraint.SolveInLocalSpace ? transformAccess2.localRotation : transformAccess2.rotation);
						float4 value = quaternion.value;
						if (math.csum(value) < 0f)
						{
							quaternion.value = -value;
						}
					}
					float3 float4;
					if (constraint.PositionConstraintMode != VRCConstraintPositionMode.ChildPosition)
					{
						float4 = constraint.RotationConfig.Offset;
					}
					else
					{
						float4 = source.ParentRotationOffset;
					}
					quaternion quaternion2 = quaternion.Euler(math.radians(float4), (math.RotationOrder)4);
					if (constraint.SolveInLocalSpace)
					{
						VRCConstraintJob.CorrectQuaternion(ref quaternion2, parentLossyScale);
					}
					quaternion quaternion3 = math.mul(quaternion, quaternion2);
					if (constraint.SolveInLocalSpace)
					{
						resultLocalRotation = quaternion3;
					}
					else
					{
						resultLocalRotation = math.mul(math.inverse(parentWorldRotation), quaternion3);
						VRCConstraintJob.CorrectQuaternion(ref resultLocalRotation, parentLossyScale);
					}
				}
			}
			if (constraint.ScaleConstraintMode != VRCConstraintScaleMode.None && constraint.ScaleConfig.Axes != VRCConstraintBase.Axis.None && constraint.ScaleConstraintMode == VRCConstraintScaleMode.MatchScale && constraint.TotalValidSourceWeight != 0f)
			{
				float3 float5;
				if (constraint.SolveInLocalSpace)
				{
					float5 = transformAccess2.localScale;
				}
				else
				{
					float4x4 float4x2 = transformAccess2.localToWorldMatrix;
					quaternion quaternion4 = transformAccess2.rotation;
					this.GetTransformLossyScale(float4x2, quaternion4, out float5);
				}
				float num = source.Weight / constraint.TotalValidSourceWeight;
				float3 float6 = float5;
				float3 float7 = math.sign(float6);
				resultLocalScale = float7 * math.pow(float7 * float6, num);
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000049C0 File Offset: 0x00002BC0
		[BurstCompile]
		private void FinalizeLocalScaleResult(in VRCConstraintJobData constraint, in int validSourceCount, in float3 parentLossyScale, ref float3 totalLocalScale)
		{
			float3 offset = constraint.ScaleConfig.Offset;
			float3 @float = VRCConstraintJob.Float3One;
			for (int i = 0; i < constraint.Sources.Length; i++)
			{
				UnsafeList<VRCConstraintJobData.ConstraintSourceData> sources = constraint.Sources;
				VRCConstraintJobData.ConstraintSourceData constraintSourceData = sources[i];
				if (constraintSourceData.SourceExists)
				{
					float num;
					if (validSourceCount > 1)
					{
						num = constraintSourceData.Weight / constraint.TotalValidSourceWeight * ((float)validSourceCount / constraint.TotalValidSourceWeight);
					}
					else
					{
						num = ((constraintSourceData.Weight != 0f) ? (1f / constraintSourceData.Weight) : 1f);
					}
					float3 float2 = math.sign(offset);
					@float *= float2 * math.pow(float2 * offset, num);
				}
			}
			totalLocalScale *= @float;
			if (!constraint.SolveInLocalSpace && !VRCConstraintJob.IsAnyAxisZero(parentLossyScale))
			{
				totalLocalScale /= parentLossyScale;
			}
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00004AC0 File Offset: 0x00002CC0
		[BurstCompile]
		private void PerformAimAt(in VRCConstraintJobData constraint, in float3 toSource, in quaternion parentWorldRotation, in float3 parentLossyScale, in float4x4 originalParentMatrix, out quaternion resultQuaternion)
		{
			TransformAccess transformAccess = this.transformDataBuffer[constraint.TransformStartIndex];
			bool flag = constraint.WorldUpTransformIndex >= 0;
			TransformAccess transformAccess2 = flag ? this.transformDataBuffer[constraint.TransformStartIndex + constraint.WorldUpTransformIndex] : default(TransformAccess);
			bool flag2 = constraint.RotationConstraintMode == VRCConstraintRotationMode.LookAtPosition;
			VRCConstraintBase.WorldUpType worldUpType;
			if (!flag2)
			{
				worldUpType = constraint.WorldUpType;
			}
			else
			{
				worldUpType = (constraint.UseUpTransform ? VRCConstraintBase.WorldUpType.ObjectUp : VRCConstraintBase.WorldUpType.SceneUp);
			}
			float3 @float;
			switch (worldUpType)
			{
			case VRCConstraintBase.WorldUpType.SceneUp:
				@float = VRCConstraintJob.Float3Up;
				goto IL_12C;
			case VRCConstraintBase.WorldUpType.ObjectUp:
			{
				float3 float2 = flag ? (constraint.SolveInLocalSpace ? transformAccess2.localPosition : transformAccess2.position) : Unity.Mathematics.float3.zero;
				@float = (constraint.SolveInLocalSpace ? (float2 - (float3)transformAccess.localPosition) : (float2 - (float3)transformAccess.position));
				goto IL_12C;
			}
			case VRCConstraintBase.WorldUpType.ObjectRotationUp:
				@float = (flag ? math.mul(transformAccess2.rotation, constraint.WorldUpVector) : constraint.WorldUpVector);
				goto IL_12C;
			case VRCConstraintBase.WorldUpType.Vector:
				@float = constraint.WorldUpVector;
				goto IL_12C;
			}
			@float = Unity.Mathematics.float3.zero;
			IL_12C:
			if (constraint.SolveInLocalSpace)
			{
				float3x3 float3x = new(originalParentMatrix);
				VRCConstraintJob.TransformPoint(float3x, @float, out @float);
			}
			float3 float3 = math.normalizesafe(constraint.AimAxis, VRCConstraintJob.Float3Forward);
			bool flag3;
			quaternion quaternion;
			if (math.lengthsq(toSource) != 0f && (flag2 || (math.lengthsq(constraint.AimAxis) != 0f && (math.lengthsq(constraint.UpAxis) != 0f || math.lengthsq(@float) <= 0f))))
			{
				flag3 = (math.lengthsq(@float) == 0f || math.lengthsq(math.cross(toSource, @float)) == 0f);
				if (flag3)
				{
					if (!flag2)
					{
						float3 float4 = math.normalize(toSource);
						VRCConstraintJob.FromToRotation(float3, float4, out quaternion);
					}
					else
					{
						quaternion = Quaternion.LookRotation(toSource, @float);
					}
				}
				else
				{
					quaternion = quaternion.LookRotationSafe(toSource, @float);
				}
			}
			else
			{
				quaternion = quaternion.identity;
				flag3 = true;
			}
			if (!flag3)
			{
				if (!flag2)
				{
					quaternion quaternion2;
					VRCConstraintJob.FromToRotation(float3, VRCConstraintJob.Float3Forward, out quaternion2);
					float3 float5 = math.mul(quaternion2, constraint.UpAxis);
					float num = math.atan2(float5.x, float5.y);
					quaternion2 = math.mul(quaternion2, quaternion.AxisAngle(float3, num));
					quaternion = math.mul(quaternion, quaternion2);
				}
				else
				{
					quaternion quaternion3 = quaternion.AxisAngle(VRCConstraintJob.Float3Forward, math.radians(-constraint.Roll));
					quaternion = math.mul(quaternion, quaternion3);
				}
			}
			resultQuaternion = math.mul(quaternion, quaternion.Euler(math.radians(constraint.RotationConfig.Offset), (math.RotationOrder)4));
			resultQuaternion = math.mul(math.inverse(parentWorldRotation), resultQuaternion);
			VRCConstraintJob.CorrectQuaternion(ref resultQuaternion, parentLossyScale);
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00004DD0 File Offset: 0x00002FD0
		[BurstCompile]
		[MethodImpl(256)]
		private static void TransformPoint(in float4x4 m, in float3 p, out float3 result)
		{
			result = math.mul(m, new float4(p, 1f)).xyz;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00004E06 File Offset: 0x00003006
		[BurstCompile]
		[MethodImpl(256)]
		private static void TransformPoint(in float3x3 m, in float3 p, out float3 result)
		{
			result = math.mul(m, p);
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00004E1F File Offset: 0x0000301F
		[BurstCompile]
		[MethodImpl(256)]
		private static bool IsAnyAxisZero(in float3 v)
		{
			return v.x == 0f || v.y == 0f || v.z == 0f;
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00004E4C File Offset: 0x0000304C
		[BurstCompile]
		[MethodImpl(256)]
		private static bool IsValidTrsMatrix(in float4x4 m)
		{
			float num = math.determinant(m);
			return !math.isnan(num) && num != 0f;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00004E7C File Offset: 0x0000307C
		[BurstCompile]
		private static bool ConstraintHasSingleLowWeightSource(in VRCConstraintJobData constraint)
		{
			if (constraint.Sources.Length != 1)
			{
				return false;
			}
			UnsafeList<VRCConstraintJobData.ConstraintSourceData> sources = constraint.Sources;
			VRCConstraintJobData.ConstraintSourceData constraintSourceData = sources[0];
			return VRCConstraintJob.IsLowWeightSource(constraintSourceData, false);
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00004EB4 File Offset: 0x000030B4
		[BurstCompile]
		private static bool IsLowWeightSource(in VRCConstraintJobData.ConstraintSourceData source, bool lessOrEqual)
		{
			float num = math.abs(source.Weight);
			if (lessOrEqual)
			{
				return num <= 1E-06f;
			}
			return num < 1E-06f;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00004EE4 File Offset: 0x000030E4
		[BurstCompile]
		private static void CorrectQuaternion(ref quaternion q, in float3 lossyScale)
		{
			if (math.cmin(lossyScale) >= 0f || math.cmax(lossyScale) < 0f)
			{
				return;
			}
			float4 value = q.value;
			if (lossyScale.x < 0f)
			{
				value.y = -value.y;
				value.z = -value.z;
			}
			if (lossyScale.y < 0f)
			{
				value.x = -value.x;
				value.z = -value.z;
			}
			if (lossyScale.z < 0f)
			{
				value.x = -value.x;
				value.y = -value.y;
			}
			q.value = value;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00004FA8 File Offset: 0x000031A8
		[BurstCompile]
		private void GetTransformLossyScale(in float4x4 localToWorldMatrix, in quaternion worldRotation, out float3 lossyScale)
		{
			float3x3 float3x = new(localToWorldMatrix);
			float3x = math.mul(new float3x3(math.inverse(worldRotation)), float3x);
			lossyScale = new float3(float3x.c0.x, float3x.c1.y, float3x.c2.z);
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00005008 File Offset: 0x00003208
		[BurstCompile]
		private static void FromToRotation(in float3 fromNormalized, in float3 toNormalized, out quaternion fromTo)
		{
			float num = math.dot(fromNormalized, toNormalized);
			if (num >= 1f)
			{
				fromTo = quaternion.identity;
				return;
			}
			if (num > -1f)
			{
				fromTo = math.normalizesafe(new quaternion(new float4(math.cross(fromNormalized, toNormalized), num + 1f)));
				return;
			}
			float3 @float = math.cross(fromNormalized, VRCConstraintJob.Float3Right);
			if (math.lengthsq(@float) > 0f)
			{
				fromTo = quaternion.AxisAngle(math.normalize(@float), 3.1415927f);
				return;
			}
			fromTo = quaternion.RotateY(3.1415927f);
		}

		// Token: 0x0600006E RID: 110 RVA: 0x000050B8 File Offset: 0x000032B8
		[MethodImpl(256)]
		private static float3 CalculateEulerZXY(quaternion q)
		{
			float4 value = q.value;
			float4 @float = value * value.wwww * new float4(2f);
			float4 float2 = value * value.yzxw * new float4(2f);
			float4 float3 = value * value;
			float num = float2.y - @float.x;
			float3 float4;
			if (num * num < 0.99999595f)
			{
				float num2 = float2.x + @float.z;
				float num3 = float3.y + float3.w - float3.x - float3.z;
				float num4 = float2.z + @float.y;
				float num5 = float3.z + float3.w - float3.x - float3.y;
				float4 = new(math.atan2(num2, num3), -math.asin(num), math.atan2(num4, num5));
			}
			else
			{
				num = math.clamp(num, -1f, 1f);
				float4 float5;
				float5 = new(float2.z, @float.y, float2.y, @float.x);
				float num6 = 2f * (float5.x * float5.w + float5.y * float5.z);
				float num7 = math.csum(float5 * float5 * new float4(-1f, 1f, -1f, 1f));
				float4 = new(math.atan2(num6, num7), -math.asin(num), 0f);
			}
			return float4.yzx;
		}

		// Token: 0x04000039 RID: 57
		[ReadOnly]
		public VRCConstraintPlayerLoopStage playerLoopStage;

		// Token: 0x0400003A RID: 58
		public UnsafeList<int> targetConstraintIndices;

		// Token: 0x0400003B RID: 59
		public UnsafeList<VRCConstraintJobData> constraints;

		// Token: 0x0400003C RID: 60
		public UnsafeList<TransformAccess> transformDataBuffer;

		// Token: 0x0400003D RID: 61
		[ReadOnly]
		private static readonly float3 Float3Right = new float3(1f, 0f, 0f);

		// Token: 0x0400003E RID: 62
		[ReadOnly]
		private static readonly float3 Float3Up = new float3(0f, 1f, 0f);

		// Token: 0x0400003F RID: 63
		[ReadOnly]
		private static readonly float3 Float3Forward = new float3(0f, 0f, 1f);

		// Token: 0x04000040 RID: 64
		[ReadOnly]
		private static readonly float3 Float3One = new float3(1f, 1f, 1f);

		// Token: 0x04000041 RID: 65
		[ReadOnly]
		private static readonly quaternion QuaternionZero = new quaternion(0f, 0f, 0f, 0f);
	}
}
