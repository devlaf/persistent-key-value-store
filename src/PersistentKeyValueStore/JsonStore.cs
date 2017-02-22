using System;
using System.IO;

namespace PersistentKeyValueStore
{	
	/// <summary>
	/// An implementation of IPersistentKeyValueStore that uses a JSON flat file to 
	/// persist key-value data.
	/// </summary>
	/// <remarks>
	/// Ultimately this wound up being a thin wrapper around the JsonDictionary 
	/// class to conform to the specifications for the interface.
	/// </remarks>
	public class JsonStore : IPersistentKeyValueStore
	{
		private JsonDictionary Dict;

		private Func<string, string> StoreNotFoundExceptionMsg = ((methodName) => string.Format("{0} -- There was " +
			"an error creating or accessing the specified file location.  See inner exception for details.", methodName));

		private Func<string, string> InvalidStoreExceptionMsg = ((methodName) => string.Format("{0} -- There was " +
			"an error reading the specified file because it is mal-formatted.  See inner exception for details.", methodName));

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="jsonFileLocation">Filepath of the json data file.  As long as it is a valid file path, 
		/// the actual file need not exist; it will be created.</param>
		/// <exception cref="StoreNotFoundException">There was an error creating or accessing the specified file location.</exception>
		/// <exception cref="InvalidStoreException">There was a formatting error in the file specified, and it cannot be read.</exception>
		public JsonStore(string jsonFileLocation)
		{
			try
			{	
				Dict = new JsonDictionary(jsonFileLocation);
			}
			catch (Exception ex)
			{
				// Wrapping these exceptions in the higher-level exceptions provided by the IPersistentKeyValueStore interface 
				if( ex is ArgumentException || ex is ArgumentNullException || ex is PathTooLongException ||
					ex is DirectoryNotFoundException || ex is IOException || ex is NotSupportedException ||
					ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
				{
					throw new StoreNotFoundException(StoreNotFoundExceptionMsg("JsonStore Constructor"), ex);
				}
				else if(ex is FormatException)
				{
					throw new InvalidStoreException(InvalidStoreExceptionMsg("JsonStore Constructor"), ex);
				}
				throw;
			}
		} 
		#endregion

		#region SetValue OverLoads
		public bool SetValue(string KEY, string VALUE, bool overwriteIfExists=true)
		{
			return SetValueWithWrappedExceptions(KEY, VALUE, overwriteIfExists);
		}
		public bool SetValue(string KEY, int VALUE, bool overwriteIfExists=true)
		{
			return SetValueWithWrappedExceptions(KEY, VALUE, overwriteIfExists);
		}
		public bool SetValue(string KEY, bool VALUE, bool overwriteIfExists=true)
		{
			return SetValueWithWrappedExceptions(KEY, VALUE, overwriteIfExists);
		}
		public bool SetValue(string KEY, float VALUE, bool overwriteIfExists=true)		
		{
			return SetValueWithWrappedExceptions(KEY, VALUE, overwriteIfExists);
		}
		public bool SetValue(string KEY, double VALUE, bool overwriteIfExists=true)
		{
			return SetValueWithWrappedExceptions(KEY, VALUE, overwriteIfExists);
		}
		private bool SetValueWithWrappedExceptions(string KEY, dynamic VALUE, bool overwriteIfExists)
		{
			try
			{
				return Dict.SetValue(KEY, VALUE, overwriteIfExists);
			}
			catch (Exception ex)
			{
				if (ex is IOException || ex is System.Security.SecurityException)
					throw new StoreNotFoundException(StoreNotFoundExceptionMsg("JsonStore.SetValue(...)"), ex);
				else if(ex is FormatException)
					throw new InvalidStoreException(InvalidStoreExceptionMsg("JsonStore.SetValue(...)"), ex);
				throw;
			}
		}
		#endregion

		#region DeleteValue
		public void DeleteValue(string KEY)
		{
			try
			{
				Dict.DeleteValue(KEY);
			}
			catch (Exception ex)
			{
				if (ex is IOException || ex is System.Security.SecurityException)
					throw new StoreNotFoundException(StoreNotFoundExceptionMsg("JsonStore.DeleteValue(...)"), ex);
				else if(ex is FormatException)
					throw new InvalidStoreException(InvalidStoreExceptionMsg("JsonStore.DeleteValue(...)"), ex);
			}
		}
		#endregion

		#region GetValue OverLoads
		public T GetValue<T>(string KEY)
		{
			dynamic value;
			Dict.Lookup(KEY, out value, true);
			return (T)value;
		}
		public T GetValue<T>(string KEY, T defaultValue)
		{
			dynamic value;
			if(!Dict.Lookup(KEY, out value, false))
				return defaultValue;
			return (T)value;
		}
		#endregion

	}
}

