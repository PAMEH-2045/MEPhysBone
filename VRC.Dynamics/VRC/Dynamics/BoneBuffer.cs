using System;
using Unity.Collections;
using UnityEngine.Jobs;

namespace VRC.Dynamics
{
	// Token: 0x0200002E RID: 46
	internal class BoneBuffer : IMemoryBufferList
	{
		// Token: 0x06000194 RID: 404 RVA: 0x0000B3F8 File Offset: 0x000095F8
		public BoneBuffer(int capacity)
		{
			this.bones = new NativeList<PhysBoneManager.Bone>(capacity, (Allocator)4);
			this.transformData = new NativeList<PhysBoneManager.TransformData>(capacity, (Allocator)4);
			this.transformAccess = new NativeList<TransformAccess>(capacity, (Allocator)4);
			this.transformArray = new TransformAccessArray(capacity, -1);
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000195 RID: 405 RVA: 0x0000B44E File Offset: 0x0000964E
		// (set) Token: 0x06000196 RID: 406 RVA: 0x0000B45C File Offset: 0x0000965C
		public int Length
		{
			get
			{
				return this.bones.Length;
			}
			set
			{
				int length = this.Length;
				this.bones.Length = value;
				this.transformData.Length = value;
				this.transformAccess.Length = value;
				int num = value - length;
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						this.transformArray.Add(null);
					}
					return;
				}
				if (num < 0)
				{
					for (int j = 0; j < -num; j++)
					{
						this.transformArray.RemoveAtSwapBack(length - 1 - j);
					}
				}
			}
		}

		// Token: 0x06000197 RID: 407 RVA: 0x0000B4D8 File Offset: 0x000096D8
		public void Move(int to, int from, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				int num = to + i;
				int num2 = from + i;
				this.bones[num] = this.bones[num2];
				this.transformData[num] = this.transformData[num2];
				this.transformAccess[num] = this.transformAccess[num2];
				this.transformArray[num] = this.transformArray[num2];
			}
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000B55C File Offset: 0x0000975C
		public void Dispose()
		{
			if (this.bones.IsCreated)
			{
				this.bones.Dispose();
			}
			if (this.transformData.IsCreated)
			{
				this.transformData.Dispose();
			}
			if (this.transformAccess.IsCreated)
			{
				this.transformAccess.Dispose();
			}
			if (this.transformArray.isCreated)
			{
				this.transformArray.Dispose();
			}
		}

		// Token: 0x06000199 RID: 409 RVA: 0x0000B5C9 File Offset: 0x000097C9
		public void Clear()
		{
			this.bones.Clear();
			this.transformData.Clear();
			this.transformAccess.Clear();
			this.transformArray.SetTransforms(null);
		}

		// Token: 0x0600019A RID: 410 RVA: 0x0000B5F8 File Offset: 0x000097F8
		public void Invalidate(int index, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				this.transformArray[index + i] = null;
			}
		}

		// Token: 0x0400011A RID: 282
		public NativeList<PhysBoneManager.Bone> bones;

		// Token: 0x0400011B RID: 283
		public NativeList<PhysBoneManager.TransformData> transformData;

		// Token: 0x0400011C RID: 284
		public NativeList<TransformAccess> transformAccess;

		// Token: 0x0400011D RID: 285
		public TransformAccessArray transformArray;
	}
}
