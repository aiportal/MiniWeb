using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace bfbd.Common.Data
{
	using bfbd.Common;

	/// <summary>
	/// 数据库引擎
	/// </summary>
	/// <remarks>2015/1/20</remarks>
	public abstract partial class DatabaseCore
	{
		#region Static Implement

		private static string _defaultConnection;
		private static string _defaultPassword = @"Z69$Y@fx";
		private static Hashtable _settings = Hashtable.Synchronized(new Hashtable());

		/// <summary>
		/// 从配置文件载入ConnectionStringSettings
		/// </summary>
		static DatabaseCore()
		{
			foreach (ConnectionStringSettings setting in ConfigurationManager.ConnectionStrings)
			{
				if (!setting.ElementInformation.IsPresent)
					continue;
				if (System.IO.Path.GetFileName(setting.ElementInformation.Source) == "machine.config")
					continue;
				AddConnectionSettings(setting.Name, setting.ConnectionString, setting.ProviderName);
			}
		}

		/// <summary>
		/// 添加数据库连接
		/// 相当于<connectionStrings></connectionStrings>设置项
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="connectionString">连接字符串</param>
		/// <param name="providerName">提供程序</param>
		public static void AddConnectionSettings(string name, string connectionString, string providerName)
		{
			_settings[name] = new ConnectionStringSettings(name, connectionString, providerName);
			if (string.IsNullOrEmpty(_defaultConnection))
				_defaultConnection = name;
		}

		/// <summary>
		/// 缺省数据库连接的名称
		/// </summary>
		public static string DefaultConnectionName
		{
			get { return _defaultConnection; }
			set
			{
				if (_settings.ContainsKey(value))
					_defaultConnection = value;
				else
					throw new ArgumentException("Connection setting not found: " + value);
			}
		}

		/// <summary>
		/// 默认密码
		/// 连接字符串中用{0}代替
		/// </summary>
		public static string DefaultPassword { get { return _defaultPassword; } set { _defaultPassword = value; } }

		#endregion
	}

	partial class DatabaseCore : IDisposable
	{
		#region Base Implement

		private string _connStr;
		private DbProviderFactory _dbFactory;
		private DbConnection _conn;

		public DatabaseCore(string connName = null)
		{
			if (_settings.Count < 1)
				throw new ConfigurationErrorsException("Can't find ConnectionString settings.");

			Contract.Assert(!string.IsNullOrEmpty(DefaultConnectionName));
			if (string.IsNullOrEmpty(connName))
				connName = DefaultConnectionName;
			var settings = _settings[connName] as ConnectionStringSettings;

			_connStr = settings.ConnectionString.Replace("{0}", DefaultPassword);
			if (settings.ProviderName == "System.Data.SQLite")
			{
				var handle = Activator.CreateInstance("System.Data.SQLite", "System.Data.SQLite.SQLiteFactory");
				_dbFactory = handle.Unwrap() as DbProviderFactory;
			}
			else if (settings.ProviderName == "MySql.Data.MySqlClient")
			{
				var handle = Activator.CreateInstance("MySql.Data", "MySql.Data.MySqlClient.MySqlClientFactory");
				_dbFactory = handle.Unwrap() as DbProviderFactory;
			}
			else
			{
				_dbFactory = DbProviderFactories.GetFactory(settings.ProviderName);
			}
		}

		protected DbConnection DbConnection
		{
			get
			{
				if (_conn == null)
				{
					_conn = _dbFactory.CreateConnection();
					_conn.ConnectionString = _connStr;
					_conn.Open();
				}
				return _conn;
			}
		}

		protected DbTransaction DbTranslation { get; set; }

		public void Rollback()
		{
			try
			{
				if (this.DbTranslation != null)
					this.DbTranslation.Rollback();
			}
			catch (Exception ex) { Logger.Exception(ex); }
		}

		public void Dispose()
		{
			if (_conn != null)
			{
				this.Rollback();

				_conn.Close();
				_conn.Dispose();
				_conn = null;
			}
		}

		#endregion
	}

	partial class DatabaseCore
	{
		#region Invoke Implement

		protected object InvokeSingleQuery(string sql, DbParameter[] prams)
		{
			try
			{
				object result = null;
				using (DbCommand cmd = this.DbConnection.CreateCommand())
				{
					cmd.Transaction = this.DbTranslation;
					cmd.CommandText = sql;
					cmd.CommandType = CommandType.Text;
					if (prams != null) 
						cmd.Parameters.AddRange(prams);
					result = cmd.ExecuteScalar();
					if (result == DBNull.Value)
						result = null;
				}
				return result;
			}
			catch (Exception ex) { this.DumpException(ex, sql, prams); throw; }
		}

		protected DataTable InvokeTableQuery(string sql, DbParameter[] prams)
		{
			try
			{
				DataTable dt = new DataTable();
				using (DbCommand cmd = this.DbConnection.CreateCommand())
				{
					cmd.Transaction = this.DbTranslation;
					cmd.CommandText = sql;
					cmd.CommandType = CommandType.Text;
					if (prams != null)
						cmd.Parameters.AddRange(prams);
					using (DbDataAdapter adp = _dbFactory.CreateDataAdapter())
					{
						adp.SelectCommand = cmd;
						adp.Fill(dt);
					}
				}
				return dt;
			}
			catch (Exception ex) { this.DumpException(ex, sql, prams); throw; }
		}

		protected DbDataReader InvokeReadQuery(string sql, DbParameter[] prams)
		{
			try
			{
				DataTable dt = new DataTable();
				using (DbCommand cmd = this.DbConnection.CreateCommand())
				{
					cmd.Transaction = this.DbTranslation;
					cmd.CommandText = sql;
					cmd.CommandType = CommandType.Text;
					if (prams != null)
						cmd.Parameters.AddRange(prams);
					return cmd.ExecuteReader();
				}
			}
			catch (Exception ex) { this.DumpException(ex, sql, prams); throw; }
		}

		protected int InvokeExecuteQuery(string sql, DbParameter[] prams)
		{
			try
			{
				int count = 0;
				using (DbCommand cmd = this.DbConnection.CreateCommand())
				{
					cmd.Transaction = this.DbTranslation;
					cmd.CommandText = sql;
					cmd.CommandType = CommandType.Text;
					if(prams != null)
						cmd.Parameters.AddRange(prams);
					count = cmd.ExecuteNonQuery();
				}
				return count;
			}
			catch (Exception ex) { this.DumpException(ex, sql, prams); throw; }
		}

		/// <summary>
		/// 生成参数列表
		/// </summary>
		/// <typeparam name="T">模板类型</typeparam>
		/// <param name="obj">对象</param>
		/// <param name="columns">用到的列</param>
		/// <param name="prefix">参数名称前缀</param>
		/// <returns>参数对象列表</returns>
		protected IEnumerable<DbParameter> GetParameters<T>(T obj, string[] columns = null, string prefix = null)
		{
			if (obj == null)
				yield break;

			var type = typeof(T) == typeof(object) ? obj.GetType() : typeof(T);
			if (type.FullName.StartsWith("System."))		// 反射参数不能是系统类
				yield break;

			foreach (var prop in type.GetProperties())
			{
				DbParameter p = this._dbFactory.CreateParameter();
				var name = prop.ColumnName();
				if (columns.IsEmpty() || columns.Contains(name))
				{
					p.ParameterName = prefix + name;
					p.Value = prop.GetValue(obj, null) ?? DBNull.Value;

					// byte数组不转string，其他数组转逗号分隔的string
					if (prop.PropertyType.IsArray && prop.PropertyType != typeof(byte[]))
					{
						p.Value = DataConvertor.Convert(p.Value);
					}
					yield return p;
				}
			}
		}

		private void DumpException(Exception ex, string sql, DbParameter[] prams)
		{
			StringBuilder sb = new StringBuilder();
			if (prams != null)
				Array.ForEach(prams, p => sb.AppendFormat("@{0} = {1},", p.ParameterName, p.Value));

			Logger.Error(string.Format(
@"<< SQL Error >> {0}
sql : {1}
parameters : {2}", ex.Message, sql, sb.ToString()));
			Logger.Exception(ex);
		}

		#endregion
	}

	partial class DatabaseCore
	{
		#region MakeSelect

		private static string[] DefaultColumns = new string[] { "*" };

		protected string MakeSelect(string tableName, string condition, string[] columns)
		{
			if (columns.IsEmpty())
				columns = DefaultColumns;

			string sql;
			string filter = MakeFilter(condition);
			sql = string.Format("SELECT {0} FROM {1} {2}", string.Join(",", columns), tableName, filter);
			return sql;
		}

		/// <param name="group">从1开头的分组列数量</param>
		protected string MakeSelect(string tableName, string condition, string[] columns, int group, string order, string limit = null)
		{
			if (columns.IsEmpty())
				columns = DefaultColumns;

			string sql;
			string filter = MakeFilter(condition);
			string group_by = MakeGroup(group, columns);
			string order_by = string.IsNullOrEmpty(order) ? "" : "ORDER BY " + order;
			sql = string.Format("SELECT {0} FROM {1} {2} {3} {4} {5}", string.Join(",", columns), tableName, filter, group_by, order_by, limit);
			return sql;
		}

		#endregion 

		#region MakeOperation

		protected string MakeInsert<T>(string tableName, T obj, string[] columns = null)
		{
			string sql;
			StringBuilder cols = new StringBuilder();
			StringBuilder vals = new StringBuilder();
			{
				var type = typeof(T) == typeof(object) ? obj.GetType() : typeof(T);
				foreach (var p in type.GetProperties())
				{
					if (Attribute.IsDefined(p, typeof(InsertIgnore)))
						continue;
					if (!p.CanRead)
						continue;
					
					if (p.GetValue(obj, null) != null)
					{
						var name = p.ColumnName();
						if (columns.IsEmpty() || columns.Contains(name))
						{
							cols.Append(cols.Length > 0 ? "," : null).Append(name);
							vals.Append(vals.Length > 0 ? "," : null).Append("@").Append(name);
						}
					}
				};
			}
			sql = string.Format("INSERT INTO {0} ({1}) VALUES({2})", tableName, cols, vals);
			return sql;
		}

		protected string MakeUpdate<T>(string tableName, T obj, string condition, string[] columns = null)
		{
			string sql;
			string filter = MakeFilter(condition);
			StringBuilder vals = new StringBuilder();
			{
				var type = typeof(T) == typeof(object) ? obj.GetType() : typeof(T);
				foreach (var p in type.GetProperties())
				{
					if (Attribute.IsDefined(p, typeof(UpdateIgnore)))
						continue;

					if (!p.CanRead)
						continue;

					var name = p.ColumnName();
					if (columns.IsEmpty() || columns.Contains(name))
					{
						var val = p.GetValue(obj, null);
						vals.Append(vals.Length > 0 ? "," : null);
						if (val != null)
							vals.Append(name).Append("=@").Append(name);
						else
							vals.Append(name).Append("=NULL");
					}
				}
			}
			sql = string.Format("UPDATE {0} SET {1} {2}", tableName, vals, filter);
			return sql;
		}

		protected string MakeDelete(string tableName, string condition)
		{
			string sql;
			string filter = MakeFilter(condition);
			sql = string.Format("DELETE FROM {0} {1}", tableName, filter);
			return sql;
		}

		#endregion

		#region Helper

		/// <summary>
		/// 构建where语句
		/// </summary>
		/// <typeparam name="T">对象类型</typeparam>
		/// <param name="condition">对象值</param>
		/// <param name="columns">使用的列</param>
		/// <param name="prefix">参数前缀</param>
		/// <returns>where语句</returns>
		protected string MakeFilter<T>(T condition, string[] columns = null, string prefix = null)
		{
			if (condition == null)
				return "";

			if (condition is string)
			{
				var str = (condition as string).TrimStart();
				if (string.IsNullOrEmpty(str))
					return "";
				if (str.StartsWith("WHERE", StringComparison.OrdinalIgnoreCase))
					return condition as string;
				else
					return " WHERE (" + condition +") ";
			}

			StringBuilder sql = new StringBuilder();
			var type = typeof(T) == typeof(object) ? condition.GetType() : typeof(T);
			foreach (var p in type.GetProperties())
			{
				if (!p.CanRead)
					continue;

				var name = p.ColumnName();
				if (columns.IsEmpty() || columns.Contains(name))
				{
					var val = p.GetValue(condition, null);
					sql.Append(sql.Length > 0 ? " AND " : null);
					if (val != null)
						sql.Append(name).Append("=@").Append(prefix + name);
					else
						sql.Append(name).Append(" IS NULL");
				}
			};
			if (sql.Length > 0)
				return sql.Insert(0, " WHERE (").Append(") ").ToString();
			else
				return "";
		}

		private string MakeGroup(int group, string[] columns)
		{
			Contract.Requires(columns != null);
			if (group < 0 || columns.Length < group)
				throw new ArgumentOutOfRangeException("group", "group must less or equal columns length.");

			StringBuilder sql = new StringBuilder();
			for (int i = 0; i < group; ++i)
			{
				var c = columns[i];

				sql.Append(sql.Length > 0 ? "," : null);
				if (c.ToLower().Contains(" as "))
					sql.Append(c.Substring(c.ToLower().LastIndexOf(" as ") + 4));
				else if (c.Contains(" "))
					sql.Append(c.Substring(c.LastIndexOf(" ") + 1));
				else
					sql.Append(c);
			}
			if (sql.Length > 0)
				return sql.Insert(0, " GROUP BY ").Append(" ").ToString();
			else
				return "";
		}

		#endregion
	}

	#region Ignore Attributes

	[AttributeUsage(AttributeTargets.Property)]
	class InsertIgnore : Attribute { }

	[AttributeUsage(AttributeTargets.Property)]
	class UpdateIgnore : Attribute { }

	#endregion

	#region DbColumnAttribute

	[AttributeUsage(AttributeTargets.Property)]
	public class DbColumnAttribute : Attribute
	{
		public string Name { get; set; }
		public DbColumnAttribute(string name)
		{
			Name = name;
		}
	}

	internal static class DbColumnExtension
	{
		public static string ColumnName(this PropertyInfo prop)
		{
			var attr = Attribute.GetCustomAttribute(prop, typeof(DbColumnAttribute)) as DbColumnAttribute;
			return attr == null ? prop.Name : attr.Name;
		}
	}
	#endregion

	#region StringExtension
	static class StringExtension
	{
		public static bool IsEmpty(this string[] arr)
		{
			return arr == null || arr.Length == 0;
		}

		public static bool Contains(this string[] arr, string str)
		{
			if (arr == null || arr.Length == 0)
				return false;
			else
				return arr.Any(s=>string.Equals(s,str, StringComparison.OrdinalIgnoreCase));
		}
	}
	#endregion
}