using System;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Utils.Attributes
{
    [CustomPropertyDrawer(typeof(EnumAttribute))]
    public class EnumPropertyDrawer : PropertyDrawer
    {
        public readonly Color selectedBackgroundColor = new(42 / 256f, 42 / 256f, 42 / 256f, 1);
        public readonly Color unselectedBackgroundColor = new(88 / 256f, 88 / 256f, 88 / 256f, 1);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumAttribute enumAttribute = (EnumAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.String)
            {
                if (enumAttribute.StringValues == null || enumAttribute.StringValues.Length == 0)
                {
                    EditorGUI.LabelField(position, label.text, "EnumAttribute requires at least one string value.");
                    return;
                }
                DrawStringToolbar(position, property, label, enumAttribute.StringValues);
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                if (enumAttribute.IntValues == null || enumAttribute.IntValues.Length == 0)
                {
                    EditorGUI.LabelField(position, label.text, "EnumAttribute requires at least one int value.");
                    return;
                }
                DrawIntToolbar(position, property, label, enumAttribute.IntValues);
            }
            else if (property.propertyType == SerializedPropertyType.Boolean)
            {
                DrawBoolToolbar(position, property, label, enumAttribute);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "EnumAttribute only works with string, int, or bool fields.");
            }
        }

        // Draws toolbar-style buttons for string values
        private void DrawStringToolbar(Rect position, SerializedProperty property, GUIContent label, string[] values)
        {
            // Define the width of each button based on the total available space
            float buttonWidth = position.width / values.Length;

            int selectedIndex = Array.IndexOf(values, property.stringValue);
            if (selectedIndex == -1) selectedIndex = 0;

            EditorGUI.BeginProperty(position, label, property);

            for (int i = 0; i < values.Length; i++)
            {
                Rect buttonRect = new Rect(position.x + (i * buttonWidth), position.y, buttonWidth, position.height);
                bool isSelected = (selectedIndex == i);

                // Create a new style for the button
                GUIStyle buttonStyle = isSelected ? new GUIStyle(GUI.skin.box) : new GUIStyle(GUI.skin.button);
                buttonStyle.padding = new RectOffset(2, 2, 2, 2);

                // Set the background color for selected buttons
                if (isSelected)
                {
                    buttonStyle.onNormal.background = MakeTexture(2, 2, selectedBackgroundColor);
                    buttonStyle.normal.textColor = Color.yellow;
                    buttonStyle.fontStyle = FontStyle.Bold;
                }
                else
                {
                    buttonStyle.onNormal.background = MakeTexture(2, 2, unselectedBackgroundColor);
                    buttonStyle.fontStyle = FontStyle.Normal;
                }

                if (GUI.Button(buttonRect, values[i], buttonStyle))
                {
                    property.stringValue = values[i];
                }
            }

            EditorGUI.EndProperty();
        }

        // Draws toolbar-style buttons for integer values
        private void DrawIntToolbar(Rect position, SerializedProperty property, GUIContent label, int[] values)
        {
            // Define the width of each button based on the total available space
            float buttonWidth = position.width / values.Length;

            int selectedIndex = Array.IndexOf(values, property.intValue);
            if (selectedIndex == -1) selectedIndex = 0;

            EditorGUI.BeginProperty(position, label, property);

            for (int i = 0; i < values.Length; i++)
            {
                Rect buttonRect = new Rect(position.x + (i * buttonWidth), position.y, buttonWidth, position.height);
                bool isSelected = (selectedIndex == i);

                // Create a new style for the button
                GUIStyle buttonStyle = isSelected ? new GUIStyle(GUI.skin.box) : new GUIStyle(GUI.skin.button);
                buttonStyle.padding = new RectOffset(2, 2, 2, 2);

                // Set the background color for selected buttons
                if (isSelected)
                {
                    buttonStyle.onNormal.background = MakeTexture(2, 2, selectedBackgroundColor);
                    buttonStyle.normal.textColor = Color.yellow;
                    buttonStyle.fontStyle = FontStyle.Bold;
                }
                else
                {
                    buttonStyle.onNormal.background = MakeTexture(2, 2, unselectedBackgroundColor);
                    buttonStyle.fontStyle = FontStyle.Normal;
                }

                if (GUI.Button(buttonRect, values[i].ToString(), buttonStyle))
                {
                    property.intValue = values[i];
                }
            }

            EditorGUI.EndProperty();
        }

        // Draws a toolbar-style button for boolean values
        private void DrawBoolToolbar(Rect position, SerializedProperty property, GUIContent label, EnumAttribute enumAttribute)
        {
            // Define the width of each button (True/False)
            float buttonWidth = position.width / 2;

            EditorGUI.BeginProperty(position, label, property);

            // Create a new style for the button
            GUIStyle selectedButtonStyle = new GUIStyle(GUI.skin.box);
            GUIStyle unselectedButtonStyle = new GUIStyle(GUI.skin.button);

            // On normal
            selectedButtonStyle.onNormal.background = MakeTexture(2, 2, selectedBackgroundColor);
            unselectedButtonStyle.onNormal.background = MakeTexture(2, 2, unselectedBackgroundColor);
            selectedButtonStyle.normal.textColor = Color.yellow;
            selectedButtonStyle.fontStyle = FontStyle.Bold;
            selectedButtonStyle.padding = new RectOffset(2, 2, 2, 2);

            if (enumAttribute.StringValues == null || enumAttribute.StringValues.Length != 2)
            {
                EditorGUI.LabelField(position, label.text, "EnumAttribute requires exactly two string values for boolean fields.");
                return;
            }

            // Draw "True" button
            if (GUI.Button(new Rect(position.x, position.y, buttonWidth, position.height), enumAttribute.StringValues[0], property.boolValue ? selectedButtonStyle : unselectedButtonStyle))
            {
                property.boolValue = true; // Set value to true
            }

            // Draw "False" button
            if (GUI.Button(new Rect(position.x + buttonWidth, position.y, buttonWidth, position.height), enumAttribute.StringValues[1], property.boolValue ? unselectedButtonStyle : selectedButtonStyle))
            {
                property.boolValue = false; // Set value to false
            }

            EditorGUI.EndProperty();
        }

        // Utility function to create a texture of the specified color
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
