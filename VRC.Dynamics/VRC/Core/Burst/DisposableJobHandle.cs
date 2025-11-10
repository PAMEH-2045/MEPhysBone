using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Jobs;

namespace VRC.Core.Burst
{
	// Token: 0x02000010 RID: 16
	public struct DisposableJobHandle : IDisposable, IEquatable<DisposableJobHandle>
	{
		// Token: 0x0600001D RID: 29 RVA: 0x00002FE4 File Offset: 0x000011E4
		[PublicAPI]
		public DisposableJobHandle(JobHandle jobHandle)
		{
			this._jobHandle = jobHandle;
			DisposableJobHandle.CullCompleteJobs();
			DisposableJobHandle._knownDisposableJobHandles.Add(this);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00003002 File Offset: 0x00001202
		[PublicAPI]
		public static IList<DisposableJobHandle> GetIncompleteDisposableJobs()
		{
			DisposableJobHandle.CullCompleteJobs();
			return DisposableJobHandle._knownDisposableJobHandles;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00003010 File Offset: 0x00001210
		private static void CullCompleteJobs()
		{
			for (int i = DisposableJobHandle._knownDisposableJobHandles.Count - 1; i >= 0; i--)
			{
				if (DisposableJobHandle._knownDisposableJobHandles[i].IsCompleted)
				{
					DisposableJobHandle._knownDisposableJobHandles.RemoveAt(i);
				}
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00003054 File Offset: 0x00001254
		[PublicAPI]
		public bool IsCompleted
		{
			get
			{
				return this._jobHandle.IsCompleted;
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00003061 File Offset: 0x00001261
		[PublicAPI]
		public void Complete()
		{
			this._jobHandle.Complete();
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000306E File Offset: 0x0000126E
		[PublicAPI]
		public void Dispose()
		{
			if (!this.IsCompleted)
			{
				this.Complete();
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000307E File Offset: 0x0000127E
		public bool Equals(DisposableJobHandle other)
		{
			return this._jobHandle.Equals(other._jobHandle);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003094 File Offset: 0x00001294
		public override bool Equals(object obj)
		{
			if (obj is DisposableJobHandle)
			{
				DisposableJobHandle other = (DisposableJobHandle)obj;
				return this.Equals(other);
			}
			return false;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000030B9 File Offset: 0x000012B9
		public static bool operator ==(DisposableJobHandle a, DisposableJobHandle b)
		{
			return a.Equals(b);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000030C3 File Offset: 0x000012C3
		public static bool operator !=(DisposableJobHandle a, DisposableJobHandle b)
		{
			return !(a == b);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000030CF File Offset: 0x000012CF
		public static implicit operator JobHandle(DisposableJobHandle disposableJobHandle)
		{
			return disposableJobHandle._jobHandle;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000030D7 File Offset: 0x000012D7
		public override int GetHashCode()
		{
			return this._jobHandle.GetHashCode();
		}

		// Token: 0x0400005F RID: 95
		private JobHandle _jobHandle;

		// Token: 0x04000060 RID: 96
		private static readonly List<DisposableJobHandle> _knownDisposableJobHandles = new List<DisposableJobHandle>(32);
	}
}
