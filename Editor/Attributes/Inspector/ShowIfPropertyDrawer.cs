using System.Collections.Generic;
using AdriKat.Toolkit.Utils;
using UnityEditor;
using UnityEditor.AnimatedValues;
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
                return (EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing);
            }
            
            string variableName = showIfAttribute.VariableName;

            bool shouldShow = EditorUtils.CheckConditionFromObject(property.serializedObject, variableName);
            if (showIfAttribute.Invert)
            {
                shouldShow = !shouldShow;
            }
            
            float faded = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);

            // Smooth height transition
            return faded * (EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            string variableName = showIfAttribute.VariableName;

            bool shouldShow = EditorUtils.CheckConditionFromObject(property.serializedObject, variableName);
            if (showIfAttribute.Invert)
            {
                shouldShow = !shouldShow;
            }

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

        private void ManageFadeAnimation(Rect position, SerializedProperty property, GUIContent label, bool shouldShow)
        {
            float faded = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);
            
            // Smooth height transition
            float height = faded * EditorGUI.GetPropertyHeight(property, label, true);

            Rect fadeRect = new Rect(position.x, position.y, position.width, height);
            if (faded > 0.5f)
            {
                EditorGUI.PropertyField(fadeRect, property, label, true);
            }
        }
    }
}
