using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x0200002F RID: 47
	internal class PhysBoneGroup : IDisposable
	{
		// Token: 0x0600019B RID: 411 RVA: 0x0000B620 File Offset: 0x00009820
		public PhysBoneGroup(PhysBoneManager manager, int groupIndex)
		{
			this.manager = manager;
			this.groupIndex = groupIndex;
			this.indexList = new NativeList<int>(8, (Allocator)4);
			this.shapes = new NativeList<ushort>(8, (Allocator)4);
		}

		// Token: 0x0600019C RID: 412 RVA: 0x0000B670 File Offset: 0x00009870
		public void Dispose()
		{
			this.indexList.Dispose();
			this.shapes.Dispose();
		}

		// Token: 0x0600019D RID: 413 RVA: 0x0000B688 File Offset: 0x00009888
		public void AddPhysBone(ChainId chainId)
		{
			if (!this.chainIds.Contains(chainId))
			{
				this.chainIds.Add(chainId);
				this.MarkDirty();
				return;
			}
			Debug.LogError(string.Format("PhysBone with id {0} is already in the execution group at index {1}. It will not be added again.", chainId, this.groupIndex));
		}

		// Token: 0x0600019E RID: 414 RVA: 0x0000B6D8 File Offset: 0x000098D8
		public bool RemovePhysBone(ChainId chainId)
		{
			int num = this.chainIds.IndexOf(chainId);
			if (num >= 0)
			{
				ListExtensions.RemoveAtSwapBack<ChainId>(this.chainIds, num);
				this.MarkDirty();
				return true;
			}
			return false;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x0000B70B File Offset: 0x0000990B
		public void MarkDirty()
		{
			this.isListDirty = true;
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x0000B714 File Offset: 0x00009914
		public NativeArray<int> GetChains()
		{
			if (this.isListDirty)
			{
				this.indexList.Clear();
				this.indexList.SetCapacity(this.chainIds.Count);
				for (int i = 0; i < this.chainIds.Count; i++)
				{
					ChainId id = this.chainIds[i];
					int num = this.manager.FindChainIndex(id);
					if (num < 0)
					{
						PhysBoneManager.ReportCriticalError(PhysBoneManager.CriticalErrorType.ChainIndexOutsideJob, null);
					}
					else
					{
						this.indexList.Add(in num);
					}
				}
				this.isListDirty = false;
			}
			return this.indexList.AsArray();
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x0000B7A6 File Offset: 0x000099A6
		public void AddShape(ushort shapeId)
		{
			if (!NativeListExtensions.Contains<ushort, ushort>(this.shapes, shapeId))
			{
				this.shapes.Add(in shapeId);
				return;
			}
			Debug.LogError(string.Format("Shape with id {0} is already in the execution group at index {1}. It will not be added again.", shapeId, this.groupIndex));
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x0000B7E4 File Offset: 0x000099E4
		public bool RemoveShape(ushort shapeId)
		{
			int num = NativeListExtensions.IndexOf<ushort, ushort>(this.shapes, shapeId);
			if (num >= 0)
			{
				this.shapes.RemoveAtSwapBack(num);
				return true;
			}
			return false;
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x0000B811 File Offset: 0x00009A11
		public NativeList<ushort> GetShapes()
		{
			return this.shapes;
		}

		// Token: 0x0400011E RID: 286
		private const int MIN_CHAINS_CAPACITY = 8;

		// Token: 0x0400011F RID: 287
		private PhysBoneManager manager;

		// Token: 0x04000120 RID: 288
		private int groupIndex;

		// Token: 0x04000121 RID: 289
		public List<ChainId> chainIds = new List<ChainId>();

		// Token: 0x04000122 RID: 290
		public NativeList<int> indexList;

		// Token: 0x04000123 RID: 291
		private bool isListDirty;

		// Token: 0x04000124 RID: 292
		private const int MIN_SHAPES_CAPACITY = 8;

		// Token: 0x04000125 RID: 293
		private NativeList<ushort> shapes;
	}
}
