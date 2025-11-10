using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace VRC.SDKBase.Validation.Performance.Stats
{
	// Token: 0x0200006C RID: 108
	[PublicAPI]
	public class AvatarPerformanceStats
	{
		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060002D3 RID: 723 RVA: 0x0000B838 File Offset: 0x00009A38
		[Obsolete("Use downloadSizeBytes instead")]
		public float? downloadSize
		{
			get
			{
				int? num = this.downloadSizeBytes;
				if (num == null)
				{
					return default(float?);
				}
				return new float?((float)num.GetValueOrDefault() / 1048576f);
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060002D4 RID: 724 RVA: 0x0000B874 File Offset: 0x00009A74
		[Obsolete("Use uncompressedSizeBytes instead")]
		public float? uncompressedSize
		{
			get
			{
				int? num = this.uncompressedSizeBytes;
				if (num == null)
				{
					return default(float?);
				}
				return new float?((float)num.GetValueOrDefault() / 1048576f);
			}
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000B8B0 File Offset: 0x00009AB0
		public void BuildAvatarStatAnalyticsList()
		{
			this._analyticsStatsValid = false;
			this._analyticsStatValues.Clear();
			if (this.polyCount != null && this.polyCount != null)
			{
				this._analyticsStatValues.Add(this.polyCount.Value);
			}
			if (this.skinnedMeshCount != null && this.skinnedMeshCount != null)
			{
				this._analyticsStatValues.Add(this.skinnedMeshCount.Value);
			}
			if (this.meshCount != null && this.meshCount != null)
			{
				this._analyticsStatValues.Add(this.meshCount.Value);
			}
			if (this.materialCount != null && this.materialCount != null)
			{
				this._analyticsStatValues.Add(this.materialCount.Value);
			}
			if (this.animatorCount != null && this.animatorCount != null)
			{
				this._analyticsStatValues.Add(this.animatorCount.Value);
			}
			if (this.boneCount != null && this.boneCount != null)
			{
				this._analyticsStatValues.Add(this.boneCount.Value);
			}
			if (this.lightCount != null && this.lightCount != null)
			{
				this._analyticsStatValues.Add(this.lightCount.Value);
			}
			if (this.particleSystemCount != null && this.particleSystemCount != null)
			{
				this._analyticsStatValues.Add(this.particleSystemCount.Value);
			}
			if (this.particleTotalCount != null && this.particleTotalCount != null)
			{
				this._analyticsStatValues.Add(this.particleTotalCount.Value);
			}
			if (this.particleMaxMeshPolyCount != null && this.particleMaxMeshPolyCount != null)
			{
				this._analyticsStatValues.Add(this.particleMaxMeshPolyCount.Value);
			}
			if (this.trailRendererCount != null && this.trailRendererCount != null)
			{
				this._analyticsStatValues.Add(this.trailRendererCount.Value);
			}
			if (this.lineRendererCount != null && this.lineRendererCount != null)
			{
				this._analyticsStatValues.Add(this.lineRendererCount.Value);
			}
			if (this.clothCount != null && this.clothCount != null)
			{
				this._analyticsStatValues.Add(this.clothCount.Value);
			}
			if (this.clothMaxVertices != null && this.clothMaxVertices != null)
			{
				this._analyticsStatValues.Add(this.clothMaxVertices.Value);
			}
			if (this.physicsColliderCount != null && this.physicsColliderCount != null)
			{
				this._analyticsStatValues.Add(this.physicsColliderCount.Value);
			}
			if (this.physicsRigidbodyCount != null && this.physicsRigidbodyCount != null)
			{
				this._analyticsStatValues.Add(this.physicsRigidbodyCount.Value);
			}
			if (this.audioSourceCount != null && this.audioSourceCount != null)
			{
				this._analyticsStatValues.Add(this.audioSourceCount.Value);
			}
			if (this.contactCount != null && this.contactCount != null)
			{
				this._analyticsStatValues.Add(this.contactCount.Value);
			}
			if (this.constraintDepth != null && this.constraintDepth != null)
			{
				this._analyticsStatValues.Add(this.constraintDepth.Value);
			}
			if (this.constraintsCount != null && this.constraintsCount != null)
			{
				this._analyticsStatValues.Add(this.constraintsCount.Value);
			}
			if (this.physBone != null && this.physBone != null)
			{
				this._analyticsStatValues.Add(this.physBone.Value.componentCount);
				this._analyticsStatValues.Add(this.physBone.Value.transformCount);
				this._analyticsStatValues.Add(this.physBone.Value.colliderCount);
				this._analyticsStatValues.Add(this.physBone.Value.collisionCheckCount);
			}
			this._analyticsStatsValid = (this._analyticsStatValues.Count == AvatarPerformanceStats._analyticsStatNames.Count);
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000BD32 File Offset: 0x00009F32
		public int GetAvatarAnalyticsStatValue(int index)
		{
			return this._analyticsStatValues[index];
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x0000BD40 File Offset: 0x00009F40
		public static string GetAvatarAnalyticsStatName(int index)
		{
			return AvatarPerformanceStats._analyticsStatNames[index];
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060002D8 RID: 728 RVA: 0x0000BD4D File Offset: 0x00009F4D
		public bool AnalyticsStatsValid
		{
			get
			{
				return this._analyticsStatsValid;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060002D9 RID: 729 RVA: 0x0000BD55 File Offset: 0x00009F55
		public static int AnalyticsStatNamesCount
		{
			get
			{
				return AvatarPerformanceStats._analyticsStatNames.Count;
			}
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0000BD61 File Offset: 0x00009F61
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Initialize()
		{
			AvatarPerformanceStats._performanceStatsLevelSet_Windows = Resources.Load<AvatarPerformanceStatsLevelSet>("Validation/Performance/StatsLevels/Windows/AvatarPerformanceStatLevels_Windows");
			AvatarPerformanceStats._performanceStatsLevelSet_Mobile = Resources.Load<AvatarPerformanceStatsLevelSet>("Validation/Performance/StatsLevels/Quest/AvatarPerformanceStatLevels_Quest");
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0000BD81 File Offset: 0x00009F81
		private static AvatarPerformanceStatsLevelSet GetAvatarPerformanceStatsLevelSet(bool isMobilePlatform)
		{
			if (isMobilePlatform)
			{
				return AvatarPerformanceStats._performanceStatsLevelSet_Mobile;
			}
			return AvatarPerformanceStats._performanceStatsLevelSet_Windows;
		}

		// Token: 0x060002DC RID: 732 RVA: 0x0000BD91 File Offset: 0x00009F91
		[PublicAPI]
		public AvatarPerformanceStats(bool isMobilePlatform)
		{
			this._performanceStatsLevelSet = AvatarPerformanceStats.GetAvatarPerformanceStatsLevelSet(isMobilePlatform);
			this._performanceRatingCache = new PerformanceRating[32];
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0000BDC8 File Offset: 0x00009FC8
		[PublicAPI]
		public void Reset()
		{
			this.avatarName = null;
			this.polyCount = default(int?);
			this.aabb = default(Bounds?);
			this.skinnedMeshCount = default(int?);
			this.meshCount = default(int?);
			this.materialCount = default(int?);
			this.animatorCount = default(int?);
			this.boneCount = default(int?);
			this.lightCount = default(int?);
			this.particleSystemCount = default(int?);
			this.particleTotalCount = default(int?);
			this.particleMaxMeshPolyCount = default(int?);
			this.particleTrailsEnabled = default(bool?);
			this.particleCollisionEnabled = default(bool?);
			this.trailRendererCount = default(int?);
			this.lineRendererCount = default(int?);
			this.clothCount = default(int?);
			this.clothMaxVertices = default(int?);
			this.physicsColliderCount = default(int?);
			this.physicsRigidbodyCount = default(int?);
			this.audioSourceCount = default(int?);
			this.downloadSizeBytes = default(int?);
			this.uncompressedSizeBytes = default(int?);
			this.textureMegabytes = default(float?);
			this.physBone = default(AvatarPerformanceStats.PhysBoneStats?);
			this.contactCount = default(int?);
			this.contactCompleteCount = default(int?);
			this.constraintsCount = default(int?);
			this.constraintDepth = default(int?);
			for (int i = 0; i < 32; i++)
			{
				this._performanceRatingCache[i] = PerformanceRating.None;
			}
			this._performanceStatsLevelSet = null;
		}

		// Token: 0x060002DE RID: 734 RVA: 0x0000BF4C File Offset: 0x0000A14C
		public void CopyTo(AvatarPerformanceStats to)
		{
			if (this.avatarName != null)
			{
				to.avatarName = this.avatarName;
			}
			if (this.polyCount != null)
			{
				to.polyCount = this.polyCount;
			}
			if (this.aabb != null)
			{
				to.aabb = this.aabb;
			}
			if (this.skinnedMeshCount != null)
			{
				to.skinnedMeshCount = this.skinnedMeshCount;
			}
			if (this.meshCount != null)
			{
				to.meshCount = this.meshCount;
			}
			if (this.materialCount != null)
			{
				to.materialCount = this.materialCount;
			}
			if (this.animatorCount != null)
			{
				to.animatorCount = this.animatorCount;
			}
			if (this.boneCount != null)
			{
				to.boneCount = this.boneCount;
			}
			if (this.lightCount != null)
			{
				to.lightCount = this.lightCount;
			}
			if (this.particleSystemCount != null)
			{
				to.particleSystemCount = this.particleSystemCount;
			}
			if (this.particleTotalCount != null)
			{
				to.particleTotalCount = this.particleTotalCount;
			}
			if (this.particleMaxMeshPolyCount != null)
			{
				to.particleMaxMeshPolyCount = this.particleMaxMeshPolyCount;
			}
			if (this.particleTrailsEnabled != null)
			{
				to.particleTrailsEnabled = this.particleTrailsEnabled;
			}
			if (this.particleCollisionEnabled != null)
			{
				to.particleCollisionEnabled = this.particleCollisionEnabled;
			}
			if (this.trailRendererCount != null)
			{
				to.trailRendererCount = this.trailRendererCount;
			}
			if (this.lineRendererCount != null)
			{
				to.lineRendererCount = this.lineRendererCount;
			}
			if (this.clothCount != null)
			{
				to.clothCount = this.clothCount;
			}
			if (this.clothMaxVertices != null)
			{
				to.clothMaxVertices = this.clothMaxVertices;
			}
			if (this.physicsColliderCount != null)
			{
				to.physicsColliderCount = this.physicsColliderCount;
			}
			if (this.physicsRigidbodyCount != null)
			{
				to.physicsRigidbodyCount = this.physicsRigidbodyCount;
			}
			if (this.audioSourceCount != null)
			{
				to.audioSourceCount = this.audioSourceCount;
			}
			if (this.downloadSizeBytes != null)
			{
				to.downloadSizeBytes = this.downloadSizeBytes;
			}
			if (this.uncompressedSizeBytes != null)
			{
				to.uncompressedSizeBytes = this.uncompressedSizeBytes;
			}
			if (this.textureMegabytes != null)
			{
				to.textureMegabytes = this.textureMegabytes;
			}
			if (this.physBone != null)
			{
				to.physBone = this.physBone;
			}
			if (this.contactCount != null)
			{
				to.contactCount = this.contactCount;
			}
			if (this.constraintsCount != null)
			{
				to.constraintsCount = this.constraintsCount;
			}
			if (this.constraintDepth != null)
			{
				to.constraintDepth = this.constraintDepth;
			}
		}

		// Token: 0x060002DF RID: 735 RVA: 0x0000C210 File Offset: 0x0000A410
		[PublicAPI]
		public AvatarPerformanceStats.Snapshot GetSnapshot()
		{
			return new AvatarPerformanceStats.Snapshot(this);
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x0000C218 File Offset: 0x0000A418
		[PublicAPI]
		public PerformanceRating GetPerformanceRatingForCategory(AvatarPerformanceCategory perfCategory)
		{
			if (this._performanceRatingCache[(int)perfCategory] == PerformanceRating.None)
			{
				this._performanceRatingCache[(int)perfCategory] = this.CalculatePerformanceRatingForCategory(perfCategory);
			}
			return this._performanceRatingCache[(int)perfCategory];
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x0000C23C File Offset: 0x0000A43C
		[PublicAPI]
		public void CalculateAllPerformanceRatings(bool isMobilePlatform)
		{
			this._performanceStatsLevelSet = AvatarPerformanceStats.GetAvatarPerformanceStatsLevelSet(isMobilePlatform);
			for (int i = 0; i < this._performanceRatingCache.Length; i++)
			{
				this._performanceRatingCache[i] = PerformanceRating.None;
			}
			foreach (AvatarPerformanceCategory avatarPerformanceCategory in AvatarPerformanceStats._performanceCategories)
			{
				if (avatarPerformanceCategory != AvatarPerformanceCategory.None && avatarPerformanceCategory != AvatarPerformanceCategory.AvatarPerformanceCategoryCount && this._performanceRatingCache[(int)avatarPerformanceCategory] == PerformanceRating.None)
				{
					this._performanceRatingCache[(int)avatarPerformanceCategory] = this.CalculatePerformanceRatingForCategory(avatarPerformanceCategory);
				}
			}
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x0000C2B0 File Offset: 0x0000A4B0
		[PublicAPI]
		public void LoadAllPerformanceRatings(Dictionary<string, object> stats, bool isMobilePlatform)
		{
			this._performanceStatsLevelSet = AvatarPerformanceStats.GetAvatarPerformanceStatsLevelSet(isMobilePlatform);
			for (int i = 0; i < this._performanceRatingCache.Length; i++)
			{
				this._performanceRatingCache[i] = PerformanceRating.None;
			}
			foreach (KeyValuePair<string, object> keyValuePair in stats)
			{
				string key = keyValuePair.Key;
				if (key != null)
				{
					switch (key.Length)
					{
					case 6:
						if (key == "bounds")
						{
							float[] array = Enumerable.ToArray<float>(Enumerable.Select<object, float>((List<object>)keyValuePair.Value, (object v) => (float)((double)v)));
							this.aabb = new Bounds?(new Bounds(Vector3.zero, new Vector3(array[0], array[1], array[2])));
						}
						break;
					case 9:
					{
						char c = key[0];
						if (c != 'b')
						{
							if (c == 'm')
							{
								if (key == "meshCount")
								{
									this.meshCount = new int?((int)((double)keyValuePair.Value));
								}
							}
						}
						else if (key == "boneCount")
						{
							this.boneCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					}
					case 10:
					{
						char c = key[0];
						if (c != 'c')
						{
							if (c == 'l')
							{
								if (key == "lightCount")
								{
									this.lightCount = new int?((int)((double)keyValuePair.Value));
								}
							}
						}
						else if (key == "clothCount")
						{
							this.clothCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					}
					case 12:
						if (key == "contactCount")
						{
							this.contactCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					case 13:
					{
						char c = key[0];
						if (c != 'a')
						{
							if (c == 't')
							{
								if (key == "totalPolygons")
								{
									this.polyCount = new int?((int)((double)keyValuePair.Value));
								}
							}
						}
						else if (key == "animatorCount")
						{
							this.animatorCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					}
					case 15:
						if (key == "constraintCount")
						{
							this.constraintsCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					case 16:
					{
						char c = key[0];
						if (c != 'a')
						{
							if (c != 'p')
							{
								if (c == 's')
								{
									if (key == "skinnedMeshCount")
									{
										this.skinnedMeshCount = new int?((int)((double)keyValuePair.Value));
									}
								}
							}
							else if (key == "physicsColliders")
							{
								this.physicsColliderCount = new int?((int)((double)keyValuePair.Value));
							}
						}
						else if (key == "audioSourceCount")
						{
							this.audioSourceCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					}
					case 17:
					{
						char c = key[5];
						if (c <= 'T')
						{
							if (c != 'M')
							{
								if (c == 'T')
								{
									if (key == "totalTextureUsage")
									{
										this.textureMegabytes = new float?((float)((double)keyValuePair.Value) / 1048576f);
									}
								}
							}
							else if (key == "totalMaxParticles")
							{
								this.particleTotalCount = new int?((int)((double)keyValuePair.Value));
							}
						}
						else if (c != 'e')
						{
							if (c == 'i')
							{
								if (key == "materialSlotsUsed")
								{
									this.materialCount = new int?((int)((double)keyValuePair.Value));
								}
							}
						}
						else if (key == "lineRendererCount")
						{
							this.lineRendererCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					}
					case 18:
					{
						char c = key[1];
						if (c != 'h')
						{
							if (c != 'o')
							{
								if (c == 'r')
								{
									if (key == "trailRendererCount")
									{
										this.trailRendererCount = new int?((int)((double)keyValuePair.Value));
									}
								}
							}
							else if (key == "totalClothVertices")
							{
								this.clothMaxVertices = new int?((int)((double)keyValuePair.Value));
							}
						}
						else if (key == "physicsRigidbodies")
						{
							this.physicsRigidbodyCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					}
					case 19:
						if (key == "particleSystemCount")
						{
							this.particleSystemCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					case 20:
					{
						char c = key[3];
						if (c != 's')
						{
							if (c == 't')
							{
								if (key == "contactCompleteCount")
								{
									this.contactCompleteCount = new int?((int)((double)keyValuePair.Value));
								}
							}
						}
						else if (key == "constraintDepthCount")
						{
							this.constraintDepth = new int?((int)((double)keyValuePair.Value));
						}
						break;
					}
					case 21:
					{
						char c = key[1];
						if (c != 'a')
						{
							if (c == 'h')
							{
								if (key == "physBoneColliderCount")
								{
									AvatarPerformanceStats.PhysBoneStats valueOrDefault = this.physBone.GetValueOrDefault();
									valueOrDefault.colliderCount = (int)((double)keyValuePair.Value);
									this.physBone = new AvatarPerformanceStats.PhysBoneStats?(valueOrDefault);
								}
							}
						}
						else if (key == "particleTrailsEnabled")
						{
							this.particleTrailsEnabled = new bool?((bool)keyValuePair.Value);
						}
						break;
					}
					case 22:
					{
						char c = key[8];
						if (c != 'C')
						{
							if (c == 'T')
							{
								if (key == "physBoneTransformCount")
								{
									AvatarPerformanceStats.PhysBoneStats valueOrDefault = this.physBone.GetValueOrDefault();
									valueOrDefault.transformCount = (int)((double)keyValuePair.Value);
									this.physBone = new AvatarPerformanceStats.PhysBoneStats?(valueOrDefault);
								}
							}
						}
						else if (key == "physBoneComponentCount")
						{
							AvatarPerformanceStats.PhysBoneStats valueOrDefault = this.physBone.GetValueOrDefault();
							valueOrDefault.componentCount = (int)((double)keyValuePair.Value);
							this.physBone = new AvatarPerformanceStats.PhysBoneStats?(valueOrDefault);
						}
						break;
					}
					case 23:
						if (key == "meshParticleMaxPolygons")
						{
							this.particleMaxMeshPolyCount = new int?((int)((double)keyValuePair.Value));
						}
						break;
					case 24:
						if (key == "particleCollisionEnabled")
						{
							this.particleCollisionEnabled = new bool?((bool)keyValuePair.Value);
						}
						break;
					case 27:
						if (key == "physBoneCollisionCheckCount")
						{
							AvatarPerformanceStats.PhysBoneStats valueOrDefault = this.physBone.GetValueOrDefault();
							valueOrDefault.collisionCheckCount = (int)((double)keyValuePair.Value);
							this.physBone = new AvatarPerformanceStats.PhysBoneStats?(valueOrDefault);
						}
						break;
					}
				}
			}
			this.BuildAvatarStatAnalyticsList();
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0000CB60 File Offset: 0x0000AD60
		[PublicAPI]
		public static string GetPerformanceCategoryDisplayName(AvatarPerformanceCategory category)
		{
			return AvatarPerformanceStats._performanceCategoryDisplayNames[category];
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0000CB6D File Offset: 0x0000AD6D
		[PublicAPI]
		public static string GetPerformanceRatingDisplayName(PerformanceRating rating)
		{
			return AvatarPerformanceStats._performanceRatingDisplayNames[rating];
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x0000CB7C File Offset: 0x0000AD7C
		[PublicAPI]
		public static AvatarPerformanceStatsLevel GetStatLevelForRating(PerformanceRating rating, bool isMobilePlatform)
		{
			AvatarPerformanceStatsLevelSet avatarPerformanceStatsLevelSet = AvatarPerformanceStats.GetAvatarPerformanceStatsLevelSet(isMobilePlatform);
			AvatarPerformanceStatsLevel result;
			switch (rating)
			{
			case PerformanceRating.None:
				result = avatarPerformanceStatsLevelSet.excellent;
				break;
			case PerformanceRating.Excellent:
				result = avatarPerformanceStatsLevelSet.excellent;
				break;
			case PerformanceRating.Good:
				result = avatarPerformanceStatsLevelSet.good;
				break;
			case PerformanceRating.Medium:
				result = avatarPerformanceStatsLevelSet.medium;
				break;
			case PerformanceRating.Poor:
				result = avatarPerformanceStatsLevelSet.poor;
				break;
			case PerformanceRating.VeryPoor:
				result = avatarPerformanceStatsLevelSet.poor;
				break;
			default:
				result = avatarPerformanceStatsLevelSet.excellent;
				break;
			}
			return result;
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0000CBF0 File Offset: 0x0000ADF0
		private PerformanceRating CalculatePerformanceRatingForCategory(AvatarPerformanceCategory perfCategory)
		{
			switch (perfCategory)
			{
			case AvatarPerformanceCategory.Overall:
			{
				PerformanceRating performanceRating = PerformanceRating.None;
				foreach (AvatarPerformanceCategory avatarPerformanceCategory in AvatarPerformanceStats._performanceCategories)
				{
					if (avatarPerformanceCategory != AvatarPerformanceCategory.None && avatarPerformanceCategory != AvatarPerformanceCategory.Overall && avatarPerformanceCategory != AvatarPerformanceCategory.AvatarPerformanceCategoryCount)
					{
						PerformanceRating performanceRatingForCategory = this.GetPerformanceRatingForCategory(avatarPerformanceCategory);
						if (performanceRatingForCategory > performanceRating)
						{
							performanceRating = performanceRatingForCategory;
						}
					}
				}
				return performanceRating;
			}
			case AvatarPerformanceCategory.DownloadSize:
				if (this.downloadSizeBytes == null)
				{
					return PerformanceRating.None;
				}
				return PerformanceRating.Excellent;
			case AvatarPerformanceCategory.UncompressedSize:
				if (this.uncompressedSizeBytes == null)
				{
					return PerformanceRating.None;
				}
				return PerformanceRating.Excellent;
			case AvatarPerformanceCategory.PolyCount:
				if (this.polyCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.polyCount.GetValueOrDefault() - y.polyCount));
			case AvatarPerformanceCategory.AABB:
				if (this.aabb == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)((AvatarPerformanceStats.ApproxLessOrEqual(y.aabb.extents.x, 0f) || (AvatarPerformanceStats.ApproxLessOrEqual(x.aabb.GetValueOrDefault().extents.x, y.aabb.extents.x) && AvatarPerformanceStats.ApproxLessOrEqual(x.aabb.GetValueOrDefault().extents.y, y.aabb.extents.y) && AvatarPerformanceStats.ApproxLessOrEqual(x.aabb.GetValueOrDefault().extents.z, y.aabb.extents.z))) ? -1 : 1));
			case AvatarPerformanceCategory.SkinnedMeshCount:
				if (this.skinnedMeshCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.skinnedMeshCount.GetValueOrDefault() - y.skinnedMeshCount));
			case AvatarPerformanceCategory.MeshCount:
				if (this.meshCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.meshCount.GetValueOrDefault() - y.meshCount));
			case AvatarPerformanceCategory.MaterialCount:
				if (this.materialCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.materialCount.GetValueOrDefault() - y.materialCount));
			case AvatarPerformanceCategory.PhysBoneComponentCount:
				if (this.physBone == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.physBone.GetValueOrDefault().componentCount - y.physBone.componentCount));
			case AvatarPerformanceCategory.PhysBoneTransformCount:
				if (this.physBone == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.physBone.GetValueOrDefault().transformCount - y.physBone.transformCount));
			case AvatarPerformanceCategory.PhysBoneColliderCount:
				if (this.physBone == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.physBone.GetValueOrDefault().colliderCount - y.physBone.colliderCount));
			case AvatarPerformanceCategory.PhysBoneCollisionCheckCount:
				if (this.physBone == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.physBone.GetValueOrDefault().collisionCheckCount - y.physBone.collisionCheckCount));
			case AvatarPerformanceCategory.ContactCount:
				if (this.contactCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.contactCount.GetValueOrDefault() - y.contactCount));
			case AvatarPerformanceCategory.AnimatorCount:
				if (this.animatorCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.animatorCount.GetValueOrDefault() - y.animatorCount));
			case AvatarPerformanceCategory.BoneCount:
				if (this.boneCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.boneCount.GetValueOrDefault() - y.boneCount));
			case AvatarPerformanceCategory.LightCount:
				if (this.lightCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.lightCount.GetValueOrDefault() - y.lightCount));
			case AvatarPerformanceCategory.ParticleSystemCount:
				if (this.particleSystemCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.particleSystemCount.GetValueOrDefault() - y.particleSystemCount));
			case AvatarPerformanceCategory.ParticleTotalCount:
				if (this.particleTotalCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.particleTotalCount.GetValueOrDefault() - y.particleTotalCount));
			case AvatarPerformanceCategory.ParticleMaxMeshPolyCount:
				if (this.particleMaxMeshPolyCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.particleMaxMeshPolyCount.GetValueOrDefault() - y.particleMaxMeshPolyCount));
			case AvatarPerformanceCategory.ParticleTrailsEnabled:
				if (this.particleTrailsEnabled == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating(delegate(AvatarPerformanceStats x, AvatarPerformanceStatsLevel y)
				{
					bool? flag = x.particleTrailsEnabled;
					bool flag2 = y.particleTrailsEnabled;
					if (flag.GetValueOrDefault() == flag2 & flag != null)
					{
						return 0f;
					}
					return (float)(x.particleTrailsEnabled.GetValueOrDefault() ? 1 : -1);
				});
			case AvatarPerformanceCategory.ParticleCollisionEnabled:
				if (this.particleCollisionEnabled == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating(delegate(AvatarPerformanceStats x, AvatarPerformanceStatsLevel y)
				{
					bool? flag = x.particleCollisionEnabled;
					bool flag2 = y.particleCollisionEnabled;
					if (flag.GetValueOrDefault() == flag2 & flag != null)
					{
						return 0f;
					}
					return (float)(x.particleCollisionEnabled.GetValueOrDefault() ? 1 : -1);
				});
			case AvatarPerformanceCategory.TrailRendererCount:
				if (this.trailRendererCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.trailRendererCount.GetValueOrDefault() - y.trailRendererCount));
			case AvatarPerformanceCategory.LineRendererCount:
				if (this.lineRendererCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.lineRendererCount.GetValueOrDefault() - y.lineRendererCount));
			case AvatarPerformanceCategory.ClothCount:
				if (this.clothCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.clothCount.GetValueOrDefault() - y.clothCount));
			case AvatarPerformanceCategory.ClothMaxVertices:
				if (this.clothMaxVertices == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.clothMaxVertices.GetValueOrDefault() - y.clothMaxVertices));
			case AvatarPerformanceCategory.PhysicsColliderCount:
				if (this.physicsColliderCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.physicsColliderCount.GetValueOrDefault() - y.physicsColliderCount));
			case AvatarPerformanceCategory.PhysicsRigidbodyCount:
				if (this.physicsRigidbodyCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.physicsRigidbodyCount.GetValueOrDefault() - y.physicsRigidbodyCount));
			case AvatarPerformanceCategory.AudioSourceCount:
				if (this.audioSourceCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.audioSourceCount.GetValueOrDefault() - y.audioSourceCount));
			case AvatarPerformanceCategory.TextureMegabytes:
				if (this.textureMegabytes == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => x.textureMegabytes.GetValueOrDefault() - y.textureMegabytes);
			case AvatarPerformanceCategory.ConstraintsCount:
				if (this.constraintsCount == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.constraintsCount.GetValueOrDefault() - y.constraintsCount));
			case AvatarPerformanceCategory.ConstraintDepth:
				if (this.constraintDepth == null)
				{
					return PerformanceRating.None;
				}
				return this.CalculatePerformanceRating((AvatarPerformanceStats x, AvatarPerformanceStatsLevel y) => (float)(x.constraintDepth.GetValueOrDefault() - y.constraintDepth));
			}
			return PerformanceRating.None;
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x0000D2B8 File Offset: 0x0000B4B8
		private PerformanceRating CalculatePerformanceRating(AvatarPerformanceStats.ComparePerformanceStatsDelegate compareFn)
		{
			if (compareFn(this, this._performanceStatsLevelSet.excellent) <= 0f)
			{
				return PerformanceRating.Excellent;
			}
			if (compareFn(this, this._performanceStatsLevelSet.good) <= 0f)
			{
				return PerformanceRating.Good;
			}
			if (compareFn(this, this._performanceStatsLevelSet.medium) <= 0f)
			{
				return PerformanceRating.Medium;
			}
			if (compareFn(this, this._performanceStatsLevelSet.poor) <= 0f)
			{
				return PerformanceRating.Poor;
			}
			return PerformanceRating.VeryPoor;
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0000D334 File Offset: 0x0000B534
		private static bool ApproxLessOrEqual(float x1, float x2)
		{
			float num = x1 - x2;
			return num < 0f || Mathf.Approximately(num, 0f);
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0000D35C File Offset: 0x0000B55C
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Avatar Name: {0}\n", this.avatarName);
			stringBuilder.AppendFormat("Overall Performance: {0}\n", this.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall));
			stringBuilder.AppendFormat("Poly Count: {0}\n", this.polyCount);
			stringBuilder.AppendFormat("Bounds: {0}\n", this.aabb.ToString());
			stringBuilder.AppendFormat("Skinned Mesh Count: {0}\n", this.skinnedMeshCount);
			stringBuilder.AppendFormat("Mesh Count: {0}\n", this.meshCount);
			stringBuilder.AppendFormat("Material Count: {0}\n", this.materialCount);
			stringBuilder.AppendFormat("Animator Count: {0}\n", this.animatorCount);
			stringBuilder.AppendFormat("Bone Count: {0}\n", this.boneCount);
			stringBuilder.AppendFormat("Light Count: {0}\n", this.lightCount);
			stringBuilder.AppendFormat("Particle System Count: {0}\n", this.particleSystemCount);
			stringBuilder.AppendFormat("Particle Total Count: {0}\n", this.particleTotalCount);
			stringBuilder.AppendFormat("Particle Max Mesh Poly Count: {0}\n", this.particleMaxMeshPolyCount);
			stringBuilder.AppendFormat("Particle Trails Enabled: {0}\n", this.particleTrailsEnabled);
			stringBuilder.AppendFormat("Particle Collision Enabled: {0}\n", this.particleCollisionEnabled);
			stringBuilder.AppendFormat("Trail Renderer Count: {0}\n", this.trailRendererCount);
			stringBuilder.AppendFormat("Line Renderer Count: {0}\n", this.lineRendererCount);
			stringBuilder.AppendFormat("PhysBone Component Count: {0}\n", (this.physBone != null) ? new int?(this.physBone.GetValueOrDefault().componentCount) : default(int?));
			stringBuilder.AppendFormat("PhysBone Transform Count: {0}\n", (this.physBone != null) ? new int?(this.physBone.GetValueOrDefault().transformCount) : default(int?));
			stringBuilder.AppendFormat("PhysBone Collider Count: {0}\n", (this.physBone != null) ? new int?(this.physBone.GetValueOrDefault().colliderCount) : default(int?));
			stringBuilder.AppendFormat("PhysBone Collision Check Count: {0}\n", (this.physBone != null) ? new int?(this.physBone.GetValueOrDefault().collisionCheckCount) : default(int?));
			stringBuilder.AppendFormat("Cloth Count: {0}\n", this.clothCount);
			stringBuilder.AppendFormat("Cloth Max Vertices: {0}\n", this.clothMaxVertices);
			stringBuilder.AppendFormat("Physics Collider Count: {0}\n", this.physicsColliderCount);
			stringBuilder.AppendFormat("Physics Rigidbody Count: {0}\n", this.physicsRigidbodyCount);
			int? num = this.downloadSizeBytes;
			int num2 = 0;
			if (num.GetValueOrDefault() > num2 & num != null)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				string text = "Download Size: {0} MB\n";
				num = this.downloadSizeBytes;
				stringBuilder2.AppendFormat(text, (num != null) ? new float?((float)num.GetValueOrDefault() / 1048576f) : default(float?));
			}
			num = this.uncompressedSizeBytes;
			num2 = 0;
			if (num.GetValueOrDefault() > num2 & num != null)
			{
				StringBuilder stringBuilder3 = stringBuilder;
				string text2 = "Uncompressed Size: {0} MB\n";
				num = this.uncompressedSizeBytes;
				stringBuilder3.AppendFormat(text2, (num != null) ? new float?((float)num.GetValueOrDefault() / 1048576f) : default(float?));
			}
			stringBuilder.AppendFormat("Constraint Count: {0}\n", this.constraintsCount);
			stringBuilder.AppendFormat("Constraint Depth: {0}\n", this.constraintDepth);
			return stringBuilder.ToString();
		}

		// Token: 0x060002EA RID: 746 RVA: 0x0000D728 File Offset: 0x0000B928
		// Note: this type is marked as 'beforefieldinit'.
		static AvatarPerformanceStats()
		{
			List<string> list = new List<string>();
			list.Add("visible_avatar_poly_count");
			list.Add("visible_avatar_skinned_mesh_count");
			list.Add("visible_avatar_mesh_count");
			list.Add("visible_avatar_material_count");
			list.Add("visible_avatar_animator_count");
			list.Add("visible_avatar_bone_count");
			list.Add("visible_avatar_light_count");
			list.Add("visible_avatar_particle_system_count");
			list.Add("visible_avatar_particle_total_count");
			list.Add("visible_avatar_particle_mesh_poly_count");
			list.Add("visible_avatar_trail_renderer_count");
			list.Add("visible_avatar_line_renderer_count");
			list.Add("visible_avatar_cloth_count");
			list.Add("visible_avatar_cloth_vertices_count");
			list.Add("visible_avatar_physics_collider_count");
			list.Add("visible_avatar_physics_rigidbody_count");
			list.Add("visible_avatar_audio_source_count");
			list.Add("visible_avatar_contact_count");
			list.Add("visible_avatar_constraint_count");
			list.Add("visible_avatar_constraint_depth");
			list.Add("visible_avatar_physbone_component_count");
			list.Add("visible_avatar_physbone_transform_count");
			list.Add("visible_avatar_physbone_collider_count");
			list.Add("visible_avatar_physbone_collision_count");
			AvatarPerformanceStats._analyticsStatNames = list;
			AvatarPerformanceStats._performanceCategories = Enumerable.Cast<AvatarPerformanceCategory>(Enum.GetValues(typeof(AvatarPerformanceCategory))).ToImmutableArray<AvatarPerformanceCategory>();
			Dictionary<AvatarPerformanceCategory, string> dictionary = new Dictionary<AvatarPerformanceCategory, string>();
			dictionary.Add(AvatarPerformanceCategory.PolyCount, "Polygons");
			dictionary.Add(AvatarPerformanceCategory.AABB, "Bounds");
			dictionary.Add(AvatarPerformanceCategory.SkinnedMeshCount, "Skinned Meshes");
			dictionary.Add(AvatarPerformanceCategory.MeshCount, "Meshes");
			dictionary.Add(AvatarPerformanceCategory.MaterialCount, "Material Slots");
			dictionary.Add(AvatarPerformanceCategory.AnimatorCount, "Animators");
			dictionary.Add(AvatarPerformanceCategory.BoneCount, "Bones");
			dictionary.Add(AvatarPerformanceCategory.LightCount, "Lights");
			dictionary.Add(AvatarPerformanceCategory.ParticleSystemCount, "Particle Systems");
			dictionary.Add(AvatarPerformanceCategory.ParticleTotalCount, "Total Max Particles");
			dictionary.Add(AvatarPerformanceCategory.ParticleMaxMeshPolyCount, "Mesh Particle Max Polygons");
			dictionary.Add(AvatarPerformanceCategory.ParticleTrailsEnabled, "Particle Trails Enabled");
			dictionary.Add(AvatarPerformanceCategory.ParticleCollisionEnabled, "Particle Collision Enabled");
			dictionary.Add(AvatarPerformanceCategory.TrailRendererCount, "Trail Renderers");
			dictionary.Add(AvatarPerformanceCategory.LineRendererCount, "Line Renderers");
			dictionary.Add(AvatarPerformanceCategory.ClothCount, "Cloths");
			dictionary.Add(AvatarPerformanceCategory.ClothMaxVertices, "Total Cloth Vertices");
			dictionary.Add(AvatarPerformanceCategory.PhysicsColliderCount, "Physics Colliders");
			dictionary.Add(AvatarPerformanceCategory.PhysicsRigidbodyCount, "Physics Rigidbodies");
			dictionary.Add(AvatarPerformanceCategory.AudioSourceCount, "Audio Sources");
			dictionary.Add(AvatarPerformanceCategory.DownloadSize, "Download Size");
			dictionary.Add(AvatarPerformanceCategory.UncompressedSize, "Uncompressed Size");
			dictionary.Add(AvatarPerformanceCategory.ContactCount, "Contact Count");
			dictionary.Add(AvatarPerformanceCategory.PhysBoneComponentCount, "PhysBone Components");
			dictionary.Add(AvatarPerformanceCategory.PhysBoneTransformCount, "PhysBone Transforms");
			dictionary.Add(AvatarPerformanceCategory.PhysBoneColliderCount, "PhysBone Colliders");
			dictionary.Add(AvatarPerformanceCategory.PhysBoneCollisionCheckCount, "PhysBone Collision Check Count");
			dictionary.Add(AvatarPerformanceCategory.TextureMegabytes, "Texture Memory");
			dictionary.Add(AvatarPerformanceCategory.ConstraintsCount, "Constraints");
			dictionary.Add(AvatarPerformanceCategory.ConstraintDepth, "Constraint Depth");
			AvatarPerformanceStats._performanceCategoryDisplayNames = dictionary;
			Dictionary<PerformanceRating, string> dictionary2 = new Dictionary<PerformanceRating, string>();
			dictionary2.Add(PerformanceRating.None, "None");
			dictionary2.Add(PerformanceRating.Excellent, "Excellent");
			dictionary2.Add(PerformanceRating.Good, "Good");
			dictionary2.Add(PerformanceRating.Medium, "Medium");
			dictionary2.Add(PerformanceRating.Poor, "Poor");
			dictionary2.Add(PerformanceRating.VeryPoor, "VeryPoor");
			AvatarPerformanceStats._performanceRatingDisplayNames = dictionary2;
			AvatarPerformanceStats._performanceStatsLevelSet_Windows = null;
			AvatarPerformanceStats._performanceStatsLevelSet_Mobile = null;
		}

		// Token: 0x04000330 RID: 816
		private const float BYTES_TO_MEGABYTES_CONVERSION_FACTOR = 1048576f;

		// Token: 0x04000331 RID: 817
		public string avatarName;

		// Token: 0x04000332 RID: 818
		public int? polyCount;

		// Token: 0x04000333 RID: 819
		public Bounds? aabb;

		// Token: 0x04000334 RID: 820
		public int? skinnedMeshCount;

		// Token: 0x04000335 RID: 821
		public int? meshCount;

		// Token: 0x04000336 RID: 822
		public int? materialCount;

		// Token: 0x04000337 RID: 823
		public int? animatorCount;

		// Token: 0x04000338 RID: 824
		public int? boneCount;

		// Token: 0x04000339 RID: 825
		public int? lightCount;

		// Token: 0x0400033A RID: 826
		public int? particleSystemCount;

		// Token: 0x0400033B RID: 827
		public int? particleTotalCount;

		// Token: 0x0400033C RID: 828
		public int? particleMaxMeshPolyCount;

		// Token: 0x0400033D RID: 829
		public bool? particleTrailsEnabled;

		// Token: 0x0400033E RID: 830
		public bool? particleCollisionEnabled;

		// Token: 0x0400033F RID: 831
		public int? trailRendererCount;

		// Token: 0x04000340 RID: 832
		public int? lineRendererCount;

		// Token: 0x04000341 RID: 833
		public int? clothCount;

		// Token: 0x04000342 RID: 834
		public int? clothMaxVertices;

		// Token: 0x04000343 RID: 835
		public int? physicsColliderCount;

		// Token: 0x04000344 RID: 836
		public int? physicsRigidbodyCount;

		// Token: 0x04000345 RID: 837
		public int? audioSourceCount;

		// Token: 0x04000346 RID: 838
		public int? downloadSizeBytes;

		// Token: 0x04000347 RID: 839
		public int? uncompressedSizeBytes;

		// Token: 0x04000348 RID: 840
		public float? textureMegabytes;

		// Token: 0x04000349 RID: 841
		public AvatarPerformanceStats.PhysBoneStats? physBone;

		// Token: 0x0400034A RID: 842
		public int? contactCount;

		// Token: 0x0400034B RID: 843
		public int? contactCompleteCount;

		// Token: 0x0400034C RID: 844
		public int? constraintsCount;

		// Token: 0x0400034D RID: 845
		public int? constraintDepth;

		// Token: 0x0400034E RID: 846
		private readonly PerformanceRating[] _performanceRatingCache;

		// Token: 0x0400034F RID: 847
		private bool _analyticsStatsValid;

		// Token: 0x04000350 RID: 848
		private List<int> _analyticsStatValues = new List<int>(AvatarPerformanceStats._analyticsStatNames.Count);

		// Token: 0x04000351 RID: 849
		private static List<string> _analyticsStatNames;

		// Token: 0x04000352 RID: 850
		private static readonly ImmutableArray<AvatarPerformanceCategory> _performanceCategories;

		// Token: 0x04000353 RID: 851
		private static readonly Dictionary<AvatarPerformanceCategory, string> _performanceCategoryDisplayNames;

		// Token: 0x04000354 RID: 852
		private static readonly Dictionary<PerformanceRating, string> _performanceRatingDisplayNames;

		// Token: 0x04000355 RID: 853
		private static AvatarPerformanceStatsLevelSet _performanceStatsLevelSet_Windows;

		// Token: 0x04000356 RID: 854
		private static AvatarPerformanceStatsLevelSet _performanceStatsLevelSet_Mobile;

		// Token: 0x04000357 RID: 855
		private AvatarPerformanceStatsLevelSet _performanceStatsLevelSet;

		// Token: 0x02000125 RID: 293
		// (Invoke) Token: 0x060004CE RID: 1230
		private delegate float ComparePerformanceStatsDelegate(AvatarPerformanceStats stats, AvatarPerformanceStatsLevel statsLevel);

		// Token: 0x02000126 RID: 294
		[Serializable]
		public struct PhysBoneStats
		{
			// Token: 0x040005DE RID: 1502
			public int componentCount;

			// Token: 0x040005DF RID: 1503
			public int transformCount;

			// Token: 0x040005E0 RID: 1504
			public int colliderCount;

			// Token: 0x040005E1 RID: 1505
			public int collisionCheckCount;
		}

		// Token: 0x02000127 RID: 295
		public readonly struct Snapshot
		{
			// Token: 0x1700005B RID: 91
			// (get) Token: 0x060004D1 RID: 1233 RVA: 0x00011590 File Offset: 0x0000F790
			[Obsolete("Use downloadSizeBytes instead")]
			public float? downloadSize
			{
				get
				{
					int? num = this.downloadSizeBytes;
					if (num == null)
					{
						return default(float?);
					}
					return new float?((float)num.GetValueOrDefault() / 1048576f);
				}
			}

			// Token: 0x1700005C RID: 92
			// (get) Token: 0x060004D2 RID: 1234 RVA: 0x000115CC File Offset: 0x0000F7CC
			[Obsolete("Use uncompressedSizeBytes instead")]
			public float? uncompressedSize
			{
				get
				{
					int? num = this.uncompressedSizeBytes;
					if (num == null)
					{
						return default(float?);
					}
					return new float?((float)num.GetValueOrDefault() / 1048576f);
				}
			}

			// Token: 0x060004D3 RID: 1235 RVA: 0x00011608 File Offset: 0x0000F808
			public Snapshot(AvatarPerformanceStats avatarPerformanceStats)
			{
				this.avatarName = avatarPerformanceStats.avatarName;
				this.polyCount = avatarPerformanceStats.polyCount;
				this.aabb = avatarPerformanceStats.aabb;
				this.skinnedMeshCount = avatarPerformanceStats.skinnedMeshCount;
				this.meshCount = avatarPerformanceStats.meshCount;
				this.materialCount = avatarPerformanceStats.materialCount;
				this.animatorCount = avatarPerformanceStats.animatorCount;
				this.boneCount = avatarPerformanceStats.boneCount;
				this.lightCount = avatarPerformanceStats.lightCount;
				this.particleSystemCount = avatarPerformanceStats.particleSystemCount;
				this.particleTotalCount = avatarPerformanceStats.particleTotalCount;
				this.particleMaxMeshPolyCount = avatarPerformanceStats.particleMaxMeshPolyCount;
				this.particleTrailsEnabled = avatarPerformanceStats.particleTrailsEnabled;
				this.particleCollisionEnabled = avatarPerformanceStats.particleCollisionEnabled;
				this.trailRendererCount = avatarPerformanceStats.trailRendererCount;
				this.lineRendererCount = avatarPerformanceStats.lineRendererCount;
				this.physBone = avatarPerformanceStats.physBone;
				this.contactCount = avatarPerformanceStats.contactCount;
				this.contactCompleteCount = avatarPerformanceStats.contactCompleteCount;
				this.clothCount = avatarPerformanceStats.clothCount;
				this.clothMaxVertices = avatarPerformanceStats.clothMaxVertices;
				this.physicsColliderCount = avatarPerformanceStats.physicsColliderCount;
				this.physicsRigidbodyCount = avatarPerformanceStats.physicsRigidbodyCount;
				this.audioSourceCount = avatarPerformanceStats.audioSourceCount;
				this.textureMegabytes = avatarPerformanceStats.textureMegabytes;
				this.downloadSizeBytes = avatarPerformanceStats.downloadSizeBytes;
				this.uncompressedSizeBytes = avatarPerformanceStats.uncompressedSizeBytes;
				this.constraintsCount = avatarPerformanceStats.constraintsCount;
				this.constraintDepth = avatarPerformanceStats.constraintDepth;
				this.overallRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.Overall);
				this.polyCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PolyCount);
				this.aabbRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.AABB);
				this.skinnedMeshCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.SkinnedMeshCount);
				this.meshCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MeshCount);
				this.materialCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.MaterialCount);
				this.animatorCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.AnimatorCount);
				this.boneCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.BoneCount);
				this.lightCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.LightCount);
				this.particleSystemCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleSystemCount);
				this.particleTotalCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleTotalCount);
				this.particleMaxMeshPolyCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleMaxMeshPolyCount);
				this.particleTrailsEnabledRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleTrailsEnabled);
				this.particleCollisionEnabledRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ParticleCollisionEnabled);
				this.trailRendererCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.TrailRendererCount);
				this.lineRendererCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.LineRendererCount);
				this.physBoneComponentCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneComponentCount);
				this.physBoneTransformCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneTransformCount);
				this.physBoneColliderCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneColliderCount);
				this.physBoneCollisionCheckCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysBoneCollisionCheckCount);
				this.contactCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ContactCount);
				this.clothCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ClothCount);
				this.clothMaxVerticesRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ClothMaxVertices);
				this.physicsColliderCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysicsColliderCount);
				this.physicsRigidbodyCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.PhysicsRigidbodyCount);
				this.audioSourceCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.AudioSourceCount);
				this.textureMegabytesRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.TextureMegabytes);
				this.downloadSizeRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.DownloadSize);
				this.uncompressedSizeRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.UncompressedSize);
				this.constraintsCountRating = avatarPerformanceStats.GetPerformanceRatingForCategory(AvatarPerformanceCategory.ConstraintsCount);
			}

			// Token: 0x060004D4 RID: 1236 RVA: 0x00011910 File Offset: 0x0000FB10
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder(1024);
				stringBuilder.AppendFormat("Avatar Name: {0}\n", this.avatarName);
				stringBuilder.AppendFormat("Overall Performance: {0}\n", this.overallRating);
				stringBuilder.AppendFormat("Poly Count: {0}\n", this.polyCount);
				stringBuilder.AppendFormat("Bounds: {0}\n", this.aabb.ToString());
				stringBuilder.AppendFormat("Skinned Mesh Count: {0}\n", this.skinnedMeshCount);
				stringBuilder.AppendFormat("Mesh Count: {0}\n", this.meshCount);
				stringBuilder.AppendFormat("Material Count: {0}\n", this.materialCount);
				stringBuilder.AppendFormat("Animator Count: {0}\n", this.animatorCount);
				stringBuilder.AppendFormat("Bone Count: {0}\n", this.boneCount);
				stringBuilder.AppendFormat("Light Count: {0}\n", this.lightCount);
				stringBuilder.AppendFormat("Particle System Count: {0}\n", this.particleSystemCount);
				stringBuilder.AppendFormat("Particle Total Count: {0}\n", this.particleTotalCount);
				stringBuilder.AppendFormat("Particle Max Mesh Poly Count: {0}\n", this.particleMaxMeshPolyCount);
				stringBuilder.AppendFormat("Particle Trails Enabled: {0}\n", this.particleTrailsEnabled);
				stringBuilder.AppendFormat("Particle Collision Enabled: {0}\n", this.particleCollisionEnabled);
				stringBuilder.AppendFormat("Trail Renderer Count: {0}\n", this.trailRendererCount);
				stringBuilder.AppendFormat("Line Renderer Count: {0}\n", this.lineRendererCount);
				stringBuilder.AppendFormat("Phys Bone Component Count: {0}\n", (this.physBone != null) ? new int?(this.physBone.GetValueOrDefault().componentCount) : default(int?));
				stringBuilder.AppendFormat("Phys Bone Transform Count: {0}\n", (this.physBone != null) ? new int?(this.physBone.GetValueOrDefault().transformCount) : default(int?));
				stringBuilder.AppendFormat("Phys Bone Collider Count: {0}\n", (this.physBone != null) ? new int?(this.physBone.GetValueOrDefault().colliderCount) : default(int?));
				stringBuilder.AppendFormat("Phys Bone Collision Check Count: {0}\n", (this.physBone != null) ? new int?(this.physBone.GetValueOrDefault().collisionCheckCount) : default(int?));
				stringBuilder.AppendFormat("Non-Local Contact Count: {0}\n", this.contactCount);
				stringBuilder.AppendFormat("Complete Contact Count: {0}\n", this.contactCompleteCount);
				stringBuilder.AppendFormat("Cloth Count: {0}\n", this.clothCount);
				stringBuilder.AppendFormat("Cloth Max Vertices: {0}\n", this.clothMaxVertices);
				stringBuilder.AppendFormat("Physics Collider Count: {0}\n", this.physicsColliderCount);
				stringBuilder.AppendFormat("Physics Rigidbody Count: {0}\n", this.physicsRigidbodyCount);
				string text = "Download Size: {0} MB\n";
				int? num = this.downloadSizeBytes;
				stringBuilder.AppendFormat(text, (num != null) ? new float?((float)num.GetValueOrDefault() / 1048576f) : default(float?));
				string text2 = "Uncompressed Size: {0} MB\n";
				num = this.uncompressedSizeBytes;
				stringBuilder.AppendFormat(text2, (num != null) ? new float?((float)num.GetValueOrDefault() / 1048576f) : default(float?));
				stringBuilder.AppendFormat("Texture Size: {0} MB\n", this.textureMegabytes);
				stringBuilder.AppendFormat("Constraint Count: {0}\n", this.constraintsCount);
				stringBuilder.AppendFormat("Constraint Depth: {0}\n", this.constraintDepth);
				return stringBuilder.ToString();
			}

			// Token: 0x040005E2 RID: 1506
			public readonly string avatarName;

			// Token: 0x040005E3 RID: 1507
			public readonly int? polyCount;

			// Token: 0x040005E4 RID: 1508
			public readonly Bounds? aabb;

			// Token: 0x040005E5 RID: 1509
			public readonly int? skinnedMeshCount;

			// Token: 0x040005E6 RID: 1510
			public readonly int? meshCount;

			// Token: 0x040005E7 RID: 1511
			public readonly int? materialCount;

			// Token: 0x040005E8 RID: 1512
			public readonly int? animatorCount;

			// Token: 0x040005E9 RID: 1513
			public readonly int? boneCount;

			// Token: 0x040005EA RID: 1514
			public readonly int? lightCount;

			// Token: 0x040005EB RID: 1515
			public readonly int? particleSystemCount;

			// Token: 0x040005EC RID: 1516
			public readonly int? particleTotalCount;

			// Token: 0x040005ED RID: 1517
			public readonly int? particleMaxMeshPolyCount;

			// Token: 0x040005EE RID: 1518
			public readonly bool? particleTrailsEnabled;

			// Token: 0x040005EF RID: 1519
			public readonly bool? particleCollisionEnabled;

			// Token: 0x040005F0 RID: 1520
			public readonly int? trailRendererCount;

			// Token: 0x040005F1 RID: 1521
			public readonly int? lineRendererCount;

			// Token: 0x040005F2 RID: 1522
			public readonly AvatarPerformanceStats.PhysBoneStats? physBone;

			// Token: 0x040005F3 RID: 1523
			public readonly int? contactCount;

			// Token: 0x040005F4 RID: 1524
			public readonly int? contactCompleteCount;

			// Token: 0x040005F5 RID: 1525
			public readonly int? clothCount;

			// Token: 0x040005F6 RID: 1526
			public readonly int? clothMaxVertices;

			// Token: 0x040005F7 RID: 1527
			public readonly int? physicsColliderCount;

			// Token: 0x040005F8 RID: 1528
			public readonly int? physicsRigidbodyCount;

			// Token: 0x040005F9 RID: 1529
			public readonly int? audioSourceCount;

			// Token: 0x040005FA RID: 1530
			public readonly float? textureMegabytes;

			// Token: 0x040005FB RID: 1531
			public readonly int? downloadSizeBytes;

			// Token: 0x040005FC RID: 1532
			public readonly int? uncompressedSizeBytes;

			// Token: 0x040005FD RID: 1533
			public readonly int? constraintsCount;

			// Token: 0x040005FE RID: 1534
			public readonly int? constraintDepth;

			// Token: 0x040005FF RID: 1535
			public readonly PerformanceRating overallRating;

			// Token: 0x04000600 RID: 1536
			public readonly PerformanceRating polyCountRating;

			// Token: 0x04000601 RID: 1537
			public readonly PerformanceRating aabbRating;

			// Token: 0x04000602 RID: 1538
			public readonly PerformanceRating skinnedMeshCountRating;

			// Token: 0x04000603 RID: 1539
			public readonly PerformanceRating meshCountRating;

			// Token: 0x04000604 RID: 1540
			public readonly PerformanceRating materialCountRating;

			// Token: 0x04000605 RID: 1541
			public readonly PerformanceRating animatorCountRating;

			// Token: 0x04000606 RID: 1542
			public readonly PerformanceRating boneCountRating;

			// Token: 0x04000607 RID: 1543
			public readonly PerformanceRating lightCountRating;

			// Token: 0x04000608 RID: 1544
			public readonly PerformanceRating particleSystemCountRating;

			// Token: 0x04000609 RID: 1545
			public readonly PerformanceRating particleTotalCountRating;

			// Token: 0x0400060A RID: 1546
			public readonly PerformanceRating particleMaxMeshPolyCountRating;

			// Token: 0x0400060B RID: 1547
			public readonly PerformanceRating particleTrailsEnabledRating;

			// Token: 0x0400060C RID: 1548
			public readonly PerformanceRating particleCollisionEnabledRating;

			// Token: 0x0400060D RID: 1549
			public readonly PerformanceRating trailRendererCountRating;

			// Token: 0x0400060E RID: 1550
			public readonly PerformanceRating lineRendererCountRating;

			// Token: 0x0400060F RID: 1551
			public readonly PerformanceRating physBoneComponentCountRating;

			// Token: 0x04000610 RID: 1552
			public readonly PerformanceRating physBoneTransformCountRating;

			// Token: 0x04000611 RID: 1553
			public readonly PerformanceRating physBoneColliderCountRating;

			// Token: 0x04000612 RID: 1554
			public readonly PerformanceRating physBoneCollisionCheckCountRating;

			// Token: 0x04000613 RID: 1555
			public readonly PerformanceRating contactCountRating;

			// Token: 0x04000614 RID: 1556
			public readonly PerformanceRating clothCountRating;

			// Token: 0x04000615 RID: 1557
			public readonly PerformanceRating clothMaxVerticesRating;

			// Token: 0x04000616 RID: 1558
			public readonly PerformanceRating physicsColliderCountRating;

			// Token: 0x04000617 RID: 1559
			public readonly PerformanceRating physicsRigidbodyCountRating;

			// Token: 0x04000618 RID: 1560
			public readonly PerformanceRating audioSourceCountRating;

			// Token: 0x04000619 RID: 1561
			public readonly PerformanceRating textureMegabytesRating;

			// Token: 0x0400061A RID: 1562
			public readonly PerformanceRating downloadSizeRating;

			// Token: 0x0400061B RID: 1563
			public readonly PerformanceRating uncompressedSizeRating;

			// Token: 0x0400061C RID: 1564
			public readonly PerformanceRating constraintsCountRating;
		}
	}
}
