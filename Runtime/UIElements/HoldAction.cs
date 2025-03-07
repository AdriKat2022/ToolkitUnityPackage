using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AdriKat.Toolkit.UIElements
{
    /// <summary>
    /// Hold action script that triggers an event when the hold is completed.<br/>
    /// It has a visual indicator that shows the progress of the hold.<br/>
    /// The input is managed by the Input System, by giving the reference of an action set to button binding.<br/>
    /// </summary>
    public class HoldAction : MonoBehaviour
    {
        #region Variables
        [Header("Input")]
        [Tooltip("If true, the input system will not be used. Instead, use the ToogleHold function. Or the ForceHold function.")]
        [SerializeField] private bool _doNotUseInputSystem = false;
        [SerializeField] private InputActionReference _holdAction;

        [Header("Hold Action Settings")]
        [SerializeField] private float _holdDuration = 1f;
        [Tooltip("The 'gravity' of the hold amount when input is released.\n" +
            "The higher, the quicker the meter resets after letting go of the input.\n" +
            "0 means no gravity, meaning the meter will never resets on its own.")]
        [SerializeField] private float _releaseMultiplier = 3f;
        [SerializeField] private float _cooldownAfterSuccessAction = 1f;

        [Header("Text and References")]
        [SerializeField] private TextMeshProUGUI _text;
        [Tooltip("Text to show when in holding state, when the input is held.")]
        [SerializeField] private string _holdingText = "Holding...";
        [Tooltip("Text to show when in success state, when the action is completed.")]
        [SerializeField] private string _successText = "Held!";
        [Tooltip("Text to show when in cancelling state, when the input is let go.")]
        [SerializeField] private string _cancellingText = "Cancelling...";
        [Tooltip("The filling image that will indicate the hold percentage.\nThis image must be in FILLED mode.")]
        [SerializeField] private Image _indicator;
        [Tooltip("The image that will be plained displayed when the hold is successful.\n" +
            "An image is here referenced, but other components/child game objects can be attached to it," +
            "since that is its game object will be toggled active.")]
        [SerializeField] private Image _successIndicator;

        [Header("Animation Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _successColor = Color.green;
        [Tooltip("How long the success indicator, and text (in success color) will remain visible before disappearing.\n" +
            "0 will skip the success animation completely.")]
        [SerializeField] private float _successAnimationDuration = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float _successAnimationFadeMult = 0.1f;
        [Tooltip("Customize how the alpha behaves over the lifetime of the hold.")]
        [SerializeField] private AnimationCurve _alpha = AnimationCurve.Linear(0, 0, 1, 1);
        [Tooltip("Customize how the filling indicator behaves over the lifetime of the hold.")]
        [SerializeField] private AnimationCurve _visualIndicator = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Hold Action Events")]
        [SerializeField] private UnityEvent OnHoldCompleted;

        [Header("Debug Log")]
        [SerializeField] private bool _debug = false;

        private bool _wasHeldAtLeastOnce = false;
        private bool _onCooldown = false;
        private bool _isHolding = false;
        private bool _forceHolding = false;
        private float _cooldownTimer = 0f;
        private float _currentHoldTime = 0f;
        private Coroutine _successAnimation;
        #endregion

        /// <summary>
        /// Forces the hold action to be held.<br/>
        /// Cannot be stopped until the hold is completed.<br/>
        /// </summary>
        public void ForceHold()
        {
            ToogleHold(true);
            _forceHolding = true;
        }

        /// <summary>
        /// Toggles the hold action.<br/>
        /// Automatically called by an InputAction callback context, but you can call it manually if you want your own input system.<br/>
        /// Especially if you do not want to use the default Input System of this script.<br/>
        /// Ignored if the hold action is on force holding. But input is registered during cooldown.<br/>
        /// </summary>
        /// <param name="holding"></param>
        public void ToogleHold(bool holding)
        {
            if (_forceHolding) return;

            _isHolding = holding;

            if (_onCooldown) return;

            _wasHeldAtLeastOnce |= holding;

            if (_wasHeldAtLeastOnce)
            {
                _text.text = holding ? _holdingText : _cancellingText;
            }

            if (_isHolding && _successAnimation != null)
            {
                CancelSuccessAnimation();
            }
        }

        private void ToogleHoldCtx(InputAction.CallbackContext ctx)
        {
            ToogleHold(ctx.performed);
        }

        private void OnEnable()
        {
            if (_doNotUseInputSystem) return;

            if (_holdAction == null)
            {
                Debug.LogWarning("Hold action not set.", gameObject);
                _doNotUseInputSystem = true;
                return;
            }

            _holdAction.action.Enable();
            _holdAction.action.performed += ToogleHoldCtx;
            _holdAction.action.canceled += ToogleHoldCtx;
        }

        private void OnDisable()
        {
            if (_doNotUseInputSystem) return;

            if (_holdAction == null)
            {
                Debug.LogWarning("Hold action not set.", gameObject);
                _doNotUseInputSystem = true;
                return;
            }

            _holdAction.action.Disable();
            _holdAction.action.performed -= ToogleHoldCtx;
            _holdAction.action.canceled -= ToogleHoldCtx;
        }

        private void Start()
        {
            _indicator.color = new Color(_normalColor.r, _normalColor.g, _normalColor.b, 0);
            _successIndicator.color = new Color(_successColor.r, _successColor.g, _successColor.b, 0);
            _text.color = new Color(_normalColor.r, _normalColor.g, _normalColor.b, 0);
            _successIndicator.gameObject.SetActive(false);

            if (_debug)
            {
                Debug.Log("Hold action initated.", gameObject);
            }
        }

        private void Update()
        {
            if (_onCooldown)
            {
                ManageCooldown();
                return;
            }

            ManageHoldTime();

            if (_wasHeldAtLeastOnce)
            {
                ManageHoldVisual();
            }

            if (_currentHoldTime >= 1)
            {
                HoldTimeCompleted();
            }
        }

        private void HoldTimeCompleted()
        {
            _onCooldown = true;
            OnHoldCompleted.Invoke();
            ToogleHold(false);
            _successAnimation = StartCoroutine(SuccessAnimation());
            _cooldownTimer = _cooldownAfterSuccessAction;
            _forceHolding = false;
            _text.text = _successText;
            _wasHeldAtLeastOnce = false;

            if (_debug)
            {
                Debug.Log("Hold completed. Fired event.", gameObject);
            }
        }

        private void ManageHoldVisual()
        {
            _indicator.fillAmount = _visualIndicator.Evaluate(_currentHoldTime);
            Color color = _normalColor;
            color.a = _alpha.Evaluate(_currentHoldTime);
            _indicator.color = color;
            _text.color = color;
        }

        private void ManageHoldTime()
        {
            if (_isHolding)
            {
                _currentHoldTime += Time.deltaTime / _holdDuration;
            }
            else
            {
                _currentHoldTime -= Time.deltaTime * _releaseMultiplier / _holdDuration;
            }

            _currentHoldTime = Mathf.Clamp01(_currentHoldTime);
        }

        private void ManageCooldown()
        {
            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer <= 0)
            {
                _currentHoldTime = 0;
                _onCooldown = false;
            }
            return;
        }

        private void CancelSuccessAnimation()
        {
            StopCoroutine(_successAnimation);
            ResetVisualState();

            if (_debug)
            {
                Debug.Log("Cancelled SuccessAnimation", gameObject);
            }
        }

        private void ResetVisualState()
        {
            _successIndicator.gameObject.SetActive(false);
            _indicator.gameObject.SetActive(true);
            Color normalHidden = _normalColor;
            normalHidden.a = 0;
            _indicator.color = normalHidden;
            _text.color = normalHidden;
        }

        private IEnumerator SuccessAnimation()
        {
            float timer = 0f;
            _successIndicator.gameObject.SetActive(true);
            _indicator.gameObject.SetActive(false);

            Color color = _successIndicator.color;
            color.a = 1;
            _successIndicator.color = color;
            _text.color = color;

            if (_debug)
            {
                Debug.Log("Started SuccessAnimation", gameObject);
            }

            while (timer < _successAnimationDuration)
            {
                if (timer > _successAnimationDuration * (1 - _successAnimationFadeMult))
                {
                    color.a = Mathematics.Mathematics.MapTo1_0(timer, _successAnimationDuration * (1 - _successAnimationFadeMult), _successAnimationDuration);
                    _successIndicator.color = color;
                    _text.color = color;
                }
                timer += Time.deltaTime;
                yield return null;
            }
            ResetVisualState();

            if (_debug)
            {
                Debug.Log("Ended SuccessAnimation", gameObject);
            }
        }
    }
}