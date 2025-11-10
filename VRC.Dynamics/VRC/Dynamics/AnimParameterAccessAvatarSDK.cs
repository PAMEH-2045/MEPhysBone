using System;
using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.Dynamics
{
	// Token: 0x0200001F RID: 31
	[PublicAPI]
	public class AnimParameterAccessAvatarSDK : IAnimParameterAccess
	{
		// Token: 0x06000119 RID: 281 RVA: 0x000090FC File Offset: 0x000072FC
		public AnimParameterAccessAvatarSDK(Animator anim, string paramName)
		{
			if (anim != null)
			{
				this.animator = anim;
				for (int i = 0; i < this.animator.parameters.Length; i++)
				{
					AnimatorControllerParameter animatorControllerParameter = this.animator.parameters[i];
					if (animatorControllerParameter.name == paramName && animatorControllerParameter.type != (AnimatorControllerParameterType)9)
					{
						this.paramHash = animatorControllerParameter.nameHash;
						this.paramType = animatorControllerParameter.type;
						this.valid = true;
						return;
					}
				}
			}
			this.valid = false;
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600011A RID: 282 RVA: 0x00009184 File Offset: 0x00007384
		// (set) Token: 0x0600011B RID: 283 RVA: 0x000091F8 File Offset: 0x000073F8
		public bool boolVal
		{
			get
			{
				if (!this.valid)
				{
					return false;
				}
				if (this.paramType == (AnimatorControllerParameterType)4)
				{
					return this.animator.GetBool(this.paramHash);
				}
				if (this.paramType == (AnimatorControllerParameterType)3)
				{
					return Convert.ToBoolean(this.animator.GetInteger(this.paramHash));
				}
				return this.paramType == (AnimatorControllerParameterType)1 && Convert.ToBoolean(this.animator.GetFloat(this.paramHash));
			}
			set
			{
				if (!this.valid)
				{
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)4)
				{
					this.animator.SetBool(this.paramHash, value);
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)3)
				{
					this.animator.SetInteger(this.paramHash, Convert.ToInt32(value));
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)1)
				{
					this.animator.SetFloat(this.paramHash, Convert.ToSingle(value));
				}
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x0600011C RID: 284 RVA: 0x0000926C File Offset: 0x0000746C
		// (set) Token: 0x0600011D RID: 285 RVA: 0x000092E0 File Offset: 0x000074E0
		public int intVal
		{
			get
			{
				if (!this.valid)
				{
					return 0;
				}
				if (this.paramType == (AnimatorControllerParameterType)4)
				{
					return Convert.ToInt32(this.animator.GetBool(this.paramHash));
				}
				if (this.paramType == (AnimatorControllerParameterType)3)
				{
					return this.animator.GetInteger(this.paramHash);
				}
				if (this.paramType == (AnimatorControllerParameterType)1)
				{
					return Convert.ToInt32(this.animator.GetFloat(this.paramHash));
				}
				return 0;
			}
			set
			{
				if (!this.valid)
				{
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)4)
				{
					this.animator.SetBool(this.paramHash, Convert.ToBoolean(value));
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)3)
				{
					this.animator.SetInteger(this.paramHash, value);
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)1)
				{
					this.animator.SetFloat(this.paramHash, Convert.ToSingle(value));
				}
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x0600011E RID: 286 RVA: 0x00009354 File Offset: 0x00007554
		// (set) Token: 0x0600011F RID: 287 RVA: 0x000093D0 File Offset: 0x000075D0
		public float floatVal
		{
			get
			{
				if (!this.valid)
				{
					return 0f;
				}
				if (this.paramType == (AnimatorControllerParameterType)4)
				{
					return Convert.ToSingle(this.animator.GetBool(this.paramHash));
				}
				if (this.paramType == (AnimatorControllerParameterType)3)
				{
					return Convert.ToSingle(this.animator.GetInteger(this.paramHash));
				}
				if (this.paramType == (AnimatorControllerParameterType)1)
				{
					return this.animator.GetFloat(this.paramHash);
				}
				return 0f;
			}
			set
			{
				if (!this.valid)
				{
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)4)
				{
					this.animator.SetBool(this.paramHash, Convert.ToBoolean(value));
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)3)
				{
					this.animator.SetInteger(this.paramHash, Convert.ToInt32(value));
					return;
				}
				if (this.paramType == (AnimatorControllerParameterType)1)
				{
					this.animator.SetFloat(this.paramHash, value);
				}
			}
		}

		// Token: 0x040000D2 RID: 210
		private Animator animator;

		// Token: 0x040000D3 RID: 211
		private int paramHash;

		// Token: 0x040000D4 RID: 212
		private AnimatorControllerParameterType paramType;

		// Token: 0x040000D5 RID: 213
		private bool valid;
	}
}
