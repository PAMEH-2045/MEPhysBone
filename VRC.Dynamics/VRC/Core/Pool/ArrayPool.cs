using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace VRC.Core.Pool
{
	// Token: 0x020006FC RID: 1788
	public static class ArrayPool
	{
		// Token: 0x060044E2 RID: 17634 RVA: 0x00128328 File Offset: 0x00126528
		public static PooledArray<T> Get<T>(int length)
		{
			return ArrayPool.ArrayPoolImpl<T>.Instance.Get(length);
		}

		// Token: 0x060044E3 RID: 17635 RVA: 0x00128338 File Offset: 0x00126538
		public static PooledArray<T> Get<T>([NotNull] out T[] array, int length)
		{
			T[] array2;
			array = (array2 = ArrayPool.Get<T>(length));
			return new PooledArray<T>(array2);
		}

		// Token: 0x060044E4 RID: 17636 RVA: 0x0012835A File Offset: 0x0012655A
		public static bool Contains<T>(T[] d)
		{
			return ArrayPool.ArrayPoolImpl<T>.Instance.Contains(d);
		}

		// Token: 0x060044E5 RID: 17637 RVA: 0x00128367 File Offset: 0x00126567
		public static void Release<T>(T[] d)
		{
			ArrayPool.ArrayPoolImpl<T>.Instance.Release(d);
		}

		// Token: 0x060044E6 RID: 17638 RVA: 0x00128374 File Offset: 0x00126574
		public static void Release<T>(ref T[] d)
		{
			ArrayPool.Release<T>(d);
			d = null;
		}

		// Token: 0x060044E7 RID: 17639 RVA: 0x00128380 File Offset: 0x00126580
		public static void Exchange<T>([NotNull] ref T[] d, int newSize)
		{
			T[] array = d;
			ArrayPool.Get<T>(out d, newSize);
			if (d != array)
			{
				ArrayPool.Release<T>(array);
				return;
			}
			if (d != null)
			{
				Array.Clear(d, 0, d.Length);
			}
		}

		// Token: 0x02000966 RID: 2406
		private class ArrayPoolImpl<T> : IPool
		{
			// Token: 0x17000EAC RID: 3756
			// (get) Token: 0x06004CF9 RID: 19705 RVA: 0x001400C3 File Offset: 0x0013E2C3
			public Type ObjectType
			{
				get
				{
					return Array.Empty<T>().GetType();
				}
			}

			// Token: 0x17000EAD RID: 3757
			// (get) Token: 0x06004CFA RID: 19706 RVA: 0x001400D0 File Offset: 0x0013E2D0
			public int CountInactive
			{
				get
				{
					int num = 0;
					object obj = this.syncLock;
					lock (obj)
					{
						foreach (ArrayPool.ArrayPoolImpl<T>.SizeGroup sizeGroup in this.Pool.Values)
						{
							num += sizeGroup.Count;
						}
					}
					return num;
				}
			}

			// Token: 0x06004CFB RID: 19707 RVA: 0x00140158 File Offset: 0x0013E358
			private ArrayPoolImpl()
			{
				PoolManager.AddPool(this);
			}

			// Token: 0x06004CFC RID: 19708 RVA: 0x0014017C File Offset: 0x0013E37C
			public PooledArray<T> Get(int length)
			{
				if (length < 1)
				{
					return new PooledArray<T>(Array.Empty<T>());
				}
				T[] array = null;
				object obj = this.syncLock;
				lock (obj)
				{
					ArrayPool.ArrayPoolImpl<T>.SizeGroup sizeGroup;
					if (this.Pool.TryGetValue(length, out sizeGroup))
					{
						array = sizeGroup.TryGetArray();
					}
				}
				if (array == null)
				{
					array = new T[length];
				}
				return new PooledArray<T>(array);
			}

			// Token: 0x06004CFD RID: 19709 RVA: 0x001401F0 File Offset: 0x0013E3F0
			public bool Contains(T[] d)
			{
				if (d == null || d.Length < 1)
				{
					return false;
				}
				object obj = this.syncLock;
				bool result;
				lock (obj)
				{
					ArrayPool.ArrayPoolImpl<T>.SizeGroup sizeGroup;
					if (!this.Pool.TryGetValue(d.Length, out sizeGroup))
					{
						result = false;
					}
					else
					{
						result = sizeGroup.Contains(d);
					}
				}
				return result;
			}

			// Token: 0x06004CFE RID: 19710 RVA: 0x00140254 File Offset: 0x0013E454
			public void Release(T[] d)
			{
				if (d == null || d.Length < 1)
				{
					return;
				}
				Array.Clear(d, 0, d.Length);
				object obj = this.syncLock;
				lock (obj)
				{
					ArrayPool.ArrayPoolImpl<T>.SizeGroup sizeGroup;
					if (this.Pool.TryGetValue(d.Length, out sizeGroup))
					{
						if (sizeGroup.Contains(d))
						{
							throw new InvalidOperationException("Array has already been returned to the pool");
						}
						sizeGroup.ReturnArray(d);
					}
					else
					{
						ArrayPool.ArrayPoolImpl<T>.SizeGroup sizeGroup2 = new ArrayPool.ArrayPoolImpl<T>.SizeGroup();
						sizeGroup2.ReturnArray(d);
						this.Pool.Add(d.Length, sizeGroup2);
					}
				}
			}

			// Token: 0x06004CFF RID: 19711 RVA: 0x001402F0 File Offset: 0x0013E4F0
			public int Cleanup()
			{
				List<int> list;
				int result;
				using (ListPool.Get<int>(out list))
				{
					int num = 0;
					object obj = this.syncLock;
					lock (obj)
					{
						foreach (KeyValuePair<int, ArrayPool.ArrayPoolImpl<T>.SizeGroup> keyValuePair in this.Pool)
						{
							num += keyValuePair.Value.Cleanup();
							if (keyValuePair.Value.Count < 1)
							{
								list.Add(keyValuePair.Key);
							}
						}
						foreach (int num2 in list)
						{
							this.Pool.Remove(num2);
						}
					}
					result = num;
				}
				return result;
			}

			// Token: 0x04002A7C RID: 10876
			private readonly object syncLock = new object();

			// Token: 0x04002A7D RID: 10877
			private readonly Dictionary<int, ArrayPool.ArrayPoolImpl<T>.SizeGroup> Pool = new Dictionary<int, ArrayPool.ArrayPoolImpl<T>.SizeGroup>();

			// Token: 0x04002A7E RID: 10878
			public static readonly ArrayPool.ArrayPoolImpl<T> Instance = new ArrayPool.ArrayPoolImpl<T>();

			// Token: 0x020009C2 RID: 2498
			private class SizeGroup
			{
				// Token: 0x17000F12 RID: 3858
				// (get) Token: 0x06004E42 RID: 20034 RVA: 0x0014225E File Offset: 0x0014045E
				public int Count
				{
					get
					{
						return this.ArrayStack.Count;
					}
				}

				// Token: 0x06004E43 RID: 20035 RVA: 0x0014226B File Offset: 0x0014046B
				public T[] TryGetArray()
				{
					if (this.ArrayStack.Count > 0)
					{
						this.LowWaterMark = Math.Min(this.LowWaterMark, this.ArrayStack.Count - 1);
						return this.ArrayStack.Pop();
					}
					return null;
				}

				// Token: 0x06004E44 RID: 20036 RVA: 0x001422A6 File Offset: 0x001404A6
				public void ReturnArray(T[] array)
				{
					this.ArrayStack.Push(array);
				}

				// Token: 0x06004E45 RID: 20037 RVA: 0x001422B4 File Offset: 0x001404B4
				public int Cleanup()
				{
					int lowWaterMark = this.LowWaterMark;
					for (;;)
					{
						int lowWaterMark2 = this.LowWaterMark;
						this.LowWaterMark = lowWaterMark2 - 1;
						if (lowWaterMark2 <= 0)
						{
							break;
						}
						this.ArrayStack.Pop();
					}
					this.LowWaterMark = this.ArrayStack.Count;
					return lowWaterMark;
				}

				// Token: 0x06004E46 RID: 20038 RVA: 0x001422FC File Offset: 0x001404FC
				public bool Contains(T[] array)
				{
					return this.ArrayStack.Contains(array);
				}

				// Token: 0x04002B27 RID: 11047
				private readonly Stack<T[]> ArrayStack = new Stack<T[]>();

				// Token: 0x04002B28 RID: 11048
				private int LowWaterMark;
			}
		}
	}
}
