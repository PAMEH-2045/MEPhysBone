using System;

namespace VRC.Core.Pool
{
	// Token: 0x020006FF RID: 1791
	public interface IPool
	{
		// Token: 0x17000D95 RID: 3477
		// (get) Token: 0x060044EE RID: 17646
		int CountInactive { get; }

		// Token: 0x17000D96 RID: 3478
		// (get) Token: 0x060044EF RID: 17647
		Type ObjectType { get; }

		// Token: 0x060044F0 RID: 17648
		int Cleanup();
	}
}
