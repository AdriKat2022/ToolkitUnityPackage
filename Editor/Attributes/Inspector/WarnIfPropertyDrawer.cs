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
        private float _helpBoxCurrentFade;
        private float _helpBoxPreferredHeight;
        private bool _currentCondition;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            WarnIfAttribute warnIfAttribute = ((WarnIfAttribute)attribute);
            
            float boxStylingYPadding = warnIfAttribute.BoxStyling.yPadding;
            
            // Smooth height transition
            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            
            _helpBoxPreferredHeight = GetHelpBoxHeight(warnIfAttribute.WarningMessage, EditorGUIUtility.currentViewWidth - warnIfAttribute.BoxStyling.xPadding) + warnIfAttribute.BoxStyling.additionalBoxHeight;
            _currentCondition = EditorUtils.CheckConditionFromObject(property.serializedObject, warnIfAttribute.ConditionName);
            _helpBoxCurrentFade = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), _currentCondition, 2f);
            
            float finalHeight = _helpBoxCurrentFade * (_helpBoxPreferredHeight + boxStylingYPadding) + propertyHeight;
            
            return finalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            WarnIfAttribute warnIfAttribute = (WarnIfAttribute)attribute;

            float fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);
            float boxStylingXPadding = warnIfAttribute.BoxStyling.xPadding;
            float boxStylingYPadding = warnIfAttribute.BoxStyling.yPadding;
            
            Rect messageBoxRect = position;
            messageBoxRect.height -= fieldHeight;
            messageBoxRect.xMin += boxStylingXPadding;
            HandleMessageBoxFade(messageBoxRect, property, warnIfAttribute.WarningType, warnIfAttribute.WarningMessage, _currentCondition);
            
            position.y += messageBoxRect.height + boxStylingYPadding;
            position.height -= messageBoxRect.height;
            EditorGUI.PropertyField(position, property, label, true);
        }
        
        private void HandleMessageBoxFade(Rect position, SerializedProperty property, WarningTypeEnum warningType, string warningMessage, bool shouldShow)
        {
            float faded = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), shouldShow, 2f);
            
            _helpBoxCurrentFade = faded;
            
            // Smooth height transition
            float height = faded * _helpBoxPreferredHeight;
            
            Rect fadeRect = new Rect(position.x, position.y, position.width, height);
            if (faded > 0.1f)
            {
                EditorGUI.HelpBox(fadeRect, warningMessage, (MessageType)warningType);
            }
        }
        
        private static float GetHelpBoxHeight(string warningMessage, float width)
        {
            return EditorStyles.helpBox.CalcHeight(new GUIContent(warningMessage), width);
        }
    }
}
