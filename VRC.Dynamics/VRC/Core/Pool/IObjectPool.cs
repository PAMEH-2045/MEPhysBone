using System;

namespace VRC.Core.Pool
{
	// Token: 0x02000700 RID: 1792
	public interface IObjectPool<T> : IPool where T : class
	{
		// Token: 0x060044F1 RID: 17649
		void Clear();

		// Token: 0x060044F2 RID: 17650
		T Get();

		// Token: 0x060044F3 RID: 17651
		PooledObject<T> Get(out T v);

		// Token: 0x060044F4 RID: 17652
		void Release(T element);
	}
}
