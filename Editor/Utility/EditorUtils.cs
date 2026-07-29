using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using AdriKat.Toolkit.Utility.Extensions;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdriKat.Toolkit.Utility
{
    public static class EditorUtils
    {
        private static readonly Dictionary<string, AnimBool> _fadeAnimations = new();

        /// <summary>
        /// Repaints all inspector editor windows.
        /// </summary>
        public static void RepaintAllInspectors()
        {
            // Debug.Log("REPAINT");
            // Find all Inspector windows and repaint them
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var windows = Resources.FindObjectsOfTypeAll(inspectorType);

            foreach (var window in windows)
            {
                var inspector = window as EditorWindow;

                if (inspector)
                {
                    // Debug.Log("Repainting inspector");
                    inspector.Repaint();
                }
            }
        }

        #region Attribute Utility

        /// <summary>
        /// Retrieves the given custom attribute attached onto the given property if it exists. 
        /// </summary>
        /// <param name="property"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this SerializedProperty property) where T : Attribute
        {
            if (property == null) return null;

            var targetObject = property.serializedObject.targetObject;
            var fieldInfo = GetFieldInfoFromProperty(property, targetObject);

            return fieldInfo?.GetCustomAttribute<T>(true);
        }

        private static FieldInfo GetFieldInfoFromProperty(SerializedProperty property, object target)
        {
            if (property == null || target == null) return null;

            Type type = target.GetType();
            string path = property.propertyPath;

            // Replace array syntax: Array.data[x] -> [x]
            path = Regex.Replace(path, @"\.Array\.data\[(\d+)\]", "[$1]");

            string[] elements = path.Split('.');

            FieldInfo fieldInfo = null;

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    // Array or List element
                    string elementName = element[..element.IndexOf("[", StringComparison.InvariantCulture)];
                    fieldInfo = GetField(type, elementName);

                    if (fieldInfo == null) return null;

                    type = GetElementType(fieldInfo.FieldType);
                }
                else
                {
                    fieldInfo = GetField(type, element);

                    if (fieldInfo == null) return null;

                    type = fieldInfo.FieldType;
                }
            }

            return fieldInfo;
        }

        private static FieldInfo GetField(Type type, string name)
        {
            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null) return field;

                type = type.BaseType;
            }

            return null;
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType();

            if (type.IsGenericType && typeof(IList).IsAssignableFrom(type)) return type.GetGenericArguments()[0];

            return type;
        }

        private static object GetParentObjectOfSerializedProperty(SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;

            string[] path = property.propertyPath.Split('.');

            // Remove the final field name because we want the parent
            for (int i = 0; i < path.Length - 1; i++)
            {
                FieldInfo field = obj.GetType().GetField(
                    path[i],
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                if (field == null)
                    return null;

                obj = field.GetValue(obj);

                if (obj == null)
                    return null;
            }

            return obj;
        }
        
        #endregion

        #region Value Extraction

        /// <summary>
        /// Turns a SerializedProperty into an array of SerializedProperties with each of its elements.
        /// </summary>
        public static IEnumerable<SerializedProperty> ToEnumerable(this SerializedProperty property)
        {
            for (int i = 0; i < property.arraySize; i++)
                yield return property.GetArrayElementAtIndex(i);
        }

        /// <summary>
        /// Retrieves the array of a property.
        /// </summary>
        /// <param name="arrayProperty">The serializedProperty to extract the array from.</param>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static T[] ExtractArray<T>(this SerializedProperty arrayProperty) where T : Object
        {
            if (arrayProperty == null) throw new ArgumentNullException(nameof(arrayProperty));

            if (!arrayProperty.isArray) throw new ArgumentException("Property is not an array.", nameof(arrayProperty));

            int size = arrayProperty.arraySize;
            T[] result = new T[size];

            for (int i = 0; i < size; i++)
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);

                if (element.propertyType != SerializedPropertyType.ObjectReference)
                {
                    throw new InvalidOperationException($"Element at index {i} is not an object reference.");
                }

                result[i] = element.objectReferenceValue as T;
            }

            return result;
        }

        public static SerializedProperty FindRelativeProperty(this SerializedProperty property, string variableName)
        {
            string path = property.propertyPath;

            int lastDot = path.LastIndexOf('.');

            path = lastDot >= 0 ? path[..(lastDot + 1)] : "";

            return property.serializedObject.FindProperty(path + variableName);
        }

        #endregion

        #region Reflection Access

        /// <summary>
        /// Evaluates a condition on a serialized object by checking for a boolean field or method
        /// matching the specified condition name. If a comparer value is provided, it will compare the field's value to the comparer value.
        /// </summary>
        /// <param name="property">The serialized property where the condition to evaluate lives in the same level.</param>
        /// <param name="conditionName">The name of the field or method to check.</param>
        /// <param name="comparerValue"></param>
        /// <param name="comparerValueIsVariableName"></param>
        /// <returns>True if the condition is valid and evaluates to true; otherwise, false.</returns>
        public static bool ResolveCondition(SerializedProperty property, string conditionName, object comparerValue = null, bool comparerValueIsVariableName = false)
        {
            var variableName = conditionName;
            SerializedProperty propertyToCompare = property.FindRelativeProperty(variableName);
            
            if (propertyToCompare == null)
            {
                // The property doesn't exist, but might be a function.
                if (!TryRunMethodRelativeToProperty(property, variableName, out object result))
                {
                    Debug.LogError($"ShowIfAttribute: Failed to find property or function '{variableName}'.");
                    return false;
                }

                // Not a function.
                if (result is not bool boolValue)
                {
                    Debug.LogError($"ShowIfAttribute: Methods returning a non-bool result are not supported.");
                    return false;
                }

                // Was a method.
                return boolValue;
            }

            if (comparerValue == null)
            {
                // Nothing to compare to, by default compare with "true".
                return CompareSerializedProperty(propertyToCompare, true);
            }

            if (comparerValueIsVariableName)
            {
                string variableNameValue = (string)comparerValue;
                comparerValue = property.FindRelativeProperty((string)comparerValue)?.boxedValue;
                if (comparerValue == null)
                {
                    Debug.LogError($"ShowIfAttribute: Could not find property '{variableNameValue}' to compare with '{variableName}' for property '{property.name}'.");
                    return false;
                }
            }
                
            return CompareSerializedProperty(propertyToCompare, comparerValue);
        }
        
        /// <summary>
        /// Evaluates a condition on a serialized property by checking its value.
        /// Supports booleans, strings, numbers, enums, objects and comparisons.
        /// </summary>
        /// <param name="serializedProperty">The serialized property to compare.</param>
        /// <param name="comparerValue">The value to compare with. If omitted, the property is checked for truthiness.</param>
        /// <returns>True if the property matches the condition; otherwise false.</returns>
        public static bool CompareSerializedProperty(SerializedProperty serializedProperty, object comparerValue)
        {
            if (serializedProperty == null) return false;

            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                {
                    bool value = serializedProperty.boolValue;

                    if (comparerValue is bool boolean) return value == boolean;

                    return value;
                }

                case SerializedPropertyType.String:
                {
                    string value = serializedProperty.stringValue;

                    if (comparerValue is string str) return value == str;

                    return !string.IsNullOrEmpty(value);
                }

                case SerializedPropertyType.Integer:
                {
                    long value = serializedProperty.longValue;

                    if (comparerValue is int intValue) return value == intValue;
                    if (comparerValue is long longValue) return value == longValue;
                    if (comparerValue is Enum enumValue) return value == Convert.ToInt64(enumValue);

                    return value != 0;
                }

                case SerializedPropertyType.Float:
                {
                    float value = serializedProperty.floatValue;

                    if (comparerValue is float floatValue) return Mathf.Approximately(value, floatValue);

                    return !Mathf.Approximately(value, 0f);
                }

                case SerializedPropertyType.Enum:
                {
                    int value = serializedProperty.enumValueIndex;

                    // Debug.Log($"{typeof(T)} IsEnum: {typeof(T).IsEnum}, IsAssignable: {serializedProperty.enumValueIndex is T}");
                    
                    if (comparerValue is Enum enumValue)
                    {
                        return value == Convert.ToInt32(enumValue);
                    }

                    return value != 0;
                }

                case SerializedPropertyType.ObjectReference:
                {
                    Object value = serializedProperty.objectReferenceValue;

                    if (comparerValue is Object obj)
                    {
                        return value == obj;
                    }

                    return value != null;
                }

                case SerializedPropertyType.ManagedReference:
                {
                    return serializedProperty.managedReferenceValue != null;
                }

                case SerializedPropertyType.ArraySize:
                {
                    int value = serializedProperty.intValue;

                    if (comparerValue is int intValue) return value == intValue;

                    return value > 0;
                }

                case SerializedPropertyType.Generic:
                {
                    // For generic types, we can check if it's a class and if the comparerValue is null or not.
                    object value = serializedProperty.boxedValue;

                    if (comparerValue == null) return value != null;

                    return value != null && value.Equals(comparerValue);
                }
                
                default:
                    Debug.LogError($"SerializedProperty type '{serializedProperty.propertyType}' is not supported for comparison.", serializedProperty.serializedObject.targetObject);
                    return false;
            }
        }
        
        /// <summary>
        /// Runs the function of the given name on the given serializedObject.
        /// </summary>
        public static object RunMethodRelativeToProperty(SerializedProperty serializedProperty, string functionName)
        {
            TryRunMethodRelativeToProperty(serializedProperty, functionName, out object result);
            return result;
        }
        
        /// <summary>
        /// Runs the function of the given name on the given serializedObject.
        /// </summary>
        public static bool TryRunMethodRelativeToProperty(SerializedProperty serializedProperty, string functionName, out object result)
            {
                result = null;
                
                if (serializedProperty == null || functionName.IsNullOrEmpty()) return false;
                
                var targetObject = serializedProperty.serializedObject.targetObject;
                Type parentObjectType = GetParentObjectOfSerializedProperty(serializedProperty).GetType();

                var method = parentObjectType.GetMethod(functionName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (method != null)
                {
                    result = method.Invoke(targetObject, null);
                    return true;
                }

                Debug.LogError($"\"{functionName}\" cannot be found or isn't supported!\nOnly methods returning an object are supported.", serializedProperty.serializedObject.targetObject);

                return false;
            }

        #endregion

        /// <summary>
        /// Generates a unique identifier for a SerializedProperty based on its serialized object's instance ID and property path.
        /// </summary>
        /// <param name="property">The SerializedProperty for which the unique identifier is generated.</param>
        /// <returns>A string representing the unique identifier of the property.</returns>
        public static string GetUniqueIDFromProperty(this SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";
        }

        #region Animations

        /// <summary>
        /// Retrieves the current "fade" value of a boolean animation associated with the specified key,
        /// creating or updating the animation if necessary.
        /// </summary>
        /// <param name="key">The unique identifier for the animation.</param>
        /// <param name="targetState">The desired target state of the animation (true or false).</param>
        /// <param name="speed">The transition speed of the animation. Defaults to 1.</param>
        /// <returns>The current "fade" value of the animation, which represents the interpolation between states.</returns>
        public static float GetBoolAnimationFade(string key, bool targetState, float speed = 1f)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            if (!_fadeAnimations.TryGetValue(key, out AnimBool fade))
            {
                // It doesn't exist, so we create it.
                fade = new(targetState)
                {
                    speed = speed
                };
                fade.valueChanged.AddListener(() => // Trigger repaint only if still animating
                {
                    if (EditorWindow.focusedWindow)
                    {
                        EditorWindow.focusedWindow.Repaint();
                    }
                });

                _fadeAnimations[key] = fade;
            }
            else
            {
                fade.target = targetState;
            }

            return fade.faded;
        }

        #endregion

        #region Paths & Folders

        /// <summary>
        /// Finds the file path of the script associated with a specific type.
        /// </summary>
        /// <param name="type">The type for which the script file path is to be located.</param>
        /// <returns>The file path of the script if found; otherwise, null.</returns>
        public static string FindScriptFilePath(Type type)
        {
            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:script");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (monoScript != null && monoScript.GetClass() == type)
                {
                    return path;
                }
            }

            Debug.LogWarning($"Script file for type {type.FullName} not found.");
            return null;
        }

        /// <summary>
        /// Recursively creates all folders in the given path.
        /// </summary>
        /// <param name="path">The full path to ensure all folders exist. Must start with 'Assets/'.</param>
        public static void CreateFoldersRecursively(string path)
        {
            if (path.IsNullOrEmpty()) return;

            string[] folders = path.Split('/', '\\');
            string currentPath = "";

            foreach (string folder in folders)
            {
                if (folder.IsNullOrEmpty()) continue;

                currentPath = currentPath.IsNullOrEmpty() ? folder : $"{currentPath}/{folder}";

                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    string parentPath = System.IO.Path.GetDirectoryName(currentPath);
                    string folderName = System.IO.Path.GetFileName(currentPath);
                    AssetDatabase.CreateFolder(parentPath, folderName);
                }
            }
        }

        #endregion

        private class NullType
        {
        }
    }
}