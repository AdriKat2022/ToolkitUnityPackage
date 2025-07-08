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
            
            float fieldHeight = base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            
            _helpBoxPreferredHeight = GetHelpBoxHeight(warnIfAttribute.WarningMessage, EditorGUIUtility.currentViewWidth - warnIfAttribute.BoxStyling.xPadding) + warnIfAttribute.BoxStyling.additionalBoxHeight;
            _currentCondition = EditorUtils.CheckConditionFromObject(property.serializedObject, warnIfAttribute.ConditionName);
            _helpBoxCurrentFade = EditorUtils.GetBoolAnimationFade(property.GetUniqueIDFromProperty(), _currentCondition, 2f);
            
            float finalHeight = _helpBoxCurrentFade * (_helpBoxPreferredHeight + boxStylingYPadding) + fieldHeight;
            
            return finalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            WarnIfAttribute warnIfAttribute = (WarnIfAttribute)attribute;

            float fieldHeight = base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            float boxStylingXPadding = warnIfAttribute.BoxStyling.xPadding;
            float boxStylingYPadding = warnIfAttribute.BoxStyling.yPadding;
            
            // Make MessageBox rect
            Rect messageBoxRect = position;
            messageBoxRect.xMin += boxStylingXPadding;
            messageBoxRect.height -= fieldHeight;
            
            // Make Field rect
            Rect fieldRect = position;
            fieldRect.y += messageBoxRect.height + boxStylingYPadding * _helpBoxCurrentFade;
            fieldRect.height = fieldHeight;

            if (warnIfAttribute.BoxStyling.showAfter)
            {
                // Switch position the field and the box.
                messageBoxRect.y += fieldRect.height;
                fieldRect.y -= messageBoxRect.height;
            }
            
            HandleMessageBoxFade(messageBoxRect, property, warnIfAttribute.WarningType, warnIfAttribute.WarningMessage, _currentCondition);
            EditorGUI.PropertyField(fieldRect, property, label, true);
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
