using System;
using System.Collections.Generic;
using System.Text;

namespace bfbd.Common.Data
{
	/// <summary>
	/// 数据库调用接口
	/// </summary>
	/// <remarks>2014/12/04</remarks>
	public partial class Database : DatabaseCore
	{
		public Database() : base() { }
		public Database(string dbName) : base(dbName) { }

		#region Invoke

		/// <summary>
		/// throw Exception when error.
		/// </summary>
		public static T Invoke<T>(Func<Database, T> func) { return Invoke(null, func); }
		
		public static T Invoke<T>(string name, Func<Database, T> func)
		{
			T result;
			try
			{
				using (Database db = new Database(name))
				{
					result = func(db);
				}
			}
			catch (Exception ex) { Logger.Exception(ex); throw; }
			return result;
		}
		
		#endregion Invoke

		#region Execute

		/// <summary>
		/// Silence when error occur.
		/// </summary>
		public static T Execute<T>(Func<Database, T> func) { return Execute(null, func); }
		
		public static T Execute<T>(string name, Func<Database, T> func)
		{
			T result = default(T);
			try
			{
				using (Database db = new Database(name))
				{
					result = func(db);
				}
			}
			catch (Exception ex) { Logger.Exception(ex); }
			return result;
		}
		
		#endregion Execute

		#region Translation

		/// <summary>
		/// Auto commit by success or rollback by exception.
		/// </summary>		
		public static void Translation(Action<Database> action) { Translation(null, action); }
		
		public static void Translation(string name, Action<Database> action)
		{
			try
			{
				using (Database db = new Database(name))
				{
					using (var tran = db.DbConnection.BeginTransaction())
					{
						db.DbTranslation = tran;
						action(db);
						tran.Commit();
					}
				}
			}
			catch (Exception ex) { Logger.Exception(ex); throw; }
		}

		public static T Translation<T>(Func<Database, T> func) { return Translation(null, func); }
		
		public static T Translation<T>(string name, Func<Database, T> func)
		{
			try
			{
				T result = default(T);
				using (Database db = new Database(name))
				{
					using (var tran = db.DbConnection.BeginTransaction())
					{
						db.DbTranslation = tran;
						result = func(db);
						tran.Commit();
					}
				}
				return result;
			}
			catch (Exception ex) { Logger.Exception(ex); throw; }
		}
		
		#endregion Translation
	}
}
