using System;

namespace VRC.Dynamics
{
	// Token: 0x02000029 RID: 41
	public interface IMemoryBufferSpanList : IMemoryBufferList
	{
		// Token: 0x06000175 RID: 373
		void UpdateSpan(int index, MemoryBuffer.MemorySpan span);
	}
}
