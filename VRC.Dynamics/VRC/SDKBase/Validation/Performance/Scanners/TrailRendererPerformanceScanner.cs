using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x02000078 RID: 120
	public sealed class TrailRendererPerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x0600030D RID: 781 RVA: 0x0000EB72 File Offset: 0x0000CD72
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			List<TrailRenderer> trailRendererBuffer = new List<TrailRenderer>();
			yield return base.ScanAvatarForComponentsOfType<TrailRenderer>(avatarObject, trailRendererBuffer);
			if (shouldIgnoreComponent != null)
			{
				trailRendererBuffer.RemoveAll((TrailRenderer c) => shouldIgnoreComponent(c));
			}
			int count = trailRendererBuffer.Count;
			perfStats.trailRendererCount = new int?(count);
			perfStats.materialCount = new int?(perfStats.materialCount.GetValueOrDefault() + count);
			yield break;
		}
	}
}
