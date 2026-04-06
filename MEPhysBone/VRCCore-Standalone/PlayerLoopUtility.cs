using System;
using System.Collections.Generic;
using UnityEngine.LowLevel;
using VRC.Core.Pool;

namespace VRC
{
	// Token: 0x02000653 RID: 1619
	public static class PlayerLoopUtility
	{
		// Token: 0x0600395C RID: 14684 RVA: 0x0010D850 File Offset: 0x0010BA50
		public static void AddNewSystem<T>(Type loopPointType, Type loopEntryType, PlayerLoopSystem.UpdateFunction updateDelegate, bool addAfter)
		{
			PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
			try
			{
				PlayerLoopSystem[] subSystemList = currentPlayerLoop.subSystemList;
				int loopIndexByType = PlayerLoopUtility.GetLoopIndexByType(subSystemList, loopPointType);
				PlayerLoopSystem[] subSystemList2 = subSystemList[loopIndexByType].subSystemList;
				int loopIndexByType2 = PlayerLoopUtility.GetLoopIndexByType(subSystemList2, loopEntryType);
				List<PlayerLoopSystem> list;
				using (ListPool.Get<PlayerLoopSystem>(out list))
				{
					PlayerLoopSystem playerLoopSystem = default(PlayerLoopSystem);
					playerLoopSystem.type = typeof(T);
					playerLoopSystem.updateDelegate = updateDelegate;
					PlayerLoopSystem playerLoopSystem2 = playerLoopSystem;
					list.AddRange(subSystemList2);
					if (addAfter)
					{
						list.Insert(loopIndexByType2 + 1, playerLoopSystem2);
					}
					else
					{
						list.Insert(loopIndexByType2, playerLoopSystem2);
					}
					subSystemList[loopIndexByType].subSystemList = list.ToArray();
					currentPlayerLoop.subSystemList = subSystemList;
					PlayerLoop.SetPlayerLoop(currentPlayerLoop);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to add new system " + loopEntryType.Name + " because the inner exception was thrown.", ex);
			}
		}

		// Token: 0x0600395D RID: 14685 RVA: 0x0010D94C File Offset: 0x0010BB4C
		public static void RemoveExistingSystem(Type removedLoopPointType, Type removedLoopEntryType)
		{
			PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
			try
			{
				PlayerLoopSystem[] subSystemList = currentPlayerLoop.subSystemList;
				int loopIndexByType = PlayerLoopUtility.GetLoopIndexByType(subSystemList, removedLoopPointType);
				PlayerLoopSystem[] subSystemList2 = subSystemList[loopIndexByType].subSystemList;
				int loopIndexByType2 = PlayerLoopUtility.GetLoopIndexByType(subSystemList2, removedLoopEntryType);
				List<PlayerLoopSystem> list;
				using (ListPool.Get<PlayerLoopSystem>(out list))
				{
					list.AddRange(subSystemList2);
					list.RemoveAt(loopIndexByType2);
					subSystemList[loopIndexByType].subSystemList = list.ToArray();
					currentPlayerLoop.subSystemList = subSystemList;
					PlayerLoop.SetPlayerLoop(currentPlayerLoop);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to remove system " + removedLoopEntryType.Name + " because the inner exception was thrown.", ex);
			}
		}

		// Token: 0x0600395E RID: 14686 RVA: 0x0010DA0C File Offset: 0x0010BC0C
		public static void MoveExistingSystem(Type sourceLoopPointType, Type sourceLoopEntryType, Type destinationLoopPointType, Type destinationLoopEntryType, bool moveSourceToAfterDestination)
		{
			if (sourceLoopEntryType == destinationLoopEntryType)
			{
				return;
			}
			bool flag = sourceLoopPointType != destinationLoopPointType;
			PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoopSystem[] subSystemList = currentPlayerLoop.subSystemList;
			try
			{
				int loopIndexByType = PlayerLoopUtility.GetLoopIndexByType(subSystemList, sourceLoopPointType);
				PlayerLoopSystem[] array = subSystemList[loopIndexByType].subSystemList;
				int loopIndexByType2 = PlayerLoopUtility.GetLoopIndexByType(array, sourceLoopEntryType);
				int num;
				PlayerLoopSystem[] array2;
				if (!flag)
				{
					num = loopIndexByType;
					array2 = array;
				}
				else
				{
					num = PlayerLoopUtility.GetLoopIndexByType(subSystemList, destinationLoopPointType);
					array2 = subSystemList[num].subSystemList;
				}
				int num2 = PlayerLoopUtility.GetLoopIndexByType(array2, destinationLoopEntryType);
				if (!flag)
				{
					List<PlayerLoopSystem> list;
					using (ListPool.Get<PlayerLoopSystem>(out list))
					{
						list.AddRange(array2);
						PlayerLoopSystem playerLoopSystem = list[loopIndexByType2];
						list.RemoveAt(loopIndexByType2);
						if (loopIndexByType2 < num2)
						{
							num2--;
						}
						if (!moveSourceToAfterDestination)
						{
							list.Insert(num2, playerLoopSystem);
						}
						else
						{
							list.Insert(num2 + 1, playerLoopSystem);
						}
						array2 = list.ToArray();
						goto IL_164;
					}
				}
				List<PlayerLoopSystem> list2;
				using (ListPool.Get<PlayerLoopSystem>(out list2))
				{
					list2.AddRange(array);
					List<PlayerLoopSystem> list3;
					using (ListPool.Get<PlayerLoopSystem>(out list3))
					{
						list3.AddRange(array2);
						PlayerLoopSystem playerLoopSystem2 = list2[loopIndexByType2];
						list2.RemoveAt(loopIndexByType2);
						if (!moveSourceToAfterDestination)
						{
							list3.Insert(num2, playerLoopSystem2);
						}
						else
						{
							list3.Insert(num2 + 1, playerLoopSystem2);
						}
						array2 = list3.ToArray();
					}
					array = list2.ToArray();
				}
				IL_164:
				subSystemList[num].subSystemList = array2;
				if (flag)
				{
					subSystemList[loopIndexByType].subSystemList = array;
				}
				currentPlayerLoop.subSystemList = subSystemList;
				PlayerLoop.SetPlayerLoop(currentPlayerLoop);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Concat(new string[]
				{
					"Failed to move existing system ",
					sourceLoopEntryType.Name,
					" to ",
					moveSourceToAfterDestination ? "after" : "before",
					" ",
					destinationLoopEntryType.Name,
					" because the inner exception was thrown."
				}), ex);
			}
		}

		// Token: 0x0600395F RID: 14687 RVA: 0x0010DC6C File Offset: 0x0010BE6C
		private static int GetLoopIndexByType(PlayerLoopSystem[] systems, Type systemType)
		{
			int num = Array.FindIndex<PlayerLoopSystem>(systems, (PlayerLoopSystem system) => system.type == systemType);
			if (num < 0)
			{
				throw new ArgumentException("Failed to find system with type " + systemType.Name);
			}
			return num;
		}
	}
}
