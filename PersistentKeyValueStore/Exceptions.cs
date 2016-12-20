using System;
using System.Runtime.Serialization;

namespace PersistentKeyValueStore
{
	#region InvalidStoreException
	/// <summary>
	/// This exception will be thrown in any case where the stored data is corrupt, 
	/// i.e. a user has manually edited a value in underlying datastore, but has screwed 
	/// up the format or provided a value that the library cannot understand.  If it is 
	/// not possible to recover using default values, this exception will be thrown.
	/// </summary>
	[Serializable]
	public class InvalidStoreException : Exception
	{
		public InvalidStoreException(string message)
			: base(message)	{ }

		public InvalidStoreException(string message, Exception innerException)
			: base(message, innerException) { }

		protected InvalidStoreException(SerializationInfo info, StreamingContext ctxt) 
			: base(info, ctxt) { }
	}
	#endregion

	#region StoreNotFoundException
	/// <summary>
	/// This exception will be thrown in the situation where this library cannot locate 
	/// the datastore on disk.  For example, if the PersistantKeyValueStore interface 
	/// is implemented using a database to persist the data, this exception would be 
	/// thrown if the database connection could not be established.
	/// </summary>
	[Serializable]
	public class StoreNotFoundException : Exception
	{
		public StoreNotFoundException(string message)
			: base(message) { }

		public StoreNotFoundException(string message, Exception innerException)
			: base(message, innerException) { }

		protected StoreNotFoundException(SerializationInfo info, StreamingContext ctxt) 
			: base(info, ctxt) { }
	}
	#endregion
}

