using System;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x0200001C RID: 28
	[Serializable]
	public struct VRCConstraintSource
	{
		// Token: 0x060000F6 RID: 246 RVA: 0x0000868E File Offset: 0x0000688E
		public VRCConstraintSource(Transform transform, float weight)
		{
			this = new VRCConstraintSource(transform, weight, Vector3.zero, Vector3.zero);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x000086A2 File Offset: 0x000068A2
		public VRCConstraintSource(Transform transform, float weight, Vector3 parentPositionOffset, Vector3 parentRotationOffset)
		{
			this.SourceTransform = transform;
			this.Weight = weight;
			this.ParentPositionOffset = parentPositionOffset;
			this.ParentRotationOffset = parentRotationOffset;
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x000086C1 File Offset: 0x000068C1
		public static VRCConstraintSource CreateDefault()
		{
			return new VRCConstraintSource(null, 1f, Vector3.zero, Vector3.zero);
		}

		// Token: 0x040000B5 RID: 181
		public Transform SourceTransform;

		// Token: 0x040000B6 RID: 182
		public float Weight;

		// Token: 0x040000B7 RID: 183
		public Vector3 ParentPositionOffset;

		// Token: 0x040000B8 RID: 184
		public Vector3 ParentRotationOffset;
	}
}
