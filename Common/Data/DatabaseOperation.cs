using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace bfbd.Common.Data
{
	/// <summary>
	/// 数据库操作接口
	/// </summary>
	/// <remarks>2015/1/20</remarks>
	partial class Database
	{
		/// <summary>
		/// 执行SQL
		/// </summary>
		/// <typeparam name="T">对象属性值的筛选类型</typeparam>
		/// <param name="sql">SQL语句</param>
		/// <param name="obj">对象的属性值应用于SQL参数</param>
		/// <returns>受影响的行数</returns>
		public int Execute<T>(string sql, T obj, params string[] parameterColumns)
		{
			var ps = base.GetParameters(obj, parameterColumns).ToArray();
			return InvokeExecuteQuery(sql, ps);
		}

		/// <summary>
		/// 添加记录
		/// </summary>
		/// <typeparam name="T">对象属性值的筛选类型</typeparam>
		/// <param name="tableName">表名称</param>
		/// <param name="obj">对象的属性值应用于SQL参数</param>
		/// <returns>受影响的行数</returns>
		public int Insert<T>(string tableName, T obj, params string[] valueColumns)
		{
			string sql = MakeInsert(tableName, obj, valueColumns);
			var ps = base.GetParameters(obj, valueColumns).ToArray();
			return InvokeExecuteQuery(sql, ps);
		}

		#region Update

		/// <summary>
		/// 更新记录
		/// </summary>
		/// <param name="tableName">表名称</param>
		/// <param name="obj">对象的属性值应用于SQL参数</param>
		/// <param name="condition">过滤条件</param>
		/// <param name="updateColumns">应用于SQL参数的列名称序列</param>
		/// <returns>受影响的行数</returns>
		public int Update<T>(string tableName, T obj, object condition, params string[] updateColumns)
		{
			Contract.Assert(!(condition is string));
			Contract.Assert(!(condition is StringBuilder));

			string filter = base.MakeFilter(condition, null, "F_");
			string sql = MakeUpdate(tableName, obj, filter, updateColumns);
			var ps = base.GetParameters(obj, updateColumns).Concat(base.GetParameters(condition, null, "F_")).ToArray();
			return InvokeExecuteQuery(sql, ps);
		}

		public int Update<T>(string tableName, T obj, string condition, params string[] updateColumns)
		{
			string sql = MakeUpdate(tableName, obj, condition, updateColumns);
			var ps = base.GetParameters(obj, updateColumns).ToArray();
			return InvokeExecuteQuery(sql, ps);
		}

		#endregion

		#region Delete

		/// <summary>
		/// 删除记录
		/// </summary>
		/// <param name="tableName">表名称</param>
		/// <param name="condition">过滤条件</param>
		/// <returns>受影响的行数</returns>
		public int Delete<T>(string tableName, T condition, params string[] filterColumns)
		{
			string filter = MakeFilter(condition, filterColumns);
			string sql = MakeDelete(tableName, filter);
			var ps = base.GetParameters(condition, filterColumns).ToArray();
			return InvokeExecuteQuery(sql, ps);
		}

		public int Delete(string tableName, string condition)
		{
			string sql = MakeDelete(tableName, condition);
			return InvokeExecuteQuery(sql, null);
		}

		#endregion
	}

	// 其他
	partial class Database
	{
		/// <summary>
		/// 添加或更新记录
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="db"></param>
		/// <param name="tableName"></param>
		/// <param name="obj"></param>
		/// <param name="columns"></param>
		public int InsertOrUpdate<T>(string tableName, T obj, params string[] keyColumns)
		{
			if (!IsExists(tableName, obj, keyColumns))
			{
				return Insert(tableName, obj);
			}
			else
			{
				string filter = base.MakeFilter(obj, keyColumns);
				return Update(tableName, obj, filter);
			}
		}

		public int InsertNotExists<T>(string tableName, T obj, params string[] keyColumns)
		{
			if (!IsExists(tableName, obj, keyColumns))
				return Insert(tableName, obj);
			else
				return 0;
		}

		//public int InsertBySelect(string tableName, object prams, string sourceTable, object condition)
		//{
		//    throw new NotImplementedException();
		//}

		//public int UpdateByStatement(string tableName, object condition, string statement)
		//{
		//    string sql;
		//    string filter = MakeFilter(condition);
		//    sql = string.Format("UPDATE [{0}] SET {1} {2}", tableName, statement, filter);
		//    return base.InvokeExecuteQuery(sql, condition);
		//}
	}
}
