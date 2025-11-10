using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace VRC.Dynamics
{
	// Token: 0x02000002 RID: 2
	public class CollisionBroadphase_HashGrid : ICollisionBroadphase
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		// (set) Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		public CollisionScene scene { get; set; }

		// Token: 0x06000003 RID: 3 RVA: 0x00002064 File Offset: 0x00000264
		public void AddShape(CollisionScene.ShapeData shape, ushort id)
		{
			for (int i = 0; i < 32; i++)
			{
				this.collisions[(int)(id * 32) + i] = -1;
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002090 File Offset: 0x00000290
		public void RemoveShape(CollisionScene.ShapeData shape, ushort id)
		{
			if (shape.isCollider)
			{
				for (int i = shape.boundsMin.x; i <= shape.boundsMax.x; i++)
				{
					for (int j = shape.boundsMin.y; j <= shape.boundsMax.y; j++)
					{
						for (int k = shape.boundsMin.z; k <= shape.boundsMax.z; k++)
						{
							NativeParallelHashMapExtensions.Remove<Vector3Int, int>(this.shapeMap, new Vector3Int(i, j, k), (int)id);
						}
					}
				}
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000211F File Offset: 0x0000031F
		public void CastShape(CollisionScene.ShapeData shape, HashSet<ushort> output)
		{
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002121 File Offset: 0x00000321
		public void DrawGizmos()
		{
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002124 File Offset: 0x00000324
		public JobHandle ScheduleJobs(float deltaTime, JobHandle jobHandle)
		{
			this.shapesToUpdate.Clear();
			jobHandle = IJobParallelForExtensions.Schedule<CollisionBroadphase_HashGrid.MoveShapesJob>(new CollisionBroadphase_HashGrid.MoveShapesJob
			{
				transformData = this.scene.transformData,
				transformLookup = this.scene.transforms.GetLookupFromId(),
				shapeData = this.scene.shapeData,
				activeShapes = this.scene.activeShapes,
				deltaTime = deltaTime,
				shapesToUpdate = this.shapesToUpdate.AsParallelWriter()
			}, this.scene.activeShapes.Length, CollisionBroadphase_HashGrid.SHAPE_BATCH_COUNT, jobHandle);
			jobHandle = IJobExtensions.Schedule<CollisionBroadphase_HashGrid.UpdateShapesJob>(new CollisionBroadphase_HashGrid.UpdateShapesJob
			{
				shapeData = this.scene.shapeData,
				shapeMap = this.shapeMap,
				shapesToUpdate = this.shapesToUpdate
			}, jobHandle);
			jobHandle = IJobParallelForExtensions.Schedule<CollisionBroadphase_HashGrid.CollisionsJob>(new CollisionBroadphase_HashGrid.CollisionsJob
			{
				shapeData = this.scene.shapeData,
				activeShapes = this.scene.activeShapes,
				shapeMap = this.shapeMap,
				collisions = this.collisions,
				collisionEvents = this.scene.collisionEvents.AsParallelWriter()
			}, this.scene.activeShapes.Length, CollisionBroadphase_HashGrid.SHAPE_BATCH_COUNT, jobHandle);
			return jobHandle;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002282 File Offset: 0x00000482
		public void OnComplete()
		{
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002284 File Offset: 0x00000484
		public void Dispose()
		{
			this.shapeMap.Dispose();
			this.shapesToUpdate.Dispose();
			this.collisions.Dispose();
		}

		// Token: 0x04000001 RID: 1
		public static int SHAPE_BATCH_COUNT = 8;

		// Token: 0x04000002 RID: 2
		public const float GRID_SIZE = 0.5f;

		// Token: 0x04000003 RID: 3
		public const float GRID_MULTI = 2f;

		// Token: 0x04000004 RID: 4
		private const int MAX_COLLISIONS_PER_SHAPE = 32;

		// Token: 0x04000005 RID: 5
		private const int MAX_COLLISIONS = 262144;

		// Token: 0x04000006 RID: 6
		private const int MAX_SHAPE_UPDATE = 8192;

		// Token: 0x04000007 RID: 7
		private const int MAP_START_CAPACITY = 128;

		// Token: 0x04000009 RID: 9
		private NativeParallelMultiHashMap<Vector3Int, int> shapeMap = new NativeParallelMultiHashMap<Vector3Int, int>(128, (Allocator)4);

		// Token: 0x0400000A RID: 10
		private NativeQueue<int> shapesToUpdate = new NativeQueue<int>((Allocator)4);

		// Token: 0x0400000B RID: 11
		private NativeArray<int> collisions = new NativeArray<int>(262144, (Allocator)4, (NativeArrayOptions)1);

		// Token: 0x0200003C RID: 60
		private struct MoveShapesJob : IJobParallelFor
		{
			// Token: 0x06000279 RID: 633 RVA: 0x0001061C File Offset: 0x0000E81C
			private TransformAccess GetTransform(int id, int index)
			{
				int num = this.transformLookup[id * 2 + index];
				if (num >= 0)
				{
					return this.transformData[num];
				}
				return default(TransformAccess);
			}

			// Token: 0x0600027A RID: 634 RVA: 0x00010654 File Offset: 0x0000E854
			public void Execute(int index)
			{
				ushort num = this.activeShapes[index];
				CollisionScene.ShapeData shapeData = this.shapeData[(int)num];
				TransformAccess transform = this.GetTransform((int)num, 0);
				TransformAccess transform2 = this.GetTransform((int)num, 1);
				shapeData.UpdateShape(transform, transform2);
				shapeData.UpdateVelocity(transform, this.deltaTime);
				Vector3Int vector3Int = Vector3Int.FloorToInt(shapeData.bounds.min * 2f);
				Vector3Int vector3Int2 = Vector3Int.FloorToInt(shapeData.bounds.max * 2f);
				if (shapeData.isCollider)
				{
					if (shapeData.boundsMin != vector3Int || shapeData.boundsMax != vector3Int2)
					{
						shapeData.nextBoundsMin = vector3Int;
						shapeData.nextBoundsMax = vector3Int2;
						this.shapesToUpdate.Enqueue((int)num);
					}
				}
				else
				{
					shapeData.boundsMin = vector3Int;
					shapeData.boundsMax = vector3Int2;
				}
				this.shapeData[(int)num] = shapeData;
			}

			// Token: 0x0600027B RID: 635 RVA: 0x00010743 File Offset: 0x0000E943
			private Vector3 Rotate(Quaternion quaternion, Vector3 vector)
			{
				return quaternion * vector;
			}

			// Token: 0x040001F4 RID: 500
			private const float VelocityLerpSpeed = 30f;

			// Token: 0x040001F5 RID: 501
			[ReadOnly]
			public NativeArray<TransformAccess> transformData;

			// Token: 0x040001F6 RID: 502
			[ReadOnly]
			public NativeArray<int> transformLookup;

			// Token: 0x040001F7 RID: 503
			[ReadOnly]
			public NativeList<ushort> activeShapes;

			// Token: 0x040001F8 RID: 504
			[NativeDisableParallelForRestriction]
			public NativeArray<CollisionScene.ShapeData> shapeData;

			// Token: 0x040001F9 RID: 505
			public NativeQueue<int>.ParallelWriter shapesToUpdate;

			// Token: 0x040001FA RID: 506
			public float deltaTime;
		}

		// Token: 0x0200003D RID: 61
		private struct UpdateShapesJob : IJob
		{
			// Token: 0x0600027C RID: 636 RVA: 0x0001074C File Offset: 0x0000E94C
			public void Execute()
			{
				int num;
				while (this.shapesToUpdate.TryDequeue(out num))
				{
					CollisionScene.ShapeData shapeData = this.shapeData[num];
					this.RemoveShape(num, shapeData.boundsMin, shapeData.boundsMax, shapeData.nextBoundsMin, shapeData.nextBoundsMax);
					this.AddShape(num, shapeData.nextBoundsMin, shapeData.nextBoundsMax, shapeData.boundsMin, shapeData.boundsMax);
					shapeData.boundsMin = shapeData.nextBoundsMin;
					shapeData.boundsMax = shapeData.nextBoundsMax;
					this.shapeData[num] = shapeData;
				}
			}

			// Token: 0x0600027D RID: 637 RVA: 0x000107E4 File Offset: 0x0000E9E4
			private void RemoveShape(int id, Vector3Int min, Vector3Int max, Vector3Int exceptMin, Vector3 exceptMax)
			{
				for (int i = min.x; i <= max.x; i++)
				{
					bool flag = i >= exceptMin.x && (float)i <= exceptMax.x;
					for (int j = min.y; j <= max.y; j++)
					{
						bool flag2 = j >= exceptMin.y && (float)j <= exceptMax.y;
						for (int k = min.z; k <= max.z; k++)
						{
							bool flag3 = k >= exceptMin.z && (float)k <= exceptMax.z;
							if (!flag || !flag2 || !flag3)
							{
								this.RemoveShape(id, new Vector3Int(i, j, k));
							}
						}
					}
				}
			}

			// Token: 0x0600027E RID: 638 RVA: 0x000108BA File Offset: 0x0000EABA
			private void RemoveShape(int id, Vector3Int pos)
			{
				NativeParallelHashMapExtensions.Remove<Vector3Int, int>(this.shapeMap, pos, id);
			}

			// Token: 0x0600027F RID: 639 RVA: 0x000108CC File Offset: 0x0000EACC
			private void AddShape(int id, Vector3Int min, Vector3Int max, Vector3Int exceptMin, Vector3Int exceptMax)
			{
				for (int i = min.x; i <= max.x; i++)
				{
					bool flag = i >= exceptMin.x && i <= exceptMax.x;
					for (int j = min.y; j <= max.y; j++)
					{
						bool flag2 = j >= exceptMin.y && j <= exceptMax.y;
						for (int k = min.z; k <= max.z; k++)
						{
							bool flag3 = k >= exceptMin.z && k <= exceptMax.z;
							if (!flag || !flag2 || !flag3)
							{
								this.AddShape(id, new Vector3Int(i, j, k));
							}
						}
					}
				}
			}

			// Token: 0x06000280 RID: 640 RVA: 0x0001099C File Offset: 0x0000EB9C
			private void AddShape(int id, Vector3Int pos)
			{
				this.shapeMap.Add(pos, id);
			}

			// Token: 0x040001FB RID: 507
			public NativeQueue<int> shapesToUpdate;

			// Token: 0x040001FC RID: 508
			public NativeArray<CollisionScene.ShapeData> shapeData;

			// Token: 0x040001FD RID: 509
			public NativeParallelMultiHashMap<Vector3Int, int> shapeMap;
		}

		// Token: 0x0200003E RID: 62
		private struct CollisionsJob : IJobParallelFor
		{
			// Token: 0x06000281 RID: 641 RVA: 0x000109AC File Offset: 0x0000EBAC
			public void Execute(int index)
			{
				ushort num = this.activeShapes[index];
				CollisionScene.ShapeData shapeData = this.shapeData[(int)num];
				if (!shapeData.isReceiver)
				{
					return;
				}
				for (int i = 0; i < shapeData.collisionCount; i++)
				{
					int collision = this.GetCollision((int)num, i);
					CollisionScene.ShapeData shapeData2 = this.shapeData[collision];
					if (shapeData.bounds.Intersects(shapeData2.bounds) && !CollisionShapes.CheckCollision(shapeData.ToCollisionShape(), shapeData2.ToCollisionShape()))
					{
						this.collisionEvents.Enqueue(new CollisionScene.CollisionEvent
						{
							found = false,
							receiver = (int)num,
							collider = collision
						});
						this.SetCollision((int)num, i, this.GetCollision((int)num, shapeData.collisionCount - 1));
						shapeData.collisionCount--;
						i--;
					}
				}
				NativeList<int> nativeList = default(NativeList<int>);
				for (int j = shapeData.boundsMin.x; j <= shapeData.boundsMax.x; j++)
				{
					for (int k = shapeData.boundsMin.y; k <= shapeData.boundsMax.y; k++)
					{
						for (int l = shapeData.boundsMin.z; l <= shapeData.boundsMax.z; l++)
						{
							int num2;
							NativeParallelMultiHashMapIterator<Vector3Int> nativeParallelMultiHashMapIterator;
							if (this.shapeMap.TryGetFirstValue(new Vector3Int(j, k, l), out num2, out nativeParallelMultiHashMapIterator))
							{
								do
								{
									if (num2 != (int)num)
									{
										if (!nativeList.IsCreated)
										{
											nativeList = new NativeList<int>(16, (Allocator)2);
										}
										if (!NativeListExtensions.Contains<int, int>(nativeList, num2))
										{
											nativeList.Add(in num2);
										}
									}
								}
								while (this.shapeMap.TryGetNextValue(out num2, ref nativeParallelMultiHashMapIterator));
							}
						}
					}
				}
				if (shapeData.collisionCount < 32 && nativeList.IsCreated)
				{
					foreach (int num3 in nativeList)
					{
						if (!this.FindCollision((int)num, shapeData.collisionCount, num3) && CollisionShapes.CheckCollision(shapeData.ToCollisionShape(), this.shapeData[num3].ToCollisionShape()))
						{
							this.collisionEvents.Enqueue(new CollisionScene.CollisionEvent
							{
								found = true,
								receiver = (int)num,
								collider = num3
							});
							this.SetCollision((int)num, shapeData.collisionCount, num3);
							shapeData.collisionCount++;
							if (shapeData.collisionCount >= 32)
							{
								break;
							}
						}
					}
				}
				this.shapeData[(int)num] = shapeData;
				if (nativeList.IsCreated)
				{
					nativeList.Dispose();
				}
			}

			// Token: 0x06000282 RID: 642 RVA: 0x00010C74 File Offset: 0x0000EE74
			private int GetCollision(int id, int offset)
			{
				return this.collisions[id * 32 + offset];
			}

			// Token: 0x06000283 RID: 643 RVA: 0x00010C87 File Offset: 0x0000EE87
			private void SetCollision(int id, int offset, int value)
			{
				this.collisions[id * 32 + offset] = value;
			}

			// Token: 0x06000284 RID: 644 RVA: 0x00010C9C File Offset: 0x0000EE9C
			private bool FindCollision(int ourId, int count, int targetId)
			{
				int num = ourId * 32;
				for (int i = 0; i < count; i++)
				{
					if (this.collisions[num + i] == targetId)
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x040001FE RID: 510
			[ReadOnly]
			public NativeParallelMultiHashMap<Vector3Int, int> shapeMap;

			// Token: 0x040001FF RID: 511
			[ReadOnly]
			public NativeList<ushort> activeShapes;

			// Token: 0x04000200 RID: 512
			[NativeDisableParallelForRestriction]
			public NativeArray<CollisionScene.ShapeData> shapeData;

			// Token: 0x04000201 RID: 513
			[NativeDisableParallelForRestriction]
			public NativeArray<int> collisions;

			// Token: 0x04000202 RID: 514
			public NativeQueue<CollisionScene.CollisionEvent>.ParallelWriter collisionEvents;
		}
	}
}
