using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    /// <summary>
    /// Attribute to display a field as a toolbar, with specified values.<br/>
    /// Only supports string, int, and bool fields.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class EnumAttribute : PropertyAttribute
    {
        public string[] StringValues { get; }
        public int[] IntValues { get; }
        /// <summary>
        /// Attribute to display a field as a toolbar, with specified values.<br/>
        /// Only supports string, int, and bool fields.<br/>
        /// Use this constructor for string fields or bool fields.
        /// </summary>
        /// <param name="values"></param>
        public EnumAttribute(params string[] values) => StringValues = values;
        /// <summary>
        /// Attribute to display a field as a toolbar, with specified values.<br/>
        /// Only supports string, int, and bool fields.<br/>
        /// Use this constructor for int fields.
        /// </summary>
        /// <param name="values"></param>
        public EnumAttribute(params int[] values) => IntValues = values;
    }
}
