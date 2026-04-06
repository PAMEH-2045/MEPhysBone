using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using VRC.Dynamics;

namespace VRC
{
    // Token: 0x02000657 RID: 1623
    public static class SystemsPlayerLoop
    {
        // Token: 0x14000008 RID: 8
        // (add) Token: 0x06003962 RID: 14690 RVA: 0x0010DCB0 File Offset: 0x0010BEB0
        // (remove) Token: 0x06003963 RID: 14691 RVA: 0x0010DCE4 File Offset: 0x0010BEE4
        public static event Action OnAvatarClone;

        // Token: 0x14000009 RID: 9
        // (add) Token: 0x06003964 RID: 14692 RVA: 0x0010DD18 File Offset: 0x0010BF18
        // (remove) Token: 0x06003965 RID: 14693 RVA: 0x0010DD4C File Offset: 0x0010BF4C
        public static event Action OnAvatarHeadChop;

        // Token: 0x06003966 RID: 14694 RVA: 0x0010DD80 File Offset: 0x0010BF80
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

        // Token: 0x06003967 RID: 14695 RVA: 0x0010DDCC File Offset: 0x0010BFCC
        private static void FixConstraints()
        {
            PlayerLoopUtility.MoveExistingSystem(typeof(PreLateUpdate), typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), typeof(PreLateUpdate), typeof(PreLateUpdate.ConstraintManagerUpdate), false);
        }

        // Token: 0x06003968 RID: 14696 RVA: 0x0010DDFC File Offset: 0x0010BFFC
        private static void SetupDynamicsLoops()
        {
            PlayerLoopUtility.AddNewSystem<SystemsPlayerLoop.VRCConstraintsUpdate>(typeof(PreLateUpdate), typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), new PlayerLoopSystem.UpdateFunction(SystemsPlayerLoop.OnVRCConstraintsUpdate), true);
            PlayerLoopUtility.AddNewSystem<SystemsPlayerLoop.VRCAvatarDynamicsPreSchedule>(typeof(PreLateUpdate), typeof(SystemsPlayerLoop.VRCConstraintsUpdate), new PlayerLoopSystem.UpdateFunction(SystemsPlayerLoop.OnAvatarDynamicsPreSchedule), true);
            PlayerLoopUtility.AddNewSystem<SystemsPlayerLoop.VRCAvatarDynamicsPostSchedule>(typeof(PreLateUpdate), typeof(PreLateUpdate.ConstraintManagerUpdate), new PlayerLoopSystem.UpdateFunction(SystemsPlayerLoop.OnAvatarDynamicsPostSchedule), true);
            PlayerLoopUtility.AddNewSystem<SystemsPlayerLoop.VRCAvatarDynamicsComplete>(typeof(PreLateUpdate), typeof(SystemsPlayerLoop.VRCAvatarDynamicsPostSchedule), new PlayerLoopSystem.UpdateFunction(SystemsPlayerLoop.OnAvatarDynamicsComplete), true);
        }

        // Token: 0x06003969 RID: 14697 RVA: 0x0010DEA1 File Offset: 0x0010C0A1
        private static void OnVRCConstraintsUpdate()
        {
            VRCDynamicsScheduler.UpdateConstraints(false);
        }

        // Token: 0x0600396A RID: 14698 RVA: 0x0010DEA9 File Offset: 0x0010C0A9
        private static void OnAvatarDynamicsPreSchedule()
        {
            VRCDynamicsScheduler.PreScheduleDynamics(false);
        }

        // Token: 0x0600396B RID: 14699 RVA: 0x0010DEB1 File Offset: 0x0010C0B1
        private static void OnAvatarDynamicsPostSchedule()
        {
            VRCDynamicsScheduler.FinalizeJob();
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
            VRCDynamicsScheduler.PostScheduleDynamics(false);
        }

        // Token: 0x0600396C RID: 14700 RVA: 0x0010DEDE File Offset: 0x0010C0DE
        private static void OnAvatarDynamicsComplete()
        {
            VRCDynamicsScheduler.CompleteDynamicsFrame();
        }

        // Token: 0x04001C62 RID: 7266
        private static bool _initialized;

        // Token: 0x02000845 RID: 2117
        private struct VRCConstraintsUpdate
        {
        }

        // Token: 0x02000846 RID: 2118
        private struct VRCAvatarDynamicsPreSchedule
        {
        }

        // Token: 0x02000847 RID: 2119
        private struct VRCAvatarDynamicsPostSchedule
        {
        }

        // Token: 0x02000848 RID: 2120
        private struct VRCAvatarDynamicsComplete
        {
        }
    }
}