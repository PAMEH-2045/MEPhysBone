using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using VRC.SDKBase.Validation.Performance.Stats;

namespace VRC.SDKBase.Validation.Performance.Scanners
{
	// Token: 0x02000075 RID: 117
	public sealed class MeshPerformanceScanner : AbstractPerformanceScanner
	{
		// Token: 0x060002FE RID: 766 RVA: 0x0000DC41 File Offset: 0x0000BE41
		public override IEnumerator RunPerformanceScanEnumerator(GameObject avatarObject, AvatarPerformanceStats perfStats, AvatarPerformance.IgnoreDelegate shouldIgnoreComponent)
		{
			List<Renderer> rendererBuffer = new List<Renderer>(16);
			yield return base.ScanAvatarForComponentsOfType<Renderer>(avatarObject, rendererBuffer);
			if (shouldIgnoreComponent != null)
			{
				rendererBuffer.RemoveAll((Renderer c) => shouldIgnoreComponent(c));
			}
			yield return this.AnalyzeGeometry(avatarObject, rendererBuffer, perfStats);
			MeshPerformanceScanner.AnalyzeMaterials(rendererBuffer, perfStats);
			MeshPerformanceScanner.AnalyzeMeshRenderers(rendererBuffer, perfStats);
			MeshPerformanceScanner.AnalyzeSkinnedMeshRenderers(rendererBuffer, perfStats);
			yield return null;
			yield break;
		}

		// Token: 0x060002FF RID: 767 RVA: 0x0000DC68 File Offset: 0x0000BE68
		private static uint? CalculateRendererPolyCount(Renderer renderer)
		{
			Mesh mesh = null;
			SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
			if (skinnedMeshRenderer != null)
			{
				mesh = skinnedMeshRenderer.sharedMesh;
			}
			if (mesh == null)
			{
				MeshRenderer meshRenderer = renderer as MeshRenderer;
				if (meshRenderer != null)
				{
					MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
					if (component != null)
					{
						mesh = component.sharedMesh;
					}
				}
			}
			if (mesh == null)
			{
				return new uint?(0U);
			}
			return MeshUtils.GetMeshTriangleCount(mesh);
		}

