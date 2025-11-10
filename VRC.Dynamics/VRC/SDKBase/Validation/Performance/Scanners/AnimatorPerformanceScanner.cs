using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x02000070 RID: 112
	public sealed class AnimatorPerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x060002F4 RID: 756 RVA: 0x0000DB65 File Offset: 0x0000BD65
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			int animatorCount = 0;
			List<Animator> animatorBuffer = new List<Animator>();
			yield return base.ScanAvatarForComponentsOfType<Animator>(avatarObject, animatorBuffer);
			if (shouldIgnoreComponent != null)
			{
				animatorBuffer.RemoveAll((Animator c) => shouldIgnoreComponent(c));
			}
			animatorCount += animatorBuffer.Count;
			List<Animation> animationBuffer = new List<Animation>();
			yield return base.ScanAvatarForComponentsOfType<Animation>(avatarObject, animationBuffer);
			if (shouldIgnoreComponent != null)
			{
				animationBuffer.RemoveAll((Animation c) => shouldIgnoreComponent(c));
			}
			animatorCount += animationBuffer.Count;
			perfStats.animatorCount = new int?(animatorCount);
			yield break;
		}
	}
}
