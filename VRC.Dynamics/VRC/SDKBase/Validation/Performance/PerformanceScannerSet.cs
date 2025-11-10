using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Attributes;
using VRC.SDKBase.Validation.Performance.Scanners;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance
{
	// Token: 0x0200006B RID: 107
	public class PerformanceScannerSet
	{
		// Token: 0x060002CF RID: 719 RVA: 0x0000B71C File Offset: 0x0000991C
		public void InitForPlatform(PerformancePlatform platform)
		{
			this.performanceScanners.Clear();
			foreach (PerformanceScannerAttribute performanceScannerAttribute in TypeUtils.FindAssemblyAttributes<PerformanceScannerAttribute>())
			{
				if (performanceScannerAttribute != null && performanceScannerAttribute.type != null)
				{
					AbstractPerformanceScanner abstractPerformanceScanner = Activator.CreateInstance(performanceScannerAttribute.type) as AbstractPerformanceScanner;
					if (abstractPerformanceScanner != null && abstractPerformanceScanner.EnabledOnPlatform(platform))
					{
						this.performanceScanners.Add(abstractPerformanceScanner);
					}
				}
			}
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0000B7A8 File Offset: 0x000099A8
		public void RunPerformanceScan(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			foreach (AbstractPerformanceScanner abstractPerformanceScanner in this.performanceScanners)
			{
				if (abstractPerformanceScanner != null)
				{
					abstractPerformanceScanner.RunPerformanceScan(avatarObject, perfStats, shouldIgnoreComponent);
				}
			}
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0000B800 File Offset: 0x00009A00
		public IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			foreach (AbstractPerformanceScanner abstractPerformanceScanner in this.performanceScanners)
			{
				if (abstractPerformanceScanner != null)
				{
					yield return abstractPerformanceScanner.RunPerformanceScanEnumerator(avatarObject, perfStats, shouldIgnoreComponent);
				}
			}
			List<AbstractPerformanceScanner>.Enumerator enumerator = default(List<AbstractPerformanceScanner>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x0400032F RID: 815
		public List<AbstractPerformanceScanner> performanceScanners = new List<AbstractPerformanceScanner>();
	}
}
