using System;
using AdriKat.Toolkit.Attributes;
using AdriKat.Toolkit.Utility;
using Toolkit.Runtime.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AdriKat.Toolkit.UIElements
{
    public class ResizableArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool resizeBothAxis;
        [ShowIf(nameof(resizeBothAxis), invert: true)]
        public RectTransform.Axis resizeAxis = RectTransform.Axis.Horizontal;
        public RectTransform rectTransformToResize;
        [Range(0, 1f)]
        public float smoothDrag = 0.1f;
        
        public bool controlPivot;
        [ShowIf(nameof(controlPivot))]
        public TextAnchor grabbedAnchor;

        public RectConstraints constraints;
        
        private Vector2 rectSizeOnBeginDrag;
        private Vector2 mousePosOnBeginDrag;
        private Vector2 targetSize; // Mouse last pos
        
        private bool isDragging;

        // private void Start()
        // {
        //     if (controlPivot)
        //     {
        //         rectTransformToResize.SetPivot(grabbedAnchor.GetOppositeAnchor());
        //     }
        // }

        private void Update()
        {
            if (!isDragging) return;

            var sizeDelta = Vector2.Lerp(rectTransformToResize.sizeDelta, targetSize, Time.deltaTime / (smoothDrag + 0.001f));
            constraints.UpdateConstraints();
            sizeDelta = constraints.GetConstrainedSize(sizeDelta);
            
            rectTransformToResize.sizeDelta = sizeDelta;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (controlPivot)
            {
                rectTransformToResize.SetPivot(grabbedAnchor.GetOppositeAnchor(), true);
            }
            
            rectSizeOnBeginDrag = rectTransformToResize.rect.size;
            mousePosOnBeginDrag = eventData.position;
            
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (resizeBothAxis)
            {
                targetSize = rectSizeOnBeginDrag - (mousePosOnBeginDrag - eventData.position);
            }
            else
            {
                var axis = (int)resizeAxis;
                targetSize[axis] = rectSizeOnBeginDrag[axis] - (mousePosOnBeginDrag[axis] - eventData.position[axis]);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            constraints.UpdateConstraints();
            rectTransformToResize.sizeDelta = constraints.GetConstrainedSize(rectTransformToResize.sizeDelta);
        }

#endif
    }
}