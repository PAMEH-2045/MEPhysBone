using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x02000074 RID: 116
	public sealed class LineRendererPerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x060002FC RID: 764 RVA: 0x0000DC15 File Offset: 0x0000BE15
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			List<LineRenderer> lineRendererBuffer = new List<LineRenderer>();
			yield return base.ScanAvatarForComponentsOfType<LineRenderer>(avatarObject, lineRendererBuffer);
			if (shouldIgnoreComponent != null)
			{
				lineRendererBuffer.RemoveAll((LineRenderer c) => shouldIgnoreComponent(c));
			}
			int count = lineRendererBuffer.Count;
			perfStats.lineRendererCount = new int?(count);
			perfStats.materialCount = new int?(perfStats.materialCount.GetValueOrDefault() + count);
			yield break;
		}
	}
}
