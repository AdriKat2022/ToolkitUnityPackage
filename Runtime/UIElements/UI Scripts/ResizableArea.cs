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
        public bool controlKnobPosition;
        [ShowIf(nameof(controlPivot))]
        public TextAnchor grabbedAnchor;

        public RectConstraints sizeConstraints;
        
        private Vector2 _rectSizeOnBeginDrag;
        private Vector2 _mousePosOnBeginDrag;
        private Vector2 _targetSize; // Mouse last pos
        private bool _isDragging;

        private void Start()
        {
            if (controlKnobPosition)
            {
                rectTransformToResize.SetPivot(grabbedAnchor.GetOppositeAnchor());
            }
        }

        private void Update()
        {
            if (!_isDragging) return;

            var sizeDelta = Vector2.Lerp(rectTransformToResize.sizeDelta, _targetSize, Time.deltaTime / (smoothDrag + 0.001f));
            sizeConstraints.UpdateConstraints();
            sizeDelta = sizeConstraints.GetConstrainedSize(sizeDelta);
            
            rectTransformToResize.sizeDelta = sizeDelta;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (controlPivot)
            {
                rectTransformToResize.SetPivot(grabbedAnchor.GetOppositeAnchor(), true);
            }
            
            _rectSizeOnBeginDrag = rectTransformToResize.rect.size;
            _mousePosOnBeginDrag = eventData.position;
            
            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (resizeBothAxis)
            {
                _targetSize = _rectSizeOnBeginDrag - (_mousePosOnBeginDrag - eventData.position);
            }
            else
            {
                var axis = (int)resizeAxis;
                _targetSize[axis] = _rectSizeOnBeginDrag[axis] - (_mousePosOnBeginDrag[axis] - eventData.position[axis]);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            sizeConstraints.UpdateConstraints();
            rectTransformToResize.sizeDelta = sizeConstraints.GetConstrainedSize(rectTransformToResize.sizeDelta);

            // Place the knob accordingly to the grabbedAnchor 
            var rectTransform = transform as RectTransform;
            if (controlKnobPosition && transform.parent != null && rectTransform != null)
            {
                rectTransform.SetPivot(TextAnchor.MiddleCenter);
                rectTransform.SetAnchor(grabbedAnchor);
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        #endif
    }
}