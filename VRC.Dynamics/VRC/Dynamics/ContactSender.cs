using System;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x02000023 RID: 35
	[AddComponentMenu("")]
	public class ContactSender : ContactBase
	{
		// Token: 0x06000151 RID: 337 RVA: 0x0000A450 File Offset: 0x00008650
		public override bool IsReceiver()
		{
			return false;
		}
	}
}
