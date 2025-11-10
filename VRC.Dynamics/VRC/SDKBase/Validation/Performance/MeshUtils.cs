using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace VRC.SDKBase.Validation.Performance
{
	// Token: 0x02000066 RID: 102
	public static class MeshUtils
	{
		// Token: 0x060002C7 RID: 711 RVA: 0x0000B50C File Offset: 0x0000970C
		[RuntimeInitializeOnLoadMethod]
		private static void GatherPrimitives()
		{
			Array values = Enum.GetValues(typeof(PrimitiveType));
			MeshUtils._primitiveMeshes = new Mesh[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				Mesh mesh = GetPrimitiveMesh((PrimitiveType)values.GetValue(i));
				MeshUtils._primitiveMeshes[i] = mesh;
			}

            // Token: 0x060002CC RID: 716 RVA: 0x0000B684 File Offset: 0x00009884
            static Mesh GetPrimitiveMesh(PrimitiveType primitiveType)
			{
                GameObject gameObject = null;
                Mesh result;
                try
                {
                    gameObject = GameObject.CreatePrimitive(primitiveType);
                    gameObject.SetActive(false);
                    MeshFilter component = gameObject.GetComponent<MeshFilter>();
                    result = ((component != null) ? component.sharedMesh : null);
                }
                finally
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
                return result;
            }
        }

		// Token: 0x060002C8 RID: 712 RVA: 0x0000B564 File Offset: 0x00009764
		public static uint? GetMeshTriangleCount(Mesh sourceMesh)
		{
			if (sourceMesh == null)
			{
				return new uint?(0U);
			}
			uint num = 0U;
			for (int i = 0; i < sourceMesh.subMeshCount; i++)
			{
				MeshTopology topology = sourceMesh.GetSubMesh(i).topology;
				if (topology == null || topology != MeshTopology.Quads)
				{
					num += sourceMesh.GetIndexCount(i) / 3U;
				}
				else
				{
					num += sourceMesh.GetIndexCount(i) / 2U;
				}
			}
			return new uint?(num);
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000B5CC File Offset: 0x000097CC
		[UsedImplicitly]
		public static bool IsPrimitiveMesh(Mesh sharedMesh)
		{
			if (sharedMesh == null)
			{
				return false;
			}
			if (MeshUtils._primitiveMeshes == null)
			{
				Debug.LogError("Can't detect primitive mesh because we haven't gathered primitives yet.");
				return false;
			}
			for (int i = 0; i < MeshUtils._primitiveMeshes.Length; i++)
			{
				if (MeshUtils._primitiveMeshes[i] == sharedMesh)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0000B61C File Offset: 0x0000981C
		[UsedImplicitly]
		public static bool IsSkinnedMesh(Mesh mesh)
		{
			if (mesh.blendShapeCount > 0 || mesh.bindposeCount > 0)
			{
				return true;
			}
			bool result;
			try
			{
				mesh.GetBoneWeights(MeshUtils._boneWeightsBuffer);
				result = (MeshUtils._boneWeightsBuffer.Count > 0);
			}
			finally
			{
				MeshUtils._boneWeightsBuffer.Clear();
			}
			return result;
		}

		

		// Token: 0x0400031C RID: 796
		private static Mesh[] _primitiveMeshes;

		// Token: 0x0400031D RID: 797
		private static List<BoneWeight> _boneWeightsBuffer = new List<BoneWeight>();
	}
}
