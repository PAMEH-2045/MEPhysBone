using System;

namespace VRC.Core.Pool
{
	// Token: 0x020006FB RID: 1787
	public readonly struct PooledArray<T> : IDisposable
	{
		// Token: 0x060044DF RID: 17631 RVA: 0x0012830A File Offset: 0x0012650A
		public PooledArray(T[] array)
		{
			this.Array = array;
		}

		// Token: 0x060044E0 RID: 17632 RVA: 0x00128313 File Offset: 0x00126513
		public void Dispose()
		{
			ArrayPool.Release<T>(this.Array);
		}

		// Token: 0x060044E1 RID: 17633 RVA: 0x00128320 File Offset: 0x00126520
		public static implicit operator T[](PooledArray<T> pa)
		{
			return pa.Array;
		}

		// Token: 0x04002174 RID: 8564
		public readonly T[] Array;
	}
}
