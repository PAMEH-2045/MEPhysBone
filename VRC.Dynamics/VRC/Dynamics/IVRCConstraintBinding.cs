using System;
using UnityEngine.Animations;

namespace VRC.Dynamics
{
	// Token: 0x0200000E RID: 14
	internal interface IVRCConstraintBinding : IDisposable
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060000B4 RID: 180
		IConstraint ApplicationUnityConstraint { get; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x060000B5 RID: 181
		VRCConstraintBase ApplicationVrcConstraint { get; }

		// Token: 0x060000B6 RID: 182
		VRCConstraintSynchronizeResult Synchronize(bool disableUnityConstraint);

		// Token: 0x060000B7 RID: 183
		void RestoreUnityConstraintEnabledState();
	}
}
