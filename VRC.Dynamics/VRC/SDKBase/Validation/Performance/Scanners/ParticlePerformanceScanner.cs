using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x02000076 RID: 118
	public sealed class ParticlePerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x06000307 RID: 775 RVA: 0x0000E686 File Offset: 0x0000C886
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			List<ParticleSystem> particleSystemBuffer = new List<ParticleSystem>();
			yield return base.ScanAvatarForComponentsOfType<ParticleSystem>(avatarObject, particleSystemBuffer);
			if (shouldIgnoreComponent != null)
			{
				particleSystemBuffer.RemoveAll((ParticleSystem c) => shouldIgnoreComponent(c));
			}
			ParticlePerformanceScanner.AnalyzeParticleSystemRenderers(particleSystemBuffer, perfStats);
			yield return null;
			yield break;
		}

		// Token: 0x06000308 RID: 776 RVA: 0x0000E6AC File Offset: 0x0000C8AC
		private static void AnalyzeParticleSystemRenderers(IEnumerable<ParticleSystem> particleSystems, AvatarPerformanceStats perfStats)
		{
			int num = 0;
			ulong num2 = 0UL;
			ulong? num3 = new ulong?(0UL);
			bool flag = false;
			bool flag2 = false;
			int num4 = 0;
			foreach (ParticleSystem particleSystem in particleSystems)
			{
				int num5 = particleSystem.main.maxParticles;
				if (particleSystem.main.ringBufferMode == ParticleSystemRingBufferMode.LoopUntilReplaced)
				{
					if (particleSystem.emission.rateOverTime.curve == null)
					{
						num5 += Mathf.CeilToInt(particleSystem.emission.rateOverTime.constant);
					}
					else
					{
						float num6 = 0f;
						foreach (Keyframe keyframe in particleSystem.emission.rateOverTime.curve.keys)
						{
							if (keyframe.value > num6)
							{
								num6 = keyframe.value;
							}
						}
						num5 += Mathf.CeilToInt(num6 * particleSystem.emission.rateOverTimeMultiplier);
					}
					if (particleSystem.emission.rateOverDistance.curve == null)
					{
						num5 += Mathf.CeilToInt(particleSystem.emission.rateOverDistance.constantMax);
					}
					else
					{
						float num7 = 0f;
						foreach (Keyframe keyframe2 in particleSystem.emission.rateOverDistance.curve.keys)
						{
							if (keyframe2.value > num7)
							{
								num7 = keyframe2.value;
							}
						}
						num5 += Mathf.CeilToInt(num7 * particleSystem.emission.rateOverDistanceMultiplier);
					}
					for (int j = 0; j < particleSystem.emission.burstCount; j++)
					{
						ParticleSystem.Burst burst = particleSystem.emission.GetBurst(j);
						num5 += Mathf.CeilToInt((float)((int)burst.maxCount * burst.cycleCount));
					}
				}
				else if (num5 <= 0)
				{
					continue;
				}
				num++;
				num2 += (ulong)num5;
				ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
				if (!(component == null))
				{
					num4++;
					if (num3 != null && component.renderMode == ParticleSystemRenderMode.Mesh && component.meshCount > 0)
					{
						uint? num8 = new uint?(0U);
						Mesh[] array = new Mesh[component.meshCount];
						int meshes = component.GetMeshes(array);
						for (int k = 0; k < meshes; k++)
						{
							Mesh mesh = array[k];
							if (!(mesh == null))
							{
								uint? meshTriangleCount = MeshUtils.GetMeshTriangleCount(mesh);
								if (meshTriangleCount == null)
								{
									num8 = default(uint?);
									break;
								}
								uint? num9 = meshTriangleCount;
								uint? num10 = num8;
								if (num9.GetValueOrDefault() > num10.GetValueOrDefault() & (num9 != null & num10 != null))
								{
									num8 = meshTriangleCount;
								}
							}
						}
						if (num8 != null)
						{
							ulong num11 = (ulong)(num5 * (int)num8.Value);
							num3 += num11;
						}
						else
						{
							num3 = default(ulong?);
						}
					}
					if (particleSystem.trails.enabled)
					{
						flag = true;
						num4++;
					}
					if (particleSystem.collision.enabled)
					{
						flag2 = true;
					}
				}
			}
			perfStats.particleSystemCount = new int?(num);
			perfStats.particleTotalCount = new int?((num2 > 2147483647UL) ? int.MaxValue : ((int)num2));
			if (num3 != null)
			{
				ulong? num12 = num3;
				ulong num13 = 2147483647UL;
				perfStats.particleMaxMeshPolyCount = new int?((num12.GetValueOrDefault() > num13 & num12 != null) ? int.MaxValue : ((int)num3.Value));
			}
			else
			{
				perfStats.particleMaxMeshPolyCount = default(int?);
			}
			perfStats.particleTrailsEnabled = new bool?(flag);
			perfStats.particleCollisionEnabled = new bool?(flag2);
			perfStats.materialCount = new int?(perfStats.materialCount.GetValueOrDefault() + num4);
		}
	}
}
