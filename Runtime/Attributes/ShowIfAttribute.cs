using System;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public readonly string VariableName;
        public readonly object ComparerValue;
        public readonly bool ComparerValueIsVariableName;
        public readonly bool ShowDisabledField;
        public readonly bool Invert;

        public bool NeedsComparison => ComparerValue != null;
        
        /// <summary>
        /// Attribute to show or hide a field in the inspector based on the value of a boolean variable.<br/>
        /// </summary>
        /// <param name="variableName">Name of the conditional variable.</param>
        /// <param name="showDisabledField">If the conditional variable is false, only disables the field instead of hiding it.</param>
        /// <param name="invert">Invert the behaviour. Only show if the conditional variable is false instead of true.</param>
        public ShowIfAttribute(string variableName, bool showDisabledField = false, bool invert = false)
        {
            VariableName = variableName;
            ShowDisabledField = showDisabledField;
            Invert = invert;
        }

        /// <summary>
        /// Attribute to show or hide a field in the inspector based on the value of a boolean variable.<br/>
        /// </summary>
        /// <param name="variableName">Name of the conditional variable.</param>
        /// <param name="comparerValue">Value to compare to the conditional variable.</param>
        /// <param name="comparerValueIsVariableName">If 'comparerValue' should be treated as literal, or as the name of another variable.</param>
        /// <param name="showDisabledField">If the conditional variable is false, only disables the field instead of hiding it.</param>
        /// <param name="invert">Invert the behaviour. Only show if the conditional variable is false instead of true.</param>
        public ShowIfAttribute(string variableName, object comparerValue, bool comparerValueIsVariableName, bool showDisabledField = false, bool invert = false)
        {
            VariableName = variableName;
            ComparerValue = comparerValue;
            ComparerValueIsVariableName = comparerValueIsVariableName;
            ShowDisabledField = showDisabledField;
            Invert = invert;

            if (comparerValueIsVariableName && comparerValue.GetType() != typeof(string))
            {
                throw new InvalidOperationException("Comparer value should be a variable name but is not a string.");
            }
        }
    }
}
