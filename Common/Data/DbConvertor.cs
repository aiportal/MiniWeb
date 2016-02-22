using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace bfbd.Common.Data
{
	/// <summary>
	/// 数据库对象转换类
	/// </summary>
	/// <remarks>2015/1/20</remarks>
	public class DbConvertor : ConvertBase
	{
		private static DbConvertor _default = new DbConvertor();

		private static readonly BindingFlags PropertyFlags = BindingFlags.Instance | BindingFlags.Public;

		public static T Convert<T>(DataRow row, object obj)
		{
			if (obj == null)
				obj = System.Activator.CreateInstance<T>();
			Contract.Assert(obj is T);

			obj = _default.SetDbProperties(obj, PropertyFlags, s =>
			{
				var col = row.Table.Columns[s];
				return col == null ? null : row[col];
			});
			return (T)obj;
		}

		public static T Convert<T>(DbDataReader reader, object obj)
		{
			if (obj == null)
				obj = System.Activator.CreateInstance<T>();
			Contract.Assert(obj is T);

			obj = _default.SetDbProperties(obj, PropertyFlags, s =>
			{
				int pos = reader.GetOrdinal(s);
				return pos >= 0 ? reader.GetValue(pos) : null;
			});
			return (T)obj;
		}

		protected object SetDbProperties(object obj, BindingFlags flags, Func<string, object> getValue)
		{
			Contract.Assert(obj != null);
			try
			{
				var props = obj.GetType().GetProperties(flags);
				foreach (var p in props)
				{
					if (p.CanWrite)
					{
						var v = getValue(p.ColumnName());
						var t = v == null ? null : ChangeType(v, p.PropertyType);
						p.SetValue(obj, t, null);
					}
				}
			}
			catch (Exception ex) { Logger.Exception(ex); throw; }
			return obj;
		}
	}
}
