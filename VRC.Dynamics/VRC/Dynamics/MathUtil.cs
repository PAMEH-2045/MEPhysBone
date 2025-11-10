using System;
using Unity.Burst;
using Unity.Mathematics;

namespace VRC.Dynamics
{
	// Token: 0x02000028 RID: 40
	[BurstCompile]
	internal class MathUtil
	{
		// Token: 0x06000169 RID: 361 RVA: 0x0000A757 File Offset: 0x00008957
		public static float DistancePointToPlane(float3 planeOrigin, float3 planeNormal, float3 point)
		{
			return math.dot(planeNormal, planeOrigin - point);
		}

		// Token: 0x0600016A RID: 362 RVA: 0x0000A766 File Offset: 0x00008966
		public static float3 ClosestPointOnPlane(float3 planeOrigin, float3 planeNormal, float3 point)
		{
			return point + planeNormal * math.dot(planeNormal, planeOrigin - point);
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0000A781 File Offset: 0x00008981
		public static float AngleBetweenTwoNormals(float3 normalA, float3 normalB)
		{
			return math.acos(math.clamp(math.dot(normalA, normalB), -1f, 1f)) * 57.29578f;
		}

		// Token: 0x0600016C RID: 364 RVA: 0x0000A7A4 File Offset: 0x000089A4
		public static float AngleBetweenTwoNormals(float3 planeNormal, float3 vectorA, float3 vectorB)
		{
			vectorA = math.normalizesafe(vectorA + planeNormal * math.dot(planeNormal, -vectorA), default(float3));
			vectorB = math.normalizesafe(vectorB + planeNormal * math.dot(planeNormal, -vectorB), default(float3));
			float num = math.acos(math.dot(vectorA, vectorB)) * 57.29578f;
			if (math.dot(math.cross(planeNormal, vectorA), vectorB) <= 0f)
			{
				return num;
			}
			return -num;
		}

		// Token: 0x0600016D RID: 365 RVA: 0x0000A82C File Offset: 0x00008A2C
		public static float2 CartesianToPolar(float3 normal)
		{
			float3 @float = math.normalizesafe(new float3(normal.x, 0f, normal.z), default(float3));
			float2 float2;
			float2.x = math.acos(math.dot(new float3(0f, 0f, 1f), @float));
			if (math.dot(new float3(1f, 0f, 0f), @float) < 0f)
			{
				float2.x = -float2.x;
			}
			float2.y = math.acos(math.dot(new float3(0f, 1f, 0f), normal)) - 1.5707964f;
			return float2 * 57.29578f;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x0000A8EC File Offset: 0x00008AEC
		public static float3 PolarToCartesian(float2 polar)
		{
			float3 @float = new(0f, 0f, 1f);
			polar *= 0.017453292f;
			return math.rotate(quaternion.Euler(polar.y, polar.x, 0f, (math.RotationOrder)4), @float);
		}

		// Token: 0x0600016F RID: 367 RVA: 0x0000A93C File Offset: 0x00008B3C
		public static float3 QuaternionToSwingTwist(quaternion rotation)
		{
			float4 value = rotation.value;
			if (value.w < 0f)
			{
				value.w = -value.w;
			}
			float num = value.x * value.x + value.w * value.w;
			float3 result;
			if (num < 1E-45f)
			{
				result.x = 0f;
				result.y = value.y;
				result.z = value.z;
			}
			else
			{
				float num2 = math.rsqrt(num);
				result.x = value.x * num2;
				result.y = (value.w * value.y + value.x * value.z) * num2;
				result.z = (value.w * value.z - value.x * value.y) * num2;
			}
			return result;
		}

		// Token: 0x06000170 RID: 368 RVA: 0x0000AA18 File Offset: 0x00008C18
		public static quaternion SwingTwistToQuaternion(float3 swingTwist)
		{
			quaternion quaternion = new quaternion(swingTwist.x, 0f, 0f, math.sqrt(math.max(0f, 1f - swingTwist.x * swingTwist.x)));
			quaternion quaternion2 = new(0f, swingTwist.y, swingTwist.z, math.sqrt(math.max(0f, 1f - swingTwist.y * swingTwist.y - swingTwist.z * swingTwist.z)));
			return math.mul(quaternion, quaternion2);
		}

		// Token: 0x06000171 RID: 369 RVA: 0x0000AAAC File Offset: 0x00008CAC
		public static float ClosestPointOnLineSegment_Ratio(float3 lineA, float3 lineB, float3 point)
		{
			float3 @float = lineB - lineA;
			float num = math.dot(point - lineA, @float);
			if (num <= 0f)
			{
				return 0f;
			}
			float num2 = math.dot(@float, @float);
			if (num2 <= num)
			{
				return 1f;
			}
			return num / num2;
		}

		// Token: 0x06000172 RID: 370 RVA: 0x0000AAF2 File Offset: 0x00008CF2
		public static float3 ClosestPointOnLineSegment(float3 lineA, float3 lineB, float3 point)
		{
			return math.lerp(lineA, lineB, MathUtil.ClosestPointOnLineSegment_Ratio(lineA, lineB, point));
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000AB04 File Offset: 0x00008D04
		public static void ClosestPointsBetweenLineSegments(float3 lineA0, float3 lineA1, float3 lineB0, float3 lineB1, out float3 resultA, out float3 resultB)
		{
			float3 @float = lineB0 - lineA0;
			float3 float2 = lineA1 - lineA0;
			float3 float3 = lineB1 - lineB0;
			float num = math.dot(@float, float2);
			float num2 = math.dot(@float, float3);
			float num3 = math.dot(float2, float2);
			float num4 = math.dot(float2, float3);
			float num5 = math.dot(float3, float3);
			float num6 = num3 * num5 - num4 * num4;
			float num7;
			float num8;
			if (num6 < 1E-45f)
			{
				num7 = math.saturate(num / num3);
				num8 = 0f;
			}
			else
			{
				num7 = math.saturate((num * num5 - num2 * num4) / num6);
				num8 = math.saturate((num * num4 - num2 * num3) / num6);
			}
			float num9 = math.saturate((num8 * num4 + num) / num3);
			float num10 = math.saturate((num7 * num4 - num2) / num5);
			resultA = lineA0 + num9 * float2;
			resultB = lineB0 + num10 * float3;
		}
	}
}
