using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AdriKat.Toolkit.UIElements
{
    public class SmartButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public object userData;
        
        public UnityEvent onClickCallbacks;
        public Action onClickCallback;

        private bool isPressed;
        private RectTransform rectTransform;

        public void SetInteractable(bool interactable)
        {
            // Maybe throw a visual or smth.
            enabled = interactable;
        }
        
        private void Awake()
        {
            // Will be only used by rectTransform things anyway.
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Register press only if the down happened inside the rect
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.pressEventCamera))
            {
                isPressed = true;
                Debug.Log("DOWN inside");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Fire only if press started inside AND release is still inside
            if (isPressed && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.pressEventCamera))
            {
                Debug.Log("CLICK callback");
                onClickCallbacks?.Invoke();
                onClickCallback?.Invoke();
            }

            isPressed = false;
        }
    }
}