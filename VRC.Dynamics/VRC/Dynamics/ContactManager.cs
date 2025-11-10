using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace VRC.Dynamics
{
	// Token: 0x02000021 RID: 33
	public class ContactManager : MonoBehaviour
	{
		// Token: 0x0600012E RID: 302 RVA: 0x00009748 File Offset: 0x00007948
		public void AddContact(ContactBase contact)
		{
			contact.manager = this;
			this.contacts.Add(contact);
			if (contact.RequiresUpdate())
			{
				this.updateList.Add(contact);
			}
			ContactReceiver contactReceiver = null;
			if (contact.shape != null && this.collision.AddShape(contact.shape))
			{
				ContactReceiver contactReceiver2 = contact as ContactReceiver;
				if (contactReceiver2 != null)
				{
					contactReceiver = contactReceiver2;
				}
			}
			ContactReceiver contactReceiver3 = contact as ContactReceiver;
			if (contactReceiver3 != null && contactReceiver3.receiverType == ContactReceiver.ReceiverType.Proximity)
			{
				VRCAvatarDynamicsScheduler.FinalizeJob();
				ushort num = this.FindAvailableId();
				contactReceiver3.receiverId = (int)num;
				if (num != CollisionScene.Shape.NullId)
				{
					ContactManager.ReceiverData receiverData = default(ContactManager.ReceiverData);
					receiverData.Init(contactReceiver3);
					this.receiverData[(int)num] = receiverData;
					int num2 = (int)num;
					this.receivesToUpdate.Add(in num2);
					this.needsShapeID.Add(contact as ContactReceiver);
				}
			}
			if (contactReceiver != null)
			{
				contactReceiver.OnReenableInCollisionScene();
			}
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00009828 File Offset: 0x00007A28
		public void RemoveContact(ContactBase contact)
		{
			contact.manager = null;
			this.contacts.Remove(contact);
			this.updateList.Remove(contact);
			if (contact.shape != null)
			{
				this.collision.RemoveShape(contact.shape);
			}
			ContactReceiver contactReceiver = contact as ContactReceiver;
			if (contactReceiver != null && contactReceiver.receiverId != (int)CollisionScene.Shape.NullId)
			{
				VRCAvatarDynamicsScheduler.FinalizeJob();
				for (int i = 0; i < this.receivesToUpdate.Length; i++)
				{
					if (this.receivesToUpdate[i] == contactReceiver.receiverId)
					{
						this.receivesToUpdate.RemoveAtSwapBack(i);
						break;
					}
				}
				ContactManager.ReceiverData receiverData = this.receiverData[contactReceiver.receiverId];
				receiverData.Dispose();
				this.receiverData[contactReceiver.receiverId] = receiverData;
			}
		}

		// Token: 0x06000130 RID: 304 RVA: 0x000098F4 File Offset: 0x00007AF4
		public IReadOnlyCollection<ContactBase> GetContacts()
		{
			return this.contacts.AsReadOnly();
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00009904 File Offset: 0x00007B04
		public Vector3 CalcRelativeVelocity(ContactBase sourceA, ContactBase sourceB)
		{
			CollisionScene.ShapeData shapeData = this.collision.GetShapeData(sourceA.shape);
			CollisionScene.ShapeData shapeData2 = this.collision.GetShapeData(sourceB.shape);
			float3 point = (shapeData.GetClosestPoint(sourceB.transform.position) + shapeData2.GetClosestPoint(sourceA.transform.position)) * 0.5f;
			return shapeData.CalcVelocityAtPoint(point) + shapeData2.CalcVelocityAtPoint(point);
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00009990 File Offset: 0x00007B90
		public void Awake()
		{
			ContactManager.Inst = this;
			this.collision = new CollisionScene();
			this.receivesToUpdate = new NativeList<int>((Allocator)4);
			this.receiverData = new NativeArray<ContactManager.ReceiverData>(8192, (Allocator)4, (NativeArrayOptions)1);
			VRCAvatarDynamicsScheduler.OnFrameComplete += new Action(this.HandleDynamicsFrameComplete);
		}

		// Token: 0x06000133 RID: 307 RVA: 0x000099E4 File Offset: 0x00007BE4
		private void OnDestroy()
		{
			VRCAvatarDynamicsScheduler.OnFrameComplete -= new Action(this.HandleDynamicsFrameComplete);
			for (int i = this.contacts.Count - 1; i >= 0; i--)
			{
				this.RemoveContact(this.contacts[i]);
			}
			this.collision.Dispose();
			foreach (ContactManager.ReceiverData receiverData in this.receiverData)
			{
				receiverData.Dispose();
			}
			this.receiverData.Dispose();
			this.receivesToUpdate.Dispose();
			if (ContactManager.Inst == this)
			{
				ContactManager.Inst = null;
			}
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00009AA8 File Offset: 0x00007CA8
		private void LateUpdate()
		{
			if (this._jobState != ContactManager.JobState.Idle)
			{
				return;
			}
			this._timer += Time.deltaTime;
			if (this._timer >= 0.016666666f)
			{
				this._jobState = ContactManager.JobState.PendingSchedule;
				this._timer = 0f;
			}
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00009AF4 File Offset: 0x00007CF4
		public JobHandle ScheduleUpdateReceiversJob(JobHandle dependsOn = default(JobHandle))
		{
			if (this._jobState != ContactManager.JobState.PendingSchedule)
			{
				return dependsOn;
			}
			this._stopwatch = Stopwatch.StartNew();
			using (ContactManager.Marker_CollisionScene.Auto())
			{
				dependsOn = this.collision.UpdateAndSchedule(0.016666666f, false, dependsOn);
			}
			using (ContactManager.Marker_CopyShapeIds.Auto())
			{
				foreach (ContactReceiver contactReceiver in this.needsShapeID)
				{
					ContactManager.ReceiverData receiverData = this.receiverData[contactReceiver.receiverId];
					receiverData.shapeId = (int)contactReceiver.shape.id;
					this.receiverData[contactReceiver.receiverId] = receiverData;
				}
				this.needsShapeID.Clear();
			}
			this._jobState = ContactManager.JobState.PendingFinalize;
			return IJobParallelForExtensions.Schedule<ContactManager.UpdateReceivers>(new ContactManager.UpdateReceivers
			{
				deltaTime = Time.deltaTime,
				activeReceivers = this.receivesToUpdate,
				receivers = this.receiverData,
				shapes = this.collision.shapeData
			}, this.receivesToUpdate.Length, ContactManager.THREAD_BATCH_SIZE, dependsOn);
		}

		// Token: 0x06000136 RID: 310 RVA: 0x00009C64 File Offset: 0x00007E64
		private void HandleDynamicsFrameComplete()
		{
			if (this._jobState != ContactManager.JobState.PendingFinalize)
			{
				return;
			}
			using (ContactManager.Marker_Trigger.Auto())
			{
				for (int i = 0; i < this.updateList.Count; i++)
				{
					this.updateList[i].UpdateContact();
				}
			}
			this._stopwatch.Stop();
			this.performanceTimeMs = (float)this._stopwatch.ElapsedMilliseconds;
			this._jobState = ContactManager.JobState.Idle;
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00009CF4 File Offset: 0x00007EF4
		public void OnDrawGizmos()
		{
			if (this.drawGizmos)
			{
				this.collision.DrawGizmos();
			}
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00009D0C File Offset: 0x00007F0C
		private ushort FindAvailableId()
		{
			ushort num = 0;
			while ((int)num < this.receiverData.Length)
			{
				if (!this.receiverData[(int)num].isValid)
				{
					return num;
				}
				num += 1;
			}
			return CollisionScene.Shape.NullId;
		}

		// Token: 0x040000E8 RID: 232
		public static ContactManager Inst;

		// Token: 0x040000E9 RID: 233
		public CollisionScene collision;

		// Token: 0x040000EA RID: 234
		protected List<ContactBase> contacts = new List<ContactBase>();

		// Token: 0x040000EB RID: 235
		protected List<ContactBase> updateList = new List<ContactBase>();

		// Token: 0x040000EC RID: 236
		private static readonly ProfilerMarker Marker_CollisionScene = new ProfilerMarker("Contact.CollisionScene");

		// Token: 0x040000ED RID: 237
		private static readonly ProfilerMarker Marker_CopyShapeIds = new ProfilerMarker("Contact.CopyShapeIds");

		// Token: 0x040000EE RID: 238
		private static readonly ProfilerMarker Marker_Trigger = new ProfilerMarker("Contact.Trigger");

		// Token: 0x040000EF RID: 239
		private float _timer;

		// Token: 0x040000F0 RID: 240
		private const float FRAME_TIME = 0.016666666f;

		// Token: 0x040000F1 RID: 241
		public float performanceTimeMs;

		// Token: 0x040000F2 RID: 242
		private Stopwatch _stopwatch;

		// Token: 0x040000F3 RID: 243
		private ContactManager.JobState _jobState;

		// Token: 0x040000F4 RID: 244
		public bool drawGizmos;

		// Token: 0x040000F5 RID: 245
		private static readonly int MAX_COLLISION_RECORDS = 128;

		// Token: 0x040000F6 RID: 246
		private static readonly int THREAD_BATCH_SIZE = 32;

		// Token: 0x040000F7 RID: 247
		private NativeList<int> receivesToUpdate;

		// Token: 0x040000F8 RID: 248
		public NativeArray<ContactManager.ReceiverData> receiverData;

		// Token: 0x040000F9 RID: 249
		private List<ContactReceiver> needsShapeID = new List<ContactReceiver>();

		// Token: 0x0200005D RID: 93
		private enum JobState
		{
			// Token: 0x04000297 RID: 663
			Idle,
			// Token: 0x04000298 RID: 664
			PendingSchedule,
			// Token: 0x04000299 RID: 665
			PendingFinalize
		}

		// Token: 0x0200005E RID: 94
		public struct ReceiverData
		{
			// Token: 0x060002BD RID: 701 RVA: 0x00012585 File Offset: 0x00010785
			public void Init(ContactReceiver receiver)
			{
				this.receiverType = receiver.receiverType;
				this.collisions = new UnsafeList<int>(ContactManager.MAX_COLLISION_RECORDS, (Allocator)4, 0);
				this.isValid = true;
			}

			// Token: 0x060002BE RID: 702 RVA: 0x000125B1 File Offset: 0x000107B1
			public void Dispose()
			{
				this.isValid = false;
				this.shapeId = (int)CollisionScene.Shape.NullId;
				this.collisions.Dispose();
			}

			// Token: 0x0400029A RID: 666
			public bool isValid;

			// Token: 0x0400029B RID: 667
			public int shapeId;

			// Token: 0x0400029C RID: 668
			public ContactReceiver.ReceiverType receiverType;

			// Token: 0x0400029D RID: 669
			public UnsafeList<int> collisions;

			// Token: 0x0400029E RID: 670
			public float collisionValue;
		}

		// Token: 0x0200005F RID: 95
		private struct UpdateReceivers : IJobParallelFor
		{
			// Token: 0x060002BF RID: 703 RVA: 0x000125D0 File Offset: 0x000107D0
			public void Execute(int index)
			{
				int num = this.activeReceivers[index];
				ContactManager.ReceiverData receiverData = this.receivers[num];
				switch (receiverData.receiverType)
				{
				case ContactReceiver.ReceiverType.OnEnter:
					receiverData.collisionValue = Mathf.Lerp(receiverData.collisionValue, 0f, 5f * this.deltaTime);
					break;
				case ContactReceiver.ReceiverType.Proximity:
				{
					float num2 = 1f;
					for (int i = 0; i < receiverData.collisions.Length; i++)
					{
						num2 = math.min(num2, this.CalcProximity(receiverData.shapeId, receiverData.collisions[i]));
					}
					receiverData.collisionValue = 1f - num2;
					break;
				}
				}
				this.receivers[num] = receiverData;
			}

			// Token: 0x060002C0 RID: 704 RVA: 0x00012694 File Offset: 0x00010894
			private float CalcProximity(int shapeA, int shapeB)
			{
				CollisionScene.ShapeData shapeData = this.shapes[shapeA];
				CollisionScene.ShapeData shapeData2 = this.shapes[shapeB];
				CollisionScene.ShapeType shapeType = shapeData.shapeType;
				if (shapeType != CollisionScene.ShapeType.Sphere)
				{
					if (shapeType - CollisionScene.ShapeType.Capsule <= 1)
					{
						CollisionScene.ShapeType shapeType2 = shapeData2.shapeType;
						if (shapeType2 == CollisionScene.ShapeType.Sphere)
						{
							Vector3 vector = MathUtil.ClosestPointOnLineSegment(shapeData.outPos0, shapeData.outPos1, shapeData2.GetMidpoint());
							return ((Vector3)shapeData2.GetClosestPoint(vector) - vector).magnitude / shapeData.outRadius;
						}
						if (shapeType2 - CollisionScene.ShapeType.Capsule <= 1)
						{
							float3 @float;
							float3 float2;
							MathUtil.ClosestPointsBetweenLineSegments(shapeData.outPos0, shapeData.outPos1, shapeData2.outPos1, shapeData2.outPos0, out @float, out float2);
							float3 float3 = @float - float2;
							float num = math.length(float3);
							float2 += float3 / num * Mathf.Min(num, shapeData2.outRadius);
							return math.length(float2 - @float) / shapeData.outRadius;
						}
					}
					return 0f;
				}
				Vector3 vector2 = shapeData.GetMidpoint();
				return ((Vector3)shapeData2.GetClosestPoint(vector2) - vector2).magnitude / shapeData.outRadius;
			}

			// Token: 0x0400029F RID: 671
			[ReadOnly]
			public NativeList<int> activeReceivers;

			// Token: 0x040002A0 RID: 672
			[NativeDisableParallelForRestriction]
			public NativeArray<ContactManager.ReceiverData> receivers;

			// Token: 0x040002A1 RID: 673
			[ReadOnly]
			public NativeArray<CollisionScene.ShapeData> shapes;

			// Token: 0x040002A2 RID: 674
			public float deltaTime;
		}
	}
}
