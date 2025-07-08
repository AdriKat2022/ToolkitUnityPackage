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
        /// Evaluates a condition on a serialized object by checking for a boolean field or method
        /// matching the specified condition name.
        /// </summary>
        /// <param name="serializedObject">The serialized object containing the condition to evaluate.</param>
        /// <param name="conditionName">The name of the field or method to check within the serialized object.</param>
        /// <returns>True if the condition is valid and evaluates to true; otherwise, false.</returns>
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
                fade.valueChanged.AddListener(RepaintAllInspectors);
                
                _fadeAnimations[key] = fade;
            }
            else
            {
                fade.target = targetState;
            }
            
            return fade.faded;
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
    }
}