using System.Collections.Generic;
using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AdriKat.Toolkit.DataStructures
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> _lists = new();

        private const float ELEMENT_SPACING_VERTICAL = 4f;
        private const float ELEMENT_REMOVE_BUTTON_WIDTH = 60f;
        private const float KEY_WIDTH_PROPORTION = 0.4f;
        private const float LABEL_WIDTH_PROPORTION = 0.4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var list = GetList(property, label);
            list.DoList(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var list = GetList(property, label);
            return list.GetHeight();
        }

        private ReorderableList GetList(SerializedProperty property, GUIContent label)
        {
            if (_lists.TryGetValue(property.propertyPath, out var list)) return list;

            var listProp = property.FindPropertyRelative("list");

            list = new ReorderableList(
                property.serializedObject,
                listProp,
                draggable: true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true
            );

            // HEADER
            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, label);
            };

            // ELEMENT HEIGHT (CRITICAL FOR STRUCT SUPPORT)
            list.elementHeightCallback = index =>
            {
                var element = listProp.GetArrayElementAtIndex(index);
                var keyProp = element.FindPropertyRelative("Key");
                var valueProp = element.FindPropertyRelative("Value");

                float keyHeight = EditorGUI.GetPropertyHeight(keyProp, true);
                float valueHeight = EditorDrawUtils.GetPropertyHeightNoFoldout(valueProp);

                return Mathf.Max(keyHeight, valueHeight) + ELEMENT_SPACING_VERTICAL;
            };

            // DRAW ELEMENT
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = listProp.GetArrayElementAtIndex(index);
                var keyProp = element.FindPropertyRelative("Key");
                var valueProp = element.FindPropertyRelative("Value");

                rect.y += 2;

                float keyWidth = rect.width * KEY_WIDTH_PROPORTION;

                Rect keyRect = new Rect(
                    rect.x,
                    rect.y,
                    keyWidth - 40,
                    EditorGUI.GetPropertyHeight(keyProp, true)
                );

                Rect valueRect = new Rect(
                    rect.x + keyRect.width + 40,
                    rect.y,
                    rect.width - keyWidth, EditorDrawUtils.GetPropertyHeightNoFoldout(valueProp)
                );

                float previous = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = valueRect.width * KEY_WIDTH_PROPORTION;
                
                EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);
                EditorDrawUtils.DrawPropertyNoFoldout(valueRect, valueProp);
                
                EditorGUIUtility.labelWidth = previous;

                DrawDuplicateWarning(keyProp, keyRect, listProp, index);
            };

            // ADD
            list.onAddCallback = l =>
            {
                listProp.arraySize++;
                property.serializedObject.ApplyModifiedProperties();
            };

            _lists[property.propertyPath] = list;
            return list;
        }

        private static void DrawDuplicateWarning(SerializedProperty keyProp, Rect keyRect, SerializedProperty listProp, int currentIndex)
        {
            object currentKey = GetKeyObject(keyProp);

            if (currentKey == null) return;

            for (int i = 0; i < listProp.arraySize; i++)
            {
                if (i == currentIndex) continue;

                var otherKeyProp = listProp
                    .GetArrayElementAtIndex(i)
                    .FindPropertyRelative("Key");

                if (Equals(currentKey, GetKeyObject(otherKeyProp)))
                {
                    Rect warningRect = new Rect(
                        keyRect.xMax + 5,
                        keyRect.y,
                        16,
                        EditorGUIUtility.singleLineHeight
                    );

                    EditorGUI.LabelField(
                        warningRect,
                        EditorGUIUtility.IconContent("console.warnicon.sml", "Duplicate Key!")
                    );

                    break;
                }
            }
        }

        private static object GetKeyObject(SerializedProperty keyProp)
        {
            return keyProp.propertyType switch
            {
                SerializedPropertyType.String => keyProp.stringValue,
                SerializedPropertyType.Integer => keyProp.intValue,
                SerializedPropertyType.Float => keyProp.floatValue,
                SerializedPropertyType.Boolean => keyProp.boolValue,
                SerializedPropertyType.Enum => keyProp.enumValueIndex,
                _ => keyProp.objectReferenceValue
            };
        }
    }
}