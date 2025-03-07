using UnityEngine;

namespace AdriKat.Utils.Attributes
{
    /// <summary>
    /// Attribute to make a method appear as a button in the inspector.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class ButtonActionAttribute : PropertyAttribute
    {
        public readonly string Name;

        public ButtonActionAttribute(string name)
        {
            Name = name;
        }
    }
}