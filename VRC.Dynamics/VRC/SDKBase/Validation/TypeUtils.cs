using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace VRC.SDKBase.Validation
{
	// Token: 0x02000061 RID: 97
	public static class TypeUtils
	{
		// Token: 0x060002A1 RID: 673 RVA: 0x0000AAC8 File Offset: 0x00008CC8
		[PublicAPI]
		public static Type GetTypeFromName(string name, Assembly[] assemblies = null)
		{
			if (TypeUtils._typeCache.ContainsKey(name))
			{
				return TypeUtils._typeCache[name];
			}
			if (assemblies == null)
			{
				assemblies = AppDomain.CurrentDomain.GetAssemblies();
			}
			Assembly[] array = assemblies;
			for (int i = 0; i < array.Length; i++)
			{
				Type type = array[i].GetType(name);
				if (!(type == null))
				{
					TypeUtils._typeCache[name] = type;
					return type;
				}
			}
			TypeUtils._typeCache[name] = null;
			return null;
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0000AB3C File Offset: 0x00008D3C
		public static IEnumerable<Type> FindDerivedTypes(Type baseType)
		{
			List<Type> list = new List<Type>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				list.AddRange(TypeUtils.FindDerivedTypes(assembly, baseType));
			}
			return list;
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x0000AB7C File Offset: 0x00008D7C
		public static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
		{
			return Enumerable.Where<Type>(assembly.GetTypes(), (Type t) => t != baseType && baseType.IsAssignableFrom(t));
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x0000ABB0 File Offset: 0x00008DB0
		public static IEnumerable<T> FindAssemblyAttributes<T>() where T : Attribute
		{
			List<T> list = new List<T>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				list.AddRange(TypeUtils.FindAssemblyAttributes<T>(assembly));
			}
			return list;
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0000ABF0 File Offset: 0x00008DF0
		public static IEnumerable<T> FindAssemblyAttributes<T>(Assembly assembly) where T : Attribute
		{
			IEnumerable<T> result;
			try
			{
				result = (T[])assembly.GetCustomAttributes(typeof(T), false);
			}
			catch
			{
				result = Enumerable.Empty<T>();
			}
			return result;
		}

		// Token: 0x040002F1 RID: 753
		private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();
	}
}
