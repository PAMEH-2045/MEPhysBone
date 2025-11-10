using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace VRC.Dynamics
{
	// Token: 0x02000005 RID: 5
	public interface ICollisionBroadphase
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600002F RID: 47
		// (set) Token: 0x06000030 RID: 48
		CollisionScene scene { get; set; }

		// Token: 0x06000031 RID: 49
		void AddShape(CollisionScene.ShapeData shape, ushort id);

		// Token: 0x06000032 RID: 50
		void RemoveShape(CollisionScene.ShapeData shape, ushort id);

		// Token: 0x06000033 RID: 51
		void CastShape(CollisionScene.ShapeData shape, HashSet<ushort> output);

		// Token: 0x06000034 RID: 52
		JobHandle ScheduleJobs(float deltaTime, JobHandle jobHandle);

		// Token: 0x06000035 RID: 53
		void OnComplete();

		// Token: 0x06000036 RID: 54
		void DrawGizmos();

		// Token: 0x06000037 RID: 55
		void Dispose();
	}
}
