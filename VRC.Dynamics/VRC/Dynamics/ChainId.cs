using System;

namespace VRC.Dynamics
{
	// Token: 0x02000024 RID: 36
	public struct ChainId : IEquatable<ChainId>
	{
		// Token: 0x06000153 RID: 339 RVA: 0x0000A45B File Offset: 0x0000865B
		public ChainId(ulong a, ulong b)
		{
			this.a = a;
			this.b = b;
		}

		// Token: 0x06000154 RID: 340 RVA: 0x0000A46B File Offset: 0x0000866B
		public bool Equals(ChainId other)
		{
			return this == other;
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0000A479 File Offset: 0x00008679
		public override bool Equals(object obj)
		{
			return obj != null && !(obj.GetType() != typeof(ChainId)) && this == (ChainId)obj;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x0000A4A8 File Offset: 0x000086A8
		public override int GetHashCode()
		{
			return HashCode.Combine<ulong, ulong>(this.a, this.b);
		}

		// Token: 0x06000157 RID: 343 RVA: 0x0000A4BB File Offset: 0x000086BB
		public static bool operator ==(ChainId left, ChainId right)
		{
			return left.a == right.a && left.b == right.b;
		}

		// Token: 0x06000158 RID: 344 RVA: 0x0000A4DB File Offset: 0x000086DB
		public static bool operator !=(ChainId left, ChainId right)
		{
			return left.a != right.a || left.b != right.b;
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000159 RID: 345 RVA: 0x0000A4FE File Offset: 0x000086FE
		public ulong A
		{
			get
			{
				return this.a;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600015A RID: 346 RVA: 0x0000A506 File Offset: 0x00008706
		public ulong B
		{
			get
			{
				return this.b;
			}
		}

		// Token: 0x04000108 RID: 264
		public static readonly ChainId Null;

		// Token: 0x04000109 RID: 265
		private ulong a;

		// Token: 0x0400010A RID: 266
		private ulong b;

		// Token: 0x02000062 RID: 98
		public enum Type : byte
		{
			// Token: 0x040002AA RID: 682
			LocalPreview,
			// Token: 0x040002AB RID: 683
			Avatar,
			// Token: 0x040002AC RID: 684
			World,
			// Token: 0x040002AD RID: 685
			Prop
		}
	}
}
