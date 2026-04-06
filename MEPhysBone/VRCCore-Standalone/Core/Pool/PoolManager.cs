using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace VRC.Core.Pool
{
	// Token: 0x02000702 RID: 1794
	[ExecuteInEditMode]
	internal class PoolManager : MonoBehaviour
	{
		// Token: 0x06004501 RID: 17665 RVA: 0x001287A8 File Offset: 0x001269A8
		private static PoolManager TryInitialize()
		{
			PoolManager result;
			try
			{
				if (PoolManager.Instance == null)
				{
					GameObject gameObject = GameObject.Find("VRCPoolManager");
					if (gameObject == null)
					{
						gameObject = new GameObject("VRCPoolManager")
						{
							hideFlags = HideFlags.HideAndDontSave
						};
						try
						{
							if (!Application.isEditor || Application.isPlaying)
							{
                                UnityEngine.Object.DontDestroyOnLoad(gameObject);
							}
						}
						catch (Exception ex)
						{
                            Debug.LogError(string.Format("Failed to make {0} DontDestroyOnLoad: {1}", "VRCPoolManager", ex));
						}
						gameObject.AddComponent<PoolManager>();
					}
					PoolManager.Instance = gameObject.GetComponent<PoolManager>();
					PoolManager.Instance.StartCoroutine(PoolManager.Instance.PoolCleanupTask());
				}
				result = PoolManager.Instance;
			}
			catch (Exception ex2)
			{
                Debug.LogWarning(string.Format("Unable to initialize PoolManager instance: {0}", ex2));
				result = null;
			}
			return result;
		}

		// Token: 0x06004502 RID: 17666 RVA: 0x0012887C File Offset: 0x00126A7C
		private IEnumerator PoolCleanupTask()
		{
			for (;;)
			{
				yield return this.PoolCleanupTaskInterval;
				using (PoolManager._cleanupProfilerMarker.Auto())
				{
					object obj = PoolManager.syncLock;
					lock (obj)
					{
						int num = 0;
						for (int i = 0; i < PoolManager.Pools.Count; i++)
						{
							IPool pool;
							if (!PoolManager.Pools[i].TryGetTarget(out pool))
							{
								PoolManager.Pools.RemoveAt(i);
								i--;
							}
							else
							{
								num += pool.Cleanup();
							}
						}
						continue;
					}
				}
				yield break;
			}
		}

		// Token: 0x06004503 RID: 17667 RVA: 0x0012888C File Offset: 0x00126A8C
		public void PrintPoolStatistics()
		{
			StringBuilder stringBuilder;
			using (StringBuilderPool.Get(out stringBuilder))
			{
				stringBuilder.Append("Pool statistics: ");
				int length = stringBuilder.Length;
				int num = 0;
				foreach (IPool pool in PoolManager.GetAllPools())
				{
					int countInactive = pool.CountInactive;
					//stringBuilder.AppendFormat("\tPool<{0}>: {1} objects\n", pool.ObjectType.GetFriendlyGenericTypeName(false), countInactive);
					stringBuilder.AppendFormat("\tPool<{0}>: {1} objects\n", "Здесь типо должно быть название пула", countInactive);
					num += countInactive;
				}
				stringBuilder.Insert(length, string.Format("{0} total objects\n", num));
				stringBuilder.AppendLine();
				Debug.LogError(stringBuilder.ToString());
			}
		}

		// Token: 0x06004504 RID: 17668 RVA: 0x0012896C File Offset: 0x00126B6C
		public static void AddPool(IPool pool)
		{
			if (pool == null)
			{
				return;
			}
			PoolManager.TryInitialize();
			object obj = PoolManager.syncLock;
			lock (obj)
			{
				if (!Enumerable.Any<WeakReference<IPool>>(PoolManager.Pools, delegate(WeakReference<IPool> p)
				{
					IPool pool2;
					return p.TryGetTarget(out pool2) && pool2 == pool;
				}))
				{
					PoolManager.Pools.Add(new WeakReference<IPool>(pool, false));
				}
			}
		}

		// Token: 0x06004505 RID: 17669 RVA: 0x001289F4 File Offset: 0x00126BF4
		public static List<IPool> GetAllPools()
		{
			PoolManager.TryInitialize();
			object obj = PoolManager.syncLock;
			List<IPool> result;
			lock (obj)
			{
				result = Enumerable.ToList<IPool>(Enumerable.Where<IPool>(Enumerable.Select<WeakReference<IPool>, IPool>(PoolManager.Pools, delegate(WeakReference<IPool> p)
				{
					IPool result2;
					if (!p.TryGetTarget(out result2))
					{
						return null;
					}
					return result2;
				}), (IPool p) => p != null));
			}
			return result;
		}

		// Token: 0x04002181 RID: 8577
		private static readonly object syncLock = new object();

		// Token: 0x04002182 RID: 8578
		private static readonly List<WeakReference<IPool>> Pools = new List<WeakReference<IPool>>();

		// Token: 0x04002183 RID: 8579
		private readonly WaitForSeconds PoolCleanupTaskInterval = new WaitForSeconds(30f);

		// Token: 0x04002184 RID: 8580
		private static readonly ProfilerMarker _cleanupProfilerMarker = new ProfilerMarker("PoolManager.PoolCleanupTask");

		// Token: 0x04002185 RID: 8581
		private static PoolManager Instance;
	}
}
