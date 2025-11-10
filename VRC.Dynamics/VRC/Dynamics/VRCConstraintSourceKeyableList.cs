using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace VRC.Dynamics
{
	// Token: 0x0200001D RID: 29
	[Serializable]
	public struct VRCConstraintSourceKeyableList : IList<VRCConstraintSource>, ICollection<VRCConstraintSource>, IEnumerable<VRCConstraintSource>, IEnumerable, IList, ICollection
	{
		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x000086D8 File Offset: 0x000068D8
		public int Count
		{
			get
			{
				return this.totalLength;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000FA RID: 250 RVA: 0x000086E0 File Offset: 0x000068E0
		private List<VRCConstraintSource> OverflowList
		{
			get
			{
				if (this.overflowList == null)
				{
					this.overflowList = new List<VRCConstraintSource>();
				}
				return this.overflowList;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000FB RID: 251 RVA: 0x000086FB File Offset: 0x000068FB
		private IEnumerator<VRCConstraintSource> ValueEnumerator
		{
			get
			{
				if (this._valueEnumerator == null)
				{
					this._valueEnumerator = new VRCConstraintSourceKeyableList.KeyableListEnumerator(ref this);
				}
				else
				{
					this._valueEnumerator.Reset();
				}
				return this._valueEnumerator;
			}
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000872C File Offset: 0x0000692C
		public VRCConstraintSourceKeyableList(int initialLength)
		{
			if (initialLength < 0)
			{
				throw new ArgumentOutOfRangeException("initialLength", "The initial length cannot be a value less than zero.");
			}
			int num = Mathf.Max(0, initialLength - 16);
			this.overflowList = new List<VRCConstraintSource>(num);
			for (int i = 0; i < num; i++)
			{
				this.overflowList.Add(VRCConstraintSource.CreateDefault());
			}
			this.totalLength = initialLength;
			this.source0 = default(VRCConstraintSource);
			this.source1 = default(VRCConstraintSource);
			this.source2 = default(VRCConstraintSource);
			this.source3 = default(VRCConstraintSource);
			this.source4 = default(VRCConstraintSource);
			this.source5 = default(VRCConstraintSource);
			this.source6 = default(VRCConstraintSource);
			this.source7 = default(VRCConstraintSource);
			this.source8 = default(VRCConstraintSource);
			this.source9 = default(VRCConstraintSource);
			this.source10 = default(VRCConstraintSource);
			this.source11 = default(VRCConstraintSource);
			this.source12 = default(VRCConstraintSource);
			this.source13 = default(VRCConstraintSource);
			this.source14 = default(VRCConstraintSource);
			this.source15 = default(VRCConstraintSource);
			this._valueEnumerator = null;
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00008850 File Offset: 0x00006A50
		public VRCConstraintSourceKeyableList(IList<VRCConstraintSource> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list", "A list must be defined.");
			}
			int count = list.Count;
			int num = Mathf.Max(0, count - 16);
			this.overflowList = new List<VRCConstraintSource>(num);
			for (int i = 0; i < num; i++)
			{
				this.overflowList.Add(list[i + 16]);
			}
			this.totalLength = count;
			this.source0 = ((count > 0) ? list[0] : default(VRCConstraintSource));
			this.source1 = ((count > 1) ? list[1] : default(VRCConstraintSource));
			this.source2 = ((count > 2) ? list[2] : default(VRCConstraintSource));
			this.source3 = ((count > 3) ? list[3] : default(VRCConstraintSource));
			this.source4 = ((count > 4) ? list[4] : default(VRCConstraintSource));
			this.source5 = ((count > 5) ? list[5] : default(VRCConstraintSource));
			this.source6 = ((count > 6) ? list[6] : default(VRCConstraintSource));
			this.source7 = ((count > 7) ? list[7] : default(VRCConstraintSource));
			this.source8 = ((count > 8) ? list[8] : default(VRCConstraintSource));
			this.source9 = ((count > 9) ? list[9] : default(VRCConstraintSource));
			this.source10 = ((count > 10) ? list[10] : default(VRCConstraintSource));
			this.source11 = ((count > 11) ? list[11] : default(VRCConstraintSource));
			this.source12 = ((count > 12) ? list[12] : default(VRCConstraintSource));
			this.source13 = ((count > 13) ? list[13] : default(VRCConstraintSource));
			this.source14 = ((count > 14) ? list[14] : default(VRCConstraintSource));
			this.source15 = ((count > 15) ? list[15] : default(VRCConstraintSource));
			this._valueEnumerator = null;
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00008A8C File Offset: 0x00006C8C
		private VRCConstraintSource Get(int index)
		{
			switch (index)
			{
			case 0:
				return this.source0;
			case 1:
				return this.source1;
			case 2:
				return this.source2;
			case 3:
				return this.source3;
			case 4:
				return this.source4;
			case 5:
				return this.source5;
			case 6:
				return this.source6;
			case 7:
				return this.source7;
			case 8:
				return this.source8;
			case 9:
				return this.source9;
			case 10:
				return this.source10;
			case 11:
				return this.source11;
			case 12:
				return this.source12;
			case 13:
				return this.source13;
			case 14:
				return this.source14;
			case 15:
				return this.source15;
			default:
				return this.OverflowList[index - 16];
			}
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00008B60 File Offset: 0x00006D60
		private void Set(int index, VRCConstraintSource value)
		{
			switch (index)
			{
			case 0:
				this.source0 = value;
				return;
			case 1:
				this.source1 = value;
				return;
			case 2:
				this.source2 = value;
				return;
			case 3:
				this.source3 = value;
				return;
			case 4:
				this.source4 = value;
				return;
			case 5:
				this.source5 = value;
				return;
			case 6:
				this.source6 = value;
				return;
			case 7:
				this.source7 = value;
				return;
			case 8:
				this.source8 = value;
				return;
			case 9:
				this.source9 = value;
				return;
			case 10:
				this.source10 = value;
				return;
			case 11:
				this.source11 = value;
				return;
			case 12:
				this.source12 = value;
				return;
			case 13:
				this.source13 = value;
				return;
			case 14:
				this.source14 = value;
				return;
			case 15:
				this.source15 = value;
				return;
			default:
			{
				int num = index - 16;
				if (num < this.OverflowList.Count)
				{
					this.OverflowList[num] = value;
					return;
				}
				if (num == this.OverflowList.Count)
				{
					this.OverflowList.Add(value);
					return;
				}
				throw new ArgumentOutOfRangeException(string.Format("Index {0} is out of range of the overflow buffer ({1} vs {2})", index, num, this.OverflowList.Count));
			}
			}
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00008C9F File Offset: 0x00006E9F
		public IEnumerator<VRCConstraintSource> GetEnumerator()
		{
			return this.ValueEnumerator;
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00008CA7 File Offset: 0x00006EA7
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.ValueEnumerator;
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00008CAF File Offset: 0x00006EAF
		int IList.Add(object item)
		{
			this.Add((VRCConstraintSource)item);
			return this.totalLength - 1;
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00008CC5 File Offset: 0x00006EC5
		public void Add(VRCConstraintSource item)
		{
			this.Set(this.totalLength, item);
			this.totalLength++;
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00008CE2 File Offset: 0x00006EE2
		public void Clear()
		{
			this.totalLength = 0;
			this.OverflowList.Clear();
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00008CF6 File Offset: 0x00006EF6
		int IList.IndexOf(object item)
		{
			return this.IndexOf((VRCConstraintSource)item);
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00008D04 File Offset: 0x00006F04
		public int IndexOf(VRCConstraintSource item)
		{
			for (int i = 0; i < this.totalLength; i++)
			{
				if (this.Get(i).Equals(item))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00008D44 File Offset: 0x00006F44
		bool IList.Contains(object item)
		{
			for (int i = 0; i < this.totalLength; i++)
			{
				if (this.Get(i).Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00008D80 File Offset: 0x00006F80
		public bool Contains(VRCConstraintSource item)
		{
			for (int i = 0; i < this.totalLength; i++)
			{
				if (this.Get(i).Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00008DC0 File Offset: 0x00006FC0
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", "The array cannot be null.");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "The starting array index cannot be negative.");
			}
			if (this.totalLength > array.Length - arrayIndex + 1)
			{
				throw new ArgumentException("The destination array has fewer elements than the collection.");
			}
			for (int i = 0; i < this.totalLength; i++)
			{
				array.SetValue(this.Get(i), i + arrayIndex);
			}
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00008E38 File Offset: 0x00007038
		public void CopyTo(VRCConstraintSource[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", "The array cannot be null.");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "The starting array index cannot be negative.");
			}
			if (this.totalLength > array.Length - arrayIndex + 1)
			{
				throw new ArgumentException("The destination array has fewer elements than the collection.");
			}
			for (int i = 0; i < this.totalLength; i++)
			{
				array[i + arrayIndex] = this.Get(i);
			}
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00008EA7 File Offset: 0x000070A7
		void IList.Remove(object item)
		{
			this.Remove((VRCConstraintSource)item);
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00008EB8 File Offset: 0x000070B8
		public bool Remove(VRCConstraintSource item)
		{
			for (int i = 0; i < this.totalLength; i++)
			{
				if (this.Get(i).Equals(item))
				{
					this.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00008F00 File Offset: 0x00007100
		public void RemoveAt(int index)
		{
			if (index < 16)
			{
				while (index < 15 && index < this.totalLength - 1)
				{
					this.Set(index, this.Get(index + 1));
					index++;
				}
				if (this.totalLength > 16)
				{
					this.Set(15, this.OverflowList[0]);
					this.OverflowList.RemoveAt(0);
				}
			}
			else
			{
				int num = index - 16;
				this.OverflowList.RemoveAt(num);
			}
			this.totalLength--;
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00008F84 File Offset: 0x00007184
		void IList.Insert(int index, object value)
		{
			this.Insert(index, (VRCConstraintSource)value);
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00008F94 File Offset: 0x00007194
		public void Insert(int index, VRCConstraintSource item)
		{
			if (index >= 16)
			{
				int num = index - 16;
				this.OverflowList.Insert(num, item);
				this.totalLength++;
				return;
			}
			for (int i = this.totalLength; i > index; i--)
			{
				this.Set(i, this.Get(i - 1));
			}
			this.Set(index, item);
			this.totalLength++;
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00009000 File Offset: 0x00007200
		public void SetLength(int newLength)
		{
			if (newLength < 0)
			{
				throw new ArgumentOutOfRangeException("newLength", "The length must be an integer greater than zero.");
			}
			if (newLength > this.totalLength)
			{
				for (int i = this.totalLength; i < Mathf.Min(newLength, 16); i++)
				{
					this.Set(i, VRCConstraintSource.CreateDefault());
				}
				int num = newLength - 16;
				int count = this.OverflowList.Count;
				for (int j = 0; j < num - count; j++)
				{
					this.OverflowList.Add(VRCConstraintSource.CreateDefault());
				}
			}
			else if (newLength < this.totalLength)
			{
				int num2 = Mathf.Max(0, newLength - 16);
				this.OverflowList.RemoveRange(num2, this.OverflowList.Count - num2);
			}
			this.totalLength = newLength;
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000111 RID: 273 RVA: 0x000090B6 File Offset: 0x000072B6
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000112 RID: 274 RVA: 0x000090B9 File Offset: 0x000072B9
		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000113 RID: 275 RVA: 0x000090BC File Offset: 0x000072BC
		bool ICollection.IsSynchronized
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000114 RID: 276 RVA: 0x000090BF File Offset: 0x000072BF
		object ICollection.SyncRoot
		{
			get
			{
				throw new NotSupportedException("SyncRoot is not supported on this list type.");
			}
		}

        // Token: 0x17000023 RID: 35
        // (get) Token: 0x06000115 RID: 277 RVA: 0x000090CB File Offset: 0x000072CB
        // (set) Token: 0x06000116 RID: 278 RVA: 0x000090D9 File Offset: 0x000072D9
        object IList.this[int index]
        {
            get
            {
                return this.Get(index);
            }
            set
            {
                this.Set(index, (VRCConstraintSource)value);
            }
        }

        // Token: 0x17000024 RID: 36
        public VRCConstraintSource this[int index]
		{
			get
			{
				return this.Get(index);
			}
			set
			{
				this.Set(index, value);
			}
		}

		// Token: 0x040000B9 RID: 185
		public const int MaxFlatLength = 16;

		// Token: 0x040000BA RID: 186
		[SerializeField]
		private VRCConstraintSource source0;

		// Token: 0x040000BB RID: 187
		[SerializeField]
		private VRCConstraintSource source1;

		// Token: 0x040000BC RID: 188
		[SerializeField]
		private VRCConstraintSource source2;

		// Token: 0x040000BD RID: 189
		[SerializeField]
		private VRCConstraintSource source3;

		// Token: 0x040000BE RID: 190
		[SerializeField]
		private VRCConstraintSource source4;

		// Token: 0x040000BF RID: 191
		[SerializeField]
		private VRCConstraintSource source5;

		// Token: 0x040000C0 RID: 192
		[SerializeField]
		private VRCConstraintSource source6;

		// Token: 0x040000C1 RID: 193
		[SerializeField]
		private VRCConstraintSource source7;

		// Token: 0x040000C2 RID: 194
		[SerializeField]
		private VRCConstraintSource source8;

		// Token: 0x040000C3 RID: 195
		[SerializeField]
		private VRCConstraintSource source9;

		// Token: 0x040000C4 RID: 196
		[SerializeField]
		private VRCConstraintSource source10;

		// Token: 0x040000C5 RID: 197
		[SerializeField]
		private VRCConstraintSource source11;

		// Token: 0x040000C6 RID: 198
		[SerializeField]
		private VRCConstraintSource source12;

		// Token: 0x040000C7 RID: 199
		[SerializeField]
		private VRCConstraintSource source13;

		// Token: 0x040000C8 RID: 200
		[SerializeField]
		private VRCConstraintSource source14;

		// Token: 0x040000C9 RID: 201
		[SerializeField]
		private VRCConstraintSource source15;

		// Token: 0x040000CA RID: 202
		[SerializeField]
		[HideInInspector]
		[NotKeyable]
		private int totalLength;

		// Token: 0x040000CB RID: 203
		[SerializeField]
		[NotKeyable]
		private List<VRCConstraintSource> overflowList;

		// Token: 0x040000CC RID: 204
		private IEnumerator<VRCConstraintSource> _valueEnumerator;

		// Token: 0x0200005B RID: 91
		private struct KeyableListEnumerator : IEnumerator<VRCConstraintSource>, IEnumerator, IDisposable
		{
			// Token: 0x060002B7 RID: 695 RVA: 0x00012522 File Offset: 0x00010722
			public KeyableListEnumerator(ref VRCConstraintSourceKeyableList list)
			{
				this._keyableList = list;
				this._index = -1;
			}

			// Token: 0x060002B8 RID: 696 RVA: 0x00012537 File Offset: 0x00010737
			public bool MoveNext()
			{
				this._index++;
				return this._index < this._keyableList.Count;
			}

			// Token: 0x060002B9 RID: 697 RVA: 0x0001255A File Offset: 0x0001075A
			public void Reset()
			{
				this._index = -1;
			}

			// Token: 0x060002BA RID: 698 RVA: 0x00012563 File Offset: 0x00010763
			void IDisposable.Dispose()
			{
			}

			// Token: 0x17000051 RID: 81
			// (get) Token: 0x060002BB RID: 699 RVA: 0x00012565 File Offset: 0x00010765
			public VRCConstraintSource Current
			{
				get
				{
					return this._keyableList.Get(this._index);
				}
			}

			// Token: 0x17000052 RID: 82
			// (get) Token: 0x060002BC RID: 700 RVA: 0x00012578 File Offset: 0x00010778
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			// Token: 0x04000291 RID: 657
			private VRCConstraintSourceKeyableList _keyableList;

			// Token: 0x04000292 RID: 658
			private int _index;
		}
	}
}
