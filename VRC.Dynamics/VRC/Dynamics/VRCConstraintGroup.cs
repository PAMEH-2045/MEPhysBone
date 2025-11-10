using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x02000008 RID: 8
	internal class VRCConstraintGroup : IDisposable
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600004D RID: 77 RVA: 0x000037B8 File Offset: 0x000019B8
		public int MemberCount
		{
			get
			{
				return this.MemberConstraintIndices.Length;
			}
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000037C5 File Offset: 0x000019C5
		public VRCConstraintGroup()
		{
			this.MemberConstraintIndices = new UnsafeList<int>(8, (Unity.Collections.Allocator)4, 0);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000037E0 File Offset: 0x000019E0
		public void Dispose()
		{
			this.MemberConstraintIndices.Dispose();
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000037F0 File Offset: 0x000019F0
		public void AddConstraint(VRCConstraintBase constraint, int groupIndex)
		{
			if (!UnsafeListExtensions.Contains<int, int>(this.MemberConstraintIndices, constraint.NativeIndex))
			{
				int nativeIndex = constraint.NativeIndex;
				this.MemberConstraintIndices.Add(in nativeIndex);
			}
			else
			{
				Debug.LogError(string.Format("Constraint with index {0} is already in the group at group index {1}. It will not be added again.", constraint.NativeIndex, groupIndex));
			}
			if (constraint.CachedExecutionGroupIndex != groupIndex)
			{
				if (constraint.CachedExecutionGroupIndex >= 0)
				{
					Debug.LogError(string.Format("Assigning a group index of {0} to a constraint on \"{1}\" when it already has a group index of {2}", groupIndex, constraint.name, constraint.CachedExecutionGroupIndex));
				}
				constraint.SetCachedExecutionGroupIndex(groupIndex);
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00003888 File Offset: 0x00001A88
		public bool RemoveConstraintSwapBack(VRCConstraintBase constraint)
		{
			for (int i = 0; i < this.MemberCount; i++)
			{
				int num = this.MemberConstraintIndices[i];
				if (constraint.NativeIndex == num)
				{
					this.RemoveConstraintAtSwapBack(constraint, i);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000038C7 File Offset: 0x00001AC7
		public void RemoveConstraintAtSwapBack(VRCConstraintBase constraint, int knownIndex)
		{
			this.RemoveAtSwapBack(knownIndex);
			constraint.SetCachedExecutionGroupIndex(-1);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000038D8 File Offset: 0x00001AD8
		public bool UpdateNativeIndex(int oldIndex, int newIndex)
		{
			int num = -1;
			bool flag = false;
			for (int i = 0; i < this.MemberConstraintIndices.Length; i++)
			{
				if (this.MemberConstraintIndices[i] == oldIndex)
				{
					num = i;
					if (flag)
					{
						break;
					}
				}
				else if (this.MemberConstraintIndices[i] == newIndex)
				{
					Debug.LogError(string.Format("Constraint with index {0} is already in a group while updating from {1}.", newIndex, oldIndex));
					flag = true;
					if (num >= 0)
					{
						break;
					}
				}
			}
			if (num < 0)
			{
				return false;
			}
			if (!flag)
			{
				this.MemberConstraintIndices[num] = newIndex;
			}
			else
			{
				this.RemoveAtSwapBack(num);
			}
			return true;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00003968 File Offset: 0x00001B68
		private void RemoveAtSwapBack(int memberIndex)
		{
			this.MemberConstraintIndices.RemoveAtSwapBack(memberIndex);
			if (math.ceilpow2(this.MemberConstraintIndices.Capacity) > math.ceilpow2(this.MemberConstraintIndices.Length) * 2)
			{
				int num = math.max(math.ceilpow2(this.MemberConstraintIndices.Length) * 2, 8);
				if (num != this.MemberConstraintIndices.Capacity)
				{
					this.MemberConstraintIndices.SetCapacity(num);
				}
			}
		}

		// Token: 0x04000030 RID: 48
		private const int MinGroupCapacity = 8;

		// Token: 0x04000031 RID: 49
		public UnsafeList<int> MemberConstraintIndices;
	}
}
