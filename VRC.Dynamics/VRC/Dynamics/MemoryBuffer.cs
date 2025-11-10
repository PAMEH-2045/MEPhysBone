using System;
using Unity.Collections;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x0200002B RID: 43
	public class MemoryBuffer
	{
		// Token: 0x0600017C RID: 380 RVA: 0x0000ABF8 File Offset: 0x00008DF8
		public MemoryBuffer(IMemoryBufferSpanList spanList, IMemoryBufferList dataList)
		{
			this.spanList = spanList;
			this.dataList = dataList;
			this.spanMap = new NativeHashMap<ChainId, int>(128, (Allocator)4);
			this.spans = new NativeList<MemoryBuffer.MemorySpan>(128, (Allocator)4);
			this.usedSpace = 0;
		}

		// Token: 0x0600017D RID: 381 RVA: 0x0000AC4C File Offset: 0x00008E4C
		public void Dispose()
		{
			if (this.spanMap.IsCreated)
			{
				this.spanMap.Dispose();
			}
			if (this.spans.IsCreated)
			{
				this.spans.Dispose();
			}
			this.spanList.Dispose();
			this.dataList.Dispose();
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000AC9F File Offset: 0x00008E9F
		public void Clear()
		{
			this.spanMap.Clear();
			this.spans.Clear();
			this.spanList.Clear();
			this.dataList.Clear();
			this.usedSpace = 0;
		}

		// Token: 0x0600017F RID: 383 RVA: 0x0000ACD4 File Offset: 0x00008ED4
		public bool ContainsId(ChainId id)
		{
			return this.spanMap.ContainsKey(id);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x0000ACE4 File Offset: 0x00008EE4
		public int FindIndex(ChainId id)
		{
			int result;
			if (this.spanMap.TryGetValue(id, out result))
			{
				return result;
			}
			return -1;
		}

		// Token: 0x06000181 RID: 385 RVA: 0x0000AD04 File Offset: 0x00008F04
		public int Request(ChainId id, int amount)
		{
			int finalDataIndex = this.GetFinalDataIndex();
			int num = finalDataIndex - this.usedSpace;
			if (num > 0 && num >= finalDataIndex / 4)
			{
				this.Compact(true);
				finalDataIndex = this.GetFinalDataIndex();
			}
			if (finalDataIndex + amount > this.dataList.Length)
			{
				this.dataList.Length = finalDataIndex + amount;
			}
			this.usedSpace += amount;
			MemoryBuffer.MemorySpan span = default(MemoryBuffer.MemorySpan);
			span.id = id;
			span.dataIndex = finalDataIndex;
			span.dataLength = amount;
			int length = this.spans.Length;
			this.spanMap.Add(id, length);
			this.spans.Add(in span);
			this.spanList.Length = this.spans.Length;
			this.spanList.UpdateSpan(length, span);
			return length;
		}

		// Token: 0x06000182 RID: 386 RVA: 0x0000ADD0 File Offset: 0x00008FD0
		public void Release(ChainId id)
		{
			int num;
			if (!this.spanMap.TryGetValue(id, out num))
			{
				return;
			}
			MemoryBuffer.MemorySpan memorySpan = this.spans[num];
			this.usedSpace -= memorySpan.dataLength;
			this.spanMap.Remove(id);
			memorySpan.id = ChainId.Null;
			this.spans[num] = memorySpan;
			this.spanList.UpdateSpan(num, memorySpan);
			this.dataList.Invalidate(memorySpan.dataIndex, memorySpan.dataLength);
		}

		// Token: 0x06000183 RID: 387 RVA: 0x0000AE58 File Offset: 0x00009058
		public void Compact(bool fullCompact = true)
		{
			int finalDataIndex = this.GetFinalDataIndex();
			int num = 0;
			int num2 = 0;
			this.spanMap.Clear();
			for (int i = 0; i < this.spans.Length; i++)
			{
				MemoryBuffer.MemorySpan memorySpan = this.spans[i];
				if (!(memorySpan.id == ChainId.Null))
				{
					if (fullCompact && memorySpan.dataIndex != num)
					{
						this.dataList.Move(num, memorySpan.dataIndex, memorySpan.dataLength);
						memorySpan.dataIndex = num;
					}
					this.spans[num2] = memorySpan;
					this.spanMap[memorySpan.id] = num2;
					this.spanList.Move(num2, i, 1);
					this.spanList.UpdateSpan(num2, memorySpan);
					num2++;
					num += memorySpan.dataLength;
				}
			}
			this.spans.Length = num2;
			this.spanList.Length = num2;
			if (fullCompact)
			{
				int finalDataIndex2 = this.GetFinalDataIndex();
				this.dataList.Invalidate(finalDataIndex2, finalDataIndex - finalDataIndex2);
			}
		}

		// Token: 0x06000184 RID: 388 RVA: 0x0000AF68 File Offset: 0x00009168
		public void PrintDebug()
		{
			Debug.Log(string.Format("MemoryBuffer - Spans:{0} Data:{1} Used:{2}", this.spans.Length, this.dataList.Length, this.usedSpace));
			for (int i = 0; i < this.spans.Length; i++)
			{
				MemoryBuffer.MemorySpan memorySpan = this.spans[i];
				Debug.Log(string.Format("Span - Id:{0} Index:{1} Size:{2}", memorySpan.id, memorySpan.dataIndex, memorySpan.dataLength));
			}
		}

		// Token: 0x06000185 RID: 389 RVA: 0x0000B004 File Offset: 0x00009204
		private int GetFinalDataIndex()
		{
			if (this.spans.Length > 0)
			{
				MemoryBuffer.MemorySpan memorySpan = this.spans[this.spans.Length - 1];
				return memorySpan.dataIndex + memorySpan.dataLength;
			}
			return 0;
		}

		// Token: 0x04000110 RID: 272
		private int usedSpace;

		// Token: 0x04000111 RID: 273
		private NativeHashMap<ChainId, int> spanMap;

		// Token: 0x04000112 RID: 274
		private NativeList<MemoryBuffer.MemorySpan> spans;

		// Token: 0x04000113 RID: 275
		private IMemoryBufferSpanList spanList;

		// Token: 0x04000114 RID: 276
		private IMemoryBufferList dataList;

		// Token: 0x02000063 RID: 99
		public struct MemorySpan
		{
			// Token: 0x040002AE RID: 686
			public ChainId id;

			// Token: 0x040002AF RID: 687
			public int dataIndex;

			// Token: 0x040002B0 RID: 688
			public int dataLength;
		}
	}
}
