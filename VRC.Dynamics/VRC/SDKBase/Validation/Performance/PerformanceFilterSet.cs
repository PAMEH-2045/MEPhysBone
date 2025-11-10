using System;
using System.Collections;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Filters;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance
{
	// Token: 0x02000067 RID: 103
	public class PerformanceFilterSet : ScriptableObject
	{
		// Token: 0x060002CD RID: 717 RVA: 0x0000B6D8 File Offset: 0x000098D8
		public IEnumerator ApplyPerformanceFilters(GameObject avatarObject, AvatarPerformanceStats perfStats, PerformanceRating ratingLimit, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent, AvatarPerformance.FilterBlockCallback onBlock, bool userForcedShow)
		{
			AbstractPerformanceFilter[] array = this.performanceFilters;
			for (int i = 0; i < array.Length; i++)
			{
				AbstractPerformanceFilter abstractPerformanceFilter = array[i];
				if (!(abstractPerformanceFilter == null))
				{
					bool avatarBlocked = false;
					yield return abstractPerformanceFilter.ApplyPerformanceFilter(avatarObject, perfStats, ratingLimit, shouldIgnoreComponent, delegate
					{
						avatarBlocked = true;
					}, userForcedShow);
					if (avatarBlocked)
					{
						onBlock();
						break;
					}
				}
			}
			array = null;
			yield break;
		}

		// Token: 0x0400031E RID: 798
		public AbstractPerformanceFilter[] performanceFilters;
	}
}
