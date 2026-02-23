using System.Collections;
using AdriKat.Toolkit.Attributes;
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
        [Tooltip("If true, the input system will not be used. Instead, you must use the ToggleHold or ForceHold function.\n" +
                 "May have unintended behaviour if this field is changed while the object is ACTIVE.")]
        public bool doNotUseInputSystem = false;
        [Tooltip("Upon destroying or disabling this object, this prevents the input action reference to be disabled.")]
        public bool doNotDisableAction;
        public InputActionReference holdAction;

        [Header("Hold Action Settings")]
        public float holdDuration = 1f;
        [Tooltip("The 'gravity' of the hold amount when input is released.\n" +
            "The higher, the quicker the meter resets after letting go of the input.\n" +
            "0 means no gravity, meaning the meter will never resets on its own.")]
        public float releaseMultiplier = 3f;
        public float cooldownAfterSuccessAction = 1f;

        [Header("Text and References")]
        public TextMeshProUGUI text;
        [Tooltip("Text to show when in holding state, when the input is held.")]
        public string neutralText = "Press [] to act.";
        [Tooltip("Text to show when in holding state, when the input is held.")]
        public string holdingText = "Holding...";
        [Tooltip("Text to show when in success state, when the action is completed.")]
        public string successText = "Held!";
        [Tooltip("Text to show when in cancelling state, when the input is let go.")]
        public string cancellingText = "Cancelling...";
        public bool useIndicator = true;
        [Tooltip("The filling image that will indicate the hold percentage.\nThis image must be in FILLED mode.")]
        [ShowIf(nameof(useIndicator))]
        public Image indicator;
        [Tooltip("The image that will be plained displayed when the hold is successful.\n" +
            "An image is here referenced, but other components/child game objects can be attached to it," +
            "since that is its game object will be toggled active.")]
        [ShowIf(nameof(useIndicator))]
        public Image successIndicator;

        [Header("Animation Settings")]
        public Color normalColor = Color.white;
        public Color successColor = Color.green;
        [Tooltip("How long the success indicator, and text (in success color) will remain visible before disappearing.\n" +
            "0 will skip the success animation completely.")]
        public float successAnimationDuration = 1f;
        [Range(0f, 1f)]
        public float successAnimationFadeMult = 0.1f;
        [Tooltip("Customize how the alpha behaves over the lifetime of the hold.")]
        public AnimationCurve alpha = AnimationCurve.Linear(0, 0, 1, 1);
        [Tooltip("Customize how the filling indicator behaves over the lifetime of the hold.")]
        public AnimationCurve visualIndicator = AnimationCurve.Linear(0, 0, 1, 1);
        [Tooltip("If the action should be shown even when deactivated.")]
        public bool hideByDefault = true;

        [Header("Hold Action Events")]
        public UnityEvent onHoldCompleted;

        [Header("Debug Log")]
        public bool debug = false;

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
            ToggleHold(true);
            _forceHolding = true;
        }

        /// <summary>
        /// Toggles the hold action.<br/>
        /// Automatically called by an InputAction callback context, but you can call it manually if you want your own input system.<br/>
        /// Especially if you do not want to use the default Input System of this script.<br/>
        /// Ignored if the hold action is on force holding. But input is registered during cooldown.<br/>
        /// </summary>
        /// <param name="holding"></param>
        public void ToggleHold(bool holding)
        {
            if (_forceHolding) return;

            _isHolding = holding;

            if (_onCooldown) return;

            
            _wasHeldAtLeastOnce |= holding;

            if (_isHolding && _successAnimation != null)
            {
                CancelSuccessAnimation();
            }

            if (_wasHeldAtLeastOnce)
            {
                text.text = holding ? holdingText : cancellingText;
            }
        }

        private void ToggleHoldCtx(InputAction.CallbackContext ctx)
        {
            ToggleHold(ctx.performed);
        }

        private void OnEnable()
        {
            if (doNotUseInputSystem) return;

            if (holdAction == null)
            {
                Debug.LogWarning("Hold action not set.", gameObject);
                doNotUseInputSystem = true;
                return;
            }

            holdAction.action.Enable();
            holdAction.action.performed += ToggleHoldCtx;
            holdAction.action.canceled += ToggleHoldCtx;
        }

        private void OnDisable()
        {
            if (doNotUseInputSystem) return;

            if (holdAction == null)
            {
                Debug.LogWarning("Hold action not set.", gameObject);
                doNotUseInputSystem = true;
                return;
            }
            
            if (!doNotDisableAction) holdAction.action.Disable();
            
            holdAction.action.performed -= ToggleHoldCtx;
            holdAction.action.canceled -= ToggleHoldCtx;
        }

        private void Start()
        {
            if (useIndicator)
            {
                indicator.color = new Color(normalColor.r, normalColor.g, normalColor.b, hideByDefault ? 0 : alpha.Evaluate(0));
                successIndicator.color = new Color(successColor.r, successColor.g, successColor.b, 0);
                successIndicator.gameObject.SetActive(false);
            }

            text.text = neutralText;
            text.color = new Color(normalColor.r, normalColor.g, normalColor.b, hideByDefault ? 0 : alpha.Evaluate(0));

            if (debug)
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

                if (_currentHoldTime <= 0)
                {
                    text.text = neutralText;
                }
            }

            if (_currentHoldTime >= 1)
            {
                HoldTimeCompleted();
            }
        }

        private void HoldTimeCompleted()
        {
            _onCooldown = true;
            onHoldCompleted.Invoke();
            ToggleHold(false);
            _successAnimation = StartCoroutine(SuccessAnimation());
            _cooldownTimer = cooldownAfterSuccessAction;
            _forceHolding = false;
            text.text = successText;
            _wasHeldAtLeastOnce = false;

            if (debug)
            {
                Debug.Log("Hold completed. Fired event.", gameObject);
            }
        }

        private void ManageHoldVisual()
        {
            indicator.fillAmount = visualIndicator.Evaluate(_currentHoldTime);
            Color color = normalColor;
            color.a = alpha.Evaluate(_currentHoldTime);
            indicator.color = color;
            text.color = color;
        }
        
        private void ManageHoldTime()
        {
            if (_isHolding)
            {
                _currentHoldTime += Time.deltaTime / holdDuration;
            }
            else
            {
                _currentHoldTime -= Time.deltaTime * releaseMultiplier / holdDuration;
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
        }

        private void CancelSuccessAnimation()
        {
            StopCoroutine(_successAnimation);
            ResetVisualState();

            if (debug)
            {
                Debug.Log("Cancelled SuccessAnimation", gameObject);
            }
        }

        private void ResetVisualState()
        {
            successIndicator.gameObject.SetActive(false);
            indicator.gameObject.SetActive(true);
            Color resetColor = normalColor;
            resetColor.a = hideByDefault ? 0 : alpha.Evaluate(0);
            indicator.color = resetColor;
            text.color = resetColor;
            text.text = neutralText;
        }

        private IEnumerator SuccessAnimation()
        {
            float timer = 0f;
            successIndicator.gameObject.SetActive(true);
            indicator.gameObject.SetActive(false);

            Color color = successIndicator.color;
            color.a = 1;
            successIndicator.color = color;
            text.color = color;

            if (debug)
            {
                Debug.Log("Started SuccessAnimation", gameObject);
            }

            while (timer < successAnimationDuration)
            {
                if (timer > successAnimationDuration * (1 - successAnimationFadeMult))
                {
                    color.a = Mathematics.Mathematics.MapTo1_0(timer, successAnimationDuration * (1 - successAnimationFadeMult), successAnimationDuration);
                    successIndicator.color = color;
                    text.color = color;
                }
                timer += Time.deltaTime;
                yield return null;
            }
            ResetVisualState();

            if (debug)
            {
                Debug.Log("Ended SuccessAnimation", gameObject);
            }
        }
    }
}