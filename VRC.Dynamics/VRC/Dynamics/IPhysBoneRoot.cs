using System;

namespace VRC.Dynamics
{
	// Token: 0x02000027 RID: 39
	public interface IPhysBoneRoot
	{
		// Token: 0x06000167 RID: 359
		ulong AddPhysBone(VRCPhysBoneBase physBone);

		// Token: 0x06000168 RID: 360
		ulong RemovePhysBone(VRCPhysBoneBase physBone);
	}
}
