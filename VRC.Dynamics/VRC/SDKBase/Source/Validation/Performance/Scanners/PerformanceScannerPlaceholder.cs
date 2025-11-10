using System;
using UnityEngine;

namespace VRC.SDKBase.Source.Validation.Performance.Scanners
{
	// Token: 0x02000085 RID: 133
	public class PerformanceScannerPlaceholder : MonoBehaviour
	{
		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000330 RID: 816 RVA: 0x0000F061 File Offset: 0x0000D261
		// (set) Token: 0x06000331 RID: 817 RVA: 0x0000F069 File Offset: 0x0000D269
		public Type type
		{
			get
			{
				return this._type;
			}
			set
			{
				this._type = value;
				this.TypeInfo = value.ToString();
			}
		}

		// Token: 0x0400038E RID: 910
		private Type _type;

		// Token: 0x0400038F RID: 911
		[SerializeField]
		private string TypeInfo;
	}
}
