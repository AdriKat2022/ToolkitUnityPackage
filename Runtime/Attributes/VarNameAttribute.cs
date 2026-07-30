namespace AdriKat.Toolkit.Attributes
{
    using UnityEngine;

    public class VarNameAttribute : PropertyAttribute
    {
        public readonly string DisplayName;
        public readonly bool KeepStyle;
        
        public VarNameAttribute(string displayName, bool keepStyle = false)
        {
            DisplayName = displayName;
            KeepStyle = keepStyle;
        }
    }
}