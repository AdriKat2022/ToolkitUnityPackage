using System;
using System.IO;

namespace AdriKat.Toolkit.Saving
{
    /// <summary>
    /// Ready-to-use quick saver utility to save and load serializable objects.
    /// All the data is automatically stored within the persistent data folder (located at Application.persistentDataPath).
    /// </summary>
    public static class DataSaver
    {
        /// <summary>
        /// Saves the data to the given filename.
        /// If a file with the same filename already exists, it will be overwritten.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="filename">The future data's location in the persistent data folder. Can contain slashes for subdirectories.</param>
        public static void SaveData(object data, string filename)
        {
            SaveUtility.SaveToBinary(data, SaveUtility.AppendToPersistentDataPath(filename));
        }
    
        /// <summary>
        /// Loads the TData object from the filename.
        /// If the file doesn't exist, it will return null.
        /// </summary>
        /// <param name="filename">The data's location. Can contain slashes for subdirectories.</param>
        /// <typeparam name="TData">The object to save/load. Must be serializable.</typeparam>
        public static TData LoadData<TData>(string filename) where TData : class
        {
            return SaveUtility.LoadFromBinary<TData>(SaveUtility.AppendToPersistentDataPath(filename));
        }
        
        /// <summary>
        /// Loads the TData object from the filename.
        /// If the file doesn't exist or the deserialization produces a null object, it will return the initializer's return value instead.
        /// </summary>
        /// <param name="filename">The data's location. Can contain slashes for subdirectories.</param>
        /// <param name="initializer">A function that generates an initialized TData.</param>
        /// <typeparam name="TData">The object to save/load. Must be serializable.</typeparam>
        /// <returns>The TData loaded from the filename, or the initializer's return value if the deserialization produced a null object.</returns>
        public static TData LoadOrInitData<TData>(string filename, Func<object> initializer) where TData : class
        {
            return SaveUtility.LoadFromBinary<TData>(SaveUtility.AppendToPersistentDataPath(filename)) ?? initializer();
        }

        /// <summary>
        /// Loads the TData object from the filename.
        /// If the file doesn't exist or the deserialization produces a null object, it will return the defaultValue instead.
        /// </summary>
        /// <param name="filename">The data's location. Can contain slashes for subdirectories.</param>
        /// <param name="defaultValue">The default value returned if the deserialization produces a null object.</param>
        /// <typeparam name="TData">The object to save/load. Must be serializable.</typeparam>
        /// <returns>The TData loaded from the filename, or the defaultValue if the deserialization produced a null object.</returns>
        public static TData LoadOrDefaultData<TData>(string filename, object defaultValue) where TData : class
        {
            return SaveUtility.LoadFromBinary<TData>(SaveUtility.AppendToPersistentDataPath(filename)) ?? defaultValue;
        }

        /// <summary>
        /// Returns true if the filename exists or false otherwise.
        /// </summary>
        /// <param name="filename">The data's location to test the existence. Can contain slashes for subdirectories.</param>
        /// <returns></returns>
        public static bool Exists(string filename)
        {
            return File.Exists(SaveUtility.AppendToPersistentDataPath(filename));
        }
    }
}