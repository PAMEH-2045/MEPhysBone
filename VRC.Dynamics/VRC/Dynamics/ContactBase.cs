using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace VRC.Dynamics
{
	// Token: 0x02000020 RID: 32
	public abstract class ContactBase : MonoBehaviour, IParameterSetup
	{
		// Token: 0x06000120 RID: 288 RVA: 0x00009443 File Offset: 0x00007643
		public Transform GetRootTransform()
		{
			if (!(this.rootTransform != null))
			{
				return base.transform;
			}
			return this.rootTransform;
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000121 RID: 289 RVA: 0x00009460 File Offset: 0x00007660
		public Vector3 axis
		{
			get
			{
				return this.rotation * Vector3.up;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000122 RID: 290 RVA: 0x00009472 File Offset: 0x00007672
		public bool IsLocalOnly
		{
			get
			{
				return this.localOnly;
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x0000947A File Offset: 0x0000767A
		public virtual void Start()
		{
			this.Init();
			if (this.hasInit && this.shape != null)
			{
				ContactManager inst = ContactManager.Inst;
				if (inst == null)
				{
					return;
				}
				inst.AddContact(this);
			}
		}

		// Token: 0x06000124 RID: 292 RVA: 0x000094A2 File Offset: 0x000076A2
		public virtual void OnEnable()
		{
			if (this.hasInit && this.shape != null)
			{
				ContactManager inst = ContactManager.Inst;
				if (inst == null)
				{
					return;
				}
				inst.AddContact(this);
			}
		}

		// Token: 0x06000125 RID: 293 RVA: 0x000094C4 File Offset: 0x000076C4
		public virtual void OnDisable()
		{
			if (this.hasInit && this.shape != null)
			{
				ContactManager inst = ContactManager.Inst;
				if (inst == null)
				{
					return;
				}
				inst.RemoveContact(this);
			}
		}

		// Token: 0x06000126 RID: 294 RVA: 0x000094E6 File Offset: 0x000076E6
		public void InitParameters()
		{
			if (this.hasInitParams)
			{
				return;
			}
			this.hasInitParams = true;
			if (ContactBase.OnInitialize != null && !ContactBase.OnInitialize.Invoke(this))
			{
				this.hasInitParams = false;
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00009513 File Offset: 0x00007713
		protected void Init()
		{
			if (this.hasInit || !this.allowInit)
			{
				return;
			}
			this.hasInit = true;
			this.InitParameters();
			if (!this.hasInitParams)
			{
				this.shape = null;
				return;
			}
			this.InitShape(false);
		}

		// Token: 0x06000128 RID: 296
		public abstract bool IsReceiver();

		// Token: 0x06000129 RID: 297 RVA: 0x0000954A File Offset: 0x0000774A
		public virtual bool RequiresUpdate()
		{
			return false;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x0000954D File Offset: 0x0000774D
		public virtual void UpdateContact()
		{
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00009550 File Offset: 0x00007750
		public void InitShape(bool force = false)
		{
			if (this.shape != null && !force)
			{
				return;
			}
			this.shape = new CollisionScene.Shape();
			this.shape.component = this;
			this.shape.transform0 = this.GetRootTransform();
			this.shape.isReceiver = this.IsReceiver();
			this.shape.isCollider = !this.IsReceiver();
			this.shape.axis = this.axis;
			this.shape.center = this.position;
			this.shape.maxSize = 6f;
			ContactBase.ShapeType shapeType = this.shapeType;
			if (shapeType == ContactBase.ShapeType.Sphere)
			{
				this.shape.shapeType = CollisionScene.ShapeType.Sphere;
				this.shape.radius = this.radius;
				return;
			}
			if (shapeType != ContactBase.ShapeType.Capsule)
			{
				return;
			}
			this.shape.shapeType = CollisionScene.ShapeType.Capsule;
			this.shape.radius = this.radius;
			this.shape.height = this.height;
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00009644 File Offset: 0x00007844
		public void UpdateShape()
		{
			if (this.shape == null)
			{
				return;
			}
			ContactBase.ShapeType shapeType = this.shapeType;
			if (shapeType != ContactBase.ShapeType.Sphere)
			{
				if (shapeType == ContactBase.ShapeType.Capsule)
				{
					this.shape.shapeType = CollisionScene.ShapeType.Capsule;
				}
			}
			else
			{
				this.shape.shapeType = CollisionScene.ShapeType.Sphere;
			}
			this.shape.radius = this.radius;
			this.shape.height = this.height;
			this.shape.axis = this.axis;
			this.shape.center = this.position;
			if (this.shape.id != CollisionScene.Shape.NullId)
			{
				ContactManager inst = ContactManager.Inst;
				if (inst == null)
				{
					return;
				}
				inst.collision.UpdateShapeData(this.shape);
			}
		}

		// Token: 0x040000D6 RID: 214
		public const float MAX_SIZE = 6f;

		// Token: 0x040000D7 RID: 215
		public const int MAX_COLLISION_TAGS = 16;

		// Token: 0x040000D8 RID: 216
		public static Func<ContactBase, bool> OnInitialize;

		// Token: 0x040000D9 RID: 217
		[Tooltip("Transform where this contact is placed.  If empty, we use this game object's transform.")]
		public Transform rootTransform;

		// Token: 0x040000DA RID: 218
		[Tooltip("Type of collision shape used by this contact.")]
		public ContactBase.ShapeType shapeType;

		// Token: 0x040000DB RID: 219
		[Tooltip("Size of the collider extending from its origin.")]
		public float radius = 0.5f;

		// Token: 0x040000DC RID: 220
		[Tooltip("Height of the capsule along the chosen axis.")]
		public float height = 2f;

		// Token: 0x040000DD RID: 221
		[Tooltip("Position offset from the root transform.")]
		public Vector3 position = Vector3.zero;

		// Token: 0x040000DE RID: 222
		[Tooltip("Rotation offset from the root transform.")]
		public Quaternion rotation = Quaternion.identity;

		// Token: 0x040000DF RID: 223
		[Tooltip("Limit this contact to only work on the local client.")]
		[NotKeyable]
		public bool localOnly;

		// Token: 0x040000E0 RID: 224
		[Tooltip("List of strings that specify what it can affect/be affected by.  For a successful collision to occur, both the sender and receiver need at least one matching pair of strings.  Collision tags are case sensitive.\n")]
		public List<string> collisionTags = new List<string>();

		// Token: 0x040000E1 RID: 225
		[NonSerialized]
		public ContactManager manager;

		// Token: 0x040000E2 RID: 226
		[NonSerialized]
		public bool allowInit = true;

		// Token: 0x040000E3 RID: 227
		private bool hasInitParams;

		// Token: 0x040000E4 RID: 228
		private bool hasInit;

		// Token: 0x040000E5 RID: 229
		[NonSerialized]
		public CollisionScene.Shape shape;

		// Token: 0x040000E6 RID: 230
		[NonSerialized]
		public int playerId;

		// Token: 0x040000E7 RID: 231
		public static Func<int, int, bool> OnValidatePlayers;

		// Token: 0x0200005C RID: 92
		public enum ShapeType
		{
			// Token: 0x04000294 RID: 660
			Sphere,
			// Token: 0x04000295 RID: 661
			Capsule
		}
	}
}
