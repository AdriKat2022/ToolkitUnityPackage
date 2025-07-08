using System;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class WarnIfAttribute : PropertyAttribute
    {
        public readonly string ConditionName;
        public readonly string WarningMessage;
        public readonly WarningTypeEnum WarningType;
        public readonly BoxStyle BoxStyling;

        /// <summary>
        /// Attribute to show or hide a field in the inspector based on the value of a boolean variable.<br/>
        /// </summary>
        /// <param name="conditionName">Name of a bool field, or of a bool-returning method. If it evaluates to true, shows the warning.</param>
        /// <param name="warningMessage">Message to display if the condition is true.</param>
        /// <param name="warningType">Type of warning wanted.</param>
        public WarnIfAttribute(
            string conditionName,
            string warningMessage,
            bool showAfter = false,
            float xPadding = 0,
            float yPadding = 0,
            float additionalBoxHeight = 0,
            WarningTypeEnum warningType = WarningTypeEnum.Warning
            )
        {
            ConditionName = conditionName;
            WarningMessage = warningMessage;
            WarningType = warningType;
            
            BoxStyling = new BoxStyle(xPadding, yPadding, additionalBoxHeight + 15, showAfter);
        }

        public struct BoxStyle
        {
            public readonly float xPadding;
            public readonly float yPadding;
            public readonly float additionalBoxHeight;
            public readonly bool showAfter;
            
            public BoxStyle(float xPadding, float yPadding, float additionalBoxHeight, bool showAfter)
            {
                this.xPadding = xPadding;
                this.yPadding = yPadding;
                this.additionalBoxHeight = additionalBoxHeight;
                this.showAfter = showAfter;
            }
        }
    }
}