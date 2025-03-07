using System;

namespace AdriKat.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ExposedFieldAttribute : Attribute
    {
        public readonly string DisplayName;

        public ExposedFieldAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
