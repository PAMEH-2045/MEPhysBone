using System;
using JetBrains.Annotations;
using Unity.Jobs;
using UnityEngine;
using VRC.Core.Burst;

namespace VRC.Dynamics
{
	// Token: 0x02000007 RID: 7
	public static class VRCAvatarDynamicsScheduler
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000041 RID: 65 RVA: 0x000035F4 File Offset: 0x000017F4
		// (remove) Token: 0x06000042 RID: 66 RVA: 0x00003628 File Offset: 0x00001828
		public static event Action OnFrameComplete;

		// Token: 0x06000043 RID: 67 RVA: 0x0000365B File Offset: 0x0000185B
		[RuntimeInitializeOnLoadMethod((RuntimeInitializeLoadType)1)]
		private static void Initialize()
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(VRCAvatarDynamicsScheduler.OnCameraPreCull));
		}

		// Token: 0x06000044 RID: 68 RVA: 0x0000367D File Offset: 0x0000187D
		[UsedImplicitly]
		public static void UpdateConstraints(bool finalizeImmediately = false)
		{
			VRCConstraintManager.UpdateConstraints();
			if (finalizeImmediately)
			{
				VRCAvatarDynamicsScheduler.PreScheduleAvatarDynamics(true);
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00003690 File Offset: 0x00001890
		[UsedImplicitly]
		public static void PreScheduleAvatarDynamics(bool finalizeImmediately = false)
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			JobHandle jobHandle = VRCConstraintManager.ScheduleExecutionJobs(VRCConstraintPlayerLoopStage.PrePhysBone, default(JobHandle));
			if (PhysBoneManager.Inst != null)
			{
				jobHandle = PhysBoneManager.Inst.ScheduleExecutionJob(jobHandle);
			}
			jobHandle = VRCConstraintManager.ScheduleExecutionJobs(VRCConstraintPlayerLoopStage.PostPhysBone, jobHandle);
			if (ContactManager.Inst != null)
			{
				jobHandle = ContactManager.Inst.ScheduleUpdateReceiversJob(jobHandle);
			}
			VRCAvatarDynamicsScheduler._currentDynamicsJobHandle = new DisposableJobHandle(jobHandle);
			JobHandle.ScheduleBatchedJobs();
			if (finalizeImmediately)
			{
				VRCAvatarDynamicsScheduler.PostScheduleAvatarDynamics(true);
			}
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003708 File Offset: 0x00001908
		[UsedImplicitly]
		public static void PostScheduleAvatarDynamics(bool finalizeImmediately = false)
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			JobHandle jobHandle = VRCConstraintManager.ScheduleExecutionJobs(VRCConstraintPlayerLoopStage.PostLocalAvatarProcess, VRCAvatarDynamicsScheduler._currentDynamicsJobHandle);
			VRCAvatarDynamicsScheduler._currentDynamicsJobHandle = new DisposableJobHandle(jobHandle);
			JobHandle.ScheduleBatchedJobs();
			if (finalizeImmediately)
			{
				VRCAvatarDynamicsScheduler.CompleteDynamicsFrame();
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00003743 File Offset: 0x00001943
		[UsedImplicitly]
		public static void CompleteDynamicsFrame()
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
			VRCConstraintManager.PostUpdateConstraints();
			VRCAvatarDynamicsScheduler.SignalFrameComplete();
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003754 File Offset: 0x00001954
		private static void OnCameraPreCull(Camera cam)
		{
			VRCAvatarDynamicsScheduler.FinalizeJob();
		}

		// Token: 0x06000049 RID: 73 RVA: 0x0000375B File Offset: 0x0000195B
		public static void FinalizeJob()
		{
			VRCAvatarDynamicsScheduler._currentDynamicsJobHandle.Complete();
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00003768 File Offset: 0x00001968
		private static void SignalFrameComplete()
		{
			int frameCount = Time.frameCount;
			if (VRCAvatarDynamicsScheduler._latestCompletedFrameNumber < frameCount)
			{
				VRCAvatarDynamicsScheduler.FinalizeJob();
				VRCAvatarDynamicsScheduler._latestCompletedFrameNumber = frameCount;
				Action onFrameComplete = VRCAvatarDynamicsScheduler.OnFrameComplete;
				if (onFrameComplete == null)
				{
					return;
				}
				onFrameComplete.Invoke();
			}
		}

		// Token: 0x0600004B RID: 75 RVA: 0x0000379D File Offset: 0x0000199D
		[UsedImplicitly]
		public static void HandleEditorPlayModeToggle()
		{
			VRCAvatarDynamicsScheduler._latestCompletedFrameNumber = -1;
		}

		// Token: 0x0400002D RID: 45
		private static DisposableJobHandle _currentDynamicsJobHandle = default(DisposableJobHandle);

		// Token: 0x0400002F RID: 47
		private static int _latestCompletedFrameNumber = -1;
	}
}
