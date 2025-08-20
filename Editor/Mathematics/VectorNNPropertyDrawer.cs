using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Mathematics
{
    [CustomPropertyDrawer(typeof(Vector2N))]
    [CustomPropertyDrawer(typeof(Vector3N))]
    public class VectorNNPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Prefix handles label & indentation
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Get properties
            SerializedProperty valueProp = property.FindPropertyRelative("value");
            SerializedProperty useNormProp = property.FindPropertyRelative("useNormalisedValue");

            // Save GUI settings
            int oldIndent = EditorGUI.indentLevel;
            float oldLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUI.indentLevel = 0;

            // Split into two rects: Vector field + toggle area
            float spacing = 20f;
            float toggleAreaWidth = 100f; // enough for "Normalized" + checkbox
            float vectorWidth = Mathf.Clamp(position.width - toggleAreaWidth - spacing, 120f, 300f);
            Rect vectorRect = new Rect(position.x, position.y, vectorWidth, position.height);
            Rect toggleRect = new Rect(vectorRect.xMax + spacing, position.y, toggleAreaWidth, position.height);

            // Draw Vector field
            EditorGUI.PropertyField(vectorRect, valueProp, GUIContent.none);

            // Draw toggle with label inline
            EditorGUIUtility.labelWidth = 70f; // "Normalized" label
            EditorGUI.PropertyField(toggleRect, useNormProp, new GUIContent("Normalized"));

            // Restore
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUI.indentLevel = oldIndent;

            EditorGUI.EndProperty();
        }
    }
}