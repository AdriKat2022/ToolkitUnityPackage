using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace AdriKat.Toolkit.Saving
{
    /// <summary>
    /// Utility class to perform save and load operations.
    /// </summary>
    public static class SaveUtility
    {
        /// <summary>
        /// Appends filepath to the persistentDataPath variable (Application.persistentDataPath).
        /// </summary>
        /// <param name="filepath">The path to append to the persistent path.</param>
        public static string AppendToPersistentDataPath(string filepath)
        {
            return Application.persistentDataPath + "/" + filepath;
        }
        
        /// <summary>
        /// Loads an object from the provided fullFilepath file using binary deserialization.
        /// </summary>
        /// <param name="fullFilepath">The full path of the file to deserialize.</param>
        /// <typeparam name="T">The object to deserialize.</typeparam>
        /// <returns>The deserialized object T.</returns>
        public static T LoadFromBinary<T>(string fullFilepath) where T : class
        {
            if (!File.Exists(fullFilepath)) return null;

            try 
            {
                using FileStream file = new FileStream(fullFilepath, FileMode.OpenOrCreate);

                if (file.Length <= 0)
                {
                    return null;
                }
                
                BinaryFormatter formatter = new BinaryFormatter();
                T playerDataSaveData = formatter.Deserialize(file) as T;
                
                file.Close();
                
                return playerDataSaveData;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Saves an object to the provided fullFilepath file using binary serialization.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <param name="filepath">The path to save the file (file's name included).</param>
        /// <typeparam name="T">The object type to serialize.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown if T is not serializable.</exception>
        public static void SaveToBinary<T>(T data, string filepath) where T : class
        {
            if (!typeof(T).IsSerializable)
            {
                throw new InvalidOperationException("The type must be serializable.");
            }
            
            Debug.Log($"Saving at '{filepath}'.");
        
            try
            {
                using FileStream file = new FileStream(filepath, FileMode.OpenOrCreate);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(file, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                throw;
            }
        }
    }
}