using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ButtonActionAttribute : PropertyAttribute
    {
        public readonly string[] actionNames;
        public bool showButtonBelow;
        public float heightSpacing;
        public string customNames; // Separated by '|'.
        public bool nicifyVariableNames; // Can only be disabled with UI Toolkit.

        public ButtonActionAttribute(params string[] actionNames)
        {
            this.actionNames = actionNames;
            this.showButtonBelow = false;
            this.heightSpacing = 2;
            this.nicifyVariableNames = true;
        }
        
        public ButtonActionAttribute(bool showButtonBelow, params string[] actionNames)
        {
            this.actionNames = actionNames;
            this.showButtonBelow = showButtonBelow;
            this.heightSpacing = 2;
            this.nicifyVariableNames = true;
        }
        
        public ButtonActionAttribute(bool showButtonBelow, float heightSpacing, params string[] actionNames)
        {
            this.showButtonBelow = showButtonBelow;
            this.actionNames = actionNames;
            this.heightSpacing = heightSpacing;
            this.nicifyVariableNames = true;
        }
    }
}