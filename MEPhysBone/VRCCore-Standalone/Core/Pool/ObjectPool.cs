using System;
using System.Collections.Generic;

namespace VRC.Core.Pool
{
	// Token: 0x02000701 RID: 1793
	public class ObjectPool<T> : IObjectPool<T>, IPool, IDisposable where T : class
	{
		// Token: 0x17000D97 RID: 3479
		// (get) Token: 0x060044F5 RID: 17653 RVA: 0x00128425 File Offset: 0x00126625
		public Type ObjectType
		{
			get
			{
				return typeof(T);
			}
		}

		// Token: 0x060044F6 RID: 17654 RVA: 0x00128434 File Offset: 0x00126634
		public ObjectPool(Func<T> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 8, int maxSize = 1024)
		{
			this.CreateFunc = createFunc;
			this.OnGetFunc = actionOnGet;
			this.OnReleaseFunc = actionOnRelease;
			this.OnDestroyFunc = actionOnDestroy;
			this.CollectionChecks = collectionCheck;
			this.Pool = new Stack<T>(defaultCapacity);
			this.MaxSize = maxSize;
			if (this.MaxSize < 1 && this.MaxSize != -1)
			{
				throw new ArgumentException("maxSize must be positive or -1", "maxSize");
			}
			PoolManager.AddPool(this);
		}

		// Token: 0x17000D98 RID: 3480
		// (get) Token: 0x060044F7 RID: 17655 RVA: 0x001284B4 File Offset: 0x001266B4
		// (set) Token: 0x060044F8 RID: 17656 RVA: 0x001284BC File Offset: 0x001266BC
		public int CountActive { get; private set; }

		// Token: 0x17000D99 RID: 3481
		// (get) Token: 0x060044F9 RID: 17657 RVA: 0x001284C5 File Offset: 0x001266C5
		public int CountInactive
		{
			get
			{
				return this.Pool.Count;
			}
		}

		// Token: 0x17000D9A RID: 3482
		// (get) Token: 0x060044FA RID: 17658 RVA: 0x001284D2 File Offset: 0x001266D2
		public int CountAll
		{
			get
			{
				return this.CountActive + this.CountInactive;
			}
		}

		// Token: 0x060044FB RID: 17659 RVA: 0x001284E4 File Offset: 0x001266E4
		public void Clear()
		{
			object obj = this.syncLock;
			lock (obj)
			{
				if (this.OnDestroyFunc != null)
				{
					while (this.Pool.Count > 0)
					{
						this.OnDestroyFunc.Invoke(this.Pool.Pop());
					}
				}
				else
				{
					this.Pool.Clear();
				}
				this.CountActive = 0;
			}
		}

		// Token: 0x060044FC RID: 17660 RVA: 0x00128560 File Offset: 0x00126760
		public void Dispose()
		{
			this.Clear();
		}

		// Token: 0x060044FD RID: 17661 RVA: 0x00128568 File Offset: 0x00126768
		public T Get()
		{
			object obj = this.syncLock;
			T result;
			lock (obj)
			{
				T t = default(T);
				if (this.Pool.Count > 0)
				{
					t = this.Pool.Peek();
					Action<T> onGetFunc = this.OnGetFunc;
					if (onGetFunc != null)
					{
						onGetFunc.Invoke(t);
					}
					this.Pool.Pop();
				}
				else
				{
					t = this.CreateFunc.Invoke();
				}
				this.LowWaterMark = Math.Min(this.LowWaterMark, this.Pool.Count);
				if (t != null)
				{
					int countActive = this.CountActive;
					this.CountActive = countActive + 1;
				}
				result = t;
			}
			return result;
		}

		// Token: 0x060044FE RID: 17662 RVA: 0x0012862C File Offset: 0x0012682C
		public PooledObject<T> Get(out T v)
		{
			return new PooledObject<T>(this, v = this.Get());
		}

		// Token: 0x060044FF RID: 17663 RVA: 0x00128650 File Offset: 0x00126850
		public void Release(T element)
		{
			if (element == null)
			{
				return;
			}
			object obj = this.syncLock;
			lock (obj)
			{
				if (this.CollectionChecks && this.Pool.Contains(element))
				{
					throw new InvalidOperationException("Pool already contains object");
				}
				this.CountActive = Math.Max(this.CountActive - 1, 0);
				if (this.MaxSize > 0 && this.Pool.Count >= this.MaxSize)
				{
					Action<T> onDestroyFunc = this.OnDestroyFunc;
					if (onDestroyFunc != null)
					{
						onDestroyFunc.Invoke(element);
					}
				}
				else
				{
					Action<T> onReleaseFunc = this.OnReleaseFunc;
					if (onReleaseFunc != null)
					{
						onReleaseFunc.Invoke(element);
					}
					this.Pool.Push(element);
				}
			}
		}

		// Token: 0x06004500 RID: 17664 RVA: 0x00128718 File Offset: 0x00126918
		public int Cleanup()
		{
			object obj = this.syncLock;
			int num;
			lock (obj)
			{
				int lowWaterMark = this.LowWaterMark;
				for (;;)
				{
					num = this.LowWaterMark;
					this.LowWaterMark = num - 1;
					if (num <= 0)
					{
						break;
					}
					T t = this.Pool.Pop();
					Action<T> onDestroyFunc = this.OnDestroyFunc;
					if (onDestroyFunc != null)
					{
						onDestroyFunc.Invoke(t);
					}
				}
				this.LowWaterMark = this.Pool.Count;
				num = lowWaterMark;
			}
			return num;
		}

		// Token: 0x04002177 RID: 8567
		private readonly object syncLock = new object();

		// Token: 0x04002178 RID: 8568
		private readonly Stack<T> Pool;

		// Token: 0x04002179 RID: 8569
		private int LowWaterMark;

		// Token: 0x0400217A RID: 8570
		private readonly Func<T> CreateFunc;

		// Token: 0x0400217B RID: 8571
		private readonly Action<T> OnGetFunc;

		// Token: 0x0400217C RID: 8572
		private readonly Action<T> OnReleaseFunc;

		// Token: 0x0400217D RID: 8573
		private readonly Action<T> OnDestroyFunc;

		// Token: 0x0400217E RID: 8574
		private readonly bool CollectionChecks;

		// Token: 0x0400217F RID: 8575
		private readonly int MaxSize;
	}
}
