using System;

namespace PersistentKeyValueStore
{
	public interface IPersistentKeyValueStore
	{
		#region SetValue(...)

		/// <summary>
		/// Modifies the value of a string entry in the store.  If the entry does not exist, creates it.
		/// </summary>
		/// <returns>
		/// Boolean that indicates if update was successful.  This will be false only if parameter overwriteIfExists=false 
		/// and the KEY already exists in the key-value store.
		/// </returns>
		/// <exception cref="StoreNotFoundException">Error connecting to the data store. </exception>
		/// <exception cref="InvalidStoreException">The data store is corrupt. </exception>
		bool SetValue(string KEY, string VALUE, bool overwriteIfExists=true);

		/// <summary>
		/// Modifies the value of a integer entry in the store.  If the entry does not exist, creates it.
		/// </summary>
		/// <returns>
		/// Boolean that indicates if update was successful.  This will be false only if parameter overwriteIfExists=false 
		/// and the KEY already exists in the key-value store.
		/// </returns>
		/// <exception cref="StoreNotFoundException">Error connecting to the data store. </exception>
		/// <exception cref="InvalidStoreException">The data store is corrupt. </exception>
		bool SetValue(string KEY, int VALUE, bool overwriteIfExists=true);

		/// <summary>
		/// Modifies the value of a boolean entry in the store.  If the entry does not exist, creates it.
		/// </summary>
		/// <returns>
		/// Boolean that indicates if update was successful.  This will be false only if parameter overwriteIfExists=false 
		/// and the KEY already exists in the key-value store.
		/// </returns>
		/// <exception cref="StoreNotFoundException">Error connecting to the data store. </exception>
		/// <exception cref="InvalidStoreException">The data store is corrupt. </exception>
		bool SetValue(string KEY, bool VALUE, bool overwriteIfExists=true);

		/// <summary>
		/// Modifies the value of a float entry in the store.  If the entry does not exist, creates it.
		/// </summary>
		/// <returns>
		/// Boolean that indicates if update was successful.  This will be false only if parameter overwriteIfExists=false 
		/// and the KEY already exists in the key-value store.
		/// </returns>
		/// <exception cref="StoreNotFoundException">Error connecting to the data store. </exception>
		/// <exception cref="InvalidStoreException">The data store is corrupt. </exception>
		bool SetValue(string KEY, float VALUE, bool overwriteIfExists=true);

		/// <summary>
		/// Modifies the value of a double entry in the store.  If the entry does not exist, creates it.
		/// </summary>
		/// <returns>
		/// Boolean that indicates if update was successful.  This will be false only if parameter overwriteIfExists=false 
		/// and the KEY already exists in the key-value store.
		/// </returns>
		/// <exception cref="StoreNotFoundException">Error connecting to the data store. </exception>
		/// <exception cref="InvalidStoreException">The data store is corrupt. </exception>
		bool SetValue(string KEY, double VALUE, bool overwriteIfExists=true);

		#endregion

		#region DeleteValue(...)

		/// <summary>
		/// Deletes a value from the key-value store.
		/// </summary>
		/// <param name="KEY">Key associated with data to delete</param>
		/// <exception cref="StoreNotFoundException">Error connecting to the data store. </exception>
		/// <exception cref="InvalidStoreException">The data store is corrupt. </exception>
		void DeleteValue(string KEY);

		#endregion

		#region GetValue(...)

		/// <summary>
		/// Retrieves the value for the given key.
		/// </summary>
		/// <typeparam name="T">int, bool, float, double, or string.  This is the expected type to retrieve from the data store.</typeparam>
		/// <param name="KEY">The key to look up.</param>
		/// <returns>Value of type T associated with the given KEY</returns>
		/// <exception cref="StoreNotFoundException">
		/// Error connecting to the data store. 
		/// </exception>
		/// <exception cref="InvalidStoreException">
		/// The data store is corrupt. 
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Type specified for T is null. 
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Illegal type T specified.  GetValue(...) can only return types int, bool, float, double, or string.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// The type T specified is not compatible with the entry in the database.
		/// </exception>
		/// <exception cref="OverflowException">
		/// Value stored in the database is a number that is out of range of the specified type T.
		/// </exception>
		T GetValue<T>(string KEY);

		/// <summary>
		/// Retrieves the value for the given key.
		/// </summary>
		/// <typeparam name="T">int, bool, float, double, or string.  This is the expected type to retrieve from the data store.</typeparam>
		/// <param name="KEY">The key to look up.</param>
		/// <param name="defaultValue">The value to return if the KEY does not exist.</param>
		/// <returns>Value of type T associated with the given KEY</returns>
		/// <exception cref="StoreNotFoundException">
		/// Error connecting to the data store. 
		/// </exception>
		/// <exception cref="InvalidStoreException">
		/// The data store is corrupt. 
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Type specified for T is null
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Illegal type T specified.  GetValue(...) can only return types int, bool, float, double, or string.
		/// </exception>
		/// <exception cref="InvalidCastException">
		/// The type T specified is not compatible with the entry in the database.
		/// </exception>
		/// <exception cref="OverflowException">
		/// Value stored in the database is a number that is out of range of the specified type T.
		/// </exception>
		T GetValue<T>(string KEY, T defaultValue);

		#endregion
	}
}

