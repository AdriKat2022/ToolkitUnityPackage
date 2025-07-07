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
        public readonly float AdditionalHeightPadding;

        /// <summary>
        /// Attribute to show or hide a field in the inspector based on the value of a boolean variable.<br/>
        /// </summary>
        /// <param name="condition">Condition evalutating to true to show the warning.</param>
        /// <param name="warningMessage">Message to display if the condition is true.</param>
        /// <param name="warningType">Type of warning wanted.</param>
        public WarnIfAttribute(string conditionName, string warningMessage, WarningTypeEnum warningType = WarningTypeEnum.Warning, float additionalHeightPadding = 10)
        {
            ConditionName = conditionName;
            WarningMessage = warningMessage;
            WarningType = warningType;
            AdditionalHeightPadding = additionalHeightPadding;
        }
    }
}