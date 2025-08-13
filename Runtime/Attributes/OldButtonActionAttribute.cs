using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    /// <summary>
    /// Attribute to make a method appear as a button in the inspector.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class OldButtonActionAttribute : PropertyAttribute
    {
        public readonly string Name;

        public OldButtonActionAttribute(string name)
        {
            Name = name;
        }
    }
}