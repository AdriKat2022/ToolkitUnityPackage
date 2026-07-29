using System;
using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            
            if (showIfAttribute.ShowDisabledField)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            
            bool shouldShow = ComputeConditionOfRelativeProperty(property, showIfAttribute);
            
            float faded = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);
            
            // The system always adds a standardVerticalSpacing between each property, even when HIDDEN by this attribute. So we compensate by subtracting it if we're faded.
            float verticalSpacingCompensation = (1 - faded) * EditorGUIUtility.standardVerticalSpacing;

            return faded * EditorGUI.GetPropertyHeight(property, label, true) - verticalSpacingCompensation;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;

            bool shouldShow = ComputeConditionOfRelativeProperty(property, showIfAttribute);

            if (showIfAttribute.ShowDisabledField)
            {
                GUI.enabled = shouldShow;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
            }
            else
            {
                ManageFadeAnimation(position, property, label, shouldShow);
            }
        }

        private static bool ComputeConditionOfRelativeProperty(SerializedProperty property, ShowIfAttribute showIfAttribute)
        {
            var variableName = showIfAttribute.VariableName;
            SerializedProperty propertyToCompare = property.FindRelativeProperty(variableName);
            
            bool shouldShow;

            if (propertyToCompare == null)
            {
                // The property doesn't exist, but might be a function.
                if (!EditorUtils.TryRunMethodRelativeToProperty(property, variableName, out object result))
                {
                    Debug.LogError($"ShowIfAttribute: Failed to find property or function '{variableName}'.");
                    return false;
                }

                // Was a method.
                if (result is bool boolValue)
                {
                    return boolValue;
                }

                Debug.LogError($"ShowIfAttribute: Methods returning a non-bool result are not supported.");
                return false;
            }
            
            if (showIfAttribute.ComparerValue != null)
            {
                object comparerValue = showIfAttribute.ComparerValue;
                
                if (showIfAttribute.ComparerValueIsVariableName)
                {
                    comparerValue = property.FindRelativeProperty((string)comparerValue)?.boxedValue;
                    if (comparerValue == null)
                    {
                        Debug.LogError($"ShowIfAttribute: Could not find property '{showIfAttribute.ComparerValue}' to compare with '{variableName}' for property '{property.name}'.");
                    }
                }
                
                shouldShow = EditorUtils.CompareSerializedProperty(propertyToCompare, comparerValue);
            }
            else
            {
                shouldShow = EditorUtils.CompareSerializedProperty(propertyToCompare, true);
            }
            
            if (showIfAttribute.Invert)
            {
                shouldShow = !shouldShow;
            }

            return shouldShow;
        }

        private static void ManageFadeAnimation(Rect position, SerializedProperty property, GUIContent label, bool shouldShow)
        {
            float fade = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);

            // Get the full height the property would take at full opacity
            float fullHeight = EditorGUI.GetPropertyHeight(property, label, true);
            float labelWidth = EditorGUIUtility.labelWidth;
            
            EditorDrawUtils.DrawClippedFadeGroup(position, fade, fullHeight, rect =>
            {
                // Restore labelWidth (it can change when entering a group).
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUI.PropertyField(rect, property, label, true);
            }, applyAlpha: true);
        }
    }
}
