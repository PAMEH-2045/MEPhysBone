using System;

namespace VRC.SDKBase.Validation.Attributes
{
	// Token: 0x0200007A RID: 122
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class PerformanceScannerAttribute : Attribute
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000314 RID: 788 RVA: 0x0000EBD3 File Offset: 0x0000CDD3
		public Type type { get; }

		// Token: 0x06000315 RID: 789 RVA: 0x0000EBDB File Offset: 0x0000CDDB
		public PerformanceScannerAttribute(Type type)
		{
			this.type = type;
		}
	}
}
