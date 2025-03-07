using System;

namespace AdriKat.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ExposedFieldAttribute : Attribute
    {
        public readonly string DisplayName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName"></param>
        public ExposedFieldAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
