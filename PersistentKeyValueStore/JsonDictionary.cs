using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PersistentKeyValueStore
{
	/// <summary>
	/// An abstraction for what is essentially a Dictionary<string, dynamic> that is 
	/// persisted to disk using a json flat file.  An up-to-date in-memory cache is 
	/// maintained to decrease lookup latency.
	/// </summary>
	public class JsonDictionary
	{
		private readonly string JsonFileLocation;
		private Dictionary<string, dynamic> Cache;
		private FileSystemWatcher Watcher;
		private bool FileIsCorrupt = false;

		/// <summary>
		/// We'll use this lock object to ensure that we maintain consistancy between the
		/// persistant json backup and the in-memory cache.  SetValue will require this lock
		/// for the duration of flushing a new value to the file and any lookups will block
		/// until that completes.
		/// </summary>
		private object DictionaryLockObject = new object();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fileLocation">Filepath of the json file.  As long as it is a valid file path, 
		/// the actual file need not exist; it will be created in this case.</param>
		/// <exception cref="ArgumentException">fileLocation is a zero-length string, contains only white space, or 
		/// contains one or more invalid characters as defined by System.IO.Path.InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException">fileLocation is null.</exception>
		/// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum 
		/// length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be 
		/// less than 260 characters. </exception>
		/// <exception cref="DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
		/// <exception cref="NotSupportedException">fileLocation is in an invalid format</exception>
		/// <exception cref="SecurityException">The caller does not have the required permission to access fileLocation</exception>
		/// <exception cref="UnauthorizedAccessException">fileLocation specified a file that is read-only. -or-
		/// This operation is not supported on the current platform. -or- fileLocation specified a directory. -or-
		/// The caller does not have the required permission. </exception>
		/// <exception cref="FormatException">A file already exists at fileLocation, but the contents are mal-formatted 
		/// or corrupt and cannot be read.</exception>
		public JsonDictionary(string fileLocation)
		{
			JsonFileLocation = fileLocation;

			bool fileAlreadyExisted = ValidateFilePath(JsonFileLocation);

			lock(DictionaryLockObject)
			{
				if(!fileAlreadyExisted)
				{
					Cache = new Dictionary<string, dynamic>();
					FlushCache();
				}
				else
				{
					Cache = DeserializeJson();
				}
			}

			string watchedDirectory = (new FileInfo(JsonFileLocation)).DirectoryName;
			Watcher = new FileSystemWatcher(watchedDirectory, "*.json");
			Watcher.Changed += new FileSystemEventHandler(OnDirectoryChanged);
			Watcher.Renamed += new RenamedEventHandler(OnDirectoryChanged);
			Watcher.Deleted += new FileSystemEventHandler(OnDirectoryChanged);
		}

		/// <summary>
		/// Lookup a value for the specified key in the json file.  Will likely hit an in-memory cache rather than read from the file.
		/// </summary>
		/// <param name="key">The key to lookup.</param>
		/// <param name="value">The value will be written here.  This will be failed with null if the specified key is not found.</param>
		/// <param name="throwOnNotFound">If the specified key is not found, should this method throw a KeyNotFoundException.  If this parameter 
		/// is false, the method will simply return false in the case of a key not found.</param>
		/// <returns>True if the key was found and the value parameter was filled with valid data, false otherwise.</returns>
		/// <exception cref="KeyNotFoundException">The specified key does not exist </exception>
		/// <exception cref="FormatException">The file is mal-formatted or corrupt and cannot be read.</exception>
		public bool Lookup(string key, out dynamic value, bool throwOnNotFound)
		{
			if(FileIsCorrupt)
				throw new FormatException("JsonDictionary.Lookup(...) -- The json file could not be parsed due to a formatting error.");

			if(!Cache.ContainsKey(key) && !throwOnNotFound)
			{
				value = null; 
				return false;
			}

			lock(DictionaryLockObject)
			{
				value = Cache[key];
			}
			return true;
		}

		/// <summary>
		/// Sets the value for the specified key.
		/// </summary>
		/// <returns>true if the value was successfully set, false otherwise.  Really the only way this would return false is if the 
		/// overWriteIfExists parameter is specified as false and a value already exists for that key.</returns>
		/// <exception cref="FormatException">The file is mal-formatted or corrupt and cannot be read.</exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
		/// <exception cref="SecurityException">The caller no longer has the required permission to access the file on disk</exception>
		public bool SetValue(string key, dynamic value, bool overWriteIfExists)
		{
			if(FileIsCorrupt)
				throw new FormatException("JsonDictionary.SetValue(...) -- The json file could not be parsed due to a formatting error.");

			if(Cache.ContainsKey(key) && !overWriteIfExists)
				return false;

			lock(DictionaryLockObject)
			{
				Cache[key] = value;
				FlushCache();
			}
			return true;
		}

		/// <summary>
		/// Deletes the value for the specified key.
		/// </summary>
		/// <exception cref="FormatException">The file is mal-formatted or corrupt and cannot be read.</exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
		/// <exception cref="SecurityException">The caller no longer has the required permission to access the file on disk</exception>
		public void DeleteValue(string key)
		{
			if(FileIsCorrupt)
				throw new FormatException("JsonDictionary.SetValue(...) -- The json file could not be parsed due to a formatting error.");

			if(!Cache.ContainsKey(key))
				return;

			lock(DictionaryLockObject)
			{
				Cache.Remove(key);
				FlushCache();
			}
		}

		private static bool ValidateFilePath(string filePath)
		{
			var directory = Path.GetDirectoryName(filePath);
			if(directory == string.Empty)
				throw new NotSupportedException("JsonDictionary.ValidateFilePath(...) -- Can't determine a file/directory distinction in the specified file path [" + filePath + "].");
			Directory.CreateDirectory(directory);

			// We'll piggy-back on top of the path checking done by System.IO.File.Open here because it's probably a decent implementation.  
			// Throws: UnauthorizedAccessException, ArgumentException, ArgumentNullException, PathTooLongException, 
			//         DirectoryNotFoundException, IOException, NotSupportedException
			using(var fs = File.Open(filePath, FileMode.OpenOrCreate))		
			{
				return fs.Length != 0;
			}
		}

		private void OnDirectoryChanged(object source, FileSystemEventArgs e)
		{
			if(!File.Exists(JsonFileLocation))	// well, crap    ---   the file must have been deleted/renamed by a third party.
			{
				lock(DictionaryLockObject)
				{
					Cache = new Dictionary<string, dynamic>();
					FlushCache();
				}
			}

			lock(DictionaryLockObject)
			{
				Cache = DeserializeJson();
			}
		}

		private void FlushCache()
		{
			string serializedCache = JsonConvert.SerializeObject(Cache);
			File.WriteAllText(JsonFileLocation, serializedCache);
		}


		private Dictionary<string, dynamic> DeserializeJson()
		{
			string serializedDictionary = File.ReadAllText(JsonFileLocation);

			Dictionary<string, dynamic> retval;
			try
			{ 
				var deserializedDictionary = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(serializedDictionary);
				retval = deserializedDictionary as Dictionary<string, dynamic>;
				FileIsCorrupt = false;
			}
			catch (Exception ex)
			{
				FileIsCorrupt = true;
				throw new FormatException("JsonDictionary.DeserializeJson() -- The json file could not be parsed due to a formatting error.", ex);
			}
			return retval;
		}
	}
}

