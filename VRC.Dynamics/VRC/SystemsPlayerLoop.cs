using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using VRC.Dynamics;

namespace VRC
{
	// Token: 0x02000654 RID: 1620
	public static class SystemsPlayerLoop
	{
		// Token: 0x14000008 RID: 8
		// (add) Token: 0x06003960 RID: 14688 RVA: 0x0010DCB8 File Offset: 0x0010BEB8
		// (remove) Token: 0x06003961 RID: 14689 RVA: 0x0010DCEC File Offset: 0x0010BEEC
		public static event Action OnAvatarClone;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06003962 RID: 14690 RVA: 0x0010DD20 File Offset: 0x0010BF20
		// (remove) Token: 0x06003963 RID: 14691 RVA: 0x0010DD54 File Offset: 0x0010BF54
		public static event Action OnAvatarHeadChop;

		// Token: 0x06003964 RID: 14692 RVA: 0x0010DD88 File Offset: 0x0010BF88
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Initialize()
		{
			if (SystemsPlayerLoop._initialized)
			{
				return;
			}
			try
			{
				SystemsPlayerLoop.FixConstraints();
				SystemsPlayerLoop.SetupDynamicsLoops();
				SystemsPlayerLoop._initialized = true;
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to set up systems player loop because an exception was thrown! Components may behave in an undesirable way.");
				Debug.LogException(ex);
			}
		}

		// Token: 0x06003965 RID: 14693 RVA: 0x0010DDD4 File Offset: 0x0010BFD4
		private static void FixConstraints()
		{
			PlayerLoopUtility.MoveExistingSystem(typeof(PreLateUpdate), typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), typeof(PreLateUpdate), typeof(PreLateUpdate.ConstraintManagerUpdate), false);
		}

		// Token: 0x06003966 RID: 14694 RVA: 0x0010DE04 File Offset: 0x0010C004
		private static void SetupDynamicsLoops()
		{
			PlayerLoopUtility.AddNewSystem<SystemsPlayerLoop.VRCConstraintsUpdate>(typeof(PreLateUpdate), typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), new PlayerLoopSystem.UpdateFunction(SystemsPlayerLoop.OnVRCConstraintsUpdate), true);
			PlayerLoopUtility.AddNewSystem<SystemsPlayerLoop.VRCAvatarDynamicsPreSchedule>(typeof(PreLateUpdate), typeof(SystemsPlayerLoop.VRCConstraintsUpdate), new PlayerLoopSystem.UpdateFunction(SystemsPlayerLoop.OnAvatarDynamicsPreSchedule), true);
			PlayerLoopUtility.AddNewSystem<SystemsPlayerLoop.VRCAvatarDynamicsPostSchedule>(typeof(PreLateUpdate), typeof(PreLateUpdate.ConstraintManagerUpdate), new PlayerLoopSystem.UpdateFunction(SystemsPlayerLoop.OnAvatarDynamicsPostSchedule), true);
			PlayerLoopUtility.AddNewSystem<SystemsPlayerLoop.VRCAvatarDynamicsComplete>(typeof(PreLateUpdate), typeof(SystemsPlayerLoop.VRCAvatarDynamicsPostSchedule), new PlayerLoopSystem.UpdateFunction(SystemsPlayerLoop.OnAvatarDynamicsComplete), true);
		}

		// Token: 0x06003967 RID: 14695 RVA: 0x0010DEA9 File Offset: 0x0010C0A9
		private static void OnVRCConstraintsUpdate()
		{
			VRCAvatarDynamicsScheduler.UpdateConstraints(false);
		}

		// Token: 0x06003968 RID: 14696 RVA: 0x0010DEB1 File Offset: 0x0010C0B1
		private static void OnAvatarDynamicsPreSchedule()
		{
			VRCAvatarDynamicsScheduler.PreScheduleAvatarDynamics(false);
		}

		// Token: 0x06003969 RID: 14697 RVA: 0x0010DEB9 File Offset: 0x0010C0B9
		private static void OnAvatarDynamicsPostSchedule()
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			Action onAvatarClone = SystemsPlayerLoop.OnAvatarClone;
			if (onAvatarClone != null)
			{
				onAvatarClone.Invoke();
			}
			Action onAvatarHeadChop = SystemsPlayerLoop.OnAvatarHeadChop;
			if (onAvatarHeadChop != null)
			{
				onAvatarHeadChop.Invoke();
			}
			VRCAvatarDynamicsScheduler.PostScheduleAvatarDynamics(false);
		}

		// Token: 0x0600396A RID: 14698 RVA: 0x0010DEE6 File Offset: 0x0010C0E6
		private static void OnAvatarDynamicsComplete()
		{
			VRCAvatarDynamicsScheduler.CompleteDynamicsFrame();
		}

		// Token: 0x04001C61 RID: 7265
		private static bool _initialized;

		// Token: 0x02000826 RID: 2086
		private struct VRCConstraintsUpdate
		{
		}

		// Token: 0x02000827 RID: 2087
		private struct VRCAvatarDynamicsPreSchedule
		{
		}

		// Token: 0x02000828 RID: 2088
		private struct VRCAvatarDynamicsPostSchedule
		{
		}

		// Token: 0x02000829 RID: 2089
		private struct VRCAvatarDynamicsComplete
		{
		}
	}
}
