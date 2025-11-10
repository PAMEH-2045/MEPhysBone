using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x02000073 RID: 115
	public sealed class LightPerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x060002FA RID: 762 RVA: 0x0000DBE9 File Offset: 0x0000BDE9
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			List<Light> lightBuffer = new List<Light>();
			yield return base.ScanAvatarForComponentsOfType<Light>(avatarObject, lightBuffer);
			if (shouldIgnoreComponent != null)
			{
				lightBuffer.RemoveAll((Light c) => shouldIgnoreComponent(c));
			}
			perfStats.lightCount = new int?(lightBuffer.Count);
			yield break;
		}
	}
}
