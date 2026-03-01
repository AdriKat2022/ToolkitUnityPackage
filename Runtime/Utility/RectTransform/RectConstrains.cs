using System;
using UnityEngine;

namespace AdriKat.Toolkit.Utility
{
    [Serializable]
    public struct RectConstraints
    {
        public UnityEngine.RectTransform fitInRectTransform;
        public bool useRectTransformWidthAsMin;
        public bool useRectTransformWidthAsMax;
        public bool useRectTransformWidthToFit;
        public bool useRectTransformHeightAsMin;
        public bool useRectTransformHeightAsMax;
        public bool useRectTransformHeightToFit;

        public float minWidth;
        public float maxWidth;
        public float minHeight;
        public float maxHeight;
        
        public RectConstraints(UnityEngine.RectTransform fitInRectTransform,
                               bool useRectTransformWidthAsMin,
                               bool useRectTransformWidthAsMax,
                               bool useRectTransformWidthToFit,
                               bool useRectTransformHeightAsMin,
                               bool useRectTransformHeightAsMax,
                               bool useRectTransformHeightToFit,
                               float minWidth,
                               float maxWidth,
                               float minHeight,
                               float maxHeight)
        {
            this.fitInRectTransform = fitInRectTransform;
            this.useRectTransformWidthAsMin = useRectTransformWidthAsMin;
            this.useRectTransformWidthAsMax = useRectTransformWidthAsMax;
            this.useRectTransformWidthToFit = useRectTransformWidthToFit;
            this.useRectTransformHeightAsMin = useRectTransformHeightAsMin;
            this.useRectTransformHeightAsMax = useRectTransformHeightAsMax;
            this.useRectTransformHeightToFit = useRectTransformHeightToFit;
            this.minWidth = minWidth;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }

        public static RectConstraints NewUnconstrained()
        {
            RectConstraints constraints = new RectConstraints()
            {
                fitInRectTransform = null,
                useRectTransformWidthAsMin = false,
                useRectTransformWidthAsMax = false,
                useRectTransformWidthToFit = false,
                useRectTransformHeightAsMin = false,
                useRectTransformHeightAsMax = false,
                useRectTransformHeightToFit = false,
                minWidth = float.NegativeInfinity,
                maxWidth = float.PositiveInfinity,
                minHeight = float.NegativeInfinity,
                maxHeight = float.PositiveInfinity,
            };

            return constraints;
        }
        
        public readonly float GetConstrainedSizeOnAxis(int axis, float value)
        {
            if (axis == 0)
            {
                return Mathf.Clamp(value, minWidth, maxWidth);
            }
            if (axis == 1)
            {
                return Mathf.Clamp(value, minHeight, maxHeight);
            }
            
            // Invalid axis. Use 0 for X and 1 for Y.
            throw new ArgumentOutOfRangeException(nameof(axis));
        }

        public readonly float GetConstrainedWidth(float width)
        {
            return Mathf.Clamp(width, minWidth, maxWidth);
        }

        public readonly float GetConstrainedHeight(float height)
        {
            return Mathf.Clamp(height, minHeight, maxHeight);
        }
        
        public readonly Vector2 GetConstrainedSize(Vector2 size)
        {
            return new Vector2(GetConstrainedWidth(size.x), GetConstrainedHeight(size.y));
        }

        public void UpdateConstraintsFromRectTransform()
        {
            if (fitInRectTransform == null)
            {
                return;
            }

            bool constraintMaxWidth = useRectTransformWidthAsMax || useRectTransformWidthToFit;
            bool constraintMaxHeight = useRectTransformHeightAsMax || useRectTransformHeightToFit;
            bool constraintMinWidth = useRectTransformWidthAsMin || useRectTransformWidthToFit;
            bool constraintMinHeight = useRectTransformHeightAsMin || useRectTransformHeightToFit;

            if (constraintMaxHeight)
            {
                maxHeight = fitInRectTransform.rect.height;
            }

            if (constraintMaxWidth)
            {
                maxWidth = fitInRectTransform.rect.width;
            }

            if (constraintMinHeight)
            {
                minHeight = fitInRectTransform.rect.height;
            }

            if (constraintMinWidth)
            {
                minWidth = fitInRectTransform.rect.width;
            }
        }
    }
}