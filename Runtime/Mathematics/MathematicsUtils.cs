using UnityEngine;

namespace AdriKat.Toolkit.Mathematics
{
    public static class MathematicsUtils
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

        public static float GetAngleDeg(float x, float y)
        {
            Debug.Log($"{x}, {y}: {Mathf.Atan2(y, x)}");
            
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        public static float GetAngle(float x, float y)
        {
            return Mathf.Atan2(y, x);
        }
    }
}