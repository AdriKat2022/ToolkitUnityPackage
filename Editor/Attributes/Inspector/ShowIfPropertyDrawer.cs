using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, AnimBool> _fadeAnimations = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            string variableName = showIfAttribute.VariableName;
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(variableName);

            if (conditionProperty == null || conditionProperty.propertyType != SerializedPropertyType.Boolean)
            {
                Debug.LogError($"ShowIfAttribute: \"{variableName}\" must be a boolean property!");
                return;
            }

            bool shouldShow = conditionProperty.boolValue;
            if (showIfAttribute.Invert)
            {
                shouldShow = !shouldShow;
            }

            if (showIfAttribute.DisableInsteadOfHidding)
            {
                GUI.enabled = shouldShow;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
            }
            else
            {
                ManageFadeAnimation(position, property, label, variableName, shouldShow);
            }
        }

        private void ManageFadeAnimation(Rect position, SerializedProperty property, GUIContent label, string variableName, bool shouldShow)
        {
            // Get or create the fade animation
            if (!_fadeAnimations.TryGetValue(variableName, out AnimBool fade))
            {
                fade = new(shouldShow)
                {
                    speed = 0.5f
                };
                fade.valueChanged.AddListener(() => property.serializedObject.ApplyModifiedProperties());
                _fadeAnimations[variableName] = fade;
            }

            // Update fade target
            fade.target = shouldShow;

            // Smooth height transition
            float height = fade.faded * EditorGUI.GetPropertyHeight(property, label, true);

            Rect fadeRect = new Rect(position.x, position.y, position.width, height);
            if (fade.faded > 0.5f)
            {
                EditorGUI.PropertyField(fadeRect, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            string variableName = showIfAttribute.VariableName;
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(variableName);

            if (conditionProperty == null || conditionProperty.propertyType != SerializedPropertyType.Boolean)
            {
                return 0;
            }

            bool shouldShow = conditionProperty.boolValue;
            if (showIfAttribute.Invert)
            {
                shouldShow = !shouldShow;
            }

            if (showIfAttribute.DisableInsteadOfHidding)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            // Get or create the fade animation
            if (!_fadeAnimations.TryGetValue(variableName, out AnimBool fade))
            {
                fade = new AnimBool(shouldShow);
                _fadeAnimations[variableName] = fade;
            }

            // Smooth height transition
            return fade.faded * EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
