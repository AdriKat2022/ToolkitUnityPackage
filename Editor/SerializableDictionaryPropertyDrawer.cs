using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.DataStructure
{

    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class KeyValuePairDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 60f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Foldout
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded,
                label,
                true
            );

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                SerializedProperty listProp = property.FindPropertyRelative("list");
                float y = position.y + EditorGUIUtility.singleLineHeight + Spacing;

                // Track keys to detect duplicates
                HashSet<object> keysSeen = new HashSet<object>();
                for (int i = 0; i < listProp.arraySize; i++)
                {
                    SerializedProperty kvProp = listProp.GetArrayElementAtIndex(i);
                    SerializedProperty keyProp = kvProp.FindPropertyRelative("Key");
                    SerializedProperty valueProp = kvProp.FindPropertyRelative("Value");

                    Rect keyRect = new Rect(position.x, y, position.width / 2 - 4, EditorGUIUtility.singleLineHeight);
                    Rect valueRect = new Rect(position.x + position.width / 2, y, position.width / 2 - ButtonWidth - 6, EditorGUIUtility.singleLineHeight);
                    Rect removeRect = new Rect(position.x + position.width - ButtonWidth, y, ButtonWidth, EditorGUIUtility.singleLineHeight);

                    // Draw key/value fields
                    EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
                    EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

                    // Remove button
                    if (GUI.Button(removeRect, "Remove"))
                    {
                        listProp.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    // Duplicate detection
                    object keyObj = keyProp.propertyType switch
                    {
                        SerializedPropertyType.String => keyProp.stringValue,
                        SerializedPropertyType.Integer => keyProp.intValue,
                        SerializedPropertyType.Float => keyProp.floatValue,
                        SerializedPropertyType.Boolean => keyProp.boolValue,
                        _ => keyProp.objectReferenceValue
                    };

                    if (keyObj != null && !keysSeen.Add(keyObj))
                    {
                        // Duplicate found, draw warning icon
                        Rect warningRect = new Rect(keyRect.xMax - 18, keyRect.y, 16, keyRect.height);
                        EditorGUI.LabelField(warningRect, new GUIContent("\u26A0", "Duplicate Key!"));
                    }

                    y += EditorGUIUtility.singleLineHeight + Spacing;
                }

                // Add button
                Rect addRect = new Rect(position.x, y, ButtonWidth, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(addRect, "Add"))
                {
                    listProp.arraySize++;
                    SerializedProperty newKV = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                    newKV.FindPropertyRelative("Key").serializedObject.ApplyModifiedProperties();
                    newKV.FindPropertyRelative("Value").serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            SerializedProperty listProp = property.FindPropertyRelative("list");
            float rows = Mathf.Max(listProp.arraySize, 0); // Space for Add button
            return (rows + 2) * (EditorGUIUtility.singleLineHeight + Spacing);
        }
    }

}