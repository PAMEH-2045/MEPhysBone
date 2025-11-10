using System;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.Dynamics
{
	// Token: 0x02000032 RID: 50
	[AddComponentMenu("")]
	public class PhysBoneRoot : MonoBehaviour, IEditorOnly
	{
		// Token: 0x060001F6 RID: 502 RVA: 0x0000EC40 File Offset: 0x0000CE40
		private void Start()
		{
			base.hideFlags = (HideFlags)61;
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x0000EC4A File Offset: 0x0000CE4A
		private void OnDestroy()
		{
			PhysBoneManager.Inst.RemoveRoot(this);
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060001F8 RID: 504 RVA: 0x0000EC57 File Offset: 0x0000CE57
		// (set) Token: 0x060001F9 RID: 505 RVA: 0x0000EC5F File Offset: 0x0000CE5F
		public bool UseFixedTime
		{
			get
			{
				return this.useFixedUpdate;
			}
			set
			{
				if (this.useFixedUpdate == value)
				{
					return;
				}
				this.useFixedUpdate = value;
				PhysBoneManager.Inst.MarkRootDirty(this);
			}
		}

		// Token: 0x0400015F RID: 351
		internal const int NullId = -1;

		// Token: 0x04000160 RID: 352
		internal bool useFixedUpdate;

		// Token: 0x04000161 RID: 353
		internal int bufferIndex = -1;

		// Token: 0x04000162 RID: 354
		[NonSerialized]
		public string avatarId;
	}
}
