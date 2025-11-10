using System;

namespace VRC.Core.Pool
{
	// Token: 0x020006FE RID: 1790
	public readonly struct PooledObject<T> : IDisposable where T : class
	{
		// Token: 0x060044EC RID: 17644 RVA: 0x001283D4 File Offset: 0x001265D4
		public PooledObject(IObjectPool<T> pool, T obj)
		{
			if (pool == null)
			{
				throw new ArgumentNullException("pool");
			}
			this.Pool = pool;
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			this.Object = obj;
		}

		// Token: 0x060044ED RID: 17645 RVA: 0x00128412 File Offset: 0x00126612
		public void Dispose()
		{
			this.Pool.Release(this.Object);
		}

		// Token: 0x04002175 RID: 8565
		public readonly IObjectPool<T> Pool;

		// Token: 0x04002176 RID: 8566
		public readonly T Object;
	}
}
