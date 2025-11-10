using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.Dynamics
{
	// Token: 0x02000022 RID: 34
	[AddComponentMenu("")]
	public class ContactReceiver : ContactBase
	{
		// Token: 0x0600013B RID: 315 RVA: 0x00009DB3 File Offset: 0x00007FB3
		public override bool IsReceiver()
		{
			return true;
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00009DB8 File Offset: 0x00007FB8
		public override void Start()
		{
			base.Start();
			if (this.shape != null)
			{
				CollisionScene.Shape shape = this.shape;
				shape.OnEnter = (Action<CollisionScene.Shape>)Delegate.Combine(shape.OnEnter, new Action<CollisionScene.Shape>(this.OnShapeEnter));
				CollisionScene.Shape shape2 = this.shape;
				shape2.OnExit = (Action<CollisionScene.Shape>)Delegate.Combine(shape2.OnExit, new Action<CollisionScene.Shape>(this.OnShapeExit));
			}
			this.InitCollisionTags();
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00009E27 File Offset: 0x00008027
		public override void OnDisable()
		{
			base.OnDisable();
			this.restoreParamValue = this.paramValue;
			this.SetParameter(0f);
			this.collisionValue = 0f;
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00009E51 File Offset: 0x00008051
		public void OnDisableInCollisionScene()
		{
			this.collisionRecords.Clear();
			this.unvalidatedCollisionRecords.Clear();
			this.UpdateParameter();
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00009E6F File Offset: 0x0000806F
		public void OnReenableInCollisionScene()
		{
			this.SetParameter(this.restoreParamValue);
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00009E80 File Offset: 0x00008080
		private void OnShapeEnter(CollisionScene.Shape shape)
		{
			ContactSender contactSender = shape.component as ContactSender;
			if (contactSender != null && !this.AttemptAddCollision(shape, contactSender))
			{
				ContactReceiver.CollisionRecord collisionRecord = new ContactReceiver.CollisionRecord
				{
					shape = shape,
					trigger = contactSender
				};
				this.unvalidatedCollisionRecords.Add(collisionRecord);
			}
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00009ED2 File Offset: 0x000080D2
		private void OnShapeExit(CollisionScene.Shape shape)
		{
			this.RemoveCollision(shape);
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00009EDC File Offset: 0x000080DC
		private void AddCollision(CollisionScene.Shape shape, ContactBase trigger)
		{
			ContactReceiver.CollisionRecord collisionRecord = default(ContactReceiver.CollisionRecord);
			collisionRecord.shape = shape;
			collisionRecord.trigger = trigger;
			this.collisionRecords.Add(collisionRecord);
			if (this.manager != null && this.receiverId != (int)CollisionScene.Shape.NullId)
			{
				ContactManager.ReceiverData receiverData = this.manager.receiverData[this.receiverId];
				int id = (int)shape.id;
				receiverData.collisions.Add(in id);
				this.manager.receiverData[this.receiverId] = receiverData;
			}
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00009F6C File Offset: 0x0000816C
		private void RemoveCollision(CollisionScene.Shape shape)
		{
			for (int i = 0; i < this.collisionRecords.Count; i++)
			{
				if (this.collisionRecords[i].shape == shape)
				{
					this.collisionRecords.RemoveAt(i);
					break;
				}
			}
			this.RemoveFromManagerData(shape);
			for (int j = 0; j < this.unvalidatedCollisionRecords.Count; j++)
			{
				if (this.unvalidatedCollisionRecords[j].shape == shape)
				{
					this.unvalidatedCollisionRecords.RemoveAt(j);
					return;
				}
			}
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00009FF0 File Offset: 0x000081F0
		private void RemoveFromManagerData(CollisionScene.Shape shape)
		{
			if (this.receiverType == ContactReceiver.ReceiverType.Constant)
			{
				this.UpdateParameter();
			}
			if (this.manager != null && this.receiverId != (int)CollisionScene.Shape.NullId)
			{
				ContactManager.ReceiverData receiverData = this.manager.receiverData[this.receiverId];
				for (int i = 0; i < receiverData.collisions.Length; i++)
				{
					if (receiverData.collisions[i] == (int)shape.id)
					{
						receiverData.collisions.RemoveAt(i);
						break;
					}
				}
				this.manager.receiverData[this.receiverId] = receiverData;
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0000A090 File Offset: 0x00008290
		private bool AttemptAddCollision(CollisionScene.Shape shape, ContactBase other)
		{
			if (!this.ValidateCollider(other))
			{
				return true;
			}
			if (!this.ValidateColliderPermissions(other))
			{
				return false;
			}
			switch (this.receiverType)
			{
			default:
				this.AddCollision(shape, other);
				this.UpdateParameter();
				break;
			case ContactReceiver.ReceiverType.OnEnter:
			{
				ContactManager inst = ContactManager.Inst;
				if (inst == null)
				{
					return false;
				}
				if (inst.CalcRelativeVelocity(this, other).magnitude > this.minVelocity)
				{
					this.AddCollision(shape, other);
					this.UpdateParameter();
					this.hasTriggered = true;
				}
				break;
			}
			case ContactReceiver.ReceiverType.Proximity:
				this.AddCollision(shape, other);
				break;
			}
			return true;
		}

		// Token: 0x06000146 RID: 326 RVA: 0x0000A128 File Offset: 0x00008328
		private bool ValidateCollider(ContactBase collider)
		{
			if (collider == null)
			{
				return false;
			}
			bool flag = this.playerId == collider.playerId;
			if (!this.allowSelf && flag)
			{
				return false;
			}
			if (!this.allowOthers && !flag)
			{
				return false;
			}
			for (int i = 0; i < collider.collisionTags.Count; i++)
			{
				if (this.collisionTags.Contains(collider.collisionTags[i]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0000A19D File Offset: 0x0000839D
		protected virtual bool ValidateColliderPermissions(ContactBase collider)
		{
			return !(collider != null) || ContactBase.OnValidatePlayers == null || ContactBase.OnValidatePlayers.Invoke(this.playerId, collider.playerId);
		}

		// Token: 0x06000148 RID: 328 RVA: 0x0000A1CA File Offset: 0x000083CA
		public bool IsColliding()
		{
			return this.collisionRecords.Count > 0;
		}

		// Token: 0x06000149 RID: 329 RVA: 0x0000A1DA File Offset: 0x000083DA
		public override bool RequiresUpdate()
		{
			return this.receiverType == ContactReceiver.ReceiverType.OnEnter || this.receiverType == ContactReceiver.ReceiverType.Proximity;
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0000A1F0 File Offset: 0x000083F0
		public override void UpdateContact()
		{
			switch (this.receiverType)
			{
			case ContactReceiver.ReceiverType.Constant:
				break;
			case ContactReceiver.ReceiverType.OnEnter:
				if (this.hasTriggered)
				{
					this.hasTriggered = false;
				}
				else
				{
					this.SetParameter(0f);
				}
				this.collisionValue = Mathf.Lerp(this.collisionValue, 0f, 5f * Time.deltaTime);
				return;
			case ContactReceiver.ReceiverType.Proximity:
				if (this.manager != null)
				{
					ContactManager.ReceiverData receiverData = this.manager.receiverData[this.receiverId];
					this.collisionValue = receiverData.collisionValue;
					this.SetParameter(this.collisionValue);
				}
				break;
			default:
				return;
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000A294 File Offset: 0x00008494
		public void RefreshValidatedCollisions()
		{
			for (int i = 0; i < this.unvalidatedCollisionRecords.Count; i++)
			{
				ContactReceiver.CollisionRecord collisionRecord = this.unvalidatedCollisionRecords[i];
				if (this.AttemptAddCollision(collisionRecord.shape, collisionRecord.trigger))
				{
					this.unvalidatedCollisionRecords.RemoveAt(i--);
				}
			}
		}

		// Token: 0x0600014C RID: 332 RVA: 0x0000A2E8 File Offset: 0x000084E8
		public void UpdateParameter()
		{
			if (this.collisionRecords.Count > 0)
			{
				this.SetParameter(1f);
				this.collisionValue = 1f;
				return;
			}
			this.SetParameter(0f);
			this.collisionValue = 0f;
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0000A327 File Offset: 0x00008527
		public virtual void SetParameter(float value)
		{
			this.paramValue = value;
			if (this.paramAccess != null)
			{
				this.paramAccess.floatVal = value;
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x0000A344 File Offset: 0x00008544
		private void InitCollisionTags()
		{
			int num = Mathf.Min(16, this.collisionTags.Count);
			for (int i = 0; i < num; i++)
			{
				this.collisionTagsHash.Add(this.collisionTags[i].GetHashCode());
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000A390 File Offset: 0x00008590
		public bool CheckForMask(IEnumerable<int> mask)
		{
			if (this.collisionTagsHash.Count == 0)
			{
				return false;
			}
			foreach (int num in mask)
			{
				if (this.collisionTagsHash.Contains(num))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040000FA RID: 250
		[Tooltip("Allow this contact to be affected by yourself.")]
		public bool allowSelf = true;

		// Token: 0x040000FB RID: 251
		[Tooltip("Allow this contact to be affected by other people.")]
		public bool allowOthers = true;

		// Token: 0x040000FC RID: 252
		[Tooltip("How the receiver reacts to incomming collisions and sets the parameter value.")]
		public ContactReceiver.ReceiverType receiverType;

		// Token: 0x040000FD RID: 253
		[Tooltip("The parameter updated on the animation controller.  This parameter DOES NOT need to be defined on the avatar descriptor.")]
		public string parameter;

		// Token: 0x040000FE RID: 254
		[Tooltip("Minimum velocity needed from an incoming collider to affect this trigger.")]
		public float minVelocity = 0.05f;

		// Token: 0x040000FF RID: 255
		[NonSerialized]
		public int receiverId = (int)CollisionScene.Shape.NullId;

		// Token: 0x04000100 RID: 256
		private List<ContactReceiver.CollisionRecord> collisionRecords = new List<ContactReceiver.CollisionRecord>();

		// Token: 0x04000101 RID: 257
		private List<ContactReceiver.CollisionRecord> unvalidatedCollisionRecords = new List<ContactReceiver.CollisionRecord>();

		// Token: 0x04000102 RID: 258
		[NonSerialized]
		public float collisionValue;

		// Token: 0x04000103 RID: 259
		private bool hasTriggered;

		// Token: 0x04000104 RID: 260
		[NonSerialized]
		public float paramValue;

		// Token: 0x04000105 RID: 261
		[NonSerialized]
		public float restoreParamValue;

		// Token: 0x04000106 RID: 262
		public IAnimParameterAccess paramAccess;

		// Token: 0x04000107 RID: 263
		internal HashSet<int> collisionTagsHash = new HashSet<int>();

		// Token: 0x02000060 RID: 96
		public enum ReceiverType
		{
			// Token: 0x040002A4 RID: 676
			Constant,
			// Token: 0x040002A5 RID: 677
			OnEnter,
			// Token: 0x040002A6 RID: 678
			Proximity
		}

		// Token: 0x02000061 RID: 97
		private struct CollisionRecord
		{
			// Token: 0x040002A7 RID: 679
			public CollisionScene.Shape shape;

			// Token: 0x040002A8 RID: 680
			public ContactBase trigger;
		}
	}
}
