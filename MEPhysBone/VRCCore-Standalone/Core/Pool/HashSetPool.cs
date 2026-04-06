using System;
using System.Collections.Generic;

namespace VRC.Core.Pool
{
	// Token: 0x02000705 RID: 1797
	public static class HashSetPool
	{
		// Token: 0x06004510 RID: 17680 RVA: 0x00128B1F File Offset: 0x00126D1F
		public static ObjectPool<HashSet<T>> GetObjectPool<T>()
		{
			return HashSetPool.HashSetPoolImpl<T>.Pool;
		}

		// Token: 0x06004511 RID: 17681 RVA: 0x00128B26 File Offset: 0x00126D26
		public static HashSet<T> Get<T>()
		{
			return HashSetPool.HashSetPoolImpl<T>.Pool.Get();
		}

		// Token: 0x06004512 RID: 17682 RVA: 0x00128B32 File Offset: 0x00126D32
		public static PooledObject<HashSet<T>> Get<T>(out HashSet<T> value)
		{
			return HashSetPool.HashSetPoolImpl<T>.Pool.Get(out value);
		}

		// Token: 0x06004513 RID: 17683 RVA: 0x00128B3F File Offset: 0x00126D3F
		public static void Release<T>(HashSet<T> toRelease)
		{
			HashSetPool.HashSetPoolImpl<T>.Pool.Release(toRelease);
		}

		// Token: 0x0200096D RID: 2413
		private static class HashSetPoolImpl<T>
		{
			// Token: 0x04002A89 RID: 10889
			public static readonly ObjectPool<HashSet<T>> Pool = new ObjectPool<HashSet<T>>(() => new HashSet<T>(), null, delegate(HashSet<T> set)
			{
				set.Clear();
			}, null, true, 8, -1);
		}
	}
}
