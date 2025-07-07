using System.Collections.Generic;
using AdriKat.Toolkit.Utils;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [CustomPropertyDrawer(typeof(WarnIfAttribute))]
    public class WarnIfPropertyDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, AnimBool> _fadeAnimations = new();

        private const float X_PADDING = 20f;
        private const float Y_PADDING = 4f;
        
        private float _helpBoxCurrentFade;
        private float _helpBoxPreferredHeight;
        private bool _currentCondition;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            WarnIfAttribute warnIfAttribute = (WarnIfAttribute)attribute;
            
            _helpBoxPreferredHeight = Mathf.Max(
                EditorGUIUtility.singleLineHeight * 2f,
                GetHelpBoxHeight(warnIfAttribute.WarningMessage, position.width - X_PADDING) + warnIfAttribute.AdditionalHeightPadding);
            
            _currentCondition = EditorUtils.CheckConditionFromObject(property.serializedObject, warnIfAttribute.ConditionName);
            
            HandleMessageBoxFade(position, property, warnIfAttribute.WarningType, warnIfAttribute.WarningMessage, _currentCondition);
            
            position.y += (_helpBoxPreferredHeight + Y_PADDING) * _helpBoxCurrentFade;
            position.height = EditorGUI.GetPropertyHeight(property, label, true);
            
            EditorGUI.PropertyField(position, property, label, true);
        }
        
        private void HandleMessageBoxFade(Rect position, SerializedProperty property, WarningTypeEnum warningType, string warningMessage, bool shouldShow)
        {
            float faded = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);
            
            _helpBoxCurrentFade = faded;
            
            // Smooth height transition
            float height = faded * _helpBoxPreferredHeight;

            position.xMin += X_PADDING;
            
            Rect fadeRect = new Rect(position.x, position.y, position.width, height);
            if (faded > 0.5f)
            {
                EditorGUI.HelpBox(fadeRect, warningMessage, (MessageType)warningType);
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Smooth height transition
            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            
            float finalHeight = _helpBoxCurrentFade * (_helpBoxPreferredHeight + Y_PADDING) + propertyHeight;
            
            return finalHeight;
        }
        
        private static float GetHelpBoxHeight(string warningMessage, float width)
        {
            return EditorStyles.helpBox.CalcHeight(new GUIContent(warningMessage), width);
        }
    }
}
