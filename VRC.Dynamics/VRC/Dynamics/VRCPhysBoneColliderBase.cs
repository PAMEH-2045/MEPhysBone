using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x02000034 RID: 52
	public abstract class VRCPhysBoneColliderBase : MonoBehaviour, PhysBoneManager.IJobSortable
	{
		// Token: 0x06000223 RID: 547 RVA: 0x0000F62C File Offset: 0x0000D82C
		public Transform GetRootTransform()
		{
			if (!(this.rootTransform != null))
			{
				return base.transform;
			}
			return this.rootTransform;
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000224 RID: 548 RVA: 0x0000F649 File Offset: 0x0000D849
		public Vector3 axis
		{
			get
			{
				return this.rotation * Vector3.up;
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000225 RID: 549 RVA: 0x0000F65B File Offset: 0x0000D85B
		// (set) Token: 0x06000226 RID: 550 RVA: 0x0000F663 File Offset: 0x0000D863
		public int ExecutionGroup { get; set; } = -1;

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000227 RID: 551 RVA: 0x0000F66C File Offset: 0x0000D86C
		public Transform SortingBaseTransform
		{
			get
			{
				return this.GetRootTransform();
			}
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0000F674 File Offset: 0x0000D874
		public void GetKnownDependencies(List<PhysBoneManager.IJobSortable> dependencies)
		{
		}

		// Token: 0x06000229 RID: 553 RVA: 0x0000F676 File Offset: 0x0000D876
		private void OnEnable()
		{
			PhysBoneManager.Inst.AddCollider(this);
		}

		// Token: 0x0600022A RID: 554 RVA: 0x0000F683 File Offset: 0x0000D883
		private void OnDisable()
		{
			if (PhysBoneManager.Inst != null)
			{
				PhysBoneManager.Inst.RemoveCollider(this);
			}
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0000F6A0 File Offset: 0x0000D8A0
		public void InitShape()
		{
			if (this.shape == null)
			{
				this.shape = new CollisionScene.Shape();
			}
			this.shape.component = this;
			this.shape.isCollider = true;
			this.shape.transform0 = this.GetRootTransform();
			switch (this.shapeType)
			{
			case VRCPhysBoneColliderBase.ShapeType.Sphere:
				this.shape.shapeType = CollisionScene.ShapeType.Sphere;
				break;
			case VRCPhysBoneColliderBase.ShapeType.Capsule:
				this.shape.shapeType = ((this.height <= this.radius * 2f) ? CollisionScene.ShapeType.Sphere : CollisionScene.ShapeType.Capsule);
				break;
			case VRCPhysBoneColliderBase.ShapeType.Plane:
				this.shape.shapeType = CollisionScene.ShapeType.Plane;
				break;
			default:
				return;
			}
			this.shape.radius = this.radius;
			this.shape.height = this.height;
			this.shape.center = this.position;
			this.shape.axis = this.axis;
			if (!this.isGlobalCollider)
			{
				this.shape.isCollider = false;
				this.shape.isReceiver = false;
			}
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0000F7A8 File Offset: 0x0000D9A8
		public void UpdateShape()
		{
			if (this.shape == null)
			{
				return;
			}
			switch (this.shapeType)
			{
			case VRCPhysBoneColliderBase.ShapeType.Sphere:
				this.shape.shapeType = CollisionScene.ShapeType.Sphere;
				break;
			case VRCPhysBoneColliderBase.ShapeType.Capsule:
				this.shape.shapeType = ((this.height <= this.radius * 2f) ? CollisionScene.ShapeType.Sphere : CollisionScene.ShapeType.Capsule);
				break;
			case VRCPhysBoneColliderBase.ShapeType.Plane:
				this.shape.shapeType = CollisionScene.ShapeType.Plane;
				break;
			default:
				return;
			}
			this.shape.radius = this.radius;
			this.shape.height = this.height;
			this.shape.center = this.position;
			this.shape.axis = this.axis;
			if (this.shape.id != CollisionScene.Shape.NullId)
			{
				PhysBoneManager.Inst.CompleteJob();
				PhysBoneManager.Inst.collision.UpdateShapeData(this.shape);
			}
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0000F88C File Offset: 0x0000DA8C
		public Bounds GetBounds()
		{
			Vector3 lossyScale = base.transform.lossyScale;
			float num = Mathf.Max(Mathf.Max(lossyScale.x, lossyScale.y), lossyScale.z);
			if (this.height <= this.radius * 2f)
			{
				return new Bounds(base.transform.position, Vector3.one * this.radius * 2f * num);
			}
			Vector3 normalized = this.axis.normalized;
			Bounds result = new(base.transform.position - normalized * this.height * 0.5f * num, Vector3.one * this.radius * 2f * num);
			result.Encapsulate(new Bounds(base.transform.position + normalized * this.height * 0.5f * num, Vector3.one * this.radius * 2f * num));
			return result;
		}

		// Token: 0x040001C5 RID: 453
		[Tooltip("Transform where this collider is placed.  If empty, we use this game object's transform.")]
		public Transform rootTransform;

		// Token: 0x040001C6 RID: 454
		[Tooltip("Type of collision shape used by this collider.")]
		public VRCPhysBoneColliderBase.ShapeType shapeType;

		// Token: 0x040001C7 RID: 455
		[Tooltip("When enabled, this collider contains bones inside its bounds.")]
		public bool insideBounds;

		// Token: 0x040001C8 RID: 456
		[Tooltip("Size of the collider extending from its origin.")]
		public float radius = 0.5f;

		// Token: 0x040001C9 RID: 457
		[Tooltip("Height of the capsule along the Y axis.")]
		public float height = 2f;

		// Token: 0x040001CA RID: 458
		[Tooltip("Position offset from the root transform.")]
		public Vector3 position = Vector3.zero;

		// Token: 0x040001CB RID: 459
		[Tooltip("Rotation offset from the root transform.")]
		public Quaternion rotation = Quaternion.identity;

		// Token: 0x040001CC RID: 460
		[Tooltip("When enabled, this collider treats bones as spheres instead of capsules. This may be advantageous in situations where bones are constantly resting on colliders.  It will also be easier for colliders to pass through bones unintentionally.")]
		public bool bonesAsSpheres;

		// Token: 0x040001CD RID: 461
		[NonSerialized]
		public bool isGlobalCollider;

		// Token: 0x040001CE RID: 462
		[NonSerialized]
		public int playerId = -1;

		// Token: 0x040001CF RID: 463
		[NonSerialized]
		public CollisionScene.Shape shape;

		// Token: 0x02000088 RID: 136
		public enum ShapeType
		{
			// Token: 0x04000382 RID: 898
			Sphere,
			// Token: 0x04000383 RID: 899
			Capsule,
			// Token: 0x04000384 RID: 900
			Plane
		}
	}
}
