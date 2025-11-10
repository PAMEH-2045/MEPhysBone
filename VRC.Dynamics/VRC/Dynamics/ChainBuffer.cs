using System;
using System.Collections.Generic;
using Unity.Collections;

namespace VRC.Dynamics
{
	// Token: 0x0200002D RID: 45
	internal class ChainBuffer : IMemoryBufferSpanList, IMemoryBufferList
	{
		// Token: 0x0600018A RID: 394 RVA: 0x0000B1B2 File Offset: 0x000093B2
		public ChainBuffer(int capacity)
		{
			this.chains = new NativeList<PhysBoneManager.Chain>(capacity, (Allocator)4);
			this.comps = new List<VRCPhysBoneBase>(capacity);
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600018B RID: 395 RVA: 0x0000B1D8 File Offset: 0x000093D8
		// (set) Token: 0x0600018C RID: 396 RVA: 0x0000B1E8 File Offset: 0x000093E8
		public int Length
		{
			get
			{
				return this.chains.Length;
			}
			set
			{
				int length = this.Length;
				this.chains.Length = value;
				int num = value - length;
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						this.comps.Add(null);
					}
					return;
				}
				if (num < 0)
				{
					for (int j = 0; j < -num; j++)
					{
						this.comps.RemoveAt(length - 1 - j);
					}
				}
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600018D RID: 397 RVA: 0x0000B24A File Offset: 0x0000944A
		public int Capacity
		{
			get
			{
				return this.chains.Capacity;
			}
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0000B258 File Offset: 0x00009458
		public void Move(int to, int from, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				int num = to + i;
				int num2 = from + i;
				PhysBoneManager.Chain chain = this.chains[num2];
				VRCPhysBoneBase vrcphysBoneBase = this.comps[num2];
				this.chains[num] = chain;
				this.comps[num] = vrcphysBoneBase;
				PhysBoneManager.Inst.MarkGroupListDirty(vrcphysBoneBase.ExecutionGroup);
			}
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0000B2C0 File Offset: 0x000094C0
		public void Dispose()
		{
			foreach (PhysBoneManager.Chain chain in this.chains)
			{
				chain.Dispose();
			}
			if (this.chains.IsCreated)
			{
				this.chains.Dispose();
			}
		}

		// Token: 0x06000190 RID: 400 RVA: 0x0000B32C File Offset: 0x0000952C
		public void Clear()
		{
			foreach (PhysBoneManager.Chain chain in this.chains)
			{
				chain.Dispose();
			}
			this.chains.Clear();
			this.comps.Clear();
		}

		// Token: 0x06000191 RID: 401 RVA: 0x0000B398 File Offset: 0x00009598
		public void UpdateSpan(int index, MemoryBuffer.MemorySpan span)
		{
			PhysBoneManager.Chain chain = this.chains[index];
			chain.boneOffset = span.dataIndex;
			chain.spanCount = span.dataLength;
			this.chains[index] = chain;
		}

		// Token: 0x06000192 RID: 402 RVA: 0x0000B3D9 File Offset: 0x000095D9
		public void RemoveAt(int index)
		{
			this.chains.RemoveAt(index);
			this.comps.RemoveAt(index);
		}

		// Token: 0x06000193 RID: 403 RVA: 0x0000B3F3 File Offset: 0x000095F3
		public void Invalidate(int index, int amount)
		{
		}

		// Token: 0x04000118 RID: 280
		public NativeList<PhysBoneManager.Chain> chains;

		// Token: 0x04000119 RID: 281
		public List<VRCPhysBoneBase> comps;
	}
}
