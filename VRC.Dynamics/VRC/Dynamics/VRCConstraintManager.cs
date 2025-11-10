using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Jobs;
using VRC.Dynamics.ManagedTypes;

namespace VRC.Dynamics
{
	// Token: 0x02000016 RID: 22
	public static class VRCConstraintManager
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x00006F0A File Offset: 0x0000510A
		public static bool IsInitialized
		{
			get
			{
				return VRCConstraintManager._instanceInitialized;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000D3 RID: 211 RVA: 0x00006F11 File Offset: 0x00005111
		// (set) Token: 0x060000D4 RID: 212 RVA: 0x00006F18 File Offset: 0x00005118
		public static bool CanExecuteConstraintJobsInEditMode
		{
			get
			{
				return VRCConstraintManager._canExecuteConstraintJobsInEditMode;
			}
			set
			{
				VRCConstraintManager._canExecuteConstraintJobsInEditMode = value;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000D5 RID: 213 RVA: 0x00006F20 File Offset: 0x00005120
		public static int ExecutionGroupCount
		{
			get
			{
				return VRCConstraintManager._constraintGrouper.ExecutionGroups.Count;
			}
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00006F34 File Offset: 0x00005134
		[RuntimeInitializeOnLoadMethod((RuntimeInitializeLoadType)1)]
		public static void Initialize()
		{
			if (VRCConstraintManager._instanceInitialized)
			{
				return;
			}
			VRCConstraintManager._constraintsManaged = new List<VRCConstraintBase>(256);
			VRCConstraintManager._constraintsManagedSet = new HashSet<VRCConstraintBase>(256);
			VRCConstraintManager._constraintsNative = new UnsafeList<VRCConstraintJobData>(256, (Unity.Collections.Allocator)4, 0);
			VRCConstraintManager._transformBuffer = new TransformAccessArray(256, -1);
			VRCConstraintManager._transformBufferOnlyTargets = new TransformAccessArray(256, -1);
			VRCConstraintManager._transformDataBuffer = new UnsafeList<TransformAccess>(256, (Unity.Collections.Allocator)4, 0);
			VRCConstraintManager._constraintTransformsBuffer = new List<ValueTuple<Transform, bool>>();
			VRCConstraintManager._emptyTransformRanges = new List<RangeInt>(8);
			VRCConstraintManager._constraintGrouper = new VRCConstraintGrouper();
			VRCConstraintManager._instanceInitialized = true;
			VRCConstraintManager._isEditor = Application.isEditor;
			Application.quitting += new Action(VRCConstraintManager.HandleQuit);
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00006FF3 File Offset: 0x000051F3
		private static void HandleQuit()
		{
			if (!VRCConstraintManager._isEditor)
			{
				VRCConstraintManager.UnInitialize();
			}
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00007004 File Offset: 0x00005204
		public static void UnInitialize()
		{
			if (!VRCConstraintManager._instanceInitialized)
			{
				return;
			}
			VRCAvatarDynamicsScheduler.FinalizeJob();
			if (VRCConstraintManager._constraintsNative.IsCreated)
			{
				for (int i = 0; i < VRCConstraintManager._constraintsNative.Length; i++)
				{
					VRCConstraintManager._constraintsNative[i].Sources.Dispose();
				}
			}
			VRCConstraintManager._constraintsNative.Dispose();
			VRCConstraintManager._transformBuffer.Dispose();
			VRCConstraintManager._transformBufferOnlyTargets.Dispose();
			VRCConstraintManager._transformDataBuffer.Dispose();
			VRCConstraintManager._constraintGrouper.Dispose();
			VRCConstraintManager._instanceInitialized = false;
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00007090 File Offset: 0x00005290
		public static void RegisterConstraint(VRCConstraintBase constraint)
		{
			if (!VRCConstraintManager._instanceInitialized || constraint == null || VRCConstraintManager.IsConstraintRegistered(constraint))
			{
				return;
			}
			constraint.EstablishTargetTransform();
			int transformCount = constraint.GetTransformCount(true);
			if (transformCount <= 0)
			{
				return;
			}
			int num = -1;
			int i = 0;
			while (i < VRCConstraintManager._emptyTransformRanges.Count)
			{
				if (VRCConstraintManager._emptyTransformRanges[i].length >= transformCount)
				{
					num = VRCConstraintManager._emptyTransformRanges[i].start;
					VRCConstraintManager._emptyTransformRanges[i] = new RangeInt(num + transformCount, VRCConstraintManager._emptyTransformRanges[i].length - transformCount);
					if (VRCConstraintManager._emptyTransformRanges[i].length == 0)
					{
						VRCConstraintManager._emptyTransformRanges.RemoveAt(i);
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			VRCConstraintManager._constraintTransformsBuffer.Clear();
			if (num == -1)
			{
				num = VRCConstraintManager._transformDataBuffer.Length;
				int num2 = num;
				constraint.GetTransforms(VRCConstraintManager._constraintTransformsBuffer);
				foreach (ValueTuple<Transform, bool> valueTuple in VRCConstraintManager._constraintTransformsBuffer)
				{
					Transform item = valueTuple.Item1;
					bool item2 = valueTuple.Item2;
					if (num2 >= VRCConstraintManager._transformBuffer.length)
					{
						VRCConstraintManager._transformBuffer.Add(item);
						VRCConstraintManager._transformBufferOnlyTargets.Add(item2 ? item : null);
					}
					else
					{
						VRCConstraintManager._transformBuffer[num2] = item;
						VRCConstraintManager._transformBufferOnlyTargets[num2] = (item2 ? item : null);
					}
					num2++;
				}
				if (VRCConstraintManager._transformBuffer.length > VRCConstraintManager._transformDataBuffer.Capacity)
				{
					VRCConstraintManager._transformDataBuffer.SetCapacity(math.ceilpow2(VRCConstraintManager._transformBuffer.length) * 2);
				}
				VRCConstraintManager._transformDataBuffer.Length = VRCConstraintManager._transformBuffer.length;
			}
			else
			{
				int num3 = num;
				constraint.GetTransforms(VRCConstraintManager._constraintTransformsBuffer);
				foreach (ValueTuple<Transform, bool> valueTuple2 in VRCConstraintManager._constraintTransformsBuffer)
				{
					Transform item3 = valueTuple2.Item1;
					bool item4 = valueTuple2.Item2;
					VRCConstraintManager._transformBuffer[num3] = item3;
					VRCConstraintManager._transformBufferOnlyTargets[num3] = (item4 ? item3 : null);
					num3++;
				}
			}
			VRCConstraintJobData vrcconstraintJobData = constraint.AllocateJobData(num);
			VRCConstraintManager._constraintsNative.Add(in vrcconstraintJobData);
			VRCConstraintManager._constraintsManaged.Add(constraint);
			VRCConstraintManager._constraintsManagedSet.Add(constraint);
			int nativeIndex = VRCConstraintManager._constraintsNative.Length - 1;
			constraint.NativeIndex = nativeIndex;
			VRCConstraintManager._constraintGrouper.RecordConstraintToAdd(constraint);
			constraint.ReEvaluatePhysBoneOrder();
			constraint.RequestFullNativeUpdate();
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00007338 File Offset: 0x00005538
		public static void UnregisterConstraint(VRCConstraintBase constraint)
		{
			if (!VRCConstraintManager._instanceInitialized || constraint == null || !VRCConstraintManager.IsConstraintRegistered(constraint))
			{
				return;
			}
			int transformCount = constraint.GetTransformCount(false);
			int nativeIndex = constraint.NativeIndex;
			VRCConstraintJobData vrcconstraintJobData = VRCConstraintManager._constraintsNative[nativeIndex];
			int transformStartIndex = vrcconstraintJobData.TransformStartIndex;
			vrcconstraintJobData.Sources.Dispose();
			VRCConstraintManager._constraintGrouper.RecordConstraintToRemove(constraint);
			VRCConstraintManager._constraintsNative.RemoveAtSwapBack(nativeIndex);
			VRCConstraintBase vrcconstraintBase = VRCConstraintManager._constraintsManaged[VRCConstraintManager._constraintsManaged.Count - 1];
			VRCConstraintManager._constraintsManaged[nativeIndex] = vrcconstraintBase;
			VRCConstraintManager._constraintsManaged.RemoveAt(VRCConstraintManager._constraintsManaged.Count - 1);
			if (constraint != vrcconstraintBase)
			{
				using (SortedDictionary<int, VRCConstraintGroup>.ValueCollection.Enumerator enumerator = VRCConstraintManager._constraintGrouper.ExecutionGroups.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.UpdateNativeIndex(vrcconstraintBase.NativeIndex, nativeIndex))
						{
							break;
						}
					}
				}
				vrcconstraintBase.NativeIndex = nativeIndex;
			}
			VRCConstraintManager._constraintsManagedSet.Remove(constraint);
			if (transformStartIndex != -1)
			{
				RangeInt rangeInt= new(transformStartIndex, transformCount);
				RangeInt rangeInt2 = rangeInt;
				bool flag = false;
				bool flag2 = false;
				for (int i = VRCConstraintManager._emptyTransformRanges.Count - 1; i >= 0; i--)
				{
					RangeInt rangeInt3 = VRCConstraintManager._emptyTransformRanges[i];
					if (rangeInt3.end == rangeInt.start)
					{
						rangeInt2.start = rangeInt3.start;
						rangeInt2.length += rangeInt3.length;
						VRCConstraintManager._emptyTransformRanges.RemoveAt(i);
						flag2 = true;
					}
					else if (rangeInt3.start == rangeInt.end)
					{
						rangeInt2.length += rangeInt3.length;
						VRCConstraintManager._emptyTransformRanges.RemoveAt(i);
						flag = true;
					}
					if (flag && flag2)
					{
						break;
					}
				}
				if (rangeInt2.start + rangeInt2.length >= VRCConstraintManager._transformDataBuffer.Length)
				{
					VRCConstraintManager._transformDataBuffer.Length = rangeInt2.start;
					for (int j = rangeInt2.end - 1; j >= rangeInt2.start; j--)
					{
						VRCConstraintManager._transformBuffer.RemoveAtSwapBack(j);
						VRCConstraintManager._transformBufferOnlyTargets.RemoveAtSwapBack(j);
					}
				}
				else
				{
					VRCConstraintManager._emptyTransformRanges.Add(rangeInt2);
					for (int k = rangeInt.start; k < rangeInt.end; k++)
					{
						VRCConstraintManager._transformBuffer[k] = null;
						VRCConstraintManager._transformBufferOnlyTargets[k] = null;
					}
				}
			}
			if (math.ceilpow2(VRCConstraintManager._constraintsNative.Capacity) > math.ceilpow2(VRCConstraintManager._constraintsNative.Length) * 2)
			{
				int num = math.max(math.ceilpow2(VRCConstraintManager._constraintsNative.Length) * 2, 256);
				if (num != VRCConstraintManager._constraintsNative.Capacity)
				{
					VRCConstraintManager._constraintsNative.SetCapacity(num);
				}
			}
			if (math.ceilpow2(VRCConstraintManager._transformDataBuffer.Capacity) > math.ceilpow2(VRCConstraintManager._transformDataBuffer.Length) * 2)
			{
				int num2 = math.max(math.ceilpow2(VRCConstraintManager._transformDataBuffer.Length) * 2, 256);
				if (num2 != VRCConstraintManager._transformDataBuffer.Capacity)
				{
					VRCConstraintManager._transformDataBuffer.SetCapacity(num2);
					TransformAccessArray transformBuffer = new(VRCConstraintManager._transformDataBuffer.Capacity, -1);
					TransformAccessArray transformBufferOnlyTargets = new(VRCConstraintManager._transformDataBuffer.Capacity, -1);
					for (int l = 0; l < VRCConstraintManager._transformDataBuffer.Length; l++)
					{
						transformBuffer.Add(VRCConstraintManager._transformBuffer[l]);
						transformBufferOnlyTargets.Add(VRCConstraintManager._transformBufferOnlyTargets[l]);
					}
					VRCConstraintManager._transformBuffer.Dispose();
					VRCConstraintManager._transformBufferOnlyTargets.Dispose();
					VRCConstraintManager._transformBuffer = transformBuffer;
					VRCConstraintManager._transformBufferOnlyTargets = transformBufferOnlyTargets;
				}
			}
		}

		// Token: 0x060000DB RID: 219 RVA: 0x00007700 File Offset: 0x00005900
		internal static void ReRegisterConstraint(VRCConstraintBase constraint)
		{
			if (VRCConstraintManager.IsConstraintRegistered(constraint))
			{
				VRCConstraintManager.UnregisterConstraint(constraint);
				VRCConstraintManager.RegisterConstraint(constraint);
			}
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00007716 File Offset: 0x00005916
		internal static void ReRegisterSameObjectConstraint(VRCConstraintBase constraint)
		{
			VRCConstraintManager._constraintGrouper.MarkRootStale(constraint);
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00007723 File Offset: 0x00005923
		internal static bool IsConstraintRegistered(VRCConstraintBase constraint)
		{
			return VRCConstraintManager._instanceInitialized && VRCConstraintManager._constraintsManagedSet.Contains(constraint);
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0000773C File Offset: 0x0000593C
		public static void Sdk_ManuallyRefreshGroups(VRCConstraintBase[] proxiedConstraints)
		{
			List<VRCConstraintBase> list = new List<VRCConstraintBase>(proxiedConstraints.Length);
			for (int i = proxiedConstraints.Length - 1; i >= 0; i--)
			{
				if (!VRCConstraintManager.IsConstraintRegistered(proxiedConstraints[i]))
				{
					VRCConstraintManager.RegisterConstraint(proxiedConstraints[i]);
					list.Add(proxiedConstraints[i]);
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			VRCConstraintGrouper constraintGrouper = VRCConstraintManager._constraintGrouper;
			IReadOnlyList<VRCConstraintBase> constraintsManaged = VRCConstraintManager._constraintsManaged;
			constraintGrouper.RefreshGroups(constraintsManaged);
			foreach (VRCConstraintBase constraint in list)
			{
				VRCConstraintManager.UnregisterConstraint(constraint);
			}
		}

		// Token: 0x060000DF RID: 223 RVA: 0x000077D8 File Offset: 0x000059D8
		internal static Transform GetBufferedTransform(VRCConstraintBase constraint, int index)
		{
			if (index < 0)
			{
				return null;
			}
			int finalTransformIndex = VRCConstraintManager.GetFinalTransformIndex(constraint, index);
			if (finalTransformIndex >= 0)
			{
				return VRCConstraintManager._transformBuffer[finalTransformIndex];
			}
			return null;
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x00007804 File Offset: 0x00005A04
		internal static void SetBufferedTransform(VRCConstraintBase constraint, int index, Transform assignedTransform)
		{
			if (index < 0)
			{
				Debug.LogError(string.Format("Received an index of {0} when attempting to assign a buffered constraint transform. The index must be zero or greater.", index));
				return;
			}
			int finalTransformIndex = VRCConstraintManager.GetFinalTransformIndex(constraint, index);
			if (finalTransformIndex >= 0 && VRCConstraintManager._transformBuffer[finalTransformIndex] != assignedTransform)
			{
				VRCConstraintManager._transformBuffer[finalTransformIndex] = assignedTransform;
			}
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00007858 File Offset: 0x00005A58
		private unsafe static int GetFinalTransformIndex(VRCConstraintBase constraint, int index)
		{
			int nativeIndex = constraint.NativeIndex;
			if (nativeIndex >= 0 && nativeIndex < VRCConstraintManager._constraintsNative.Length)
			{
				int transformStartIndex = VRCConstraintManager._constraintsNative.Ptr[nativeIndex].TransformStartIndex;
				int num = transformStartIndex + index;
				if (num >= 0 && num < VRCConstraintManager._transformBuffer.length)
				{
					return num;
				}
				Debug.LogError(string.Format("Final calculated index {0} is out of range of the transform buffer (length {1}). Transform index = {2}, sub-index = {3}", new object[]
				{
					num,
					VRCConstraintManager._transformBuffer.length,
					transformStartIndex,
					index
				}));
			}
			return -1;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x000078FC File Offset: 0x00005AFC
		public static void UpdateConstraints()
		{
			if (!VRCConstraintManager._instanceInitialized)
			{
				return;
			}
			VRCAvatarDynamicsScheduler.FinalizeJob();
			bool passiveUpdate = !VRCConstraintManager._isEditor;
			VRCConstraintManager.UpdatedConstraintsBuffer.Clear();
			VRCConstraintManager.UpdatedConstraintsBuffer.AddRange(VRCConstraintManager._constraintsManaged);
			foreach (VRCConstraintBase vrcconstraintBase in VRCConstraintManager.UpdatedConstraintsBuffer)
			{
				try
				{
					using (VRCConstraintManager._updateConstraintsProfilerMarkerSync.Auto())
					{
						vrcconstraintBase.SynchronizeWithBinding();
					}
					int nativeIndex = vrcconstraintBase.NativeIndex;
					VRCConstraintJobData jobData = VRCConstraintManager._constraintsNative.ElementAt(nativeIndex);
					using (VRCConstraintManager._updateConstraintsProfilerMarkerReAlloc.Auto())
					{
						vrcconstraintBase.CheckReallocation(jobData);
					}
					if (nativeIndex != vrcconstraintBase.NativeIndex)
					{
						jobData = VRCConstraintManager._constraintsNative.ElementAt(vrcconstraintBase.NativeIndex);
					}
					using (VRCConstraintManager._updateConstraintsProfilerMarkerJobData.Auto())
					{
						vrcconstraintBase.EstablishTargetTransform();
						vrcconstraintBase.EstablishPlayerLoopStage();
						vrcconstraintBase.UpdateJobData(ref jobData, passiveUpdate);
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					VRCConstraintManager.UnregisterConstraint(vrcconstraintBase);
				}
			}
			VRCConstraintGrouper constraintGrouper = VRCConstraintManager._constraintGrouper;
			IReadOnlyList<VRCConstraintBase> constraintsManaged = VRCConstraintManager._constraintsManaged;
			constraintGrouper.RefreshGroups(constraintsManaged);
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x00007A74 File Offset: 0x00005C74
		private static JobHandle ScheduleReadJob(JobHandle dependsOn = default(JobHandle))
		{
			return IJobParallelForTransformExtensions.Schedule<ReadTransformJob>(new ReadTransformJob
			{
				transformDataBuffer = VRCConstraintManager._transformDataBuffer
			}, VRCConstraintManager._transformBuffer, dependsOn);
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00007AA4 File Offset: 0x00005CA4
		internal static JobHandle ScheduleExecutionJobs(VRCConstraintPlayerLoopStage playerLoopStage, JobHandle dependsOn = default(JobHandle))
		{
			if (!VRCConstraintManager.CanExecuteConstraintJobsInEditMode && !Application.isPlaying)
			{
				return dependsOn;
			}
			SortedDictionary<int, VRCConstraintGroup> executionGroups = VRCConstraintManager._constraintGrouper.ExecutionGroups;
			if (executionGroups.Count > 0)
			{
				if (playerLoopStage == VRCConstraintPlayerLoopStage.PrePhysBone)
				{
					dependsOn = VRCConstraintManager.ScheduleReadJob(dependsOn);
				}
				foreach (VRCConstraintGroup vrcconstraintGroup in executionGroups.Values)
				{
					dependsOn = IJobParallelForExtensions.Schedule<VRCConstraintJob>(new VRCConstraintJob
					{
						playerLoopStage = playerLoopStage,
						targetConstraintIndices = vrcconstraintGroup.MemberConstraintIndices,
						constraints = VRCConstraintManager._constraintsNative,
						transformDataBuffer = VRCConstraintManager._transformDataBuffer
					}, vrcconstraintGroup.MemberCount, 32, dependsOn);
				}
			}
			return dependsOn;
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x00007B6C File Offset: 0x00005D6C
		[UsedImplicitly]
		public static void PostUpdateConstraints()
		{
			if (!VRCConstraintManager._instanceInitialized)
			{
				return;
			}
			foreach (VRCConstraintBase vrcconstraintBase in VRCConstraintManager._constraintsManaged)
			{
				vrcconstraintBase.PostUpdateJobData();
			}
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x00007BC4 File Offset: 0x00005DC4
		public static bool TryCreateSubstituteConstraint(IConstraint unityConstraint, out VRCConstraintBase substituteConstraint, VRCConstraintManager.IConstraintSubstituteCreator substituteCreator = null, bool keepBinding = true)
		{
			return VRCConstraintManager.TryCreateSubstituteConstraint<VRCPositionConstraintBase, VRCRotationConstraintBase, VRCScaleConstraintBase, VRCParentConstraintBase, VRCAimConstraintBase, VRCLookAtConstraintBase>(unityConstraint, out substituteConstraint, substituteCreator, keepBinding);
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x00007BD0 File Offset: 0x00005DD0
		public static bool TryCreateSubstituteConstraint<TPos, TRot, TSca, TPar, TAim, TLoo>(IConstraint unityConstraint, out VRCConstraintBase substituteConstraint, VRCConstraintManager.IConstraintSubstituteCreator substituteCreator = null, bool keepBinding = true) where TPos : VRCPositionConstraintBase where TRot : VRCRotationConstraintBase where TSca : VRCScaleConstraintBase where TPar : VRCParentConstraintBase where TAim : VRCAimConstraintBase where TLoo : VRCLookAtConstraintBase
		{
			if (unityConstraint == null)
			{
				Debug.LogError("Cannot auto-convert a null Unity constraint.");
				substituteConstraint = null;
				return false;
			}
			GameObject gameObject = ((Component)unityConstraint).gameObject;
			VRCConstraintBase vrcconstraintBase = null;
			IVRCConstraintBinding ivrcconstraintBinding = null;
			PositionConstraint positionConstraint = unityConstraint as PositionConstraint;
			if (positionConstraint == null)
			{
				RotationConstraint rotationConstraint = unityConstraint as RotationConstraint;
				if (rotationConstraint == null)
				{
					ScaleConstraint scaleConstraint = unityConstraint as ScaleConstraint;
					if (scaleConstraint == null)
					{
						ParentConstraint parentConstraint = unityConstraint as ParentConstraint;
						if (parentConstraint == null)
						{
							AimConstraint aimConstraint = unityConstraint as AimConstraint;
							if (aimConstraint == null)
							{
								LookAtConstraint lookAtConstraint = unityConstraint as LookAtConstraint;
								if (lookAtConstraint != null)
								{
									TLoo tloo;
									if ((tloo = ((substituteCreator != null) ? substituteCreator.CreateSubstituteComponent<TLoo>(gameObject) : default(TLoo))) == null)
									{
										tloo = gameObject.AddComponent<TLoo>();
									}
									TLoo tloo2 = tloo;
									ivrcconstraintBinding = new LookAtVRCConstraintBinding(lookAtConstraint, tloo2);
									vrcconstraintBase = tloo2;
								}
							}
							else
							{
								TAim taim;
								if ((taim = ((substituteCreator != null) ? substituteCreator.CreateSubstituteComponent<TAim>(gameObject) : default(TAim))) == null)
								{
									taim = gameObject.AddComponent<TAim>();
								}
								TAim taim2 = taim;
								ivrcconstraintBinding = new AimVRCConstraintBinding(aimConstraint, taim2);
								vrcconstraintBase = taim2;
							}
						}
						else
						{
							TPar tpar;
							if ((tpar = ((substituteCreator != null) ? substituteCreator.CreateSubstituteComponent<TPar>(gameObject) : default(TPar))) == null)
							{
								tpar = gameObject.AddComponent<TPar>();
							}
							TPar tpar2 = tpar;
							ivrcconstraintBinding = new ParentVRCConstraintBinding(parentConstraint, tpar2);
							vrcconstraintBase = tpar2;
						}
					}
					else
					{
						TSca tsca;
						if ((tsca = ((substituteCreator != null) ? substituteCreator.CreateSubstituteComponent<TSca>(gameObject) : default(TSca))) == null)
						{
							tsca = gameObject.AddComponent<TSca>();
						}
						TSca tsca2 = tsca;
						ivrcconstraintBinding = new ScaleVRCConstraintBinding(scaleConstraint, tsca2);
						vrcconstraintBase = tsca2;
					}
				}
				else
				{
					TRot trot;
					if ((trot = ((substituteCreator != null) ? substituteCreator.CreateSubstituteComponent<TRot>(gameObject) : default(TRot))) == null)
					{
						trot = gameObject.AddComponent<TRot>();
					}
					TRot trot2 = trot;
					ivrcconstraintBinding = new RotationVRCConstraintBinding(rotationConstraint, trot2);
					vrcconstraintBase = trot2;
				}
			}
			else
			{
				TPos tpos;
				if ((tpos = ((substituteCreator != null) ? substituteCreator.CreateSubstituteComponent<TPos>(gameObject) : default(TPos))) == null)
				{
					tpos = gameObject.AddComponent<TPos>();
				}
				TPos tpos2 = tpos;
				ivrcconstraintBinding = new PositionVRCConstraintBinding(positionConstraint, tpos2);
				vrcconstraintBase = tpos2;
			}
			if (ivrcconstraintBinding != null)
			{
				vrcconstraintBase.AssignBinding(ivrcconstraintBinding, keepBinding);
				substituteConstraint = vrcconstraintBase;
				return true;
			}
			Debug.LogError("Failed to auto-convert Unity constraint with type " + unityConstraint.GetType().Name + ". This might mean no equivalent VRChat type could be found.");
			substituteConstraint = null;
			return false;
		}

		// Token: 0x0400008F RID: 143
		private const int JobBatchCount = 32;

		// Token: 0x04000090 RID: 144
		private const int MinArrayCapacity = 256;

		// Token: 0x04000091 RID: 145
		private static bool _instanceInitialized = false;

		// Token: 0x04000092 RID: 146
		private static bool _canExecuteConstraintJobsInEditMode = true;

		// Token: 0x04000093 RID: 147
		private static bool _isEditor;

		// Token: 0x04000094 RID: 148
		private static List<VRCConstraintBase> _constraintsManaged;

		// Token: 0x04000095 RID: 149
		private static HashSet<VRCConstraintBase> _constraintsManagedSet;

		// Token: 0x04000096 RID: 150
		private static UnsafeList<VRCConstraintJobData> _constraintsNative;

		// Token: 0x04000097 RID: 151
		private static TransformAccessArray _transformBuffer;

		// Token: 0x04000098 RID: 152
		private static TransformAccessArray _transformBufferOnlyTargets;

		// Token: 0x04000099 RID: 153
		private static UnsafeList<TransformAccess> _transformDataBuffer;

		// Token: 0x0400009A RID: 154
		private static List<RangeInt> _emptyTransformRanges;

		// Token: 0x0400009B RID: 155
		private static List<(Transform constraintTransform, bool isTarget)> _constraintTransformsBuffer;

		// Token: 0x0400009C RID: 156
		private static ProfilerMarker _updateConstraintsProfilerMarkerSync = new ProfilerMarker("UpdateConstraints Sync");

		// Token: 0x0400009D RID: 157
		private static ProfilerMarker _updateConstraintsProfilerMarkerReAlloc = new ProfilerMarker("UpdateConstraints CheckReallocation");

		// Token: 0x0400009E RID: 158
		private static ProfilerMarker _updateConstraintsProfilerMarkerJobData = new ProfilerMarker("UpdateConstraints UpdateJobData");

		// Token: 0x0400009F RID: 159
		private static VRCConstraintGrouper _constraintGrouper;

		// Token: 0x040000A0 RID: 160
		private static readonly List<VRCConstraintBase> UpdatedConstraintsBuffer = new List<VRCConstraintBase>(64);

		// Token: 0x0200005A RID: 90
		public interface IConstraintSubstituteCreator
		{
			// Token: 0x060002B6 RID: 694
			T CreateSubstituteComponent<T>(GameObject hostGameObject) where T : VRCConstraintBase;
		}
	}
}
