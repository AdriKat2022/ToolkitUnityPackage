using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEngine;

namespace AdriKat.Toolkit.DesignPatterns
{
    [CustomPropertyDrawer(typeof(ModuleType), true)]
    public class ModuleTypePropertyDrawer : PropertyDrawer
    {
        private const float Padding = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty enabledProperty = property.FindPropertyRelative("enabled");

            Rect header = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.BeginProperty(position, label, property);

            bool isEnabled = enabledProperty.boolValue;
            
            string uniqueIDFromProperty = property.GetUniqueIDFromProperty();
            bool foldout = EditorPrefs.GetBool(uniqueIDFromProperty, false);
            
            bool expanded = EditorDrawUtils.DrawModuleHeader(
                header,
                new GUIContent(label.text),
                ref isEnabled,
                ref foldout,
                false,
                Color.gray2
            );
            
            EditorPrefs.SetBool(uniqueIDFromProperty, foldout);
            enabledProperty.boolValue = isEnabled;
            
            if (expanded)
            {
                EditorGUI.indentLevel++;

                float y = position.y + EditorGUIUtility.singleLineHeight + Padding;

                foreach (SerializedProperty child in property)
                {
                    if (child.name == "enabled") continue;

                    Rect field = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

                    EditorGUI.PropertyField(field, child, true);

                    y += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            string uniqueIDFromProperty = property.GetUniqueIDFromProperty();
            bool foldout = EditorPrefs.GetBool(uniqueIDFromProperty, false);

            float height = EditorGUIUtility.singleLineHeight;

            if (!foldout) return height;

            height += Padding;
            
            foreach (SerializedProperty child in property)
            {
                if (child.name == "enabled") continue;

                height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            height -= EditorGUIUtility.standardVerticalSpacing;

            return height;
        }
    }
}