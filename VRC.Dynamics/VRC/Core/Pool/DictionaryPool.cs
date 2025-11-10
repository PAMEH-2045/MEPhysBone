using System;
using System.Collections.Generic;

namespace VRC.Core.Pool
{
	// Token: 0x02000704 RID: 1796
	public static class DictionaryPool
	{
		// Token: 0x0600450C RID: 17676 RVA: 0x00128AF2 File Offset: 0x00126CF2
		public static ObjectPool<Dictionary<TKey, TValue>> GetObjectPool<TKey, TValue>()
		{
			return DictionaryPool.DictionaryPoolImpl<TKey, TValue>.Pool;
		}

		// Token: 0x0600450D RID: 17677 RVA: 0x00128AF9 File Offset: 0x00126CF9
		public static Dictionary<TKey, TValue> Get<TKey, TValue>()
		{
			return DictionaryPool.DictionaryPoolImpl<TKey, TValue>.Pool.Get();
		}

		// Token: 0x0600450E RID: 17678 RVA: 0x00128B05 File Offset: 0x00126D05
		public static PooledObject<Dictionary<TKey, TValue>> Get<TKey, TValue>(out Dictionary<TKey, TValue> value)
		{
			return DictionaryPool.DictionaryPoolImpl<TKey, TValue>.Pool.Get(out value);
		}

		// Token: 0x0600450F RID: 17679 RVA: 0x00128B12 File Offset: 0x00126D12
		public static void Release<TKey, TValue>(Dictionary<TKey, TValue> toRelease)
		{
			DictionaryPool.DictionaryPoolImpl<TKey, TValue>.Pool.Release(toRelease);
		}

		// Token: 0x0200096C RID: 2412
		private static class DictionaryPoolImpl<TKey, TValue>
		{
			// Token: 0x04002A88 RID: 10888
			public static readonly ObjectPool<Dictionary<TKey, TValue>> Pool = new ObjectPool<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>(), null, delegate(Dictionary<TKey, TValue> dict)
			{
				dict.Clear();
			}, null, true, 8, -1);
		}
	}
}
