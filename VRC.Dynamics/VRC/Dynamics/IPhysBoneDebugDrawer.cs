using System;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x02000030 RID: 48
	public interface IPhysBoneDebugDrawer
	{
		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060001A4 RID: 420
		// (set) Token: 0x060001A5 RID: 421
		float Alpha { get; set; }

		// Token: 0x060001A6 RID: 422
		void Line(Vector3 pos1, Vector3 pos2, float radius, Color color);

		// Token: 0x060001A7 RID: 423
		void Sphere(Vector3 pos, float radius, Color color);
	}
}
