using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

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
        
        /// <summary>
        /// Get a boolean value from a method or field from an instanced object.
        /// </summary>
        /// <param name="serializedObject">The object-instance to check.</param>
        /// <param name="conditionName">The name of the bool-returning method or bool field.</param>
        /// <returns>The value of the bool field or the value returned by the method.</returns>
        public static bool CheckConditionFromObject(SerializedObject serializedObject, string conditionName)
        {
            if (serializedObject == null || string.IsNullOrEmpty(conditionName)) return false;
            
            var targetObject = serializedObject.targetObject;
            
            var method = targetObject.GetType().GetMethod(conditionName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                if (method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
                {
                    return (bool)method.Invoke(targetObject, null);
                }
                Debug.LogError($"WarnAttribute: \"{conditionName}\" must return a boolean value and have no parameters!", serializedObject.targetObject);
                return false;
            }
            
            var field = targetObject.GetType().GetField(conditionName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                if (field.FieldType == typeof(bool))
                {
                    return (bool)field.GetValue(targetObject);
                }
                Debug.LogError($"WarnAttribute: \"{conditionName}\" is not a boolean field!", serializedObject.targetObject);
                return false;
            }

            Debug.LogError($"WarnAttribute: \"{conditionName}\" cannot be found or isn't supported!\nOnly bool fields and methods returning bool are supported.", serializedObject.targetObject);
            
            return false;
        }

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
                // fade.valueChanged.AddListener(RepaintAllInspectors);
                
                _fadeAnimations[key] = fade;
            }
            else
            {
                fade.target = targetState;
            }
            
            return fade.faded;
        }
        
        public static string GetUniqueIDFromProperty(this SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";
        }
    }
}