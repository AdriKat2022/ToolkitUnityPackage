using System;
using System.Reflection;
using AdriKat.Toolkit.Utility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer
    {
        private SerializedProperty _minField;
        private SerializedProperty _maxField;
        private bool _initialized;

        private void FindMaxAndMinFieldsInProperty(SerializedProperty serializedProperty)
        {
            _minField = null;
            _maxField = null;

            // Debug.Log($"Finding fields in serializedProperty {serializedProperty.type}");
            var fieldValueType = fieldInfo.FieldType;
            
            foreach (FieldInfo field in fieldValueType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                // Debug.Log($"Found field {field}");
                
                if (field.GetCustomAttribute<MinOfSliderAttribute>() != null)
                {
                    _minField = serializedProperty.FindPropertyRelative(field.Name);

                    // Debug.Log($"Finding field {field.Name} for min of slider in {fieldValueType.Name}: {_minField}");
                    
                    if (_maxField != null) break; // Bail out early if we already have everything.
                }
                else if (field.GetCustomAttribute<MaxOfSliderAttribute>() != null)
                {
                    _maxField = serializedProperty.FindPropertyRelative(field.Name);
                    
                    // Debug.Log($"Finding field {field.Name} for min of slider in {fieldValueType.Name}: {_maxField}");
                    
                    if (_minField != null) break; // Same.
                }
            }
            
            _initialized = true;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Get the target object.
            Object targetObject = property.serializedObject.targetObject;

            // Trouve les champs marqués [MinOfSlider] et [MaxOfSlider]
            if (!_initialized) FindMaxAndMinFieldsInProperty(property);

            if (_minField == null && _maxField == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Attribute [MinOfSlider] and [MaxOfSlider] are missing."), EditorStyles.helpBox);
                EditorGUI.EndProperty();
                return;
            }
            if (_minField == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Attribute [MinOfSlider] is missing."), EditorStyles.helpBox);
                EditorGUI.EndProperty();
                return;
            }
            if (_maxField == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Attribute [MaxOfSlider] is missing."), EditorStyles.helpBox);
                EditorGUI.EndProperty();
                return;
            }
            if (_minField.propertyType != _maxField.propertyType)
            {
                EditorGUI.LabelField(position, label, new GUIContent($"Min and Max fields must be of the same type (minType: {_minField.propertyType}; maxType: {_maxField.propertyType})."), EditorStyles.helpBox);
                EditorGUI.EndProperty();
                return;
            }

            // Get the attribute
            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)fieldInfo.GetCustomAttribute(typeof(MinMaxSliderAttribute));
            
            float minValue = 0;
            float maxValue = 0;

            bool typeIsInt;
            
            if (_minField.propertyType == SerializedPropertyType.Float)
            {
                typeIsInt = false;
                minValue = _minField.floatValue;
                maxValue = _maxField.floatValue;
            }
            else
            {
                typeIsInt = true;
                minValue = _minField.intValue;
                maxValue = _maxField.intValue;
            }

            // Show label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Compute Rects
            var manualFieldsWidth = 50;
            var spacing = 5;
            Rect minRect = new(position.x, position.y, manualFieldsWidth, position.height);
            Rect sliderRect = new(minRect.xMax + spacing, position.y, position.width - (manualFieldsWidth + spacing) * 2, position.height);
            Rect maxRect = new(sliderRect.xMax + spacing, position.y, manualFieldsWidth, position.height);

            bool isSliderDrawable = sliderRect.width > 10;
            
            // Affiche les champs min et max
            EditorGUI.BeginChangeCheck();

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            if (typeIsInt)
            {
                minValue = EditorGUI.IntField(minRect, GUIContent.none, (int)minValue);
                maxValue = EditorGUI.IntField(maxRect, GUIContent.none, (int)maxValue);
                
                if (isSliderDrawable)
                {
                    EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, (int)minMaxSliderAttribute.Minimum, (int)minMaxSliderAttribute.Maximum);
                }
            }
            else
            {
                minValue = EditorGUI.FloatField(minRect, GUIContent.none, minValue);
                maxValue = EditorGUI.FloatField(maxRect, GUIContent.none, maxValue);
                
                if (isSliderDrawable)
                {
                    EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, minMaxSliderAttribute.Minimum, minMaxSliderAttribute.Maximum);
                }
            }

            EditorGUI.indentLevel = oldIndent;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetObject, "MinMaxSlider change");
                if (typeIsInt)
                {
                    _minField.intValue = (int)minValue;
                    _maxField.intValue = (int)maxValue;
                }
                else
                {
                    _minField.floatValue = minValue;
                    _maxField.floatValue = maxValue;
                }
                EditorUtility.SetDirty(targetObject);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}