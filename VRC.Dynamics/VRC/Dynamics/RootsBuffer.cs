using System;
using System.Collections.Generic;
using Unity.Collections;

namespace VRC.Dynamics
{
	// Token: 0x0200002C RID: 44
	internal class RootsBuffer : IDisposable
	{
		// Token: 0x06000186 RID: 390 RVA: 0x0000B047 File Offset: 0x00009247
		public RootsBuffer(int capacity)
		{
			this.roots = new NativeList<PhysBoneManager.ChainRoot>(capacity, (Allocator)4);
			this.comps = new List<PhysBoneRoot>(capacity);
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000B078 File Offset: 0x00009278
		public void Dispose()
		{
			if (this.roots.IsCreated)
			{
				this.roots.Dispose();
			}
		}

		// Token: 0x06000188 RID: 392 RVA: 0x0000B094 File Offset: 0x00009294
		public void Add(PhysBoneRoot root)
		{
			if (root.bufferIndex != -1)
			{
				return;
			}
			PhysBoneManager.ChainRoot chainRoot;
			if (this.available.Count > 0)
			{
				int num = this.available[0];
				ListExtensions.RemoveAtSwapBack<int>(this.available, 0);
				int num2 = num;
				chainRoot = new PhysBoneManager.ChainRoot
				{
					isUsed = true,
					useFixedTime = root.useFixedUpdate
				};
				this.roots[num2] = chainRoot;
				this.comps[num] = root;
				root.bufferIndex = num;
				return;
			}
			chainRoot = default(PhysBoneManager.ChainRoot);
			chainRoot.isUsed = true;
			chainRoot.useFixedTime = root.useFixedUpdate;
			this.roots.Add(in chainRoot);
			this.comps.Add(root);
			root.bufferIndex = this.roots.Length - 1;
		}

		// Token: 0x06000189 RID: 393 RVA: 0x0000B15C File Offset: 0x0000935C
		public void Remove(int index)
		{
			if (index < 0 || index >= this.roots.Length)
			{
				return;
			}
			this.roots[index] = new PhysBoneManager.ChainRoot
			{
				isUsed = false
			};
			this.comps[index] = null;
			this.available.Add(index);
		}

		// Token: 0x04000115 RID: 277
		public NativeList<PhysBoneManager.ChainRoot> roots;

		// Token: 0x04000116 RID: 278
		public List<PhysBoneRoot> comps;

		// Token: 0x04000117 RID: 279
		private List<int> available = new List<int>();
	}
}
