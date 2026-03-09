using System;
using System.IO;

namespace AdriKat.Toolkit.Saving
{
    /// <summary>
    /// Ready-to-use quick saver utility to save and load serializable objects.
    /// All the data is automatically stored within the persistent data folder (located at Application.persistentDataPath).
    /// </summary>
    /// <typeparam name="TData">The object to save/load. Must be serializable.</typeparam>
    public static class DataSaver<TData> where TData : class
    {
        /// <summary>
        /// Saves the data to the given filename.
        /// If a file with the same filename already exists, it will be overwritten.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="filename">The future data's location. Can contain slashes for subdirectories.</param>
        public static void SaveData(TData data, string filename)
        {
            SaveUtility.SaveToBinary(data, SaveUtility.AppendToPersistentDataPath(filename));
        }
    
        /// <summary>
        /// Loads the TData object from the filename.
        /// If the file doesn't exist, it will return null.
        /// </summary>
        /// <param name="filename">The data's location. Can contain slashes for subdirectories.</param>
        public static TData LoadData(string filename)
        {
            return SaveUtility.LoadFromBinary<TData>(SaveUtility.AppendToPersistentDataPath(filename));
        }
        
        /// <summary>
        /// Loads the TData object from the filename.
        /// If the file doesn't exist or the deserialization produces a null object, it will return the initializer's return value instead.
        /// </summary>
        /// <param name="filename">The data's location. Can contain slashes for subdirectories.</param>
        /// <param name="initializer">A function that generates an initialized TData.</param>
        /// <returns>The TData loaded from the filename, or the initializer's return value if the deserialization produced a null object.</returns>
        public static TData LoadOrInitData(string filename, Func<TData> initializer)
        {
            return SaveUtility.LoadFromBinary<TData>(SaveUtility.AppendToPersistentDataPath(filename)) ?? initializer();
        }

        /// <summary>
        /// Loads the TData object from the filename.
        /// If the file doesn't exist or the deserialization produces a null object, it will return the defaultValue instead.
        /// </summary>
        /// <param name="filename">The data's location. Can contain slashes for subdirectories.</param>
        /// <param name="defaultValue">The default value returned if the deserialization produces a null object.</param>
        /// <returns>The TData loaded from the filename, or the defaultValue if the deserialization produced a null object.</returns>
        public static TData LoadOrDefaultData(string filename, TData defaultValue)
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