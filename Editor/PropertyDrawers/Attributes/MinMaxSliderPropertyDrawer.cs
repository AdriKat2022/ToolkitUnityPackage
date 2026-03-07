using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer
    {
        private FieldInfo _minField;
        private FieldInfo _maxField;
        private bool _initialized;

        private void FindMaxAndMinFields(Type fieldValueType)
        {
            _minField = null;
            _maxField = null;

            foreach (FieldInfo field in fieldValueType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<MinOfSliderAttribute>() != null)
                {
                    _minField = field;

                    if (_maxField != null) break; // Bail out early if we already have everything.
                }
                else if (field.GetCustomAttribute<MaxOfSliderAttribute>() != null)
                {
                    _maxField = field;
                    
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
            FieldInfo fieldInfo = targetObject.GetType().GetField(property.name);
            object fieldValue = fieldInfo.GetValue(targetObject);

            if (fieldValue == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Null"));
                EditorGUI.EndProperty();
                return;
            }

            // Trouve les champs marqués [MinOfSlider] et [MaxOfSlider]
            if (!_initialized) FindMaxAndMinFields(fieldValue.GetType());

            if (_minField == null && _maxField == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Attribute [MinOfSlider] and [MaxOfSlider] are missing."), EditorStyles.helpBox);
                EditorGUI.EndProperty();
                return;
            }
            else if (_minField == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Attribute [MinOfSlider] is missing."), EditorStyles.helpBox);
                EditorGUI.EndProperty();
                return;
            }
            else if (_maxField == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Attribute [MaxOfSlider] is missing."), EditorStyles.helpBox);
                EditorGUI.EndProperty();
                return;
            }
            
            // Get the attribute
            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)fieldInfo.GetCustomAttribute(typeof(MinMaxSliderAttribute));
            
            float minValue = (float)_minField.GetValue(fieldValue);
            float maxValue = (float)_maxField.GetValue(fieldValue);
            
            // int minValueInt = (int)minMaxSliderAttribute.Minimum;
            // int maxValueInt = (int)maxValue;
            
            // Show label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Compute Rects for the slider.
            Rect minRect = new Rect(position.x, position.y, 40, position.height);
            Rect sliderRect = new Rect(position.x + 45, position.y, position.width - 90, position.height);
            Rect maxRect = new Rect(position.x + position.width - 40, position.y, 40, position.height);

            // Affiche les champs min et max
            EditorGUI.BeginChangeCheck();
            

            // if (minMaxSliderAttribute.IsInteger)
            // {
            //     minValue = EditorGUI.IntField(minRect, minValueInt);
            //     maxValue = EditorGUI.IntField(maxRect, maxValueInt);
            //     EditorGUI.MinMaxSlider(sliderRect, ref minValueInt, ref maxValueInt, (int)minMaxSliderAttribute.Minimum, (int)minMaxSliderAttribute.Maximum);
            // }
            // else
            {
                minValue = EditorGUI.FloatField(minRect, minValue);
                maxValue = EditorGUI.FloatField(maxRect, maxValue);
                EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, minMaxSliderAttribute.Minimum, minMaxSliderAttribute.Maximum);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetObject, "MinMaxSlider changed");
                _minField.SetValue(fieldValue, minValue);
                _maxField.SetValue(fieldValue, maxValue);
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