using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        // readonly dictionaries for colors

        private Dictionary<RequiredAttribute.WarningTypeEnum, Color> _colors = new()
    {
        { RequiredAttribute.WarningTypeEnum.Error, new Color(1f, .5f, .4f) },
        { RequiredAttribute.WarningTypeEnum.Warning, Color.yellow },
        { RequiredAttribute.WarningTypeEnum.Info, new Color(.75f, 1, 1) }
    };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RequiredAttribute requiredAttribute = (RequiredAttribute)attribute;

            bool isInvalid = IsPropertyInvalid(property, requiredAttribute.AllowEmptyString);

            if (isInvalid)
            {
                // Change color to indicate error
                GUI.color = _colors[requiredAttribute.WarningType];
            }

            // Draw the property field
            EditorGUI.PropertyField(position, property, label, true);

            // Reset color
            GUI.color = Color.white;

            // Show a warning if the field is empty/null
            if (isInvalid)
            {
                position.y += EditorGUI.GetPropertyHeight(property, label, true);
                position.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.HelpBox(position, requiredAttribute.WarningMessage, (MessageType)requiredAttribute.WarningType);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RequiredAttribute requiredAttribute = (RequiredAttribute)attribute;

            bool isInvalid = IsPropertyInvalid(property, requiredAttribute.AllowEmptyString);

            float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);

            // Add extra space for the warning message if the field is invalid
            return isInvalid ? baseHeight + EditorGUIUtility.singleLineHeight + 2 : baseHeight;
        }

        private bool IsPropertyInvalid(SerializedProperty property, bool allowEmptyString)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                return property.objectReferenceValue == null;
            }
            if (property.propertyType == SerializedPropertyType.String)
            {
                return (property.stringValue == null || (string.IsNullOrEmpty(property.stringValue) && !allowEmptyString));
            }
            return false; // Other types are not checked
        }
    }
}
