using System;

namespace VRC.Core.Pool
{
	// Token: 0x020006FD RID: 1789
	public static class GenericPool
	{
		// Token: 0x060044E8 RID: 17640 RVA: 0x001283B4 File Offset: 0x001265B4
		public static ObjectPool<T> GetObjectPool<T>() where T : class, new()
		{
			return GenericPool.GenericPoolImpl<T>.Pool;
		}

		// Token: 0x060044E9 RID: 17641 RVA: 0x001283BB File Offset: 0x001265BB
		public static T Get<T>() where T : class, new()
		{
			return GenericPool.GenericPoolImpl<T>.Get();
		}

		// Token: 0x060044EA RID: 17642 RVA: 0x001283C2 File Offset: 0x001265C2
		public static PooledObject<T> Get<T>(out T value) where T : class, new()
		{
			return GenericPool.GenericPoolImpl<T>.Get(out value);
		}

		// Token: 0x060044EB RID: 17643 RVA: 0x001283CA File Offset: 0x001265CA
		public static void Release<T>(T toRelease) where T : class, new()
		{
			GenericPool.GenericPoolImpl<T>.Release(toRelease);
		}

		// Token: 0x02000967 RID: 2407
		private static class GenericPoolImpl<T> where T : class, new()
		{
			// Token: 0x06004D01 RID: 19713 RVA: 0x00140410 File Offset: 0x0013E610
			public static T Get()
			{
				return GenericPool.GenericPoolImpl<T>.Pool.Get();
			}

			// Token: 0x06004D02 RID: 19714 RVA: 0x0014041C File Offset: 0x0013E61C
			public static PooledObject<T> Get(out T value)
			{
				return GenericPool.GenericPoolImpl<T>.Pool.Get(out value);
			}

			// Token: 0x06004D03 RID: 19715 RVA: 0x00140429 File Offset: 0x0013E629
			public static void Release(T toRelease)
			{
				GenericPool.GenericPoolImpl<T>.Pool.Release(toRelease);
			}

			// Token: 0x04002A7F RID: 10879
			public static readonly ObjectPool<T> Pool = new ObjectPool<T>(() => Activator.CreateInstance<T>(), null, null, null, true, 8, -1);
		}
	}
}
