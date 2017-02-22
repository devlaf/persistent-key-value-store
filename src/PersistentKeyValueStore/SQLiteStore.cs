using System;
using System.Collections.Generic;
using SQLiteWrapper;

namespace PersistentKeyValueStore
{
	/// <summary>
	/// This class creates, manages, and persists data for the application via a table in a sqlite 
	/// database.  Note that there exists an implicit assumption for the IPersistentKeyValueStore 
	/// interface that latency in retrieving data should be low - if we were not using SQLite here, 
	/// caching etc. might be in order.
	/// </summary>
	public sealed class SQLiteStore : IPersistentKeyValueStore
	{
		private const string TableName = "KeyValueDataStore";
		private IDatabase Database;

		#region Construction

		/// <summary>
		/// Constructor
		/// </summary>
		/// <exception cref="DatabaseConnectionException">Error connecting to the Database.</exception>
		public SQLiteStore(SQLiteDatabase.DatabaseLocationInfo locationInfo) 
		{
			Database = new SQLiteDatabase(locationInfo);

			// SQLite does not require specifying column type or max size
			string sql = string.Format("CREATE TABLE IF NOT EXISTS '{0}' (KEY, VALUE, TYPE);", TableName);    

			Database.ExecuteNonQuery(sql);
		}

		#endregion

		#region SetValue(...)

		public bool SetValue(string KEY, string VALUE, bool overwriteIfExists=true)
		{
			return WriteValueToDB(KEY, VALUE, "STR", overwriteIfExists);
		}

		public bool SetValue(string KEY, int VALUE, bool overwriteIfExists=true)
		{
			return WriteValueToDB(KEY, VALUE.ToString(), "INT", overwriteIfExists);
		}

		public bool SetValue(string KEY, bool VALUE, bool overwriteIfExists=true)
		{
			return WriteValueToDB(KEY, VALUE ? "True" : "False", "BOOL", overwriteIfExists);
		}

		public bool SetValue(string KEY, float VALUE, bool overwriteIfExists=true)
		{
			return WriteValueToDB(KEY, VALUE.ToString(), "FLOAT", overwriteIfExists);
		}

		public bool SetValue(string KEY, double VALUE, bool overwriteIfExists=true)
		{
			return WriteValueToDB(KEY, VALUE.ToString(), "DOUBLE", overwriteIfExists);
		}

		private bool WriteValueToDB(string KEY, string VALUE, string TYPE, bool overwriteIfExists)  
		{
			if (Database.ValueExistsInColumn(TableName, "KEY", KEY))
			{
				if (!overwriteIfExists)
					return false;

				Database.ExecuteNonQuery(String.Format("update {0} set VALUE = '{1}', TYPE = '{2}' " +
					"where KEY = '{3}';", TableName, VALUE, TYPE, KEY));
			}
			else
			{
				string values  = string.Format("'{0}', '{1}', '{2}'", KEY, VALUE, TYPE);
				Database.ExecuteNonQuery(String.Format("insert into {0}(KEY, VALUE, TYPE) values({1});",
					TableName, values));

			}
			return true;
		}

		#endregion

		#region DeleteValue(...)

		public void DeleteValue(string KEY)
		{
			if (!Database.ValueExistsInColumn(TableName, "KEY", KEY))
				return;
			Database.ExecuteNonQuery(String.Format("delete from {0} where KEY = '{1}';", TableName, KEY));
		}
		#endregion

		#region GetValue(...)

		private static List<Type> _availableTypes = new List<Type>( new Type[] { 
			typeof(int), typeof(bool), typeof(float), typeof(double), typeof(string) 
		});

		public T GetValue<T>(string KEY)
		{
			if (!_availableTypes.Contains(typeof(T)))
			{
				throw new ArgumentException("Illegal type [" + typeof(T) + "]. GetValue(...) can only " +
					"return types int, bool, float, double, or string.");
			}

			string sql = String.Format("SELECT VALUE FROM '{0}' WHERE KEY='{1}' LIMIT 1", TableName, KEY);
			var value = Database.ExecuteScalar(sql).ToString();

			if(value == null)
				throw new KeyNotFoundException("Key " + KEY + " does not exist in the database.");

			// Convert.ChangeType(...) does not recognize numerics as boolean values.
			if (typeof(T) == typeof(bool) && value == "0" || value == "1")
				return (T)(object)(value == "1");

			return (T)Convert.ChangeType(value, typeof(T));
		}

		public T GetValue<T>(string KEY, T defaultValue)
		{
			try
			{
				return GetValue<T>(KEY);
			}
			catch (KeyNotFoundException) 
			{
				return defaultValue;
			}
		}

		#endregion
	}
}

