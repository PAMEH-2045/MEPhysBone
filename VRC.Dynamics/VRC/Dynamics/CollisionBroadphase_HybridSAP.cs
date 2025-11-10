using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace VRC.Dynamics
{
	// Token: 0x02000003 RID: 3
	public class CollisionBroadphase_HybridSAP : ICollisionBroadphase
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000C RID: 12 RVA: 0x000022FC File Offset: 0x000004FC
		// (set) Token: 0x0600000D RID: 13 RVA: 0x00002304 File Offset: 0x00000504
		public CollisionScene scene { get; set; }

		// Token: 0x0600000E RID: 14 RVA: 0x0000230D File Offset: 0x0000050D
		public void AddShape(CollisionScene.ShapeData shape, ushort id)
		{
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002310 File Offset: 0x00000510
		public void RemoveShape(CollisionScene.ShapeData shape, ushort id)
		{
			if (shape.isCollider || shape.isReceiver)
			{
				for (int i = shape.boundsMin.x; i <= shape.boundsMax.x; i++)
				{
					for (int j = shape.boundsMin.y; j <= shape.boundsMax.y; j++)
					{
						for (int k = shape.boundsMin.z; k <= shape.boundsMax.z; k++)
						{
							Vector3Int vector3Int = new(i, j, k);
							int num;
							if (this.gridMap.TryGetValue(vector3Int, out num))
							{
								CollisionBroadphase_HybridSAP.GridCell gridCell = this.gridCells[num];
								gridCell.RemoveShape(id);
								this.gridCells[num] = gridCell;
								if (gridCell.records.Length == 0)
								{
									this.gridMap.Remove(vector3Int);
									this.cellCache.Add(in num);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x0000240C File Offset: 0x0000060C
		public void CastShape(CollisionScene.ShapeData shape, HashSet<ushort> output)
		{
			shape.boundsMin = Vector3Int.FloorToInt(shape.bounds.min * 0.2f);
			shape.boundsMax = Vector3Int.FloorToInt(shape.bounds.max * 0.2f);
			for (int i = shape.boundsMin.x; i <= shape.boundsMax.x; i++)
			{
				for (int j = shape.boundsMin.y; j <= shape.boundsMax.y; j++)
				{
					for (int k = shape.boundsMin.z; k <= shape.boundsMax.z; k++)
					{
						int num;
						if (this.gridMap.TryGetValue(new Vector3Int(i, j, k), out num))
						{
							CollisionBroadphase_HybridSAP.GridCell gridCell = this.gridCells[num];
							this.CastShape(gridCell, shape, output);
						}
					}
				}
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000024F8 File Offset: 0x000006F8
		public JobHandle ScheduleJobs(float deltaTime, JobHandle jobHandle)
		{
			this.shapesToUpdate.Clear();
			int num = this.gridCells.Length + 4;
			jobHandle = IJobParallelForExtensions.Schedule<CollisionBroadphase_HybridSAP.MoveShapesJob>(new CollisionBroadphase_HybridSAP.MoveShapesJob
			{
				transformData = this.scene.transformData,
				transformLookup = this.scene.transforms.GetLookupFromId(),
				shapeData = this.scene.shapeData,
				activeShapes = this.scene.activeShapes,
				deltaTime = deltaTime,
				shapesToUpdate = this.shapesToUpdate.AsParallelWriter()
			}, this.scene.activeShapes.Length, CollisionBroadphase_HybridSAP.SHAPE_BATCH_COUNT, jobHandle);
			jobHandle = IJobExtensions.Schedule<CollisionBroadphase_HybridSAP.UpdateShapesJob>(new CollisionBroadphase_HybridSAP.UpdateShapesJob
			{
				shapeData = this.scene.shapeData,
				cellCache = this.cellCache,
				gridMap = this.gridMap,
				gridCells = this.gridCells,
				shapesToUpdate = this.shapesToUpdate
			}, jobHandle);
			jobHandle = IJobParallelForExtensions.Schedule<CollisionBroadphase_HybridSAP.UpdateGridCellsJob>(new CollisionBroadphase_HybridSAP.UpdateGridCellsJob
			{
				gridCells = this.gridCells,
				shapeData = this.scene.shapeData,
				collisionPairs = this.collisionPairs.AsParallelWriter()
			}, num, CollisionBroadphase_HybridSAP.CELL_BATCH_COUNT, jobHandle);
			jobHandle = IJobExtensions.Schedule<CollisionBroadphase_HybridSAP.CollisionEventsJob>(new CollisionBroadphase_HybridSAP.CollisionEventsJob
			{
				prevCollisionPairs = this.prevCollisionPairs,
				collisionEvents = this.scene.collisionEvents,
				gridCells = this.gridCells
			}, jobHandle);
			return jobHandle;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002689 File Offset: 0x00000889
		public void OnComplete()
		{
		}

		// Token: 0x06000013 RID: 19 RVA: 0x0000268C File Offset: 0x0000088C
		public void Dispose()
		{
			foreach (CollisionBroadphase_HybridSAP.GridCell gridCell in this.gridCells)
			{
				gridCell.Dispose();
			}
			this.gridCells.Dispose();
			this.gridMap.Dispose();
			this.cellCache.Dispose();
			this.shapesToUpdate.Dispose();
			this.collisionPairs.Dispose();
			this.prevCollisionPairs.Dispose();
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002724 File Offset: 0x00000924
		private void CastShape(CollisionBroadphase_HybridSAP.GridCell gridCell, CollisionScene.ShapeData shape, HashSet<ushort> output)
		{
			CollisionBroadphase_HybridSAP.CellRecord cellRecord = default(CollisionBroadphase_HybridSAP.CellRecord);
			cellRecord.isReceiver = shape.isReceiver;
			cellRecord.isCollider = shape.isCollider;
			cellRecord.min = Vector3Int.FloorToInt(shape.bounds.min * 100f);
			cellRecord.max = Vector3Int.CeilToInt(shape.bounds.max * 100f);
			cellRecord.shapeData = shape.ToCollisionShape();
			for (int i = 0; i < gridCell.records.Length; i++)
			{
				CollisionBroadphase_HybridSAP.CellRecord cellRecord2 = gridCell.records[i];
				if (cellRecord.max.x < cellRecord2.min.x)
				{
					break;
				}
				if (cellRecord.min.x <= cellRecord2.max.x && cellRecord.min.y < cellRecord2.max.y && cellRecord.max.y > cellRecord2.min.y && cellRecord.min.z < cellRecord2.max.z && cellRecord.max.z > cellRecord2.min.z)
				{
					bool flag = cellRecord.isReceiver && cellRecord2.isCollider;
					bool flag2 = cellRecord2.isReceiver && cellRecord.isCollider;
					if ((flag || flag2) && CollisionShapes.CheckCollision(cellRecord.shapeData, cellRecord2.shapeData))
					{
						output.Add(cellRecord2.shape);
					}
				}
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000028C0 File Offset: 0x00000AC0
		public void DrawGizmos()
		{
			NativeArray<int> valueArray = this.gridMap.GetValueArray((Allocator)2);
			for (int i = 0; i < valueArray.Length; i++)
			{
				int num = valueArray[i];
				CollisionBroadphase_HybridSAP.GridCell gridCell = this.gridCells[num];
				Vector3 vector = (Vector3)gridCell.position * 5f + Vector3.one * 5f * 0.5f;
				Vector3 vector2 = Vector3.one * 5f;
				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(vector, vector2);
				for (int j = 0; j < gridCell.records.Length; j++)
				{
					CollisionBroadphase_HybridSAP.CellRecord cellRecord = gridCell.records[j];
					Vector3 vector3 = (Vector3)cellRecord.min * 0.01f;
					Vector3 vector4 = (Vector3)cellRecord.max * 0.01f;
					Vector3 vector5 = (vector3 + vector4) * 0.5f;
					vector2 = vector4 - vector3;
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireCube(vector5, vector2);
				}
			}
			valueArray.Dispose();
		}

		// Token: 0x0400000C RID: 12
		private static int SHAPE_BATCH_COUNT = 8;

		// Token: 0x0400000D RID: 13
		private static int CELL_BATCH_COUNT = 2;

		// Token: 0x0400000E RID: 14
		private const float GRID_SIZE = 5f;

		// Token: 0x0400000F RID: 15
		private const float GRID_MULTI = 0.2f;

		// Token: 0x04000010 RID: 16
		private const int GRID_MAP_START_CAPACITY = 128;

		// Token: 0x04000011 RID: 17
		private const int GRID_CELL_MAX_CACHE = 16;

		// Token: 0x04000012 RID: 18
		private const int GRID_CELL_MAX_ACTIVE_LIST = 128;

		// Token: 0x04000013 RID: 19
		private const int MAX_COLLISION_PAIRS = 8192;

		// Token: 0x04000014 RID: 20
		private const int INITIAL_COLLISIONS_PER_CELL = 256;

		// Token: 0x04000015 RID: 21
		private const int MAX_COLLISIONS_PER_CELL = 1024;

		// Token: 0x04000017 RID: 23
		private NativeQueue<ushort> shapesToUpdate = new NativeQueue<ushort>((Allocator)4);

		// Token: 0x04000018 RID: 24
		private NativeParallelHashMap<CollisionBroadphase_HybridSAP.Pair, bool> collisionPairs = new NativeParallelHashMap<CollisionBroadphase_HybridSAP.Pair, bool>(8192, (Allocator)4);

		// Token: 0x04000019 RID: 25
		public NativeList<CollisionBroadphase_HybridSAP.Pair> prevCollisionPairs = new NativeList<CollisionBroadphase_HybridSAP.Pair>(8192, (Allocator)4);

		// Token: 0x0400001A RID: 26
		private NativeHashMap<Vector3Int, int> gridMap = new NativeHashMap<Vector3Int, int>(128, (Allocator)4);

		// Token: 0x0400001B RID: 27
		private NativeList<CollisionBroadphase_HybridSAP.GridCell> gridCells = new NativeList<CollisionBroadphase_HybridSAP.GridCell>(128, (Allocator)4);

		// Token: 0x0400001C RID: 28
		private NativeList<int> cellCache = new NativeList<int>(16, (Allocator)4);

		// Token: 0x0200003F RID: 63
		[BurstCompile]
		public struct CellRecord
		{
			// Token: 0x04000203 RID: 515
			public ushort shape;

			// Token: 0x04000204 RID: 516
			public Vector3Int min;

			// Token: 0x04000205 RID: 517
			public Vector3Int max;

			// Token: 0x04000206 RID: 518
			public bool isCollider;

			// Token: 0x04000207 RID: 519
			public bool isReceiver;

			// Token: 0x04000208 RID: 520
			public CollisionShapes.CollisionShape shapeData;
		}

		// Token: 0x02000040 RID: 64
		[BurstCompile]
		private struct GridCell
		{
			// Token: 0x06000285 RID: 645 RVA: 0x00010CCE File Offset: 0x0000EECE
			public bool IsValid()
			{
				return this.isValid;
			}

			// Token: 0x06000286 RID: 646 RVA: 0x00010CD8 File Offset: 0x0000EED8
			public void Init()
			{
				this.records = new UnsafeList<CollisionBroadphase_HybridSAP.CellRecord>(128, (Allocator)4, 0);
				this.activeList = new UnsafeList<CollisionBroadphase_HybridSAP.CellRecord>(128, (Allocator)4, 0);
				this.collisions = new UnsafeList<CollisionBroadphase_HybridSAP.Pair>(256, (Allocator)4, 0);
				this.isValid = true;
			}

			// Token: 0x06000287 RID: 647 RVA: 0x00010D31 File Offset: 0x0000EF31
			public void Dispose()
			{
				this.position = Vector3Int.zero;
				this.records.Dispose();
				this.activeList.Dispose();
				this.collisions.Dispose();
				this.isValid = false;
			}

			// Token: 0x06000288 RID: 648 RVA: 0x00010D68 File Offset: 0x0000EF68
			public void RemoveShape(ushort id)
			{
				for (int i = 0; i < this.records.Length; i++)
				{
					if (this.records[i].shape == id)
					{
						this.records.RemoveAt(i);
						return;
					}
				}
			}

			// Token: 0x06000289 RID: 649 RVA: 0x00010DAC File Offset: 0x0000EFAC
			public void AddShape(ushort id)
			{
				for (int i = 0; i < this.records.Length; i++)
				{
					if (this.records[i].shape == id)
					{
						Debug.LogError("Shape being added twice!");
					}
				}
				CollisionBroadphase_HybridSAP.CellRecord cellRecord = default(CollisionBroadphase_HybridSAP.CellRecord);
				cellRecord.shape = id;
				this.records.Add(in cellRecord);
			}

			// Token: 0x04000209 RID: 521
			private bool isValid;

			// Token: 0x0400020A RID: 522
			public Vector3Int position;

			// Token: 0x0400020B RID: 523
			public UnsafeList<CollisionBroadphase_HybridSAP.CellRecord> activeList;

			// Token: 0x0400020C RID: 524
			public UnsafeList<CollisionBroadphase_HybridSAP.CellRecord> records;

			// Token: 0x0400020D RID: 525
			public UnsafeList<CollisionBroadphase_HybridSAP.Pair> collisions;
		}

		// Token: 0x02000041 RID: 65
		public struct Pair : IEquatable<CollisionBroadphase_HybridSAP.Pair>
		{
			// Token: 0x0600028A RID: 650 RVA: 0x00010E0C File Offset: 0x0000F00C
			public Pair(ushort shapeA, bool isReceiverA, ushort shapeB, bool isReceiverB)
			{
				if (shapeA < shapeB)
				{
					this.shapeA = shapeA;
					this.isReceiverA = isReceiverA;
					this.shapeB = shapeB;
					this.isReceiverB = isReceiverB;
					return;
				}
				this.shapeA = shapeB;
				this.isReceiverA = isReceiverB;
				this.shapeB = shapeA;
				this.isReceiverB = isReceiverA;
			}

			// Token: 0x0600028B RID: 651 RVA: 0x00010E58 File Offset: 0x0000F058
			[MethodImpl(256)]
			public bool Equals(CollisionBroadphase_HybridSAP.Pair other)
			{
				return this.shapeA == other.shapeA && this.shapeB == other.shapeB;
			}

			// Token: 0x0600028C RID: 652 RVA: 0x00010E78 File Offset: 0x0000F078
			[MethodImpl(256)]
			public override int GetHashCode()
			{
				return this.shapeA.GetHashCode() ^ this.shapeB.GetHashCode() << 2;
			}

			// Token: 0x0400020E RID: 526
			public ushort shapeA;

			// Token: 0x0400020F RID: 527
			public bool isReceiverA;

			// Token: 0x04000210 RID: 528
			public ushort shapeB;

			// Token: 0x04000211 RID: 529
			public bool isReceiverB;
		}

		// Token: 0x02000042 RID: 66
		private struct MoveShapesJob : IJobParallelFor
		{
			// Token: 0x0600028D RID: 653 RVA: 0x00010E94 File Offset: 0x0000F094
			private TransformAccess GetTransform(int id, int index)
			{
				int num = this.transformLookup[id * 2 + index];
				if (num >= 0)
				{
					return this.transformData[num];
				}
				return default(TransformAccess);
			}

			// Token: 0x0600028E RID: 654 RVA: 0x00010ECC File Offset: 0x0000F0CC
			public void Execute(int index)
			{
				ushort num = this.activeShapes[index];
				CollisionScene.ShapeData shapeData = this.shapeData[(int)num];
				TransformAccess transform = this.GetTransform((int)num, 0);
				TransformAccess transform2 = this.GetTransform((int)num, 1);
				shapeData.UpdateShape(transform, transform2);
				shapeData.UpdateVelocity(transform, this.deltaTime);
				Vector3Int vector3Int = Vector3Int.FloorToInt(shapeData.bounds.min * 0.2f);
				Vector3Int vector3Int2 = Vector3Int.FloorToInt(shapeData.bounds.max * 0.2f);
				if (shapeData.isCollider || shapeData.isReceiver)
				{
					if (shapeData.boundsMin != vector3Int || shapeData.boundsMax != vector3Int2)
					{
						shapeData.nextBoundsMin = vector3Int;
						shapeData.nextBoundsMax = vector3Int2;
						this.shapesToUpdate.Enqueue(num);
					}
				}
				else
				{
					shapeData.boundsMin = vector3Int;
					shapeData.boundsMax = vector3Int2;
				}
				this.shapeData[(int)num] = shapeData;
			}

			// Token: 0x0600028F RID: 655 RVA: 0x00010FC3 File Offset: 0x0000F1C3
			private Vector3 Rotate(Quaternion quaternion, Vector3 vector)
			{
				return quaternion * vector;
			}

			// Token: 0x04000212 RID: 530
			private const float VelocityLerpSpeed = 30f;

			// Token: 0x04000213 RID: 531
			[ReadOnly]
			public NativeArray<TransformAccess> transformData;

			// Token: 0x04000214 RID: 532
			[ReadOnly]
			public NativeArray<int> transformLookup;

			// Token: 0x04000215 RID: 533
			[ReadOnly]
			public NativeList<ushort> activeShapes;

			// Token: 0x04000216 RID: 534
			[NativeDisableParallelForRestriction]
			public NativeArray<CollisionScene.ShapeData> shapeData;

			// Token: 0x04000217 RID: 535
			public NativeQueue<ushort>.ParallelWriter shapesToUpdate;

			// Token: 0x04000218 RID: 536
			public float deltaTime;
		}

		// Token: 0x02000043 RID: 67
		private struct UpdateShapesJob : IJob
		{
			// Token: 0x06000290 RID: 656 RVA: 0x00010FCC File Offset: 0x0000F1CC
			public void Execute()
			{
				ushort num;
				while (this.shapesToUpdate.TryDequeue(out num))
				{
					CollisionScene.ShapeData shapeData = this.shapeData[(int)num];
					this.RemoveShape(num, shapeData.boundsMin, shapeData.boundsMax, shapeData.nextBoundsMin, shapeData.nextBoundsMax);
					this.AddShape(num, shapeData.nextBoundsMin, shapeData.nextBoundsMax, shapeData.boundsMin, shapeData.boundsMax);
					shapeData.boundsMin = shapeData.nextBoundsMin;
					shapeData.boundsMax = shapeData.nextBoundsMax;
					this.shapeData[(int)num] = shapeData;
				}
				if (this.cellCache.Length > 16)
				{
					int num2 = 0;
					for (int i = 0; i < this.gridCells.Length; i++)
					{
						CollisionBroadphase_HybridSAP.GridCell gridCell = this.gridCells[i];
						if (gridCell.records.Length > 0)
						{
							if (num2 < i)
							{
								CollisionBroadphase_HybridSAP.GridCell gridCell2 = this.gridCells[num2];
								this.gridCells[num2] = gridCell;
								this.gridCells[i] = gridCell2;
								this.gridMap[gridCell.position] = num2;
							}
							num2++;
						}
					}
					int num3 = num2 + 16;
					for (int j = num3; j < this.gridCells.Length; j++)
					{
						this.gridCells[j].Dispose();
					}
					this.gridCells.Length = num3;
					this.cellCache.Length = 16;
					for (int k = 0; k < this.cellCache.Length; k++)
					{
						this.cellCache[k] = num2 + k;
					}
				}
			}

			// Token: 0x06000291 RID: 657 RVA: 0x00011170 File Offset: 0x0000F370
			private void RemoveShape(ushort id, Vector3Int min, Vector3Int max, Vector3Int exceptMin, Vector3 exceptMax)
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

			// Token: 0x06000292 RID: 658 RVA: 0x00011248 File Offset: 0x0000F448
			private void RemoveShape(ushort id, Vector3Int key)
			{
				int num;
				if (!this.gridMap.TryGetValue(key, out num))
				{
					return;
				}
				CollisionBroadphase_HybridSAP.GridCell gridCell = this.gridCells[num];
				gridCell.RemoveShape(id);
				this.gridCells[num] = gridCell;
				if (gridCell.records.Length == 0)
				{
					this.gridMap.Remove(key);
					this.cellCache.Add(in num);
				}
			}

			// Token: 0x06000293 RID: 659 RVA: 0x000112B0 File Offset: 0x0000F4B0
			private void AddShape(ushort id, Vector3Int min, Vector3Int max, Vector3Int exceptMin, Vector3Int exceptMax)
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

			// Token: 0x06000294 RID: 660 RVA: 0x00011380 File Offset: 0x0000F580
			private void AddShape(ushort id, Vector3Int key)
			{
				int num;
				CollisionBroadphase_HybridSAP.GridCell gridCell;
				if (!this.gridMap.TryGetValue(key, out num))
				{
					if (this.cellCache.Length > 0)
					{
						num = this.cellCache[this.cellCache.Length - 1];
						gridCell = this.gridCells[num];
						this.cellCache.Length = this.cellCache.Length - 1;
					}
					else
					{
						num = this.gridCells.Length;
						gridCell = default(CollisionBroadphase_HybridSAP.GridCell);
						gridCell.Init();
						this.gridCells.Add(in gridCell);
					}
					gridCell.position = key;
					this.gridMap[key] = num;
				}
				else
				{
					gridCell = this.gridCells[num];
				}
				gridCell.AddShape(id);
				this.gridCells[num] = gridCell;
			}

			// Token: 0x04000219 RID: 537
			public NativeQueue<ushort> shapesToUpdate;

			// Token: 0x0400021A RID: 538
			public NativeArray<CollisionScene.ShapeData> shapeData;

			// Token: 0x0400021B RID: 539
			public NativeHashMap<Vector3Int, int> gridMap;

			// Token: 0x0400021C RID: 540
			public NativeList<CollisionBroadphase_HybridSAP.GridCell> gridCells;

			// Token: 0x0400021D RID: 541
			public NativeList<int> cellCache;
		}

		// Token: 0x02000044 RID: 68
		private struct UpdateGridCellsJob : IJobParallelFor
		{
			// Token: 0x06000295 RID: 661 RVA: 0x0001144C File Offset: 0x0000F64C
			public void Execute(int index)
			{
				if (index >= this.gridCells.Length)
				{
					return;
				}
				CollisionBroadphase_HybridSAP.GridCell gridCell = this.gridCells[index];
				if (!gridCell.IsValid())
				{
					return;
				}
				this.UpdateRecords(gridCell);
				this.InsertionSort(gridCell);
				this.FindPairs(ref gridCell);
				this.gridCells[index] = gridCell;
			}

			// Token: 0x06000296 RID: 662 RVA: 0x000114A4 File Offset: 0x0000F6A4
			private void UpdateRecords(CollisionBroadphase_HybridSAP.GridCell cell)
			{
				for (int i = 0; i < cell.records.Length; i++)
				{
					CollisionBroadphase_HybridSAP.CellRecord cellRecord = cell.records[i];
					CollisionScene.ShapeData shapeData = this.shapeData[(int)cellRecord.shape];
					cellRecord.isCollider = shapeData.isCollider;
					cellRecord.isReceiver = shapeData.isReceiver;
					cellRecord.shapeData = shapeData.ToCollisionShape();
					cellRecord.min = Vector3Int.FloorToInt(shapeData.bounds.min * 100f);
					cellRecord.max = Vector3Int.CeilToInt(shapeData.bounds.max * 100f);
					cell.records[i] = cellRecord;
				}
			}

			// Token: 0x06000297 RID: 663 RVA: 0x00011568 File Offset: 0x0000F768
			private void InsertionSort(CollisionBroadphase_HybridSAP.GridCell cell)
			{
				for (int i = 1; i < cell.records.Length; i++)
				{
					CollisionBroadphase_HybridSAP.CellRecord cellRecord = cell.records[i];
					int j;
					for (j = i - 1; j >= 0; j--)
					{
						CollisionBroadphase_HybridSAP.CellRecord cellRecord2 = cell.records[j];
						if (cellRecord.min.x >= cellRecord2.min.x)
						{
							break;
						}
						cell.records[j + 1] = cellRecord2;
					}
					cell.records[j + 1] = cellRecord;
				}
			}

			// Token: 0x06000298 RID: 664 RVA: 0x000115F0 File Offset: 0x0000F7F0
			private void FindPairs(ref CollisionBroadphase_HybridSAP.GridCell cell)
			{
				cell.collisions.Clear();
				cell.activeList.Clear();
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < cell.records.Length; i++)
				{
					CollisionBroadphase_HybridSAP.CellRecord cellRecord = cell.records[i];
					for (int j = 0; j < cell.activeList.Length; j++)
					{
						CollisionBroadphase_HybridSAP.CellRecord cellRecord2 = cell.activeList[j];
						if (cellRecord.min.x > cellRecord2.max.x)
						{
							cell.activeList.RemoveAtSwapBack(j);
							j--;
						}
						else
						{
							num2++;
							if (cellRecord.min.y < cellRecord2.max.y && cellRecord.max.y > cellRecord2.min.y && cellRecord.min.z < cellRecord2.max.z && cellRecord.max.z > cellRecord2.min.z)
							{
								bool flag = cellRecord.isReceiver && cellRecord2.isCollider;
								bool flag2 = cellRecord2.isReceiver && cellRecord.isCollider;
								if ((flag || flag2) && CollisionShapes.CheckCollision(cellRecord.shapeData, cellRecord2.shapeData))
								{
									CollisionBroadphase_HybridSAP.Pair pair = new CollisionBroadphase_HybridSAP.Pair(cellRecord.shape, flag, cellRecord2.shape, flag2);
									if (cell.collisions.Length >= 1024)
									{
										return;
									}
									cell.collisions.Add(in pair);
								}
							}
						}
					}
					if (cell.activeList.Length < 128)
					{
						cell.activeList.AddNoResize(cellRecord);
					}
					else
					{
						num++;
					}
				}
			}

			// Token: 0x0400021E RID: 542
			[NativeDisableParallelForRestriction]
			public NativeList<CollisionBroadphase_HybridSAP.GridCell> gridCells;

			// Token: 0x0400021F RID: 543
			[ReadOnly]
			public NativeArray<CollisionScene.ShapeData> shapeData;

			// Token: 0x04000220 RID: 544
			public NativeParallelHashMap<CollisionBroadphase_HybridSAP.Pair, bool>.ParallelWriter collisionPairs;
		}

		// Token: 0x02000045 RID: 69
		private struct CollisionEventsJob : IJob
		{
			// Token: 0x06000299 RID: 665 RVA: 0x000117B8 File Offset: 0x0000F9B8
			public void Execute()
			{
				int num = 0;
				NativeHashMap<CollisionBroadphase_HybridSAP.Pair, bool> nativeHashMap = new NativeHashMap<CollisionBroadphase_HybridSAP.Pair, bool>(8192, (Allocator)2);
				foreach (CollisionBroadphase_HybridSAP.GridCell gridCell in this.gridCells)
				{
					if (gridCell.IsValid())
					{
						for (int i = 0; i < gridCell.collisions.Length; i++)
						{
							if (num < 8192)
							{
								UnsafeList<CollisionBroadphase_HybridSAP.Pair> collisions = gridCell.collisions;
								if (nativeHashMap.TryAdd(collisions[i], true))
								{
									num++;
								}
							}
						}
						if (num >= 8192)
						{
							break;
						}
					}
				}
				NativeArray<CollisionBroadphase_HybridSAP.Pair> keyArray = nativeHashMap.GetKeyArray((Allocator)2);
				for (int j = 0; j < this.prevCollisionPairs.Length; j++)
				{
					CollisionBroadphase_HybridSAP.Pair pair = this.prevCollisionPairs[j];
					if (nativeHashMap.ContainsKey(pair))
					{
						nativeHashMap.Remove(pair);
					}
					else
					{
						if (pair.isReceiverA)
						{
							this.collisionEvents.Enqueue(new CollisionScene.CollisionEvent
							{
								receiver = (int)pair.shapeA,
								collider = (int)pair.shapeB,
								found = false
							});
						}
						if (pair.isReceiverB)
						{
							this.collisionEvents.Enqueue(new CollisionScene.CollisionEvent
							{
								receiver = (int)pair.shapeB,
								collider = (int)pair.shapeA,
								found = false
							});
						}
					}
				}
				NativeArray<CollisionBroadphase_HybridSAP.Pair> keyArray2 = nativeHashMap.GetKeyArray((Allocator)2);
				for (int k = 0; k < keyArray2.Length; k++)
				{
					CollisionBroadphase_HybridSAP.Pair pair2 = keyArray2[k];
					if (pair2.isReceiverA)
					{
						this.collisionEvents.Enqueue(new CollisionScene.CollisionEvent
						{
							receiver = (int)pair2.shapeA,
							collider = (int)pair2.shapeB,
							found = true
						});
					}
					if (pair2.isReceiverB)
					{
						this.collisionEvents.Enqueue(new CollisionScene.CollisionEvent
						{
							receiver = (int)pair2.shapeB,
							collider = (int)pair2.shapeA,
							found = true
						});
					}
				}
				this.prevCollisionPairs.Clear();
				this.prevCollisionPairs.AddRange(keyArray);
				keyArray2.Dispose();
				nativeHashMap.Dispose();
			}

			// Token: 0x04000221 RID: 545
			public NativeList<CollisionBroadphase_HybridSAP.Pair> prevCollisionPairs;

			// Token: 0x04000222 RID: 546
			public NativeQueue<CollisionScene.CollisionEvent> collisionEvents;

			// Token: 0x04000223 RID: 547
			[ReadOnly]
			public NativeList<CollisionBroadphase_HybridSAP.GridCell> gridCells;
		}
	}
}
