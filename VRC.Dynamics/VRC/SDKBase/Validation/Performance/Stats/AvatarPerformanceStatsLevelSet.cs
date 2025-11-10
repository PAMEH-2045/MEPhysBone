using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRC.SDKBase.Validation.Performance.Stats
{
	// Token: 0x0200006E RID: 110
	public class AvatarPerformanceStatsLevelSet : ScriptableObject
	{
		// Token: 0x04000371 RID: 881
		[FormerlySerializedAs("veryGood")]
		public AvatarPerformanceStatsLevel excellent;

		// Token: 0x04000372 RID: 882
		public AvatarPerformanceStatsLevel good;

		// Token: 0x04000373 RID: 883
		public AvatarPerformanceStatsLevel medium;

		// Token: 0x04000374 RID: 884
		[FormerlySerializedAs("bad")]
		public AvatarPerformanceStatsLevel poor;
	}
}
