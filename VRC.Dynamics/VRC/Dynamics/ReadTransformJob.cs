using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Jobs;

namespace VRC.Dynamics
{
	// Token: 0x0200000A RID: 10
	internal struct ReadTransformJob : IJobParallelForTransform
	{
		// Token: 0x06000060 RID: 96 RVA: 0x00003E0B File Offset: 0x0000200B
		public void Execute(int index, TransformAccess transform)
		{
			this.transformDataBuffer[index] = transform;
		}

		// Token: 0x04000038 RID: 56
		public UnsafeList<TransformAccess> transformDataBuffer;
	}
}
