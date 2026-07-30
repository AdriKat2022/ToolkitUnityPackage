using System;
using AdriKat.Toolkit.Attributes;
using AdriKat.Toolkit.DesignPatterns;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AdriKat.Toolkit.Animations
{
    public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Time")]
        [Tooltip("Whether is will use the Time.unscaledDeltaTime instead of the regular Time.deltaTime. Useful for pause menus that must function when the timescale is 0.")]
        public bool useUnscaledDeltaTime;
        
        [Header("Settings")]
        public WaveRotationSettings waveRotationSettings;
        private float _time;

        [Space]
        public HoverScaleSettings hoverScaleSettings;
        private float _scaleProgress;
        private Vector3 _scaleOnHover;
        private Vector3 _startScale;
        private Vector3 _targetScale;
        private AnimationCurve _currentCurve;
        private float _currentDuration;
        private bool _isMouseOver;

        [Space]
        public ClickAnimationSettings clickAnimationSettings;
        private Vector3 _scaleOnClick;
        private bool _isMouseClicked;

        [Header("Sound")]
        public bool playSoundOnHover;
        [ShowIf(nameof(playSoundOnHover))] public AudioClip soundOnHover;
        public bool playSoundOnClick;
        [ShowIf(nameof(playSoundOnClick))] public AudioClip soundOnClick;
        [ShowIf(nameof(playSoundOnClick))] public bool onUpClickInstead;

        [Header("Events")]
        public bool fireEvents;
        [ShowIf(nameof(fireEvents))] public UnityEvent onHover;
        [ShowIf(nameof(fireEvents))] public UnityEvent onClickDown;
        [ShowIf(nameof(fireEvents))] public UnityEvent onClickUp;

        // For custom selection
        public void ToggleClick(bool isClicked)
        {
            _isMouseClicked = isClicked;
        }
        
        public void Select()
        {
            _isMouseOver = true;
        }

        public void Deselect()
        {
            _isMouseOver = false;
        }   
        
#if UNITY_EDITOR

        private void OnValidate()
        {
            UpdateVectorScales();
        }
#endif
        
        private void OnEnable()
        {
            _time = 0;
        }

        private void Awake()
        {
            transform.localScale = Vector3.one;
            UpdateVectorScales();
            _time = 0;

            _isMouseOver = false;
        }

        private void Update()
        {
            float deltaTime = useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (waveRotationSettings.enabled) HandleWaveMotion();
            if (hoverScaleSettings.enabled) HandleGrowth(deltaTime);
            
            _time += deltaTime;
        }

        private void HandleGrowth(float deltaTime)
        {
            Vector3 desiredScale = Vector3.one;
            AnimationCurve desiredCurve = hoverScaleSettings.curve;
            float desiredDuration = hoverScaleSettings.duration;

            if (_isMouseOver)
            {
                if (clickAnimationSettings.enabled && _isMouseClicked)
                {
                    desiredScale = _scaleOnClick;
                    desiredCurve = clickAnimationSettings.curve;
                    desiredDuration = clickAnimationSettings.duration;
                }
                else
                {
                    desiredScale = _scaleOnHover;
                }
            }

            // Target changed -> restart animation
            if (desiredScale != _targetScale)
            {
                _startScale = transform.localScale;
                _targetScale = desiredScale;
                _scaleProgress = 0f;
            }

            _scaleProgress += deltaTime / desiredDuration;
    
            float curveValue = desiredCurve.Evaluate(Mathf.Clamp01(_scaleProgress));

            transform.localScale = Vector3.LerpUnclamped(
                _startScale,
                _targetScale,
                curveValue
            );
        }

        private void HandleWaveMotion()
        {
            if (waveRotationSettings.duration == 0) return;
            
            float rotationAngle;
            
            if (waveRotationSettings.animationType == WaveRotationSettings.AnimationType.Custom)
            {
                var t = _time / waveRotationSettings.duration;
                float curveTime = waveRotationSettings.pingPong ? Mathf.PingPong(t, 1) : t % 1;
                float lerpAmount = waveRotationSettings.curve.Evaluate(curveTime);
                rotationAngle = lerpAmount * waveRotationSettings.amplitude;
            }
            else
            {
                rotationAngle = Mathf.Sin(_time / waveRotationSettings.duration * Mathf.PI * 2) * waveRotationSettings.amplitude;
            }
            
            transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
            
            if (fireEvents) onHover.Invoke();
            
            if (playSoundOnHover)
            {
                AudioSource.PlayClipAtPoint(soundOnHover, Camera.main.transform.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isMouseClicked = true;
            if (fireEvents) onClickDown.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isMouseClicked = false;
            if (fireEvents) onClickUp.Invoke();
        }
        
        private void UpdateVectorScales()
        {
            _scaleOnHover = Vector3.one * (1 + hoverScaleSettings.addedScale);
            _scaleOnClick = Vector3.one * (1 - clickAnimationSettings.clickScaleDepth);
        }
        
        [Serializable]
        public class WaveRotationSettings : ModuleType
        {
            public float amplitude = 30f;
            public float duration = 0.5f;
            public AnimationType animationType = AnimationType.Sin;
            [ShowIf(nameof(animationType), AnimationType.Custom, false)]
            public AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);
            [ShowIf(nameof(animationType), AnimationType.Custom, false)]
            public bool pingPong = true;

            public enum AnimationType
            {
                Sin,
                Custom
            }
        }

        [Serializable]
        public class HoverScaleSettings : ModuleType
        {
            public float addedScale = 0.2f;
            public float duration = 0.5f;
            public AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);
        }

        [Serializable]
        public class ClickAnimationSettings : ModuleType
        {
            public float clickScaleDepth = 0.2f;
            public float duration = 0.5f;
            public AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);
        }
    }
    
}