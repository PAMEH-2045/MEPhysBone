using System;

namespace VRC.Dynamics
{
	// Token: 0x0200001E RID: 30
	internal enum VRCConstraintSynchronizeResult
	{
		// Token: 0x040000CE RID: 206
		NoChanges,
		// Token: 0x040000CF RID: 207
		DidReceiveChangesNoReallocation,
		// Token: 0x040000D0 RID: 208
		DidReceiveChangesSameGameObjectReallocation,
		// Token: 0x040000D1 RID: 209
		DidReceiveChangesFullReallocation
	}
}
