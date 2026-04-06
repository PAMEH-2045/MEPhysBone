using System;
using System.Text;

namespace VRC.Core.Pool
{
	// Token: 0x02000706 RID: 1798
	public static class StringBuilderPool
	{
		// Token: 0x06004514 RID: 17684 RVA: 0x00128B4C File Offset: 0x00126D4C
		public static ObjectPool<StringBuilder> GetObjectPool()
		{
			return StringBuilderPool.Pool;
		}

		// Token: 0x06004515 RID: 17685 RVA: 0x00128B53 File Offset: 0x00126D53
		public static StringBuilder Get()
		{
			return StringBuilderPool.Pool.Get();
		}

		// Token: 0x06004516 RID: 17686 RVA: 0x00128B5F File Offset: 0x00126D5F
		public static PooledObject<StringBuilder> Get(out StringBuilder value)
		{
			return StringBuilderPool.Pool.Get(out value);
		}

		// Token: 0x06004517 RID: 17687 RVA: 0x00128B6C File Offset: 0x00126D6C
		public static void Release(StringBuilder toRelease)
		{
			StringBuilderPool.Pool.Release(toRelease);
		}

		// Token: 0x04002186 RID: 8582
		private static readonly ObjectPool<StringBuilder> Pool = new ObjectPool<StringBuilder>(() => new StringBuilder(), null, delegate(StringBuilder sb)
		{
			sb.Clear();
		}, null, true, 8, 16);
	}
}
