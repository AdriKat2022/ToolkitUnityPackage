using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(DisplayClassAttribute))]
    public class DisplayClassPropertyDrawer : PropertyDrawer
    {
        #region UI Toolkit

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            if (property.propertyType != SerializedPropertyType.ObjectReference &&
                property.propertyType != SerializedPropertyType.ManagedReference)
            {
                root.Add(new HelpBox(
                    "Display class attribute can only be used on Object fields.",
                    HelpBoxMessageType.Error
                ));

                return root;
            }

            var displayClassAttribute = (DisplayClassAttribute)attribute;

            var foldout = new Foldout
            {
                text = property.displayName
            };

            var field = new PropertyField(property);
            foldout.Add(field);

            foldout.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetBool(
                    $"{property.serializedObject}{property.propertyPath}",
                    evt.newValue
                );
            });

            foldout.value = EditorPrefs.GetBool(
                $"{property.serializedObject}{property.propertyPath}"
            );

            root.Add(foldout);

            return root;
        }

        #endregion
        
        #region IMGUI
        
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
                () => (Object)EditorUtils.RunMethodRelativeToProperty(property, displayClassAttribute.functionActionName),
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
        
        #endregion
    }
}