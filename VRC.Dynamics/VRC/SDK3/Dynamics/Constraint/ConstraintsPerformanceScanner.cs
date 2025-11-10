using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Scanners;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDK3.Dynamics.Constraint
{
	// Token: 0x0200000D RID: 13
	public sealed class ConstraintsPerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x06000012 RID: 18 RVA: 0x00002C29 File Offset: 0x00000E29
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			List<IConstraint> engineConstraintBuffer = new List<IConstraint>();
			yield return base.ScanAvatarForComponentsOfType<IConstraint>(avatarObject, engineConstraintBuffer);
			if (shouldIgnoreComponent != null)
			{
				engineConstraintBuffer.RemoveAll((IConstraint c) => shouldIgnoreComponent(c as Component));
			}
			List<IVRCConstraint> vrcConstraintBuffer = new List<IVRCConstraint>();
			yield return base.ScanAvatarForComponentsOfType<IVRCConstraint>(avatarObject, vrcConstraintBuffer);
			if (shouldIgnoreComponent != null)
			{
				vrcConstraintBuffer.RemoveAll((IVRCConstraint c) => shouldIgnoreComponent(c as Component));
			}
			int count = engineConstraintBuffer.Count;
			int count2 = vrcConstraintBuffer.Count;
			int num = count + count2;
			perfStats.constraintsCount = new int?(num);
			int num2 = 0;
			if (num > 0)
			{
				int num3;
				num2 = this.GetGroupDepth(vrcConstraintBuffer, out num3);
				int num4 = count2 - num3;
				num4 += count;
				if (num4 > 0)
				{
					num2 += num4;
				}
			}
			perfStats.constraintDepth = new int?(num2);
			yield break;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002C50 File Offset: 0x00000E50
		private int GetGroupDepth(List<IVRCConstraint> vrcConstraints, out int vrcCachedConstraintsCount)
		{
			int num = 0;
			vrcCachedConstraintsCount = 0;
			foreach (IVRCConstraint ivrcconstraint in vrcConstraints)
			{
				int latestValidExecutionGroupIndex = ivrcconstraint.LatestValidExecutionGroupIndex;
				if (latestValidExecutionGroupIndex >= 0)
				{
					vrcCachedConstraintsCount++;
					if (latestValidExecutionGroupIndex >= num)
					{
						num = latestValidExecutionGroupIndex + 1;
					}
				}
			}
			return num;
		}
	}
}
