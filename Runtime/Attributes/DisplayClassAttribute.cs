using System;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class DisplayClassAttribute : PropertyAttribute
    {
        public readonly string actionName;
        public readonly string functionActionName;
        
        public DisplayClassAttribute()
        { }

        /// <summary>
        /// Show the contents of the loaded object with a foldout. Only works on classes.<br/>
        /// Additionally, shows a REMOVE button when an object is attached, and a CREATE button when none is attached.<br/>
        /// The CREATE button will call the function whose name corresponds to functionActionName.<br/>
        /// The REMOVE button will remove the attached object from the slot.
        /// </summary>
        /// <param name="actionName">The text to display on the button. Null or empty will make it disappear.</param>
        /// <param name="functionActionName">Name of the function that will return a new object to create. If null or empty, nothing happens.</param>
        public DisplayClassAttribute(string actionName, string functionActionName)
        {
            this.actionName = actionName;
            this.functionActionName = functionActionName;
        }
    }
}