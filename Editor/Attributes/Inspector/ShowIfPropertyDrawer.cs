using System;
using AdriKat.Toolkit.Utils;
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

            bool shouldShow = ComputeCondition(property.serializedObject, variableName, showIfAttribute);
            
            float faded = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);

            return faded * EditorGUI.GetPropertyHeight(property, label, true);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            string variableName = showIfAttribute.VariableName;

            bool shouldShow = ComputeCondition(property.serializedObject, variableName, showIfAttribute);

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

        private static bool ComputeCondition(SerializedObject serializedObject, string variableName, ShowIfAttribute showIfAttribute)
        {
            bool shouldShow;

            if (showIfAttribute.NeedsComparison)
            {
                object valueToCompare = serializedObject.GetPropertyValue(variableName);
                object otherValue = showIfAttribute.ComparerValue;
                
                if (showIfAttribute.ComparerValueIsVariableName)
                {
                    otherValue = serializedObject.GetPropertyValue((string)otherValue);
                }
                
                if (valueToCompare.GetType() != otherValue.GetType())
                {
                    throw new InvalidOperationException($"{valueToCompare.GetType()} and {otherValue.GetType()} are not the same type and cannot be compared.");
                }
                
                // We need to get the type right before calling the CheckCondition function, so the switch statement is needed.
                shouldShow = otherValue switch
                {
                    string stringValue => EditorUtils.CheckConditionFromObject(serializedObject, variableName, stringValue),
                    bool boolValue => EditorUtils.CheckConditionFromObject(serializedObject, variableName, boolValue),
                    int intValue => EditorUtils.CheckConditionFromObject(serializedObject, variableName, intValue),
                    Enum enumValue => EditorUtils.CheckConditionFromObject(serializedObject, variableName, enumValue),
                    _ => EditorUtils.CheckConditionFromObject(serializedObject, variableName, otherValue)
                };
            }
            else
            {
                shouldShow = EditorUtils.CheckConditionFromObject(serializedObject, variableName);
            }
            
            if (showIfAttribute.Invert)
            {
                shouldShow = !shouldShow;
            }

            return shouldShow;
        }

        private void ManageFadeAnimation(Rect position, SerializedProperty property, GUIContent label, bool shouldShow)
        {
            float fade = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);

            // Get the full height the property would take at full opacity
            float fullHeight = EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
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
