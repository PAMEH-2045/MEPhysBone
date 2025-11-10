using System;
using System.Collections;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Filters
{
	// Token: 0x02000079 RID: 121
	public abstract class AbstractPerformanceFilter : ScriptableObject
	{
		// Token: 0x0600030F RID: 783
		public abstract IEnumerator ApplyPerformanceFilter(GameObject avatarObject, AvatarPerformanceStats perfStats, PerformanceRating ratingLimit, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent, AvatarPerformance.FilterBlockCallback onBlock, bool userForcedShow);

		// Token: 0x06000310 RID: 784 RVA: 0x0000EB9E File Offset: 0x0000CD9E
		protected static IEnumerator RemoveComponentsOfTypeEnumerator<T>(GameObject target) where T : Component
		{
			if (target == null)
			{
				yield break;
			}
			foreach (T t in target.GetComponentsInChildren<T>(true))
			{
				if (!(t == null) && !(t.gameObject == null))
				{
					yield return AbstractPerformanceFilter.RemoveComponent(t);
				}
			}
			T[] array = null;
			yield break;
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0000EBAD File Offset: 0x0000CDAD
		protected static IEnumerator RemoveComponent(Component targetComponent)
		{
			yield return AbstractPerformanceFilter.RemoveDependencies(targetComponent);
            UnityEngine.Object.Destroy(targetComponent);
			yield return null;
			yield break;
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0000EBBC File Offset: 0x0000CDBC
		protected static IEnumerator RemoveDependencies(Component targetComponent)
		{
			if (targetComponent == null)
			{
				yield break;
			}
			Component[] components = targetComponent.GetComponents<Component>();
			if (components == null || components.Length == 0)
			{
				yield break;
			}
			Type componentType = targetComponent.GetType();
			foreach (Component component in components)
			{
				if (!(component == null))
				{
					bool flag = false;
					object[] customAttributes = component.GetType().GetCustomAttributes(typeof(RequireComponent), true);
					if (customAttributes.Length != 0)
					{
						object[] array2 = customAttributes;
						for (int j = 0; j < array2.Length; j++)
						{
							RequireComponent requireComponent = array2[j] as RequireComponent;
							if (requireComponent != null && (!(requireComponent.m_Type0 != componentType) || !(requireComponent.m_Type1 != componentType) || !(requireComponent.m_Type2 != componentType)))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							yield return AbstractPerformanceFilter.RemoveComponent(component);
						}
					}
				}
			}
			Component[] array = null;
			yield break;
		}
	}
}
