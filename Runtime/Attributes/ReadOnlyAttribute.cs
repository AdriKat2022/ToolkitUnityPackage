using System;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        /// <summary>
        /// Attribute to make a field read-only in the inspector.<br/>
        /// The field won't be manually editable.
        /// </summary>
        public ReadOnlyAttribute()
        {
        }
    }
}