using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace VRC.Dynamics.ManagedTypes
{
	// Token: 0x0200003B RID: 59
	public abstract class VRCWorldUpConstraintBase : VRCConstraintBase
	{
		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000268 RID: 616 RVA: 0x0001041F File Offset: 0x0000E61F
		protected override VRCConstraintPositionMode PositionMode
		{
			get
			{
				return VRCConstraintPositionMode.None;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000269 RID: 617 RVA: 0x00010422 File Offset: 0x0000E622
		protected override VRCConstraintScaleMode ScaleMode
		{
			get
			{
				return VRCConstraintScaleMode.None;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600026A RID: 618 RVA: 0x00010425 File Offset: 0x0000E625
		protected virtual bool UsesWorldUpTransform
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600026B RID: 619 RVA: 0x00010428 File Offset: 0x0000E628
		protected internal override void UpdateTypeSpecificJobData(ref VRCConstraintJobData jobData)
		{
			if (this.UsesWorldUpTransform && jobData.WorldUpTransformIndex >= 0)
			{
				VRCConstraintManager.SetBufferedTransform(this, jobData.WorldUpTransformIndex, this.WorldUpTransform);
			}
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0001044D File Offset: 0x0000E64D
		protected internal sealed override bool IsDependentOnTransform(Transform otherTransform)
		{
			return base.IsDependentOnTransform(otherTransform) || (this.UsesWorldUpTransform && this.WorldUpTransform != null && this.WorldUpTransform.IsChildOf(otherTransform));
		}

		// Token: 0x0600026D RID: 621 RVA: 0x00010484 File Offset: 0x0000E684
		protected internal sealed override int RecalculateTransformCount()
		{
			int num = base.RecalculateTransformCount();
			if (this.UsesWorldUpTransform && this.WorldUpTransform != null)
			{
				num++;
			}
			return num;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x000104B3 File Offset: 0x0000E6B3
		internal sealed override void GetTransforms(List<(Transform constraintTransform, bool isTarget)> results)
		{
			base.GetTransforms(results);
			if (this.UsesWorldUpTransform && this.WorldUpTransform != null)
			{
				results.Add(new ValueTuple<Transform, bool>(this.WorldUpTransform, false));
			}
		}

		// Token: 0x0600026F RID: 623 RVA: 0x000104E4 File Offset: 0x0000E6E4
		protected internal sealed override Transform GetManagedWorldUpTransform()
		{
			if (this.UsesWorldUpTransform)
			{
				return this.WorldUpTransform;
			}
			return null;
		}

		// Token: 0x06000270 RID: 624 RVA: 0x000104F8 File Offset: 0x0000E6F8
		protected internal override bool RequiresReallocation(in VRCConstraintJobData jobData, out bool sameGameObjectOnly)
		{
			if (base.RequiresReallocation(jobData, out sameGameObjectOnly))
			{
				return true;
			}
			sameGameObjectOnly = false;
			Transform managedWorldUpTransform = this.GetManagedWorldUpTransform();
			int worldUpTransformIndex = jobData.WorldUpTransformIndex;
			Transform bufferedTransform = VRCConstraintManager.GetBufferedTransform(this, worldUpTransformIndex);
			if (managedWorldUpTransform != bufferedTransform)
			{
				if (managedWorldUpTransform != null && managedWorldUpTransform == null)
				{
					if (worldUpTransformIndex < 0)
					{
						return false;
					}
					this.WorldUpTransform = null;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0001054D File Offset: 0x0000E74D
		protected override void ApplyZeroOffset()
		{
			this.RotationOffset = Vector3.zero;
		}

		// Token: 0x06000272 RID: 626 RVA: 0x0001055A File Offset: 0x0000E75A
		internal override void AcceptOffsetBaker(VRCConstraintOffsetBaker baker)
		{
			baker.Bake(this);
		}

		// Token: 0x06000273 RID: 627 RVA: 0x00010564 File Offset: 0x0000E764
		internal quaternion GenerateForwardLook(Vector3 avgPosition)
		{
			Transform effectiveTargetTransform = base.GetEffectiveTargetTransform();
			Vector3 vector = this.SolveInLocalSpace ? effectiveTargetTransform.localPosition : effectiveTargetTransform.position;
			float3 @float = avgPosition - vector;
			float3 float2 = this.DetermineUpVector(effectiveTargetTransform);
			quaternion quaternion;
			if (!this.ForwardLookShouldApplyIdentity(@float, float2))
			{
				if (math.lengthsq(float2) == 0f || math.lengthsq(math.cross(@float, float2)) == 0f)
				{
					this.ForwardLookHandleZeroUp(@float, float2, out quaternion);
				}
				else
				{
					quaternion = quaternion.LookRotationSafe(@float, float2);
					quaternion = this.ReOrientateForwardLook(quaternion);
				}
			}
			else
			{
				quaternion = quaternion.identity;
			}
			return quaternion;
		}

		// Token: 0x06000274 RID: 628
		protected abstract float3 DetermineUpVector(Transform targetTransform);

		// Token: 0x06000275 RID: 629
		protected abstract bool ForwardLookShouldApplyIdentity(float3 toSource, float3 worldUpVector);

		// Token: 0x06000276 RID: 630
		protected abstract void ForwardLookHandleZeroUp(float3 toSource, float3 worldUpVector, out quaternion look);

		// Token: 0x06000277 RID: 631
		protected abstract quaternion ReOrientateForwardLook(quaternion look);

		// Token: 0x040001F1 RID: 497
		public Vector3 RotationAtRest = Vector3.zero;

		// Token: 0x040001F2 RID: 498
		public Vector3 RotationOffset = Vector3.zero;

		// Token: 0x040001F3 RID: 499
		public Transform WorldUpTransform;
	}
}
