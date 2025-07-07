using System;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class RequiredAttribute : PropertyAttribute
    {
        public readonly string WarningMessage = "This field is required.";
        public readonly bool AllowEmptyString = false;
        public readonly WarningTypeEnum WarningType = WarningTypeEnum.Warning;

        public RequiredAttribute(string warningMessage, WarningTypeEnum warningType = WarningTypeEnum.Warning, bool allowEmptyString = false)
        {
            WarningMessage = warningMessage;
            WarningType = warningType;
            AllowEmptyString = allowEmptyString;
        }
        
        public RequiredAttribute()
        { }

    }
    
    public enum WarningTypeEnum
    {
        Info = 1,
        Warning = 2,
        Error = 3,
    }
}
