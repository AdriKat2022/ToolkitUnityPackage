using System.Reflection;
using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(ButtonActionAttribute))]
    public class ButtonActionPropertyDrawer : PropertyDrawer
    {
        private const float PADDING = 10;
        private const float SPACING = 2;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the attribute's data.
            var buttonActionAttribute = (ButtonActionAttribute)attribute;

            if (buttonActionAttribute.showButtonBelow)
            {
                EditorGUI.PropertyField(position, property, label, true);
                position.y += buttonActionAttribute.heightSpacing;
                position.y += EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            
            int actionCount = buttonActionAttribute.actionNames.Length;
            
            float buttonWidth = (position.width - PADDING) / actionCount - SPACING * (actionCount - 1);
            
            float buttonHeight = EditorGUIUtility.singleLineHeight;
            
            // For each action name, get the method instance and make a button.
            for (int i = 0; i < actionCount; i++)
            {
                var actionName = buttonActionAttribute.actionNames[i];

                if (actionName.IsNullOrEmpty()) continue;
                
                var method = property.serializedObject.targetObject.GetType().GetMethod(actionName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                var buttonStyle = new GUIStyle(GUI.skin.button);
                
                if (method == null)
                {
                    buttonStyle.normal.textColor = Color.red;
                }
                
                Rect buttonRect = new Rect(position.x + i * buttonWidth + i * SPACING, position.y, buttonWidth, buttonHeight);
                
                if (GUI.Button(buttonRect, ObjectNames.NicifyVariableName(actionName), buttonStyle))
                {
                    if (method != null)
                    {
                        method.Invoke(property.serializedObject.targetObject, null);
                    }
                    else
                    {
                        Debug.LogError($"Method {actionName} not found in {property.serializedObject.targetObject.GetType().Name}");
                    }
                }
            }
            
            position.y += buttonActionAttribute.heightSpacing;
            position.y += buttonHeight + EditorGUIUtility.standardVerticalSpacing;
            
            if (!buttonActionAttribute.showButtonBelow)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var buttonActionAttribute = (ButtonActionAttribute)attribute;
            
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + buttonActionAttribute.heightSpacing;
        }
    }
}