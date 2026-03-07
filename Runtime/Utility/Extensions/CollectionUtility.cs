using System;
using System.Collections.Generic;
using AdriKat.Toolkit.DataStructures;

namespace AdriKat.Toolkit.Utility.Extensions
{
    public static class CollectionUtility
    {
        public static void AddItem<K, V>(this SerializableDictionary<K, List<V>> serializableDictionnary, K key, V value)
        {
            if (serializableDictionnary.ContainsKey(key))
            {
                serializableDictionnary[key].Add(value);
                return;
            }

            serializableDictionnary.Add(key, new List<V> { value });
        }

        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return (list == null || list.Count == 0);
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return (array == null || array.Length == 0);
        }

        public static T[] InitializeArrayWithValue<T>(int count, T value)
        {
            T[] array = new T[count];

            for (var i = 0; i < count; i++)
            {
                array[i] = value;
            }

            return array;
        }
    }
}