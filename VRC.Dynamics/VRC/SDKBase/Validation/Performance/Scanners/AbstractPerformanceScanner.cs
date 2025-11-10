using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x0200006F RID: 111
	public abstract class AbstractPerformanceScanner
	{
		// Token: 0x060002ED RID: 749
		public abstract IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent);

		// Token: 0x060002EE RID: 750 RVA: 0x0000DA5C File Offset: 0x0000BC5C
		public void RunPerformanceScan(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			this._limitComponentScansPerFrame = false;
			try
			{
				this._coroutines.Push(this.RunPerformanceScanEnumerator(avatarObject, perfStats, shouldIgnoreComponent));
				while (this._coroutines.Count > 0)
				{
					IEnumerator enumerator = this._coroutines.Peek();
					if (enumerator.MoveNext())
					{
						IEnumerator enumerator2 = enumerator.Current as IEnumerator;
						if (enumerator2 != null)
						{
							this._coroutines.Push(enumerator2);
						}
					}
					else
					{
						this._coroutines.Pop();
					}
				}
				this._coroutines.Clear();
			}
			finally
			{
				this._limitComponentScansPerFrame = true;
			}
		}

		// Token: 0x060002EF RID: 751 RVA: 0x0000DAF8 File Offset: 0x0000BCF8
		protected IEnumerator ScanAvatarForComponentsOfType(Type componentType, GameObject avatarObject, List<Component> destinationBuffer)
		{
			yield return this.HandleComponentScansPerFrameLimit();
			destinationBuffer.Clear();
			destinationBuffer.AddRange(avatarObject.GetComponentsInChildren(componentType, true));
			yield break;
		}

		// Token: 0x060002F0 RID: 752 RVA: 0x0000DB1C File Offset: 0x0000BD1C
		protected IEnumerator ScanAvatarForComponentsOfType<T>(GameObject avatarObject, List<T> destinationBuffer)
		{
			yield return this.HandleComponentScansPerFrameLimit();
			destinationBuffer.Clear();
			avatarObject.GetComponentsInChildren<T>(true, destinationBuffer);
			yield return null;
			yield break;
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x0000DB39 File Offset: 0x0000BD39
		private IEnumerator HandleComponentScansPerFrameLimit()
		{
			if (!this._limitComponentScansPerFrame)
			{
				yield break;
			}
			while (AbstractPerformanceScanner._componentScansThisFrame >= 10)
			{
				if (Time.frameCount > AbstractPerformanceScanner._componentScansFrameNumber)
				{
					AbstractPerformanceScanner._componentScansFrameNumber = Time.frameCount;
					AbstractPerformanceScanner._componentScansThisFrame = 0;
					break;
				}
				yield return null;
			}
			AbstractPerformanceScanner._componentScansThisFrame++;
			yield break;
		}

		// Token: 0x060002F2 RID: 754 RVA: 0x0000DB48 File Offset: 0x0000BD48
		public virtual bool EnabledOnPlatform(PerformancePlatform platform)
		{
			return true;
		}

		// Token: 0x04000375 RID: 885
		private const int MAXIMUM_COMPONENT_SCANS_PER_FRAME = 10;

		// Token: 0x04000376 RID: 886
		private static int _componentScansThisFrame;

		// Token: 0x04000377 RID: 887
		private static int _componentScansFrameNumber;

		// Token: 0x04000378 RID: 888
		private readonly Stack<IEnumerator> _coroutines = new Stack<IEnumerator>();

		// Token: 0x04000379 RID: 889
		private bool _limitComponentScansPerFrame = true;
	}
}
