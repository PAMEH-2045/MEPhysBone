using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace VRC.Core
{
	// Token: 0x020006FA RID: 1786
	public sealed class UpdateDelegator
	{
		// Token: 0x060044DA RID: 17626 RVA: 0x001281A0 File Offset: 0x001263A0
		public static void Dispatch(Action job, UpdateDelegator.JobPriority priority = UpdateDelegator.JobPriority.Normal)
		{
			if (job == null)
			{
				Debug.LogError("Ignoring NULL job");
				return;
			}
			object queueLock = UpdateDelegator._queueLock;
			lock (queueLock)
			{
				UpdateDelegator._jobQueue.Push(new UpdateDelegator.QueuedJob(job, priority));
			}
		}

		// Token: 0x060044DB RID: 17627 RVA: 0x001281F8 File Offset: 0x001263F8
		public static void DispatchAfter(Action job, float seconds, UpdateDelegator.JobPriority priority = UpdateDelegator.JobPriority.Normal)
		{
			UniTaskExtensions.Forget(UniTaskExtensions.ContinueWith(UniTask.Delay(TimeSpan.FromSeconds((double)seconds), false, PlayerLoopTiming.Update, default(CancellationToken)), delegate()
			{
				UpdateDelegator.Dispatch(job, priority);
			}));
		}

		// Token: 0x060044DC RID: 17628 RVA: 0x00128248 File Offset: 0x00126448
		public static void ManagedUpdate()
		{
			for (int i = 64; i > 0; i--)
			{
				try
				{
					object queueLock = UpdateDelegator._queueLock;
					UpdateDelegator.QueuedJob queuedJob;
					lock (queueLock)
					{
						if (!UpdateDelegator._jobQueue.Pop(out queuedJob))
						{
							break;
						}
					}
					queuedJob.Invoke();
				}
				catch (Exception ex)
				{
					Debug.LogErrorFormat("Caught {0} in UpdateDelegator Job: {1}\n{2}", new object[]
					{
						ex.GetType().Name,
						ex.Message,
						ex.StackTrace
					});
				}
			}
		}

		// Token: 0x04002171 RID: 8561
		private static readonly object _queueLock = new object();

		// Token: 0x04002172 RID: 8562
		private static readonly PriorityQueue<UpdateDelegator.QueuedJob> _jobQueue = new PriorityQueue<UpdateDelegator.QueuedJob>();

		// Token: 0x04002173 RID: 8563
		private const int MaxJobsPerFrame = 64;

		// Token: 0x02000963 RID: 2403
		[PublicAPI]
		public enum JobPriority
		{
			// Token: 0x04002A73 RID: 10867
			ApiBlocking,
			// Token: 0x04002A74 RID: 10868
			ApiMetadata,
			// Token: 0x04002A75 RID: 10869
			Normal
		}

		// Token: 0x02000964 RID: 2404
		private struct QueuedJob : IComparable<UpdateDelegator.QueuedJob>
		{
			// Token: 0x06004CF4 RID: 19700 RVA: 0x00140048 File Offset: 0x0013E248
			public QueuedJob([NotNull] Action job, UpdateDelegator.JobPriority priority)
			{
				this._job = job;
				this._priority = priority;
				ulong nextId = UpdateDelegator.QueuedJob._nextId;
				UpdateDelegator.QueuedJob._nextId = nextId + 1UL;
				this._id = nextId;
			}

			// Token: 0x06004CF5 RID: 19701 RVA: 0x0014006C File Offset: 0x0013E26C
			public void Invoke()
			{
				this._job.Invoke();
			}

			// Token: 0x06004CF6 RID: 19702 RVA: 0x00140079 File Offset: 0x0013E279
			int IComparable<UpdateDelegator.QueuedJob>.CompareTo(UpdateDelegator.QueuedJob other)
			{
				if (this._priority == other._priority)
				{
					return this._id.CompareTo(other._id);
				}
				return this._priority - other._priority;
			}

			// Token: 0x04002A76 RID: 10870
			private readonly Action _job;

			// Token: 0x04002A77 RID: 10871
			private readonly UpdateDelegator.JobPriority _priority;

			// Token: 0x04002A78 RID: 10872
			private readonly ulong _id;

			// Token: 0x04002A79 RID: 10873
			private static ulong _nextId;
		}
	}
}