		// Token: 0x06000300 RID: 768 RVA: 0x0000DCD4 File Offset: 0x0000BED4
		private static bool RendererHasMesh(Renderer renderer)
		{
			MeshRenderer meshRenderer = renderer as MeshRenderer;
			if (meshRenderer != null)
			{
				MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
				return !(component == null) && component.sharedMesh != null;
			}
			SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
			return skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null;
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0000DD2E File Offset: 0x0000BF2E
		private IEnumerator AnalyzeGeometry(GameObject avatarObject, List<Renderer> renderers, AvatarPerformanceStats perfStats)
		{
			List<Renderer> lodGroupRendererIgnoreBuffer = new List<Renderer>(16);
			List<LODGroup> lodBuffer = new List<LODGroup>(16);
			ulong? polyCount = new ulong?(0UL);
			Bounds bounds = new Bounds(avatarObject.transform.position, Vector3.zero);
			yield return base.ScanAvatarForComponentsOfType<LODGroup>(avatarObject, lodBuffer);
			try
			{
				checked
				{
					foreach (LODGroup lodgroup in lodBuffer)
					{
						LOD[] lods = lodgroup.GetLODs();
						ulong? num = new ulong?(0UL);
						foreach (LOD lod in lods)
						{
							uint? num2 = new uint?(0U);
							foreach (Renderer renderer in lod.renderers)
							{
								lodGroupRendererIgnoreBuffer.Add(renderer);
								uint? num3 = MeshPerformanceScanner.CalculateRendererPolyCount(renderer);
								if (num3 == null)
								{
									num2 = default(uint?);
									break;
								}
								num2 += num3;
							}
							if (num2 == null)
							{
								num = default(ulong?);
								break;
							}
							uint? num4 = num2;
							unchecked
							{
								ulong? num5 = (num4 != null) ? new ulong?((ulong)num4.GetValueOrDefault()) : default(ulong?);
								ulong? num6 = num;
								if (num5.GetValueOrDefault() > num6.GetValueOrDefault() & (num5 != null & num6 != null))
								{
									num4 = num2;
									num = ((num4 != null) ? new ulong?((ulong)num4.GetValueOrDefault()) : default(ulong?));
								}
							}
						}
						if (num == null)
						{
							polyCount = default(ulong?);
						}
						else
						{
							polyCount += num;
						}
					}
				}
			}
			catch (OverflowException)
			{
				if (polyCount != null)
				{
					polyCount = ulong.MaxValue;
				}
			}
			foreach (Renderer renderer2 in renderers)
			{
				if (renderer2 is MeshRenderer || renderer2 is SkinnedMeshRenderer)
				{
					if (!MeshPerformanceScanner.RendererHasMesh(renderer2))
					{
						continue;
					}
					bounds.Encapsulate(renderer2.bounds);
				}
				if (polyCount != null && !lodGroupRendererIgnoreBuffer.Contains(renderer2))
				{
					uint? num7 = MeshPerformanceScanner.CalculateRendererPolyCount(renderer2);
					if (num7 == null)
					{
						polyCount = default(ulong?);
					}
					else
					{
						polyCount += (ulong)num7.Value;
					}
				}
			}
			bounds.center -= avatarObject.transform.position;
			lodGroupRendererIgnoreBuffer.Clear();
			lodBuffer.Clear();
			if (polyCount != null)
			{
				perfStats.polyCount = new int?((polyCount.Value > 2147483647UL) ? int.MaxValue : ((int)polyCount.Value));
			}
			else
			{
				perfStats.polyCount = default(int?);
			}
			perfStats.aabb = new Bounds?(bounds);
			yield break;
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0000DD54 File Offset: 0x0000BF54
		private static void AnalyzeMaterials(List<Renderer> renderers, AvatarPerformanceStats perfStats)
		{
			using (new ProfilerMarker("AnalyzeMaterials").Auto())
			{
				HashSet<Material> hashSet = new HashSet<Material>();
				List<Material> list = new List<Material>();
				foreach (Renderer renderer in renderers)
				{
					if (!(renderer == null))
					{
						renderer.GetSharedMaterials(list);
						hashSet.UnionWith(list);
					}
				}
				HashSet<Texture> hashSet2 = new HashSet<Texture>();
				List<int> list2 = new List<int>();
				foreach (Material material in hashSet)
				{
					if (!(material == null))
					{
						material.GetTexturePropertyNameIDs(list2);
						foreach (int num in list2)
						{
							Texture texture = material.GetTexture(num);
							if (!(texture == null) && !hashSet2.Contains(texture))
							{
								hashSet2.Add(texture);
							}
						}
					}
				}
				long num2 = 0L;
				foreach (Texture texture2 in hashSet2)
				{
					Texture2D texture2D = texture2 as Texture2D;
					if (texture2D == null)
					{
						if (!(texture2 is RenderTexture))
						{
							Cubemap cubemap = texture2 as Cubemap;
							if (cubemap == null)
							{
								Texture2DArray texture2DArray = texture2 as Texture2DArray;
								if (texture2DArray == null)
								{
									Texture3D texture3D = texture2 as Texture3D;
									if (texture3D != null)
									{
										TextureFormat format = texture3D.format;
										float num3;
										if (!MeshPerformanceScanner._texture2DBytesPerPixelLookup.TryGetValue(format, out num3))
										{
											num3 = 16f;
										}
										int width = texture3D.width;
										int height = texture3D.height;
										int depth = texture3D.depth;
										int num4 = width * height * depth;
										num2 += (long)Mathf.RoundToInt((float)num4 * num3);
									}
								}
								else
								{
									TextureFormat format2 = texture2DArray.format;
									float num5;
									if (!MeshPerformanceScanner._texture2DBytesPerPixelLookup.TryGetValue(format2, out num5))
									{
										num5 = 16f;
									}
									int width2 = texture2DArray.width;
									int height2 = texture2DArray.height;
									int depth2 = texture2DArray.depth;
									int num6 = width2 * height2 * depth2;
									num2 += (long)Mathf.RoundToInt((float)num6 * num5);
								}
							}
							else
							{
								TextureFormat format3 = cubemap.format;
								float num7;
								if (!MeshPerformanceScanner._texture2DBytesPerPixelLookup.TryGetValue(format3, out num7))
								{
									num7 = 16f;
								}
								int width3 = cubemap.width;
								int height3 = cubemap.height;
								int mipmapCount = cubemap.mipmapCount;
								int num8 = width3 * height3;
								for (int i = 0; i < mipmapCount; i++)
								{
									num2 += (long)Mathf.RoundToInt((float)(num8 >> 2 * i) * num7);
								}
							}
						}
					}
					else
					{
						TextureFormat format4 = texture2D.format;
						float num9;
						if (!MeshPerformanceScanner._texture2DBytesPerPixelLookup.TryGetValue(format4, out num9))
						{
							num9 = 16f;
						}
						int width4 = texture2D.width;
						int height4 = texture2D.height;
						int mipmapCount2 = texture2D.mipmapCount;
						int num10 = width4 * height4;
						for (int j = 0; j < mipmapCount2; j++)
						{
							num2 += (long)Mathf.RoundToInt((float)(num10 >> 2 * j) * num9);
						}
					}
				}
				perfStats.textureMegabytes = new float?((float)num2 / 1048576f);
			}
		}

		// Token: 0x06000303 RID: 771 RVA: 0x0000E120 File Offset: 0x0000C320
		private static void AnalyzeSkinnedMeshRenderers(List<Renderer> renderers, AvatarPerformanceStats perfStats)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			HashSet<Transform> hashSet = new HashSet<Transform>();
			foreach (Renderer renderer in renderers)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
				if (!(skinnedMeshRenderer == null))
				{
					num++;
					Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
					if (sharedMesh != null)
					{
						num2 += sharedMesh.subMeshCount;
					}
					foreach (Transform transform in skinnedMeshRenderer.bones)
					{
						if (!(transform == null) && !hashSet.Contains(transform))
						{
							hashSet.Add(transform);
							num3++;
						}
					}
				}
			}
			hashSet.Clear();
			perfStats.skinnedMeshCount = new int?(num);
			perfStats.boneCount = new int?(num3);
			perfStats.materialCount = new int?(perfStats.materialCount.GetValueOrDefault() + num2);
		}

