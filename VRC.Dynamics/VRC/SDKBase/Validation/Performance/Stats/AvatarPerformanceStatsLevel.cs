using System;
using UnityEngine;

namespace VRC.SDKBase.Validation.Performance.Stats
{
	// Token: 0x0200006D RID: 109
	public class AvatarPerformanceStatsLevel : ScriptableObject
	{
		// Token: 0x04000358 RID: 856
		public int polyCount;

		// Token: 0x04000359 RID: 857
		public Bounds aabb;

		// Token: 0x0400035A RID: 858
		public int skinnedMeshCount;

		// Token: 0x0400035B RID: 859
		public int meshCount;

		// Token: 0x0400035C RID: 860
		public int materialCount;

		// Token: 0x0400035D RID: 861
		public int animatorCount;

		// Token: 0x0400035E RID: 862
		public int boneCount;

		// Token: 0x0400035F RID: 863
		public int lightCount;

		// Token: 0x04000360 RID: 864
		public int particleSystemCount;

		// Token: 0x04000361 RID: 865
		public int particleTotalCount;

		// Token: 0x04000362 RID: 866
		public int particleMaxMeshPolyCount;

		// Token: 0x04000363 RID: 867
		public bool particleTrailsEnabled;

		// Token: 0x04000364 RID: 868
		public bool particleCollisionEnabled;

		// Token: 0x04000365 RID: 869
		public int trailRendererCount;

		// Token: 0x04000366 RID: 870
		public int lineRendererCount;

		// Token: 0x04000367 RID: 871
		public int clothCount;

		// Token: 0x04000368 RID: 872
		public int clothMaxVertices;

		// Token: 0x04000369 RID: 873
		public int physicsColliderCount;

		// Token: 0x0400036A RID: 874
		public int physicsRigidbodyCount;

		// Token: 0x0400036B RID: 875
		public int audioSourceCount;

		// Token: 0x0400036C RID: 876
		public float textureMegabytes;

		// Token: 0x0400036D RID: 877
		public AvatarPerformanceStats.PhysBoneStats physBone;

		// Token: 0x0400036E RID: 878
		public int contactCount;

		// Token: 0x0400036F RID: 879
		public int constraintsCount;

		// Token: 0x04000370 RID: 880
		public int constraintDepth;
	}
}
