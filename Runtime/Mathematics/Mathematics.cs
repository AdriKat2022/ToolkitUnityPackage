using UnityEngine;

namespace AdriKat.Toolkit.Mathematics
{
    public class Mathematics
    {
        public enum ClipBehaviour
        {
            Clamp,
            Extrapolate
        }

        public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax, ClipBehaviour clipBehaviour = ClipBehaviour.Clamp)
        {
            if (clipBehaviour == ClipBehaviour.Clamp)
            {
                value = Mathf.Clamp(value, fromMin, fromMax);
            }

            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }

        public static float MapTo01(float value, float fromMin, float fromMax, ClipBehaviour clipBehaviour = ClipBehaviour.Clamp)
        {
            if (clipBehaviour == ClipBehaviour.Clamp)
            {
                value = Mathf.Clamp(value, fromMin, fromMax);
            }

            return (value - fromMin) / (fromMax - fromMin);
        }

        public static float MapTo1_0(float value, float fromMin, float fromMax, ClipBehaviour clipBehaviour = ClipBehaviour.Clamp)
        {
            if (clipBehaviour == ClipBehaviour.Clamp)
            {
                value = Mathf.Clamp(value, fromMin, fromMax);
            }

            return 1 - (value - fromMin) / (fromMax - fromMin);
        }
    }
}