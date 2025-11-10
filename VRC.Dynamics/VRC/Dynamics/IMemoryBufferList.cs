using System;

namespace VRC.Dynamics
{
	// Token: 0x0200002A RID: 42
	public interface IMemoryBufferList
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000176 RID: 374
		// (set) Token: 0x06000177 RID: 375
		int Length { get; set; }

		// Token: 0x06000178 RID: 376
		void Move(int to, int from, int amount);

		// Token: 0x06000179 RID: 377
		void Dispose();

		// Token: 0x0600017A RID: 378
		void Clear();

		// Token: 0x0600017B RID: 379
		void Invalidate(int index, int amount);
	}
}
