using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x02000072 RID: 114
	public sealed class ClothPerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x060002F8 RID: 760 RVA: 0x0000DBBD File Offset: 0x0000BDBD
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			List<Cloth> clothBuffer = new List<Cloth>();
			yield return base.ScanAvatarForComponentsOfType<Cloth>(avatarObject, clothBuffer);
			if (shouldIgnoreComponent != null)
			{
				clothBuffer.RemoveAll((Cloth c) => shouldIgnoreComponent(c));
			}
			int num = 0;
			foreach (Cloth cloth in clothBuffer)
			{
				if (!(cloth == null) && cloth.coefficients != null)
				{
					num += cloth.coefficients.Length;
				}
			}
			perfStats.clothCount = new int?(clothBuffer.Count);
			perfStats.clothMaxVertices = new int?(num);
			yield break;
		}
	}
}
