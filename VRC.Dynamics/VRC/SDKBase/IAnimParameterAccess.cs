using System;

namespace VRC.SDKBase
{
	// Token: 0x02000022 RID: 34
	public interface IAnimParameterAccess
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060000F2 RID: 242
		// (set) Token: 0x060000F3 RID: 243
		bool boolVal { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060000F4 RID: 244
		// (set) Token: 0x060000F5 RID: 245
		int intVal { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060000F6 RID: 246
		// (set) Token: 0x060000F7 RID: 247
		float floatVal { get; set; }
	}
}
