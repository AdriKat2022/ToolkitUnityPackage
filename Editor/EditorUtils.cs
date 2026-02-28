using System;
using System.Collections.Generic;
using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdriKat.Toolkit.Utils
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
            var inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
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

        /// <summary>
        /// Retrieves the value of a property.
        /// </summary>
        public static object GetPropertyValue(this SerializedObject serializedObject, string propertyName)
        {
            return serializedObject.FindProperty(propertyName)?.boxedValue;
        }
        
        /// <summary>
        /// Evaluates a condition on a serialized object by checking for a boolean field or method matching the specified condition name.<br/>
        /// If the comparerValue type matches the 
        /// </summary>
        /// <param name="serializedObject">The serialized object containing the condition to evaluate.</param>
        /// <param name="conditionName">The name of the field or method to check within the serialized object.</param>
        /// <param name="comparerValue">The value to compare with the condition field if the type matches.</param>
        /// <returns>True if the condition is valid and evaluates to true; otherwise, false.</returns>
        public static bool CheckConditionFromObject<T>(SerializedObject serializedObject, string conditionName, T comparerValue = default)
        {
            if (serializedObject == null || string.IsNullOrEmpty(conditionName)) return false;
            
            var targetObject = serializedObject.targetObject;

            Type parentObjectType = targetObject.GetType();
            
            // Try for method.
            var method = parentObjectType.GetMethod(conditionName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                // Check if it's a method that returns the same type as the comparerValue and has no parameters.
                if (method.ReturnType == typeof(T) && method.GetParameters().Length == 0)
                {
                    return comparerValue.Equals((T)method.Invoke(targetObject, null));
                }
                
                // Check if it's a bool method and has no parameters.
                if (method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
                {
                    return (bool)method.Invoke(targetObject, null);
                }
                Debug.LogError($"WarnAttribute: \"{conditionName}\" must return a boolean value and have no parameters!", serializedObject.targetObject);
                return false;
            }
            
            // Try for field.
            var field = parentObjectType.GetField(conditionName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                object fieldValue = field.GetValue(targetObject);
                
                if (field.FieldType == typeof(T))
                {
                    return comparerValue.Equals((T)fieldValue);
                }
                if (field.FieldType == comparerValue.GetType())
                {
                    return comparerValue.Equals(fieldValue);
                }
                if (field.FieldType == typeof(bool))
                {
                    return (bool)fieldValue;
                }
                if (field.FieldType == typeof(string))
                {
                    return string.IsNullOrEmpty((string)fieldValue);
                }
                if (field.FieldType.IsClass)
                {
                    return fieldValue != null;
                }
                Debug.LogError($"WarnAttribute: \"{conditionName}\" is not a boolean field or a object field!", serializedObject.targetObject);
                return false;
            }

            Debug.LogError($"WarnAttribute: \"{conditionName}\" cannot be found or isn't supported!\nOnly bool fields, class fields, and parameterless methods returning a bool are supported.", serializedObject.targetObject);
            return false;
        }
        
        /// <summary>
        /// Evaluates a condition on a serialized object by checking for a boolean field or method
        /// matching the specified condition name.
        /// </summary>
        /// <param name="serializedObject">The serialized object containing the condition to evaluate.</param>
        /// <param name="conditionName">The name of the field or method to check within the serialized object.</param>
        /// <returns>True if the condition is valid and evaluates to true; otherwise, false.</returns>
        public static bool CheckConditionFromObject(SerializedObject serializedObject, string conditionName)
        {
            return CheckConditionFromObject(serializedObject, conditionName, new NullType());
        }
        
        public static Object CreateObjectFromFunction(SerializedObject serializedObject, string functionName)
        {
            if (serializedObject == null || functionName.IsNullOrEmpty()) return null;
            
            var targetObject = serializedObject.targetObject;
            Type parentObjectType = targetObject.GetType();
            
            var method = parentObjectType.GetMethod(functionName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                return (Object)method.Invoke(targetObject, null);
            }
            
            Debug.LogError($"\"{functionName}\" cannot be found or isn't supported!\nOnly methods returning an object are supported.", serializedObject.targetObject);
            
            return null;
        }
        
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