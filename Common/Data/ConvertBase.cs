using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace bfbd.Common.Data
{
	/// <summary>
	/// 数据转换类核心引擎
	/// </summary>
	/// <remarks>2014-09-30</remarks>
	public partial class ConvertBase
	{
		/// <summary>
		/// 字段分隔符
		/// </summary>
		protected char FieldSeparator = ':';

		/// <summary>
		/// 数组分隔符
		/// </summary>
		protected char ArraySeparator = ',';
	}

	partial class ConvertBase
	{
		#region Base Implement

		/// <summary>
		/// 通用的类型转换函数
		/// </summary>
		protected object ChangeType(object obj, Type type)
		{
			if (obj == null || obj == DBNull.Value)
			{
				return null;
			}
			if (obj is string && string.IsNullOrEmpty(obj as string))
			{
				return null;
			}
			if (type == typeof(object) || type == obj.GetType())
			{
				return obj;
			}

			object result = null;
			try
			{
				type = Nullable.GetUnderlyingType(type) ?? type;
				if (obj is string)
				{
					result = ObjectFromString(obj as string, type);
				}
				else if (type == typeof(string))
				{
					result = ObjectToString(obj);
				}
				else if (type.IsEnum)
				{
					result = Enum.ToObject(type, obj);
				}
				else
				{
					result = System.Convert.ChangeType(obj, type);
				}
			}
			catch (Exception ex) { Logger.Exception(ex); throw; }
			return result;
		}

		#endregion

		#region Inner Implement

		/// <summary>
		/// 从字符串反序列化
		/// </summary>
		private object ObjectFromString(string str, Type type)
		{
			object result = null;
			if (string.IsNullOrEmpty(str))
			{
				result = type.IsValueType ? Activator.CreateInstance(type) : null;
			}
			else if (type.IsEnum)
			{
				// enum parse, ignore case.
				result = type.IgEnumParse(str);
			}
			else if (type.IsArray)
			{
				result = ArrayFromString(str, type);
			}
			else
			{
				var parse = type.GetParseMethod();
				if (parse != null)
				{
					result = parse.Invoke(null, new object[] { str });
				}
				else
				{
					if (type.IsValueType && !type.IsPrimitive)
						result = StructFromString(str, type);
					else
						result = System.Convert.ChangeType(str, type);
				}
			}
			return result;
		}

		/// <summary>
		/// 序列化到字符串
		/// </summary>
		private string ObjectToString(object obj)
		{
			if (obj == null || obj == DBNull.Value)
				return null;
			if (obj is string)
				return obj as string;

			Type type = obj.GetType();
			type = Nullable.GetUnderlyingType(type) ?? type;

			string result = null;
			if (type == typeof(Guid))
				result = ((Guid)obj).ToString("n");
			else if (type == typeof(DateTime))
				result = ((DateTime)obj).ToString();
			else if (type == typeof(Decimal))
				result = ((Decimal)obj).ToString();
			else if (type.IsEnum)
				result = obj.ToString();
			else if (type.IsArray)
				result = ArrayToString(obj);
			else
			{
				var parse = type.GetParseMethod();
				if (parse != null)
					result = obj.ToString();
				else
				{
					if (type.IsValueType && !type.IsPrimitive)
						result = StructToString(obj);
					else
						result = obj.ToString();
				}
			}
			return result;
		}

		private object StructFromString(string str, Type type)
		{
			Contract.Assert(!string.IsNullOrEmpty(str));
			Contract.Assert(type.IsValueType);

			var flds = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (flds.Length > 0)
			{
				object result = System.Activator.CreateInstance(type);
				string[] vals = str.Split(FieldSeparator);
				for (int i = 0; i < flds.Length && i < vals.Length; ++i)
				{
					object v = ObjectFromString(vals[i], flds[i].FieldType);
					flds[i].SetValue(result, v);
				}
				return result;
			}
			else
			{
				return System.Convert.ChangeType(str, type);
			}
		}

		private string StructToString(object obj)
		{
			Contract.Assert(obj != null);
			Contract.Assert(obj.GetType().IsValueType);

			var flds = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (flds.Length > 0)
			{
				StringBuilder sb = new StringBuilder();
				foreach (var fld in flds)
				{
					sb.Append(ObjectToString(fld.GetValue(obj)));
					sb.Append(FieldSeparator);
				}
				return sb.ToString().TrimEnd(FieldSeparator);
			}
			else
			{
				return obj.ToString();
			}
		}

		private object ArrayFromString(string str, Type type)
		{
			Contract.Assert(!string.IsNullOrEmpty(str));
			Contract.Assert(type.IsArray);

			string[] vals = str.Split(new char[] { ArraySeparator });
			var list = new System.Collections.ArrayList(vals.Length);
			var t = type.GetElementType();
			for (int i = 0; i < vals.Length; ++i)
			{
				list.Add(ObjectFromString(vals[i], t));
			}
			return list.ToArray(t);
		}

		private string ArrayToString(object objs)
		{
			Contract.Assert(objs != null);
			Contract.Assert(objs.GetType().IsArray);

			StringBuilder sb = new StringBuilder();
			foreach (var obj in (Array)objs)
			{
				sb.Append(ObjectToString(obj));
				sb.Append(ArraySeparator);
			}
			return sb.ToString().TrimEnd(ArraySeparator);
		}

		#endregion
	}

	#region TypeExtension

	internal static class TypeExtension
	{
		// 获取类型的 Parse(string) 函数
		public static MethodInfo GetParseMethod(this Type type)
		{
			var ms = type.GetMember("Parse", MemberTypes.Method, BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo m in ms)
			{
				var ps = m.GetParameters();
				if (m.ReturnType == type && ps.Length == 1 && ps[0].ParameterType == typeof(string))
					return m;
			}
			return null;
		}

		// 枚举类型的匹配项
		public static object IgEnumParse(this Type type, string str)
		{
			Contract.Assert(type.IsEnum);
			var names = Enum.GetNames(type);
			var idx = names.ToList().FindIndex(s => string.Equals(s, str, StringComparison.OrdinalIgnoreCase));
			if (idx >= 0)
				return Enum.Parse(type, names[idx]);
			else
				return Enum.Parse(type, str);
		}
	}

	#endregion
}

