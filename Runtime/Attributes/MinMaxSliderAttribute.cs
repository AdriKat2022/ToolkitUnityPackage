using System;
using UnityEngine;

namespace AdriKat.Toolkit.Attributes
{
    /// <summary>
    /// Use this to mark the field as the minimum for the slider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MinOfSliderAttribute : Attribute {}

    /// <summary>
    /// Use this to mark the field as the maximum for the slider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MaxOfSliderAttribute : Attribute {}

    /// <summary>
    /// Use this to display the field as a min max slider.
    /// You must mark the min and max fields with [MinOfSlider] and [MaxOfSlider]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public readonly float Minimum;
        public readonly float Maximum;
        // public bool IsInteger;
        
        public MinMaxSliderAttribute()
        {
            Minimum = 0;
            Maximum = 1;
        }
        
        public MinMaxSliderAttribute(float min, float max)
        {
            Minimum = min;
            Maximum = max;
        }
        
        // public MinMaxSliderAttribute(int min, int max)
        // {
        //     Minimum = min;
        //     Maximum = max;
        //     IsInteger = true;
        // }
    }
}