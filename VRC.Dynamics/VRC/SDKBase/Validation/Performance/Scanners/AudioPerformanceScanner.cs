using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Source.Validation.Performance.Scanners;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x02000071 RID: 113
	public sealed class AudioPerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x060002F6 RID: 758 RVA: 0x0000DB91 File Offset: 0x0000BD91
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			List<AudioSource> audioSourceBuffer = new List<AudioSource>();
			yield return base.ScanAvatarForComponentsOfType<AudioSource>(avatarObject, audioSourceBuffer);
			if (shouldIgnoreComponent != null)
			{
				audioSourceBuffer.RemoveAll((AudioSource c) => shouldIgnoreComponent(c));
			}
			perfStats.audioSourceCount = new int?(audioSourceBuffer.Count);
			List<PerformanceScannerPlaceholder> placeholderBuffer = new List<PerformanceScannerPlaceholder>();
			yield return base.ScanAvatarForComponentsOfType<PerformanceScannerPlaceholder>(avatarObject, placeholderBuffer);
			using (List<PerformanceScannerPlaceholder>.Enumerator enumerator = placeholderBuffer.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PerformanceScannerPlaceholder performanceScannerPlaceholder = enumerator.Current;
					if (performanceScannerPlaceholder && performanceScannerPlaceholder.type == typeof(AudioSource) && !shouldIgnoreComponent(performanceScannerPlaceholder))
					{
						perfStats.audioSourceCount++;
					}
				}
				yield break;
			}
			yield break;
		}
	}
}
