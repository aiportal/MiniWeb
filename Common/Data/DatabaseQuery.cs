using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace bfbd.Common.Data
{
	/// <summary>
	/// 数据库查询接口
	/// </summary>
	/// <remarks>2015/1/20</remarks>
	public partial class Database
	{
		#region IsExists

		private static readonly string[] NullColumn = new string[] { "NULL as Column0" };

		/// <summary>
		/// 符合条件的记录是否存在
		/// </summary>
		/// <typeparam name="T">筛选对象类型</typeparam>
		/// <param name="tableName">表名称</param>
		/// <param name="condition">筛选对象</param>
		/// <returns>是否存在</returns>
		public bool IsExists<T>(string tableName, T condition)
		{
			string filter = MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, NullColumn);
			var ps = GetParameters(condition).ToArray();
			using (var dr = base.InvokeReadQuery(sql, ps))
			{
				return dr.HasRows;
			}
		}

		/// <summary>
		/// 符合条件的记录是否存在
		/// </summary>
		/// <param name="tableName">表名称</param>
		/// <param name="condition">筛选对象</param>
		/// <param name="filterColumns">筛选对象中用到的列</param>
		/// <returns>是否存在</returns>
		public bool IsExists(string tableName, object condition, params string[] filterColumns)
		{
			string filter = MakeFilter(condition, filterColumns);
			string sql = MakeSelect(tableName, filter, NullColumn);
			var ps = GetParameters(condition, filterColumns).ToArray();
			using (var dr = base.InvokeReadQuery(sql, ps))
			{
				return dr.HasRows;
			}
		}

		public bool IsExists(string tableName, string condition)
		{
			string sql = MakeSelect(tableName, condition, NullColumn);
			using (var dr = base.InvokeReadQuery(sql, null))
			{
				return dr.HasRows;
			}
		}

		#endregion
	}

	partial class Database
	{
		#region SelectRow

		public DataRow SelectRow<T>(string tableName, T condition, params string[] columns)
		{
			string filter = base.MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, columns);
			var ps = base.GetParameters(condition).ToArray();
			DataTable dt = InvokeTableQuery(sql, ps);
			return (dt.Rows.Count > 0) ? dt.Rows[0] : null;
		}

		public DataRow SelectRow(string tableName, string condition, params string[] columns)
		{
			string sql = MakeSelect(tableName, condition, columns);
			DataTable dt = InvokeTableQuery(sql, null);
			return (dt.Rows.Count > 0) ? dt.Rows[0] : null;
		}

		#endregion

		#region SelectTable

		public DataTable SelectTable<T>(string tableName, T condition, params string[] columns)
		{
			string filter = base.MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, columns);
			var ps = base.GetParameters(condition).ToArray();
			return InvokeTableQuery(sql, ps);
		}

		public DataTable SelectTable(string tableName, string condition, params string[] columns)
		{
			string sql = MakeSelect(tableName, condition, columns);
			return InvokeTableQuery(sql, null);
		}

		/// <summary>
		/// 选择一个数据表
		/// </summary>
		/// <typeparam name="T">数据表</typeparam>
		/// <param name="tableName">数据表名称</param>
		/// <param name="condition">条件对象</param>
		/// <param name="group">从1开头的分组列数量</param>
		/// <param name="columns">选择列</param>
		/// <returns></returns>
		public DataTable SelectTable<T>(string tableName, T condition, int group, params string[] columns)
		{
			string filter = base.MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, columns, group, null);
			var ps = base.GetParameters(condition).ToArray();
			return InvokeTableQuery(sql, ps);
		}

		public DataTable SelectTable(string tableName, string condition, int group, params string[] columns)
		{
			string sql = MakeSelect(tableName, condition, columns, group, null);
			return InvokeTableQuery(sql, null);
		}

		#endregion
	}

	partial class Database
	{
		#region SelectSingle

		/// <summary>
		/// 选择单个值
		/// </summary>
		/// <typeparam name="T">返回的值类型</typeparam>
		/// <typeparam name="T">筛选对象类型</typeparam>
		/// <param name="tableName">表名称</param>
		/// <param name="columnName">列名称</param>
		/// <param name="condition">条件对象</param>
		/// <param name="order">排序条件</param>
		/// <returns>值</returns>
		public T SelectSingle<T>(string tableName, string columnName, object condition, string orderby = null)
		{
			Contract.Assert(!(condition is StringBuilder));

			string filter = base.MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, new string[] { columnName }, 0, orderby);
			var ps = base.GetParameters(condition).ToArray();
			var val = InvokeSingleQuery(sql, ps);
			return DataConvertor.Convert<T>(val);
		}

		public T SelectSingle<T>(string tableName, string columnName, string condition)
		{
			string sql = MakeSelect(tableName, condition, new string[] { columnName });
			var val = InvokeSingleQuery(sql, null);
			return DataConvertor.Convert<T>(val);
		}

		#endregion

		#region SelectTuple

		public Tuple<T1, T2> SelectTuple<T1, T2>(string tableName, object condition, string column1, string column2)
		{
			string filter = base.MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, new string[] { column1, column2 });
			using (var dr = InvokeReadQuery(sql, null))
			{
				if (dr.Read())
				{
					return new Tuple<T1, T2>(
						   DataConvertor.Convert<T1>(dr[0]),
						   DataConvertor.Convert<T2>(dr[1])
					   );
				}
			}
			return null;
		}

		public Tuple<T1, T2, T3> SelectTuple<T1, T2, T3>(string tableName, string condition, string column1, string column2, string column3)
		{
			string filter = base.MakeFilter(condition);
			string sql = MakeSelect(tableName, condition, new string[] { column1, column2, column3 });
			using (var dr = InvokeReadQuery(sql, null))
			{
				if (dr.Read())
				{
					return new Tuple<T1, T2, T3>(
						   DataConvertor.Convert<T1>(dr[0]),
						   DataConvertor.Convert<T2>(dr[1]),
						   DataConvertor.Convert<T3>(dr[2])
					   );
				}
			}
			return null;
		}

		#endregion

		#region SelectObject

		/// <summary>
		/// 选择单个对象
		/// </summary>
		/// <typeparam name="T">返回对象类型</typeparam>
		/// <typeparam name="T">条件对象类型</typeparam>
		/// <param name="tableName">表名称</param>
		/// <param name="condition">条件对象</param>
		/// <param name="columns">选择的列名称</param>
		/// <returns>对象</returns>
		public T SelectObject<T>(string tableName, object condition, params string[] columns)
		{
			string filter = base.MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, columns);
			var ps = base.GetParameters(condition).ToArray();
			using (DataTable dt = InvokeTableQuery(sql, ps))
			{
				return (dt.Rows.Count > 0) ? DbConvertor.Convert<T>(dt.Rows[0], null) : default(T);
			}
		}

		public T SelectObject<T>(string tableName, string condition, params string[] columns)
		{
			string sql = MakeSelect(tableName, condition, columns);
			using (DataTable dt = InvokeTableQuery(sql, null))
			{
				return (dt.Rows.Count > 0) ? DbConvertor.Convert<T>(dt.Rows[0], null) : default(T);
			}
		}

		#endregion

		#region SelectValues

		/// <summary>
		/// 选择一列数值
		/// </summary>
		/// <typeparam name="T">数值类型</typeparam>
		/// <param name="tableName">表名称</param>
		/// <param name="columnName">列名称</param>
		/// <param name="condition">条件对象</param>
		/// <returns>数值序列</returns>
		public IEnumerable<T> SelectValues<T>(string tableName, string columnName, object condition, string orderby = null)
		{
			string filter = base.MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, new string[] { columnName }, 0, orderby);
			var ps = base.GetParameters(condition).ToArray();
			using (DbDataReader dr = InvokeReadQuery(sql, ps))
			{
				while (dr.Read())
					yield return DataConvertor.Convert<T>(dr[0]);
			}
		}

		public IEnumerable<T> SelectValues<T>(string tableName, string columnName, string condition, string orderby = null)
		{
			string sql = MakeSelect(tableName, condition, new string[] { columnName }, 0, orderby);
			using (DbDataReader dr = InvokeReadQuery(sql, null))
			{
				while (dr.Read())
					yield return DataConvertor.Convert<T>(dr[0]);
			}
		}

		#endregion

		#region SelectDictionary

		/// <summary>
		/// 选择一个字典
		/// </summary>
		/// <typeparam name="K">关键字类型</typeparam>
		/// <typeparam name="V">数值类型</typeparam>
		/// <typeparam name="T">条件类型</typeparam>
		/// <param name="tableName">数据表</param>
		/// <param name="keyColumn">关键字列</param>
		/// <param name="valueColumn">数值列</param>
		/// <param name="condition">条件对象</param>
		/// <returns>字典</returns>
		public Dictionary<K, V> SelectDictionary<K, V>(string tableName, string keyColumn, string valueColumn, object condition, string orderby = null)
		{
			string filter = base.MakeFilter(condition);
			var cols = new string[] { keyColumn, valueColumn };
			string sql = MakeSelect(tableName, filter, cols, 0, orderby);
			var ps = base.GetParameters(condition).ToArray();

			var dic = new Dictionary<K, V>();
			using (DbDataReader dr = InvokeReadQuery(sql, ps))
			{
				while (dr.Read())
				{
					var key = DataConvertor.Convert<K>(dr[0]);
					dic[key] = DataConvertor.Convert<V>(dr[1]);
				}
			}
			return dic;
		}

		public Dictionary<K, V> SelectDictionary<K, V>(string tableName, string keyColumn, string valueColumn, string condition, string orderby = null)
		{
			var cols = new string[] { keyColumn, valueColumn };
			string sql = MakeSelect(tableName, condition, cols, 0, orderby);
			var dic = new Dictionary<K, V>();
			using (DbDataReader dr = InvokeReadQuery(sql, null))
			{
				while (dr.Read())
				{
					var key = DataConvertor.Convert<K>(dr[0]);
					dic[key] = DataConvertor.Convert<V>(dr[1]);
				}
			}
			return dic;
		}

		#endregion
	}

	partial class Database
	{
		#region SelectObjects

		public IEnumerable<T> SelectObjects<T>(string tableName, object condition, params string[] columns)
		{
			string filter = MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, columns);
			var ps = base.GetParameters(condition).ToArray();
			using (DbDataReader dr = InvokeReadQuery(sql, ps))
			{
				while (dr.Read())
					yield return DbConvertor.Convert<T>(dr, null);
			}
		}

		public IEnumerable<T> SelectObjects<T>(string tableName, string condition, params string[] columns)
		{
			string filter = MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, columns);
			using (DbDataReader dr = InvokeReadQuery(sql, null))
			{
				while (dr.Read())
					yield return DbConvertor.Convert<T>(dr, null);
			}
		}

		public IEnumerable<T> SelectObjects<T>(string tableName, object condition, int group, params string[] columns)
		{
			Contract.Assert(group <= columns.Length);

			group = group <= columns.Length ? group : columns.Length;
			string filter = MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, columns, group, null);
			var ps = base.GetParameters(condition).ToArray();
			using (DbDataReader dr = InvokeReadQuery(sql, ps))
			{
				while (dr.Read())
					yield return DbConvertor.Convert<T>(dr, null);
			}
		}

		public IEnumerable<T> SelectObjects<T>(string tableName, string condition, int group, params string[] columns)
		{
			Contract.Requires(group <= columns.Length);

			group = group <= columns.Length ? group : columns.Length;
			string filter = MakeFilter(condition);
			string sql = MakeSelect(tableName, filter, columns, group, null);
			using (DbDataReader dr = InvokeReadQuery(sql, null))
			{
				while (dr.Read())
					yield return DbConvertor.Convert<T>(dr, null);
			}
		}

		#endregion

		public IEnumerable<T> SelectPaged<T>(string tableName, object condition, string orderby, int limit, int offset, params string[] columns)
		{
			string filter = MakeFilter(condition);
			return SelectPaged<T>(tableName, filter, orderby, limit, offset, columns);
		}

		public IEnumerable<T> SelectPaged<T>(string tableName, string condition, string orderby, int limit, int offset, params string[] columns)
		{
			var paged = string.Format("LIMIT {0} OFFSET {1}", limit, offset);
			string sql = MakeSelect(tableName, condition, columns, 0, orderby, paged);
			using (var dr = InvokeReadQuery(sql, null))
			{
				while (dr.Read())
					yield return DbConvertor.Convert<T>(dr, null);
			}
		}
	}
}
