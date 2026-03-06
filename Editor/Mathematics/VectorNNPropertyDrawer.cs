using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.Mathematics
{
    [CustomPropertyDrawer(typeof(Vector2N))]
    [CustomPropertyDrawer(typeof(Vector3N))]
    public class VectorNNPropertyDrawer : PropertyDrawer
    {
        private const float SPACING = 10f;
        private const float TOGGLE_AREA_WIDTH = 100f;

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

            // EditorGUI.indentLevel = 0;

            // Split into two rects: Vector field + toggle area
            float vectorWidth = Mathf.Clamp(position.width - TOGGLE_AREA_WIDTH - SPACING, 120f, 300f);
            Rect vectorRect = new Rect(position.x, position.y, vectorWidth, position.height);
            Rect toggleRect = new Rect(vectorRect.xMax + SPACING, position.y, TOGGLE_AREA_WIDTH, position.height);

            // Draw Vector field
            EditorGUI.PropertyField(vectorRect, valueProp, GUIContent.none);

            // Draw toggle with label inline
            EditorGUIUtility.labelWidth = 70f; // "Normalized" label
            useNormProp.boolValue = EditorGUI.ToggleLeft(toggleRect, new GUIContent("Normalized"), useNormProp.boolValue);

            // Restore
            EditorGUIUtility.labelWidth = oldLabelWidth;
            // EditorGUI.indentLevel = oldIndent;

            EditorGUI.EndProperty();
        }
    }
}