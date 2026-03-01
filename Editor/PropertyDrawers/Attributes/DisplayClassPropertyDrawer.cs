using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(DisplayClassAttribute))]
    public class DisplayClassPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference && property.propertyType != SerializedPropertyType.ManagedReference) 
            {
                EditorGUI.HelpBox(position, "Display class attribute can only be used on Object fields.", MessageType.Error);
                return;
            }
            
            // Get the attribute's data.
            var displayClassAttribute = (DisplayClassAttribute)attribute;

            var foldoutKey = $"{property.serializedObject}{property.propertyPath}";
            
            bool folded = EditorPrefs.GetBool(foldoutKey);
            
            EditorDrawUtils.DrawObjectWithFoldout(position,
                property,
                label,
                displayClassAttribute.actionName,
                () => EditorUtils.RunMethodFromSerializedObject<Object>(property.serializedObject, displayClassAttribute.functionActionName),
                ref folded);
            
            EditorPrefs.SetBool(foldoutKey, folded);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference && property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            
            bool folded = EditorPrefs.GetBool($"{property.serializedObject}{property.propertyPath}");
            return EditorDrawUtils.GetPropertyHeightOfObjectContent(property, folded);
        }
    }
}