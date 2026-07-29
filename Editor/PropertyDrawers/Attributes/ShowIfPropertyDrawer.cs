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
            
            string variableName = showIfAttribute.VariableName;

            bool shouldShow = ComputeConditionOfRelativeProperty(property, variableName, showIfAttribute);
            
            float faded = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);
            
            // The system always adds a standardVerticalSpacing between each property, even when HIDDEN by this attribute. So we compensate by subtracting it if we're faded.
            float verticalSpacingCompensation = (1 - faded) * EditorGUIUtility.standardVerticalSpacing;

            return faded * EditorGUI.GetPropertyHeight(property, label, true) - verticalSpacingCompensation;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            string variableName = showIfAttribute.VariableName;

            bool shouldShow = ComputeConditionOfRelativeProperty(property, variableName, showIfAttribute);

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

        private static bool ComputeConditionOfRelativeProperty(SerializedProperty property, string variableName, ShowIfAttribute showIfAttribute)
        {
            SerializedProperty propertyToCompare = property.FindRelativeProperty(variableName);
            
            bool shouldShow;

            if (showIfAttribute.ComparerValue != null)
            {
                object comparerValue = showIfAttribute.ComparerValue;
                
                if (showIfAttribute.ComparerValueIsVariableName)
                {
                    comparerValue = propertyToCompare.FindRelativeProperty((string)comparerValue)?.boxedValue;
                    if (comparerValue == null)
                    {
                        Debug.LogError($"ShowIfAttribute: Could not find property '{showIfAttribute.ComparerValue}' to compare with '{variableName}' for property '{property.name}'.");
                    }
                }
                
                // We need to get the type right before calling the CheckCondition function, so the switch statement is needed.
                shouldShow = comparerValue switch
                {
                    string stringValue => EditorUtils.CompareSerializedProperty(propertyToCompare, stringValue),
                    bool boolValue => EditorUtils.CompareSerializedProperty(propertyToCompare, boolValue),
                    int intValue => EditorUtils.CompareSerializedProperty(propertyToCompare, intValue),
                    Enum enumValue => EditorUtils.CompareSerializedProperty(propertyToCompare, enumValue),
                    _ => throw new InvalidOperationException($"Type {comparerValue?.GetType()} is not supported for comparison in ShowIfAttribute.")
                };
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
