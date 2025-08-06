using System;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class DisplayClassAttribute : PropertyAttribute
    {
        public string actionName = "Create";
        public string functionActionName;
        
        public DisplayClassAttribute()
        { }

        /// <summary>
        /// Show the contents of the loaded object with a foldout. Only works on classes.
        /// </summary>
        public DisplayClassAttribute(string actionName, string functionActionName)
        {
            this.actionName = actionName;
            this.functionActionName = functionActionName;
        }
    }
}