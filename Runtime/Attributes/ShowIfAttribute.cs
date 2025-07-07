using System;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public readonly string VariableName;
        public readonly bool ShowDisabledField;
        public readonly bool Invert;

        /// <summary>
        /// Attribute to show or hide a field in the inspector based on the value of a boolean variable.<br/>
        /// </summary>
        /// <param name="variableName">Name of the conditional variable.</param>
        /// <param name="showDisabledField">If the conditional variable is false, only disables the field instead of hiding it.</param>
        /// <param name="invert">Invert the beheviour. Only show if the conditional variable is false instead of true.</param>
        public ShowIfAttribute(string variableName, bool showDisabledField = false, bool invert = false)
        {
            VariableName = variableName;
            ShowDisabledField = showDisabledField;
            Invert = invert;
        }
    }
}
