using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace VRC.Dynamics
{
	// Token: 0x02000025 RID: 37
	public class FixedTransformAccessArray : IDisposable
	{
		// Token: 0x0600015C RID: 348 RVA: 0x0000A510 File Offset: 0x00008710
		public FixedTransformAccessArray(int capacity = 0)
		{
			this.transformArray = new TransformAccessArray(capacity, -1);
			this.lookupToId = new NativeArray<int>(capacity, (Allocator)4, (NativeArrayOptions)1);
			this.lookupFromId = new NativeArray<int>(capacity, (Allocator)4, (NativeArrayOptions)1);
			for (int i = 0; i < capacity; i++)
			{
				this.lookupToId[i] = -1;
				this.lookupFromId[i] = -1;
			}
			this.length = 0;
		}

		// Token: 0x0600015D RID: 349 RVA: 0x0000A584 File Offset: 0x00008784
		public void Add(Transform element, int id)
		{
			int num;
			if (this.emptyQueue.Count > 0)
			{
				num = this.emptyQueue[0];
				this.emptyQueue.RemoveAt(0);
				this.transformArray[num] = element;
			}
			else
			{
				num = this.transformArray.length;
				this.transformArray.Add(element);
			}
			this.lookupToId[num] = id;
			this.lookupFromId[id] = num;
			this.length++;
		}

		// Token: 0x0600015E RID: 350 RVA: 0x0000A60C File Offset: 0x0000880C
		public void Remove(int id)
		{
			int num = this.lookupFromId[id];
			if (num < 0)
			{
				return;
			}
			this.transformArray[num] = null;
			this.emptyQueue.Add(num);
			this.lookupToId[num] = -1;
			this.lookupFromId[id] = -1;
			this.length--;
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000A66C File Offset: 0x0000886C
		public void ChangeId(int prev, int next)
		{
			int num = this.lookupFromId[prev];
			this.lookupToId[num] = next;
			this.lookupFromId[next] = num;
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000A6A0 File Offset: 0x000088A0
		public Transform FindTransform(int id)
		{
			if (id < 0 || id >= this.lookupFromId.Length)
			{
				return null;
			}
			return this.transformArray[this.lookupFromId[id]];
		}

		// Token: 0x06000161 RID: 353 RVA: 0x0000A6D0 File Offset: 0x000088D0
		public void Dispose()
		{
			if (this.transformArray.isCreated)
			{
				this.transformArray.Dispose();
			}
			if (this.lookupToId.IsCreated)
			{
				this.lookupToId.Dispose();
			}
			if (this.lookupFromId.IsCreated)
			{
				this.lookupFromId.Dispose();
			}
			this.emptyQueue.Clear();
			this.length = 0;
		}

		// Token: 0x06000162 RID: 354 RVA: 0x0000A737 File Offset: 0x00008937
		public int GetLength()
		{
			return this.length;
		}

		// Token: 0x06000163 RID: 355 RVA: 0x0000A73F File Offset: 0x0000893F
		public TransformAccessArray GetTransformAccessArray()
		{
			return this.transformArray;
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000A747 File Offset: 0x00008947
		public NativeArray<int> GetLookupToId()
		{
			return this.lookupToId;
		}

		// Token: 0x06000165 RID: 357 RVA: 0x0000A74F File Offset: 0x0000894F
		public NativeArray<int> GetLookupFromId()
		{
			return this.lookupFromId;
		}

		// Token: 0x0400010B RID: 267
		private TransformAccessArray transformArray;

		// Token: 0x0400010C RID: 268
		private NativeArray<int> lookupToId;

		// Token: 0x0400010D RID: 269
		private NativeArray<int> lookupFromId;

		// Token: 0x0400010E RID: 270
		private List<int> emptyQueue = new List<int>();

		// Token: 0x0400010F RID: 271
		private int length;
	}
}
