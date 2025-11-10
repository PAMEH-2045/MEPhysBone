using System;
using Unity.Burst;
using Unity.Mathematics;

namespace VRC.Dynamics
{
	// Token: 0x02000006 RID: 6
	[BurstCompile]
	public static class CollisionShapes
	{
		// Token: 0x06000038 RID: 56 RVA: 0x000033A4 File Offset: 0x000015A4
		public static bool CheckCollision(CollisionShapes.CollisionShape shapeA, CollisionShapes.CollisionShape shapeB)
		{
			if (shapeB.shapeType == CollisionScene.ShapeType.None)
			{
				return false;
			}
			bool result = false;
			switch (shapeA.shapeType)
			{
			case CollisionScene.ShapeType.AABB:
				result = CollisionShapes.CheckCollision_AABB(shapeA.ToAABB(), shapeB);
				break;
			case CollisionScene.ShapeType.Sphere:
				result = CollisionShapes.CheckCollision_Sphere(shapeA.ToSphere(), shapeB);
				break;
			case CollisionScene.ShapeType.Capsule:
			case CollisionScene.ShapeType.Finger:
				result = CollisionShapes.CheckCollision_Capsule(shapeA.ToCapsule(), shapeB);
				break;
			}
			return result;
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00003410 File Offset: 0x00001610
		public static bool CheckCollision_AABB(CollisionShapes.AABB shapeA, CollisionShapes.CollisionShape shapeB)
		{
			switch (shapeB.shapeType)
			{
			case CollisionScene.ShapeType.AABB:
				return true;
			case CollisionScene.ShapeType.Sphere:
				return CollisionShapes.Overlap_AABB_Sphere(shapeA, shapeB.ToSphere());
			case CollisionScene.ShapeType.Capsule:
			case CollisionScene.ShapeType.Finger:
				return CollisionShapes.Overlap_AABB_Capsule(shapeA, shapeB.ToCapsule());
			default:
				return false;
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003460 File Offset: 0x00001660
		public static bool CheckCollision_Sphere(CollisionShapes.Sphere shapeA, CollisionShapes.CollisionShape shapeB)
		{
			switch (shapeB.shapeType)
			{
			case CollisionScene.ShapeType.AABB:
				return CollisionShapes.Overlap_AABB_Sphere(shapeB.ToAABB(), shapeA);
			case CollisionScene.ShapeType.Sphere:
				return CollisionShapes.Overlap_Sphere_Sphere(shapeA, shapeB.ToSphere());
			case CollisionScene.ShapeType.Capsule:
			case CollisionScene.ShapeType.Finger:
				return CollisionShapes.Overlap_Sphere_Capsule(shapeA, shapeB.ToCapsule());
			default:
				return false;
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x000034BC File Offset: 0x000016BC
		public static bool CheckCollision_Capsule(CollisionShapes.Capsule shapeA, CollisionShapes.CollisionShape shapeB)
		{
			switch (shapeB.shapeType)
			{
			case CollisionScene.ShapeType.AABB:
				return CollisionShapes.Overlap_AABB_Capsule(shapeB.ToAABB(), shapeA);
			case CollisionScene.ShapeType.Sphere:
				return CollisionShapes.Overlap_Sphere_Capsule(shapeB.ToSphere(), shapeA);
			case CollisionScene.ShapeType.Capsule:
			case CollisionScene.ShapeType.Finger:
				return CollisionShapes.Overlap_Capsule_Capsule(shapeA, shapeB.ToCapsule());
			default:
				return false;
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003515 File Offset: 0x00001715
		public static bool Overlap_AABB_Sphere(CollisionShapes.AABB shapeA, CollisionShapes.Sphere shapeB)
		{
			return true;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003518 File Offset: 0x00001718
		public static bool Overlap_AABB_Capsule(CollisionShapes.AABB shapeA, CollisionShapes.Capsule shapeB)
		{
			return true;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000351C File Offset: 0x0000171C
		public static bool Overlap_Sphere_Sphere(CollisionShapes.Sphere shapeA, CollisionShapes.Sphere shapeB)
		{
			float num = shapeA.radius + shapeB.radius;
			return math.lengthsq(shapeA.position - shapeB.position) <= num * num;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00003558 File Offset: 0x00001758
		public static bool Overlap_Sphere_Capsule(CollisionShapes.Sphere shapeA, CollisionShapes.Capsule shapeB)
		{
			float3 @float = MathUtil.ClosestPointOnLineSegment(shapeB.pos0, shapeB.pos1, shapeA.position);
			float num = shapeA.radius + shapeB.radius;
			return math.lengthsq(shapeA.position - @float) <= num * num;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000035A4 File Offset: 0x000017A4
		public static bool Overlap_Capsule_Capsule(CollisionShapes.Capsule shapeA, CollisionShapes.Capsule shapeB)
		{
			float3 @float;
			float3 float2;
			MathUtil.ClosestPointsBetweenLineSegments(shapeA.pos0, shapeA.pos1, shapeB.pos0, shapeB.pos1, out @float, out float2);
			float num = shapeA.radius + shapeB.radius;
			return math.lengthsq(@float - float2) <= num * num;
		}

		// Token: 0x0200004D RID: 77
		[BurstCompile]
		public struct CollisionShape
		{
			// Token: 0x060002AE RID: 686 RVA: 0x0001238C File Offset: 0x0001058C
			internal CollisionShapes.AABB ToAABB()
			{
				return new CollisionShapes.AABB
				{
					pos0 = this.pos0,
					pos1 = this.pos1
				};
			}

			// Token: 0x060002AF RID: 687 RVA: 0x000123BC File Offset: 0x000105BC
			internal CollisionShapes.Sphere ToSphere()
			{
				return new CollisionShapes.Sphere
				{
					position = this.pos0,
					radius = this.radius
				};
			}

			// Token: 0x060002B0 RID: 688 RVA: 0x000123EC File Offset: 0x000105EC
			internal CollisionShapes.Capsule ToCapsule()
			{
				return new CollisionShapes.Capsule
				{
					pos0 = this.pos0,
					pos1 = this.pos1,
					radius = this.radius
				};
			}

			// Token: 0x060002B1 RID: 689 RVA: 0x0001242C File Offset: 0x0001062C
			internal CollisionShapes.Plane ToPlane()
			{
				return new CollisionShapes.Plane
				{
					position = this.pos0,
					normal = this.pos1
				};
			}

			// Token: 0x0400025F RID: 607
			public CollisionScene.ShapeType shapeType;

			// Token: 0x04000260 RID: 608
			public float3 pos0;

			// Token: 0x04000261 RID: 609
			public float3 pos1;

			// Token: 0x04000262 RID: 610
			public float radius;
		}

		// Token: 0x0200004E RID: 78
		[BurstCompile]
		public struct AABB
		{
			// Token: 0x060002B2 RID: 690 RVA: 0x0001245C File Offset: 0x0001065C
			public float3 ClosestPoint(float3 point)
			{
				return math.clamp(point, this.pos0, this.pos1);
			}

			// Token: 0x04000263 RID: 611
			public float3 pos0;

			// Token: 0x04000264 RID: 612
			public float3 pos1;
		}

		// Token: 0x0200004F RID: 79
		[BurstCompile]
		public struct Sphere
		{
			// Token: 0x060002B3 RID: 691 RVA: 0x00012470 File Offset: 0x00010670
			public float3 ClosestPoint(float3 point)
			{
				float3 @float = point - this.position;
				float num = math.length(@float);
				if (num > this.radius)
				{
					return this.position + @float / num * this.radius;
				}
				return point;
			}

			// Token: 0x04000265 RID: 613
			public float3 position;

			// Token: 0x04000266 RID: 614
			public float radius;
		}

		// Token: 0x02000050 RID: 80
		[BurstCompile]
		public struct Capsule
		{
			// Token: 0x060002B4 RID: 692 RVA: 0x000124BC File Offset: 0x000106BC
			public float3 ClosestPoint(float3 point)
			{
				float3 @float = MathUtil.ClosestPointOnLineSegment(this.pos0, this.pos1, point);
				float3 float2 = point - @float;
				float num = math.length(float2);
				if (num > this.radius)
				{
					return @float + float2 / num * this.radius;
				}
				return point;
			}

			// Token: 0x04000267 RID: 615
			public float3 pos0;

			// Token: 0x04000268 RID: 616
			public float3 pos1;

			// Token: 0x04000269 RID: 617
			public float radius;
		}

		// Token: 0x02000051 RID: 81
		[BurstCompile]
		public struct Plane
		{
			// Token: 0x060002B5 RID: 693 RVA: 0x0001250E File Offset: 0x0001070E
			public float3 ClosestPoint(float3 point)
			{
				return MathUtil.ClosestPointOnPlane(this.position, this.normal, point);
			}

			// Token: 0x0400026A RID: 618
			public float3 position;

			// Token: 0x0400026B RID: 619
			public float3 normal;
		}
	}
}
