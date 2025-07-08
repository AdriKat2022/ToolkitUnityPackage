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
            float xPadding = 0,
            float yPadding = 4,
            float additionalBoxHeight = 10,
            WarningTypeEnum warningType = WarningTypeEnum.Warning
            )
        {
            ConditionName = conditionName;
            WarningMessage = warningMessage;
            WarningType = warningType;
            
            BoxStyling = new BoxStyle(xPadding, yPadding, additionalBoxHeight + 15);
        }

        public struct BoxStyle
        {
            public readonly float xPadding;
            public readonly float yPadding;
            public readonly float additionalBoxHeight;
            
            public BoxStyle(float xPadding, float yPadding, float additionalBoxHeight)
            {
                this.xPadding = xPadding;
                this.yPadding = yPadding;
                this.additionalBoxHeight = additionalBoxHeight;
            }
        }
    }
}