		// Token: 0x06000304 RID: 772 RVA: 0x0000E224 File Offset: 0x0000C424
		private static void AnalyzeMeshRenderers(IEnumerable<Renderer> renderers, AvatarPerformanceStats perfStats)
		{
			int num = 0;
			int num2 = 0;
			foreach (Renderer renderer in renderers)
			{
				MeshRenderer meshRenderer = renderer as MeshRenderer;
				if (!(meshRenderer == null))
				{
					num++;
					MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
					if (!(component == null))
					{
						Mesh sharedMesh = component.sharedMesh;
						if (sharedMesh != null)
						{
							num2 += sharedMesh.subMeshCount;
						}
					}
				}
			}
			perfStats.meshCount = new int?(num);
			perfStats.materialCount = new int?(perfStats.materialCount.GetValueOrDefault() + num2);
		}

		// Token: 0x06000306 RID: 774 RVA: 0x0000E2D8 File Offset: 0x0000C4D8
		// Note: this type is marked as 'beforefieldinit'.
		static MeshPerformanceScanner()
		{
			Dictionary<TextureFormat, float> dictionary = new Dictionary<TextureFormat, float>();
			dictionary.Add((TextureFormat)1, 1f);
			dictionary.Add((TextureFormat)2, 2f);
			dictionary.Add((TextureFormat)3, 4f);
			dictionary.Add((TextureFormat)4, 4f);
			dictionary.Add((TextureFormat)5, 4f);
			dictionary.Add((TextureFormat)7, 2f);
			dictionary.Add((TextureFormat)9, 2f);
			dictionary.Add((TextureFormat)10, 0.5f);
			dictionary.Add((TextureFormat)12, 1f);
			dictionary.Add((TextureFormat)13, 2f);
			dictionary.Add((TextureFormat)14, 4f);
			dictionary.Add((TextureFormat)15, 2f);
			dictionary.Add((TextureFormat)16, 4f);
			dictionary.Add((TextureFormat)17, 8f);
			dictionary.Add((TextureFormat)18, 4f);
			dictionary.Add((TextureFormat)19, 8f);
			dictionary.Add((TextureFormat)20, 16f);
			dictionary.Add((TextureFormat)26, 0.5f);
			dictionary.Add((TextureFormat)27, 1f);
			dictionary.Add((TextureFormat)24, 1f);
			dictionary.Add((TextureFormat)25, 1f);
			dictionary.Add((TextureFormat)28, 0.5f);
			dictionary.Add((TextureFormat)29, 1f);
			dictionary.Add((TextureFormat)30, 0.25f);
			dictionary.Add((TextureFormat)31, 0.25f);
			dictionary.Add((TextureFormat)32, 0.5f);
			dictionary.Add((TextureFormat)33, 0.5f);
			dictionary.Add((TextureFormat)34, 0.5f);
			dictionary.Add((TextureFormat)41, 0.5f);
			dictionary.Add((TextureFormat)42, 0.5f);
			dictionary.Add((TextureFormat)43, 1f);
			dictionary.Add((TextureFormat)44, 1f);
			dictionary.Add((TextureFormat)45, 0.5f);
			dictionary.Add((TextureFormat)46, 0.5f);
			dictionary.Add((TextureFormat)47, 1f);
			dictionary.Add((TextureFormat)48, 1f);
			dictionary.Add((TextureFormat)49, 0.64f);
			dictionary.Add((TextureFormat)50, 0.445f);
			dictionary.Add((TextureFormat)51, 0.25f);
			dictionary.Add((TextureFormat)52, 0.16f);
			dictionary.Add((TextureFormat)53, 0.11125f);
			dictionary.Add((TextureFormat)62, 2f);
			dictionary.Add((TextureFormat)63, 1f);
			dictionary.Add((TextureFormat)64, 0.5f);
			dictionary.Add((TextureFormat)65, 1f);
			MeshPerformanceScanner._texture2DBytesPerPixelLookup = dictionary;
			Dictionary<RenderTextureFormat, float> dictionary2 = new Dictionary<RenderTextureFormat, float>();
			dictionary2.Add((RenderTextureFormat)1, 6f);
			dictionary2.Add((RenderTextureFormat)16, 1f);
			dictionary2.Add((RenderTextureFormat)28, 2f);
			dictionary2.Add((RenderTextureFormat)3, 6f);
			dictionary2.Add((RenderTextureFormat)14, 4f);
			dictionary2.Add((RenderTextureFormat)25, 2f);
			dictionary2.Add((RenderTextureFormat)23, 4f);
			dictionary2.Add((RenderTextureFormat)15, 2f);
			dictionary2.Add((RenderTextureFormat)19, 4f);
			dictionary2.Add((RenderTextureFormat)4, 2f);
			dictionary2.Add((RenderTextureFormat)12, 8f);
			dictionary2.Add((RenderTextureFormat)13, 4f);
			dictionary2.Add((RenderTextureFormat)18, 8f);
			dictionary2.Add((RenderTextureFormat)0, 4f);
			dictionary2.Add((RenderTextureFormat)10, 8f);
			dictionary2.Add((RenderTextureFormat)6, 2f);
			dictionary2.Add((RenderTextureFormat)5, 2f);
			dictionary2.Add((RenderTextureFormat)8, 2f);
			dictionary2.Add((RenderTextureFormat)20, 4f);
			dictionary2.Add((RenderTextureFormat)22, 4f);
			dictionary2.Add((RenderTextureFormat)11, 128f);
			dictionary2.Add((RenderTextureFormat)2, 64f);
			dictionary2.Add((RenderTextureFormat)17, 128f);
			dictionary2.Add((RenderTextureFormat)27, 4f);
			dictionary2.Add((RenderTextureFormat)26, 4f);
			dictionary2.Add((RenderTextureFormat)24, 8f);
			MeshPerformanceScanner._renderTextureBytesPerPixelLookup = dictionary2;
		}

		// Token: 0x0400037A RID: 890
		private const float TEXTURE_2D_MAXIMUM_BYTES_PER_PIXEL = 16f;

		// Token: 0x0400037B RID: 891
		private static readonly Dictionary<TextureFormat, float> _texture2DBytesPerPixelLookup;

		// Token: 0x0400037C RID: 892
		private const float RENDER_TEXTURE_2D_MAXIMUM_BYTES_PER_PIXEL = 16f;

		// Token: 0x0400037D RID: 893
		private static readonly Dictionary<RenderTextureFormat, float> _renderTextureBytesPerPixelLookup;
	}
}
