using AdriKat.Toolkit.Attributes;
using AdriKat.Toolkit.Utility;
using AdriKat.Toolkit.Utility.Extensions;
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
        
        [Tooltip("Takes control of the rectTransform's pivot such as the opposite grabbed corner stays in place while resizing.")]
        public bool controlRectTransformPivot;
        [ShowIf(nameof(controlRectTransformPivot))]
        public TextAnchor grabbableCorner;
        [ShowIf(nameof(controlRectTransformPivot))]
        public bool controlKnobPosition;

        public RectConstraints sizeConstraints;
        
        private RectTransform rectTransform;
        private Vector2 _rectSizeOnBeginDrag;
        private Vector2 _mousePosOnBeginDrag;
        private Vector2 _targetSize; // Mouse last pos
        private bool _isDragging;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            
            if (controlRectTransformPivot)
            {
                rectTransformToResize.SetPivot(grabbableCorner.GetOppositeAnchor());

                if (controlKnobPosition)
                {
                    rectTransform.SetPivot(TextAnchor.MiddleCenter);
                    rectTransform.SetAnchor(grabbableCorner);
                    rectTransform.anchoredPosition = Vector2.zero;
                }
            }
        }

        private void Update()
        {
            if (!_isDragging) return;

            var sizeDelta = Vector2.Lerp(rectTransformToResize.sizeDelta, _targetSize, Time.deltaTime / (smoothDrag + 0.001f));
            sizeConstraints.UpdateConstraintsFromRectTransform();
            sizeDelta = sizeConstraints.GetConstrainedSize(sizeDelta);
            
            rectTransformToResize.sizeDelta = sizeDelta;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (controlRectTransformPivot)
            {
                rectTransformToResize.SetPivot(grabbableCorner.GetOppositeAnchor(), true);
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
            sizeConstraints.UpdateConstraintsFromRectTransform();
            rectTransformToResize.sizeDelta = sizeConstraints.GetConstrainedSize(rectTransformToResize.sizeDelta);

            // Place the knob accordingly to the grabbedAnchor 
            var rectTransform = transform as RectTransform;
            if (controlKnobPosition && transform.parent != null && rectTransform != null)
            {
                rectTransform.SetPivot(TextAnchor.MiddleCenter);
                rectTransform.SetAnchor(grabbableCorner);
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        #endif
    }
}