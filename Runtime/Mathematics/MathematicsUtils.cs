using UnityEngine;

namespace AdriKat.Toolkit.Mathematics
{
    /// <summary>
    /// Utility class to aid with mathematics.
    /// </summary>
    public static class MathematicsUtils
    {

        #region Mapping
        
        /// <summary>
        /// Defines the clipping behaviour of a value in a range.
        /// </summary>
        public enum ClipBehaviour
        {
            /// <summary>
            /// Sets the value to the max or the min of the range if the value is greater or smaller than the range respectively.
            /// </summary>
            Clamp,
            /// <summary>
            /// The value repeats the range discontinuously when outside the range. Ressembles to a modulo.
            /// </summary>
            Repeat,
            /// <summary>
            /// The value repeats the range continuously when outside the range by bouncing back and forth. 
            /// </summary>
            PingPong,
            /// <summary>
            /// The value doesn't get restricted.
            /// </summary>
            Extrapolate,
        }
        
        /// <summary>
        /// Constrains the given value within the lowerBound and the higherBound according to the given clipBehaviour.
        /// </summary>
        /// <param name="value">The value to constrain.</param>
        /// <param name="lowerBound">The lower bound of the range.</param>
        /// <param name="higherBound">The higher bound of the range.</param>
        /// <param name="clipBehaviour">The clip behaviour to apply.</param>
        public static float ApplyClipBehaviour(this float value, float lowerBound, float higherBound, ClipBehaviour clipBehaviour)
        {
            float range = higherBound - lowerBound;
            bool shouldInvert = range < 0;

            if (shouldInvert)
            {
                // Invert bounds for easier calculation, we'll invert back later.
                (lowerBound, higherBound) = (higherBound, lowerBound);
                range *= -1; // Also keep the range positive.
            }

            // Now safely assume that higherBound > lowerBound.
            switch (clipBehaviour)
            {
                case ClipBehaviour.Clamp:
                    value = Mathf.Clamp(value, lowerBound, higherBound);
                    break;
                
                case ClipBehaviour.Repeat:
                    while (value < lowerBound) value += range;
                    while (value > higherBound) value -= range;
                    break;
                
                case ClipBehaviour.PingPong:
                    // Same as repeat but keep track of the inversion polarity.
                    // Each time we're getting closer to the range, we're switching polarity.
                    while (value < lowerBound) { value += range; shouldInvert = !shouldInvert; }
                    while (value > higherBound) { value -= range; shouldInvert = !shouldInvert; }
                    break;

                case ClipBehaviour.Extrapolate:
                default:
                    // Do nothing.
                    break;
            }

            if (shouldInvert)
            {
                // Invert the value around the range.
                value = higherBound - value + lowerBound;
            }
            
            return value;
        }
        
        /// <summary>
        /// Maps the value from the [fromMin, fromMax] range to the [toMin, toMax] range.
        /// The value is constrained using the given clip behaviour.
        /// </summary>
        /// <param name="value">The value to transmute.</param>
        /// <param name="fromMin">The lower bound of the input range.</param>
        /// <param name="fromMax">The higher bound of the input range.</param>
        /// <param name="toMin">The lower bound of the destination range.</param>
        /// <param name="toMax">The higher bound of the destination range.</param>
        /// <param name="clipBehaviour">The type of constraining to apply to the value regarding the set of destination.</param>
        /// <returns>The transmuted value.</returns>
        public static float Map(this float value, float fromMin, float fromMax, float toMin, float toMax, ClipBehaviour clipBehaviour = ClipBehaviour.Clamp)
        {
            value = value.ApplyClipBehaviour(fromMin, fromMax, clipBehaviour);

            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }
        
        /// <summary>
        /// Maps the value from the [fromMin, fromMax] range to the [0, 1] range.
        /// The value is constrained using the given clip behaviour.
        /// </summary>
        /// <param name="value">The value to transmute.</param>
        /// <param name="fromMin">The lower bound of the input range.</param>
        /// <param name="fromMax">The higher bound of the input range.</param>
        /// <param name="clipBehaviour">The type of constraining to apply to the value regarding the set of destination.</param>
        /// <returns>The transmuted value.</returns>
        public static float MapTo01(this float value, float fromMin, float fromMax, ClipBehaviour clipBehaviour = ClipBehaviour.Clamp)
        {
            value = value.ApplyClipBehaviour(fromMin, fromMax, clipBehaviour);

            return (value - fromMin) / (fromMax - fromMin);
        }

                
        /// <summary>
        /// Maps the value from the [fromMin, fromMax] range to the [1, 0] range (the [0, 1] range but reversed).
        /// The value is constrained using the given clip behaviour.
        /// </summary>
        /// <param name="value">The value to transmute.</param>
        /// <param name="fromMin">The lower bound of the input range.</param>
        /// <param name="fromMax">The higher bound of the input range.</param>
        /// <param name="clipBehaviour">The type of constraining to apply to the value regarding the set of destination.</param>
        /// <returns>The transmuted value.</returns>
        public static float MapTo10(this float value, float fromMin, float fromMax, ClipBehaviour clipBehaviour = ClipBehaviour.Clamp)
        {
            value = value.ApplyClipBehaviour(fromMin, fromMax, clipBehaviour);
            
            return 1 - (value - fromMin) / (fromMax - fromMin);
        }

        #endregion
        
        #region Geometry & Trigonometry
        
        /// <summary>
        /// Rotates the vector by the given number of degrees.
        /// </summary>
        /// <returns>The rotated vector.</returns>
        public static Vector2 Rotate(this Vector2 vectorToRotate, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
        
            // Apply rotation matrix.
            return new (
                Mathf.Cos(rad) * vectorToRotate.x - Mathf.Sin(rad) * vectorToRotate.y,
                Mathf.Sin(rad) * vectorToRotate.x + Mathf.Cos(rad) * vectorToRotate.y
            );
        }

        /// <summary>
        /// Computes the angle between the vector and the reference vector.
        /// </summary>
        /// <param name="vector">Vector to compare.</param>
        /// <param name="referenceVector">Reference vector.</param>
        /// <param name="returnSignedAngle">If true, return a signed angle (–180° to 180°). Otherwise, returns an unsigned angle (0 to 180°).</param>
        /// <param name="axis">If returning a signed angle, specifies the reference axis. By default, the reference axis is Vector3.forward (which is perfect to calculate angles on the 2D plane X and Y).</param>
        /// <returns>The angle in degrees from the reference vector to the vector to compare.</returns>
        public static float GetAngleBetweenVectors(
            this Vector3 vector,
            Vector3 referenceVector,
            bool returnSignedAngle = true,
            Vector3 axis = default)
        {
            vector.Normalize();
            referenceVector.Normalize();

            if (returnSignedAngle)
            {
                if (axis == default) axis = Vector3.forward; // Axe par défaut si non spécifié
                return Vector3.SignedAngle(vector, referenceVector, axis);
            }
            else
            {
                float angleRad = Mathf.Acos(Mathf.Clamp(Vector3.Dot(vector, referenceVector), -1f, 1f));
                return angleRad * Mathf.Rad2Deg;
            }
        }
        
        /// <summary>
        /// Computes the angle of the given vector within the trigonometry circle.
        /// </summary>
        /// <returns>The angle in degrees between the (1,0) vector to the given vector.</returns>
        public static float GetAngleDeg(this Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Computes the angle of the given vector within the trigonometry circle.
        /// </summary>
        /// <returns>The angle in radians between the (1,0) vector to the given vector.</returns>
        public static float GetAngle(this Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x);
        }
        
        #endregion
    }
}