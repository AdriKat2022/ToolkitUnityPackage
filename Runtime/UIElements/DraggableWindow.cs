using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AdriKat.Toolkit.UIElements
{
    public class DraggableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform windowToDrag;
        [Range(0, 1f)]
        public float smoothDrag = 0.1f;
        
        private Vector2 anchoredPosOnBeginDrag;
        private Vector2 mousePosOnBeginDrag;
        private Vector2 windowTargetPos; // Mouse last pos
        private bool isDragging;
        
        private void Update()
        {
            if (!isDragging) return;
            
            windowToDrag.anchoredPosition = Vector2.Lerp(windowToDrag.anchoredPosition, windowTargetPos, Time.deltaTime / (smoothDrag + 0.001f));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            anchoredPosOnBeginDrag = windowToDrag.anchoredPosition;
            mousePosOnBeginDrag = eventData.position;
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            windowTargetPos = anchoredPosOnBeginDrag - (mousePosOnBeginDrag - eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
    }
}