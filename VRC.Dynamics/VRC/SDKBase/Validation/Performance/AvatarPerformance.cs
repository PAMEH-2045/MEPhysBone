using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance
{
	// Token: 0x02000063 RID: 99
	public static class AvatarPerformance
	{
		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060002BC RID: 700 RVA: 0x0000B37A File Offset: 0x0000957A
		// (set) Token: 0x060002BD RID: 701 RVA: 0x0000B381 File Offset: 0x00009581
		[PublicAPI]
		public static AvatarPerformance.IgnoreDelegate ShouldIgnoreComponent { get; set; }

		// Token: 0x060002BE RID: 702 RVA: 0x0000B38C File Offset: 0x0000958C
		[PublicAPI]
		public static void CalculatePerformanceStats(string avatarName, GameObject avatarObject, AvatarPerformanceStats perfStats, bool mobilePlatform)
		{
			perfStats.Reset();
			perfStats.avatarName = avatarName;
			PerformanceScannerSet performanceScannerSet = AvatarPerformance.GetPerformanceScannerSet(mobilePlatform);
			if (performanceScannerSet != null)
			{
				performanceScannerSet.RunPerformanceScan(avatarObject, perfStats, new AvatarPerformance.IgnoreDelegate(AvatarPerformance.ShouldIgnoreComponentInternal));
			}
			perfStats.CalculateAllPerformanceRatings(mobilePlatform);
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000B3CB File Offset: 0x000095CB
		[PublicAPI]
		public static IEnumerator CalculatePerformanceStatsEnumerator(string avatarName, GameObject avatarObject, AvatarPerformanceStats perfStats, bool mobilePlatform)
		{
			perfStats.Reset();
			perfStats.avatarName = avatarName;
			PerformanceScannerSet performanceScannerSet = AvatarPerformance.GetPerformanceScannerSet(mobilePlatform);
			if (performanceScannerSet != null)
			{
				yield return performanceScannerSet.RunPerformanceScanEnumerator(avatarObject, perfStats, new AvatarPerformance.IgnoreDelegate(AvatarPerformance.ShouldIgnoreComponentInternal));
			}
			perfStats.CalculateAllPerformanceRatings(mobilePlatform);
			yield break;
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000B3EF File Offset: 0x000095EF
		[PublicAPI]
		public static IEnumerator ApplyPerformanceFiltersEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, PerformanceRating minPerfRating, AvatarPerformance.FilterBlockCallback onBlock, bool mobilePlatform, bool userForcedShow)
		{
			if (!mobilePlatform && userForcedShow)
			{
				yield break;
			}
			if (minPerfRating == PerformanceRating.None)
			{
				yield break;
			}
			PerformanceFilterSet performanceFilterSet = AvatarPerformance.GetPerformanceFilterSet(mobilePlatform);
			if (performanceFilterSet == null)
			{
				yield break;
			}
			bool avatarBlocked = false;
			yield return performanceFilterSet.ApplyPerformanceFilters(avatarObject, perfStats, minPerfRating, new AvatarPerformance.IgnoreDelegate(AvatarPerformance.ShouldIgnoreComponentInternal), delegate
			{
				avatarBlocked = true;
			}, userForcedShow);
			if (!avatarBlocked)
			{
				yield break;
			}
			onBlock();
			yield break;
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000B424 File Offset: 0x00009624
		[PublicAPI]
		private static PerformanceScannerSet GetPerformanceScannerSet(bool mobilePlatform)
		{
			PerformanceScannerSet performanceScannerSet = new PerformanceScannerSet();
			if (mobilePlatform)
			{
				performanceScannerSet.InitForPlatform(PerformancePlatform.Android);
			}
			else
			{
				performanceScannerSet.InitForPlatform(PerformancePlatform.Windows);
			}
			return performanceScannerSet;
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0000B44B File Offset: 0x0000964B
		[PublicAPI]
		private static PerformanceFilterSet GetPerformanceFilterSet(bool mobilePlatform)
		{
			if (!mobilePlatform)
			{
				return Resources.Load<PerformanceFilterSet>("Validation/Performance/FilterSets/PerformanceFilterSet_Windows");
			}
			return Resources.Load<PerformanceFilterSet>("Validation/Performance/FilterSets/PerformanceFilterSet_Quest");
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0000B468 File Offset: 0x00009668
		private static bool IsEditorOnly(Component component)
		{
			if (component.CompareTag("EditorOnly"))
			{
				return true;
			}
			Transform[] componentsInParent = component.transform.GetComponentsInParent<Transform>(true);
			for (int i = 0; i < componentsInParent.Length; i++)
			{
				if (componentsInParent[i].CompareTag("EditorOnly"))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0000B4B1 File Offset: 0x000096B1
		[PublicAPI]
		private static bool ShouldIgnoreComponentInternal(Component component)
		{
			if (Application.isEditor)
			{
				if (component == null)
				{
					return false;
				}
				if (AvatarPerformance.IsEditorOnly(component))
				{
					return true;
				}
			}
			return AvatarPerformance.ShouldIgnoreComponent != null && AvatarPerformance.ShouldIgnoreComponent(component);
		}

		// Token: 0x040002F6 RID: 758
		[PublicAPI]
		public static readonly PerformanceRating AvatarPerformanceRatingMinimumToDisplayDefault = Application.isMobilePlatform ? PerformanceRating.Medium : PerformanceRating.VeryPoor;

		// Token: 0x040002F7 RID: 759
		[PublicAPI]
		public static readonly PerformanceRating AvatarPerformanceRatingMinimumToDisplayMin = PerformanceRating.Medium;

		// Token: 0x040002F8 RID: 760
		[PublicAPI]
		public static readonly PerformanceRating AvatarPerformanceRatingMinimumToDisplayMax = Application.isMobilePlatform ? PerformanceRating.Poor : PerformanceRating.VeryPoor;

		// Token: 0x0200011D RID: 285
		// (Invoke) Token: 0x060004A9 RID: 1193
		[PublicAPI]
		public delegate bool IgnoreDelegate(Component component);

		// Token: 0x0200011E RID: 286
		// (Invoke) Token: 0x060004AD RID: 1197
		[PublicAPI]
		public delegate void FilterBlockCallback();
	}
}
