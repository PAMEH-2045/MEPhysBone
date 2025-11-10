using System;
using System.Collections.Generic;

namespace VRC.Core
{
	// Token: 0x020006F8 RID: 1784
	public class PriorityQueue<T> where T : IComparable<T>
	{
		// Token: 0x17000D93 RID: 3475
		// (get) Token: 0x060044C4 RID: 17604 RVA: 0x00127B3E File Offset: 0x00125D3E
		public IReadOnlyList<T> Data
		{
			get
			{
				return this.data;
			}
		}

		// Token: 0x17000D94 RID: 3476
		// (get) Token: 0x060044C5 RID: 17605 RVA: 0x00127B46 File Offset: 0x00125D46
		public int Count
		{
			get
			{
				return this.data.Count;
			}
		}

		// Token: 0x060044C6 RID: 17606 RVA: 0x00127B53 File Offset: 0x00125D53
		public PriorityQueue()
		{
			this.data = new List<T>();
		}

		// Token: 0x060044C7 RID: 17607 RVA: 0x00127B66 File Offset: 0x00125D66
		public PriorityQueue(int capacity)
		{
			this.data = new List<T>(capacity);
		}

		// Token: 0x060044C8 RID: 17608 RVA: 0x00127B7A File Offset: 0x00125D7A
		private void CheckForSort()
		{
			if (this.requiresSort)
			{
				this.data.Sort(PriorityQueue<T>.reverseComparer);
				this.requiresSort = false;
			}
		}

		// Token: 0x060044C9 RID: 17609 RVA: 0x00127BA0 File Offset: 0x00125DA0
		public void Concat(PriorityQueue<T> toAdd)
		{
			if (toAdd.Count == 0)
			{
				return;
			}
			this.CheckForSort();
			int num = this.data.Count + toAdd.Count;
			if (this.concatBuffer == null)
			{
				this.concatBuffer = new List<T>(num);
			}
			else if (this.concatBuffer.Capacity < num)
			{
				this.concatBuffer.Capacity = num;
			}
			IReadOnlyList<T> readOnlyList = toAdd.Data;
			int i = 0;
			int j = 0;
			while (i < this.data.Count)
			{
				if (j >= toAdd.Count)
				{
					break;
				}
				T t = this.data[i];
				if (t.CompareTo(readOnlyList[j]) > 0)
				{
					this.concatBuffer.Add(this.data[i++]);
				}
				else
				{
					this.concatBuffer.Add(readOnlyList[j++]);
				}
			}
			while (i < this.data.Count)
			{
				this.concatBuffer.Add(this.data[i++]);
			}
			while (j < toAdd.Count)
			{
				this.concatBuffer.Add(readOnlyList[j++]);
			}
			List<T> list = this.concatBuffer;
			List<T> list2 = this.data;
			this.data = list;
			this.concatBuffer = list2;
			this.concatBuffer.Clear();
		}

		// Token: 0x060044CA RID: 17610 RVA: 0x00127CF4 File Offset: 0x00125EF4
		public void Push(T val)
		{
			if (this.data.Count == 0)
			{
				this.data.Add(val);
				return;
			}
			this.CheckForSort();
			int i = this.data.BinarySearch(val, PriorityQueue<T>.reverseComparer);
			if (i < 0)
			{
				i = ~i;
			}
			while (i > 0)
			{
				T t = this.data[i - 1];
				if (t.CompareTo(val) != 0)
				{
					break;
				}
				i--;
			}
			this.data.Insert(i, val);
		}

		// Token: 0x060044CB RID: 17611 RVA: 0x00127D78 File Offset: 0x00125F78
		public bool Pop(out T element)
		{
			int num = this.Count;
			if (num == 0)
			{
				element = default(T);
				return false;
			}
			this.CheckForSort();
			element = this.data[--num];
			this.data.RemoveAt(num);
			return true;
		}

		// Token: 0x060044CC RID: 17612 RVA: 0x00127DC4 File Offset: 0x00125FC4
		public bool Peek(out T element)
		{
			int count = this.Count;
			if (count == 0)
			{
				element = default(T);
				return false;
			}
			this.CheckForSort();
			element = this.data[count - 1];
			return true;
		}

		// Token: 0x060044CD RID: 17613 RVA: 0x00127E00 File Offset: 0x00126000
		public bool PopNextMatch(Predicate<T> match, ref int startIndex, out T element)
		{
			int count = this.Count;
			if (count == 0 || startIndex < 0 || startIndex >= count)
			{
				element = default(T);
				startIndex = -1;
				return false;
			}
			this.CheckForSort();
			int i = count - startIndex - 1;
			while (i >= 0)
			{
				if (match.Invoke(this.data[i]))
				{
					element = this.data[i];
					this.data.RemoveAt(i);
					return true;
				}
				i--;
				startIndex++;
			}
			element = default(T);
			startIndex = -1;
			return false;
		}

		// Token: 0x060044CE RID: 17614 RVA: 0x00127E89 File Offset: 0x00126089
		public void RemoveWhere(Predicate<T> match)
		{
			this.data.RemoveAll(match);
		}

		// Token: 0x060044CF RID: 17615 RVA: 0x00127E98 File Offset: 0x00126098
		public bool Remove(T element)
		{
			return this.data.Remove(element);
		}

		// Token: 0x060044D0 RID: 17616 RVA: 0x00127EA6 File Offset: 0x001260A6
		public void RemoveAt(int index)
		{
			this.data.RemoveAt(index);
		}

		// Token: 0x060044D1 RID: 17617 RVA: 0x00127EB4 File Offset: 0x001260B4
		public void Clear()
		{
			this.data.Clear();
			this.requiresSort = false;
		}

		// Token: 0x060044D2 RID: 17618 RVA: 0x00127EC8 File Offset: 0x001260C8
		public void SetDirty()
		{
			this.requiresSort = true;
		}

		// Token: 0x04002167 RID: 8551
		private List<T> data;

		// Token: 0x04002168 RID: 8552
		private List<T> concatBuffer;

		// Token: 0x04002169 RID: 8553
		private bool requiresSort;

		// Token: 0x0400216A RID: 8554
		private static readonly PriorityQueue<T>.ReverseComparer reverseComparer;

		// Token: 0x02000962 RID: 2402
		private struct ReverseComparer : IComparer<T>
		{
			// Token: 0x06004CF3 RID: 19699 RVA: 0x00140038 File Offset: 0x0013E238
			public int Compare(T x, T y)
			{
				return y.CompareTo(x);
			}
		}
	}
}
