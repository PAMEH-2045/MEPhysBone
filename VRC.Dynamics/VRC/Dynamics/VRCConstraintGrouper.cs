using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Profiling;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x02000009 RID: 9
	internal class VRCConstraintGrouper : IDisposable
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000055 RID: 85 RVA: 0x000039D8 File Offset: 0x00001BD8
		private bool GroupsAreStale
		{
			get
			{
				return !this._objectDisposed && (this._unprocessedConstraints.Count > 0 || this._staleRootTransforms.Count > 0);
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000056 RID: 86 RVA: 0x00003A02 File Offset: 0x00001C02
		public SortedDictionary<int, VRCConstraintGroup> ExecutionGroups
		{
			get
			{
				if (this._objectDisposed)
				{
					throw new ObjectDisposedException("The constraint grouper has been disposed and can no longer be used.");
				}
				return this._executionGroups;
			}
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00003A1D File Offset: 0x00001C1D
		public VRCConstraintGrouper()
		{
			this._executionGroups = new SortedDictionary<int, VRCConstraintGroup>();
			this._staleRootTransforms = new HashSet<Transform>();
			this._objectDisposed = false;
			this._unprocessedConstraints = new Dictionary<int, List<VRCConstraintBase>>();
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00003A50 File Offset: 0x00001C50
		public void Dispose()
		{
			if (this._objectDisposed)
			{
				return;
			}
			foreach (VRCConstraintGroup vrcconstraintGroup in this._executionGroups.Values)
			{
				vrcconstraintGroup.Dispose();
			}
			this._executionGroups.Clear();
			this._objectDisposed = true;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00003AC0 File Offset: 0x00001CC0
		public void RecordConstraintToAdd([NotNull] VRCConstraintBase constraintToAdd)
		{
			Transform dependencyRoot = constraintToAdd.DependencyRoot;
			if (!constraintToAdd.IsPendingUnprocessed)
			{
				this.GetConstraintListForRoot(this._unprocessedConstraints, dependencyRoot).Add(constraintToAdd);
				constraintToAdd.IsPendingUnprocessed = true;
			}
			if (constraintToAdd.CachedExecutionGroupIndex < 0)
			{
				this._staleRootTransforms.Add(dependencyRoot);
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00003B0C File Offset: 0x00001D0C
		public void RecordConstraintToRemove([NotNull] VRCConstraintBase constraintToRemove)
		{
			if (this._unprocessedConstraints.Count > 0)
			{
				int instanceID = constraintToRemove.DependencyRoot.GetInstanceID();
				List<VRCConstraintBase> list;
				if (this._unprocessedConstraints.TryGetValue(instanceID, out list) && list.Remove(constraintToRemove))
				{
					constraintToRemove.IsPendingUnprocessed = false;
					return;
				}
			}
			VRCConstraintGroup vrcconstraintGroup = null;
			int num = constraintToRemove.CachedExecutionGroupIndex;
			bool flag = num >= 0;
			foreach (KeyValuePair<int, VRCConstraintGroup> keyValuePair in this._executionGroups)
			{
				int num2;
				VRCConstraintGroup vrcconstraintGroup2;
				keyValuePair.Deconstruct(out num2, out vrcconstraintGroup2);
				int num3 = num2;
				VRCConstraintGroup vrcconstraintGroup3 = vrcconstraintGroup2;
				if (vrcconstraintGroup3.RemoveConstraintSwapBack(constraintToRemove))
				{
					num = num3;
					vrcconstraintGroup = vrcconstraintGroup3;
					break;
				}
			}
			if (vrcconstraintGroup != null && vrcconstraintGroup.MemberCount == 0)
			{
				vrcconstraintGroup.Dispose();
				this._executionGroups.Remove(num);
			}
			if (!flag)
			{
				this._staleRootTransforms.Add(constraintToRemove.DependencyRoot);
			}
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00003C00 File Offset: 0x00001E00
		internal void MarkRootStale(VRCConstraintBase constraint)
		{
			this._staleRootTransforms.Add(constraint.DependencyRoot);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00003C14 File Offset: 0x00001E14
		private List<VRCConstraintBase> GetConstraintListForRoot(in Dictionary<int, List<VRCConstraintBase>> constraintsDictionary, [NotNull] Transform dependencyRoot)
		{
			int instanceID = dependencyRoot.GetInstanceID();
			List<VRCConstraintBase> list;
			if (!constraintsDictionary.TryGetValue(instanceID, out list))
			{
				list = new List<VRCConstraintBase>();
				constraintsDictionary.Add(instanceID, list);
			}
			return list;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00003C44 File Offset: 0x00001E44
		public void RefreshGroups(in IReadOnlyList<VRCConstraintBase> constraintsManaged)
		{
			if (!this.GroupsAreStale)
			{
				return;
			}
			if (this.GroupsAreStale)
			{
				using (VRCConstraintGrouper._reorganizeGroupsProfilerMarker.Auto())
				{
					this.PrepareGroupsForReorganize(constraintsManaged);
					foreach (KeyValuePair<int, List<VRCConstraintBase>> keyValuePair in this._unprocessedConstraints)
					{
						int num;
						List<VRCConstraintBase> unprocessedConstraints;
						keyValuePair.Deconstruct(out num, out unprocessedConstraints);
						VRCConstraintBase.ReorganizeGroupsFastForRoot(unprocessedConstraints, this._executionGroups);
					}
					this._unprocessedConstraints.Clear();
					this._staleRootTransforms.Clear();
				}
			}
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00003D00 File Offset: 0x00001F00
		private void PrepareGroupsForReorganize(in IReadOnlyList<VRCConstraintBase> constraintsManaged)
		{
			foreach (VRCConstraintBase vrcconstraintBase in constraintsManaged)
			{
				vrcconstraintBase.SetCachedExecutionGroupIndex(-1);
			}
			foreach (VRCConstraintGroup vrcconstraintGroup in this._executionGroups.Values)
			{
				vrcconstraintGroup.Dispose();
			}
			this._executionGroups.Clear();
			foreach (VRCConstraintBase vrcconstraintBase2 in constraintsManaged)
			{
				if (!vrcconstraintBase2.IsPendingUnprocessed)
				{
					this.GetConstraintListForRoot(this._unprocessedConstraints, vrcconstraintBase2.DependencyRoot).Add(vrcconstraintBase2);
					vrcconstraintBase2.IsPendingUnprocessed = true;
				}
			}
		}

		// Token: 0x04000032 RID: 50
		private readonly SortedDictionary<int, VRCConstraintGroup> _executionGroups;

		// Token: 0x04000033 RID: 51
		private readonly HashSet<Transform> _staleRootTransforms;

		// Token: 0x04000034 RID: 52
		private bool _objectDisposed;

		// Token: 0x04000035 RID: 53
		private readonly Dictionary<int, List<VRCConstraintBase>> _unprocessedConstraints;

		// Token: 0x04000036 RID: 54
		private static readonly HashSet<int> RemovedGroupIndicesBuffer = new HashSet<int>();

		// Token: 0x04000037 RID: 55
		private static ProfilerMarker _reorganizeGroupsProfilerMarker = new ProfilerMarker("ReorganizeGroups");
	}
}
