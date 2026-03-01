using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ButtonActionAttribute : PropertyAttribute
    {
        public bool showButtonBelow;
        public string[] actionNames;
        public float heightSpacing;
        
        public ButtonActionAttribute(params string[] actionNames)
        {
            this.showButtonBelow = false;
            this.actionNames = actionNames;
        }
        
        public ButtonActionAttribute(bool showButtonBelow, params string[] actionNames)
        {
            this.showButtonBelow = showButtonBelow;
            this.actionNames = actionNames;
        }
        
        public ButtonActionAttribute(bool showButtonBelow, float heightSpacing, params string[] actionNames)
        {
            this.showButtonBelow = showButtonBelow;
            this.actionNames = actionNames;
            this.heightSpacing = heightSpacing;
        }
    }
}