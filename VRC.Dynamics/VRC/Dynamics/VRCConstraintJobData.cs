using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace VRC.Dynamics
{
	// Token: 0x0200000C RID: 12
	public struct VRCConstraintJobData
	{
		// Token: 0x04000042 RID: 66
		public int TransformStartIndex;

		// Token: 0x04000043 RID: 67
		public VRCConstraintPositionMode PositionConstraintMode;

		// Token: 0x04000044 RID: 68
		public VRCConstraintRotationMode RotationConstraintMode;

		// Token: 0x04000045 RID: 69
		public VRCConstraintScaleMode ScaleConstraintMode;

		// Token: 0x04000046 RID: 70
		[MarshalAs(4)]
		public bool HasParentTransform;

		// Token: 0x04000047 RID: 71
		[MarshalAs(4)]
		public bool IsActive;

		// Token: 0x04000048 RID: 72
		public float GlobalWeight;

		// Token: 0x04000049 RID: 73
		[MarshalAs(4)]
		public bool SolveInLocalSpace;

		// Token: 0x0400004A RID: 74
		[MarshalAs(4)]
		public bool FreezeToWorld;

		// Token: 0x0400004B RID: 75
		[MarshalAs(4)]
		public bool FreezeToWorldHasTrs;

		// Token: 0x0400004C RID: 76
		public float3 FrozenWorldPosition;

		// Token: 0x0400004D RID: 77
		public quaternion FrozenWorldRotation;

		// Token: 0x0400004E RID: 78
		public float3 FrozenWorldScale;

		// Token: 0x0400004F RID: 79
		public VRCConstraintPlayerLoopStage PlayerLoopStage;

		// Token: 0x04000050 RID: 80
		[MarshalAs(4)]
		public bool AttachedToAvatarClone;

		// Token: 0x04000051 RID: 81
		[MarshalAs(4)]
		public bool Locked;

		// Token: 0x04000052 RID: 82
		public VRCConstraintJobData.ConstraintConfigurationData PositionConfig;

		// Token: 0x04000053 RID: 83
		public VRCConstraintJobData.ConstraintConfigurationData RotationConfig;

		// Token: 0x04000054 RID: 84
		public VRCConstraintJobData.ConstraintConfigurationData ScaleConfig;

		// Token: 0x04000055 RID: 85
		public float3 AimAxis;

		// Token: 0x04000056 RID: 86
		public float3 UpAxis;

		// Token: 0x04000057 RID: 87
		[MarshalAs(4)]
		public bool UseUpTransform;

		// Token: 0x04000058 RID: 88
		public float Roll;

		// Token: 0x04000059 RID: 89
		public VRCConstraintBase.WorldUpType WorldUpType;

		// Token: 0x0400005A RID: 90
		public float3 WorldUpVector;

		// Token: 0x0400005B RID: 91
		public int WorldUpTransformIndex;

		// Token: 0x0400005C RID: 92
		public float TotalValidSourceWeight;

		// Token: 0x0400005D RID: 93
		public float3 OriginalLocalEulersHint;

		// Token: 0x0400005E RID: 94
		[MarshalAs(4)]
		public bool HasOriginalLocalEulersHint;

		// Token: 0x0400005F RID: 95
		public UnsafeList<VRCConstraintJobData.ConstraintSourceData> Sources;

		// Token: 0x02000052 RID: 82
		public struct ConstraintSourceData
		{
			// Token: 0x0400026C RID: 620
			public int SourceIndex;

			// Token: 0x0400026D RID: 621
			[MarshalAs(4)]
			public bool SourceExists;

			// Token: 0x0400026E RID: 622
			public float Weight;

			// Token: 0x0400026F RID: 623
			public float3 ParentPositionOffset;

			// Token: 0x04000270 RID: 624
			public float3 ParentRotationOffset;
		}

		// Token: 0x02000053 RID: 83
		public struct ConstraintConfigurationData
		{
			// Token: 0x04000271 RID: 625
			public float3 AtRest;

			// Token: 0x04000272 RID: 626
			public float3 Offset;

			// Token: 0x04000273 RID: 627
			public VRCConstraintBase.Axis Axes;
		}
	}
}
