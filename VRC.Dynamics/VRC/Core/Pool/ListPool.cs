using System;
using System.Collections.Generic;

namespace VRC.Core.Pool
{
	// Token: 0x02000703 RID: 1795
	public static class ListPool
	{
		// Token: 0x06004508 RID: 17672 RVA: 0x00128AC5 File Offset: 0x00126CC5
		public static ObjectPool<List<T>> GetObjectPool<T>()
		{
			return ListPool.ListPoolImpl<T>.Pool;
		}

		// Token: 0x06004509 RID: 17673 RVA: 0x00128ACC File Offset: 0x00126CCC
		public static List<T> Get<T>()
		{
			return ListPool.ListPoolImpl<T>.Pool.Get();
		}

		// Token: 0x0600450A RID: 17674 RVA: 0x00128AD8 File Offset: 0x00126CD8
		public static PooledObject<List<T>> Get<T>(out List<T> value)
		{
			return ListPool.ListPoolImpl<T>.Pool.Get(out value);
		}

		// Token: 0x0600450B RID: 17675 RVA: 0x00128AE5 File Offset: 0x00126CE5
		public static void Release<T>(List<T> toRelease)
		{
			ListPool.ListPoolImpl<T>.Pool.Release(toRelease);
		}

		// Token: 0x0200096B RID: 2411
		private static class ListPoolImpl<T>
		{
			// Token: 0x04002A87 RID: 10887
			public static readonly ObjectPool<List<T>> Pool = new ObjectPool<List<T>>(() => new List<T>(), null, delegate(List<T> list)
			{
				list.Clear();
			}, null, true, 8, -1);
		}
	}
}
