using System;
using AdriKat.Toolkit.Attributes;
using AdriKat.Toolkit.DataStructure;
using AdriKat.Toolkit.Mathematics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdriKat.Toolkit.UIElements
{
    public class SlideSelector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool interactableOnStart = true;
        public bool interactable = true;
        public WarpSelectionMode warpSelectionMode = WarpSelectionMode.Clamp;
        [ButtonAction(nameof(SetChildrenAsOptions), heightSpacing = 20, showButtonBelow = true)]
        public bool makeOptionsClickable;
        
        public Transform[] options;
        public UnityEvent<int, Transform> callbackOnConfirm;

        [Header("Options Layout")]
        public TMP_Text selectionTextDisplay;
        public Vector2N selectionSlideAxe = new(Vector2.right, true);
        [Range(0.01f, 0.5f)]
        public float animationSlideTime = 0.2f;
        public bool animateScale = true;
        [ShowIf(nameof(animateScale))] public float defaultScale = 1f;
        [ShowIf(nameof(animateScale))] public float selectedScale = 2f;
        public float spacing = 20f;
        public Vector2 offset;
        
        [Header("Input")]
        public bool allowDragging = true;
        public bool autoSetEventToButtons;
        public Button previousButton; 
        public Button nextButton; 
        // public bool showConfirmButton = true;
        public Button confirmButton;
        public bool disableOnSubmit = true;
        // public InputActionReference leftAction;
        // public InputActionReference rightAction;

        private Vector2 onDragBeginPosition;
        private Vector2 onDragBeginOffset;
        
        private Vector2 currentOffset; // The direct start position of the options. This value is animated.
        private int currentOptionSelected;
        private bool isDragging;
        private SmartButton[] optionButtons;
        
        private void SetCurrentOptionSelected(int index)
        {
            currentOptionSelected = index;
            
            if (selectionTextDisplay)
            {
                selectionTextDisplay.text = options[index].name;
            }
        }
        
        private void Start()
        {
            interactable |= interactableOnStart;
            
            if (makeOptionsClickable)
            {
                MakeOptionsClickable();
            }
            
            if (selectionTextDisplay && !options.IsNullOrEmpty())
            {
                selectionTextDisplay.text = options[currentOptionSelected].name;
            }
            
            if (autoSetEventToButtons)
            {
                if (previousButton) previousButton.onClick.AddListener(MoveSelectionPrevious);
                if (nextButton) nextButton.onClick.AddListener(MoveSelectionNext);
                if (confirmButton) confirmButton.onClick.AddListener(ConfirmSelection);
            }
            
            ToggleInteractable(interactable);
        }

        private void Update()
        {
            if (options.IsNullOrEmpty()) return;

            if (!isDragging)
            {
                // Only calculate the current offset if not dragging, otherwise it's the dragging's job to compute it.
                CalculateCurrentOffsetFromCurrentSelected();
            }
            
            UpdateOptionPositions();
        }

        private void UpdateOptionPositions()
        {
            // Place all objects in order
            for (int i = 0; i < options.Length; i++)
            {
                options[i].localPosition = offset - currentOffset + i * spacing * selectionSlideAxe.Get();
            }

            if (animateScale)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    options[i].localScale = Vector3.one * (i == currentOptionSelected ? selectedScale : defaultScale);
                }
            }
        }

        private void CalculateCurrentOffsetFromCurrentSelected()
        {
            // Calculate current offset
            if (animationSlideTime > 0)
            {
                currentOffset = Vector2.Lerp(currentOffset, selectionSlideAxe.Get() * (currentOptionSelected * spacing), Time.deltaTime / animationSlideTime);
            }
            else
            {
                // Snap it.
                currentOffset = selectionSlideAxe.Get() * (currentOptionSelected * spacing);
            }
        }

        private void SelectClosestOptionFromCurrentOffset()
        {
            // Project pos
            float projected = Vector2.Dot(currentOffset, selectionSlideAxe.Get());
            
            int index = Mathf.RoundToInt(projected / spacing);
            
            SetCurrentOptionSelected(Mathf.Clamp(index, 0, options.Length - 1));
        }

        private void MakeOptionsClickable()
        {
            optionButtons = new SmartButton[options.Length];

            SmartButton smartBtn;
            for (int i = 0; i < options.Length; i++)
            {
                smartBtn = options[i].gameObject.AddComponent<SmartButton>();
                smartBtn.userData = i;
                int optionIndex = i;
                smartBtn.onClickCallback = () => SetCurrentOptionSelected(optionIndex);
                optionButtons[i] = smartBtn;
            }
        }
        
        // public void Focus()
        // {
        //     // Activate input actions
        // }
        //
        // public void Unfocus()
        // {
        //     // Deactivate input actions
        // }
        
        public void ToggleInteractable(bool isInteractable)
        {
            interactable = isInteractable;
            
            if (!optionButtons.IsNullOrEmpty())
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    optionButtons[i].SetInteractable(isInteractable);
                }
            }
            
            if (previousButton) previousButton.interactable = isInteractable;
            if (nextButton) nextButton.interactable = isInteractable;
            if (confirmButton) confirmButton.interactable = isInteractable;
        }
        
        public void ConfirmSelection()
        {
            if (disableOnSubmit) ToggleInteractable(false);
            
            callbackOnConfirm?.Invoke(currentOptionSelected, options[currentOptionSelected]);
        }
        
        public void MoveSelectionNext()
        {
            SwitchActiveSelection(1);
        }

        public void MoveSelectionPrevious()
        {
            SwitchActiveSelection(-1);
        }

        public void SwitchActiveSelection(int jumpCount)
        {
            switch (warpSelectionMode)
            {
                case WarpSelectionMode.Clamp:
                {
                    SetCurrentOptionSelected(Mathf.Clamp(currentOptionSelected + jumpCount, 0, options.Length - 1));
                    break;
                }
                case WarpSelectionMode.WarpAround:
                {
                    bool wentBackwards = jumpCount < 0;
                    
                    int cur = currentOptionSelected + jumpCount;
                    
                    while (cur < 0 || cur >= options.Length)
                    {
                        cur += wentBackwards ? options.Length : -options.Length;
                    }
                    
                    SetCurrentOptionSelected(cur);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum WarpSelectionMode
        {
            Clamp,
            WarpAround
        }

        private void SetChildrenAsOptions()
        {
            options = new Transform[transform.childCount];

            for (int i = 0; i < transform.childCount; i++)
            {
                options[i] = transform.GetChild(i);
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!allowDragging || !interactable) return;
            
            isDragging = true;
            
            onDragBeginOffset = currentOffset;
            onDragBeginPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!allowDragging || !interactable) return;
            
            currentOffset = onDragBeginOffset - selectionSlideAxe.Get() * Vector2.Dot(selectionSlideAxe.Get(),(eventData.position - onDragBeginPosition));
            SelectClosestOptionFromCurrentOffset();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!allowDragging || !interactable) return;
            
            isDragging = false;
        }
        
#if UNITY_EDITOR

        private void OnValidate()
        {
            ToggleInteractable(interactable);
        }

#endif
    }
}