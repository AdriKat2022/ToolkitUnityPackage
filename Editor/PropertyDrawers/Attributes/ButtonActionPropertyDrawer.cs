using System.Reflection;
using AdriKat.Toolkit.Utility.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(ButtonActionAttribute))]
    public class ButtonActionPropertyDrawer : PropertyDrawer
    {
        private const float PADDING = 10;
        private const float SPACING = 2;
        
        #region UI Toolkit
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var buttonActionAttribute = (ButtonActionAttribute)attribute;

            VisualElement root = new VisualElement();
            VisualElement buttonContainer = MakeButtonsForProperty(property, buttonActionAttribute);

            if (buttonActionAttribute.showButtonBelow)
            {
                root.Add(new PropertyField(property));
                root.Add(buttonContainer);
                buttonContainer.style.marginTop = buttonActionAttribute.heightSpacing;
            }
            else
            {
                root.Add(buttonContainer);
                root.Add(new PropertyField(property));
                buttonContainer.style.marginBottom = buttonActionAttribute.heightSpacing;
            }

            return root;
        }

        private static VisualElement MakeButtonsForProperty(SerializedProperty property, ButtonActionAttribute buttonActionAttribute)
        {
            var buttonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            int actionCount = buttonActionAttribute.actionNames.Length;

            string[] optionalNames = buttonActionAttribute.customNames?.Split('|');
            
            for (int i = 0; i < actionCount; i++)
            {
                string actionName = buttonActionAttribute.actionNames[i];

                if (string.IsNullOrEmpty(actionName)) continue;

                var method = property.serializedObject.targetObject
                    .GetType()
                    .GetMethod(
                        actionName,
                        BindingFlags.NonPublic |
                        BindingFlags.Public |
                        BindingFlags.Instance
                    );

                var button = new Button();

                if (optionalNames == null)
                {
                    button.text = buttonActionAttribute.nicifyVariableNames ? ObjectNames.NicifyVariableName(actionName) : actionName;
                }
                else
                {
                    button.text = optionalNames[i%optionalNames.Length];
                }

                if (method == null)
                {
                    button.style.color = Color.red;
                }

                button.clicked += () =>
                {
                    if (method != null)
                    {
                        method.Invoke(property.serializedObject.targetObject, null);
                    }
                    else
                    {
                        Debug.LogError($"Method {actionName} not found in {property.serializedObject.targetObject.GetType().Name}");
                    }
                };

                button.style.flexGrow = 1;
                buttonContainer.Add(button);
            }

            return buttonContainer;
        }

        #endregion
        
        #region IMGUI
        
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
        
        #endregion
    }
}