using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Diagnostics.Contracts;

namespace bfbd.Common.Data
{
	/// <summary>
	/// 通用对象转换类
	/// </summary>
	/// <remarks>2015/1/18</remarks>
	public partial class DataConvertor : ConvertBase
	{
		private static DataConvertor _default = new DataConvertor();

		public static string Convert(object obj)
		{
			return (obj == null) ? null : _default.ChangeType(obj, typeof(string)) as string;
		}

		public static T Convert<T>(object obj)
		{
			obj = _default.ChangeType(obj, typeof(T));
			return (obj == null) ? default(T) : (T)obj;
		}

		public static object Convert(object obj, Type type)
		{
			return _default.ChangeType(obj, type);
		}

		public static object Convert(NameValueCollection coll, Type type, object obj = null)
		{
			if (obj == null)
				obj = System.Activator.CreateInstance(type);
			Contract.Assert(obj.GetType() == type);

			var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
			foreach (var p in props)
			{
				Contract.Assert(p.CanWrite);
				var v = coll[p.Name];
				var t = v == null ? null : _default.ChangeType(v, p.PropertyType);
				p.SetValue(obj, t, null);
			}
			return obj;
		}

		public static object Convert(Dictionary<string, object> dic, Type type, object obj = null)
		{
			if (obj == null)
				obj = System.Activator.CreateInstance(type);
			Contract.Assert(obj.GetType() == type);

			var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
			foreach (var p in props)
			{
				Contract.Assert(p.CanWrite);
				var v = dic.ContainsKey(p.Name) ? dic[p.Name] : null;
				var t = v == null ? null : _default.ChangeType(v, p.PropertyType);
				p.SetValue(obj, t, null);
			}
			return obj;
		}
	}
}
