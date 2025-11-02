using System;
using UnityEngine;

namespace Toolkit.Runtime.Utility
{
    public static class RectTransformUtility
    {
        /// <summary>
        /// Translate the textAnchor to a vector2 variable in a (x,y) form with:<br/>
        /// x for horizontal 0-1 > left-right;<br/>
        /// y for vertical 0-1 > down-up;<br/>
        /// For example: UpperLeft => (0, 1) and LowerMiddle => (0.5, 0)
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Vector2 AsVector2(this TextAnchor textAnchor)
        {
            return textAnchor switch
            {
                TextAnchor.UpperLeft => new Vector2(0, 1),
                TextAnchor.UpperCenter => new Vector2(0.5f, 1),
                TextAnchor.UpperRight => new Vector2(1, 1),
                TextAnchor.MiddleLeft => new Vector2(0, 0.5f),
                TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
                TextAnchor.MiddleRight => new Vector2(1, 0.5f),
                TextAnchor.LowerLeft => new Vector2(0, 0),
                TextAnchor.LowerCenter => new Vector2(0.5f, 0),
                TextAnchor.LowerRight => new Vector2(1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(textAnchor), textAnchor, null)
            };
        }
        
        /// <summary>
        /// Sets the pivot point of a RectTransform based on a TextAnchor value.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to modify.</param>
        /// <param name="pivot">The TextAnchor value determining the pivot position.</param>
        /// <param name="keepPosition">Corrects the position so the pivot change doesn't affect the visible position.</param>
        public static void SetPivot(this RectTransform rectTransform, TextAnchor pivot, bool keepPosition = false)
        {
            Vector2 newPivot = pivot.AsVector2();

            if (keepPosition)
            {
                Vector2 pivotDifference = newPivot - rectTransform.pivot;
                rectTransform.position += new Vector3(pivotDifference.x * rectTransform.rect.width, pivotDifference.y * rectTransform.rect.height);
            }
            
            rectTransform.pivot = newPivot;
        }

        /// <summary>
        /// Quickly set the anchor of the rectTransform.
        /// This will set both anchorMax and anchorMin.
        /// </summary>
        public static void SetAnchor(this RectTransform rectTransform, TextAnchor anchor)
        {
            Vector2 newPivot = anchor.AsVector2();

            rectTransform.anchorMax = newPivot;
            rectTransform.anchorMin = newPivot;
        }
        
        /// <summary>
        /// Retrieves the opposite TextAnchor value relative to the given anchor.
        /// </summary>
        /// <param name="anchor">The original TextAnchor value.</param>
        /// <returns>The opposite TextAnchor value corresponding to the provided anchor.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the provided anchor value is not a valid TextAnchor enumeration.
        /// </exception>
        public static TextAnchor GetOppositeAnchor(this TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => TextAnchor.LowerRight,
                TextAnchor.UpperCenter => TextAnchor.LowerCenter,
                TextAnchor.UpperRight => TextAnchor.LowerLeft,
                TextAnchor.MiddleLeft => TextAnchor.MiddleRight,
                TextAnchor.MiddleCenter => TextAnchor.MiddleCenter,
                TextAnchor.MiddleRight => TextAnchor.MiddleLeft,
                TextAnchor.LowerLeft => TextAnchor.UpperRight,
                TextAnchor.LowerCenter => TextAnchor.UpperCenter,
                TextAnchor.LowerRight => TextAnchor.UpperLeft,
                _ => throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null)
            };
        }
    }
}