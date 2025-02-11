using UnityEngine;
using UnityEngine.EventSystems;

namespace AdriKat.Utils
{
    public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Time")]
        [Tooltip("Whether is will use the Time.unscaledDeltaTime instead of the regular Time.deltaTime. Useful for pause menus that must function when the time scale is 0.")]
        [SerializeField] private bool useUnscaledDeltaTime = false;
        [SerializeField] private bool activateOnAwake = true;
        private bool isActive;

        [Header("Wave Rotation")]
        [SerializeField] private bool enableRotation = false;
        [Tooltip("In degree")]
        [SerializeField] private float rotationAmplitude = 30f;
        [Tooltip("In loops per second")]
        [SerializeField] private float rotationSpeed = 1;
        private float _time;

        [Header("Growth on hover")]
        [SerializeField] private bool enableHoverScale = false;
        [SerializeField] private float addedScale = 0.2f;
        [SerializeField] private float hoverSpeed = 15f;
        private Vector3 scaleOnHover;

        [Header("Click animation")]
        [SerializeField] private bool enableClickAnimation = false;
        [SerializeField] private float clickScaleDepth = 0.2f;
        [SerializeField] private float clickSpeed = 15f;
        private Vector3 scaleOnClick;

        [Header("Sound")]
        [SerializeField] private bool playSoundOnHover;
        [SerializeField] private AudioClip soundOnHover;
        [SerializeField] private bool playSoundOnClick;
        [SerializeField] private AudioClip soundOnClick;
        [Space]
        [SerializeField] private bool onUpClickInstead = false;
        [Tooltip("Will not trigger the click sound but lets another script trigger the function PlayButtonClickSound")]
        [SerializeField] private bool doNotTriggerAutomatically = false;

        private bool isMouseOver;

#if UNITY_EDITOR

        private void OnValidate()
        {
            scaleOnHover = Vector3.one * (1 + addedScale);
            scaleOnClick = Vector3.one * (1 - clickScaleDepth);
        }

#endif


        private void OnEnable()
        {
            _time = 0;
        }

        private void Awake()
        {
            isActive = activateOnAwake;

            transform.localScale = Vector3.one;
            scaleOnHover = Vector3.one * (1 + addedScale);
            scaleOnClick = Vector3.one * (1 - clickScaleDepth);
            _time = 0;

            isMouseOver = false;
        }

        public void PlayButtonClickSound()
        {
            //AudioManager.Instance.PlaySoundEffect(soundOnClick);
        }
        public void PlayButtonHoverSound()
        {
            //AudioManager.Instance.PlaySoundEffect(soundOnHover);
        }

        private void Update()
        {
            if (!isActive) return;

            float deltaTime = useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (enableRotation)
            {
                HandleWaveMotion();
            }

            if (enableHoverScale)
            {
                HandleGrowthOnHover(deltaTime);
            }


            if (playSoundOnClick
                && isMouseOver
                && !doNotTriggerAutomatically
                && (
                    Input.GetMouseButtonUp(0) && onUpClickInstead
                    || Input.GetMouseButtonDown(0) && !onUpClickInstead
                    ))
            {

                PlayButtonClickSound();
            }


            _time += deltaTime;
        }

        private void HandleGrowthOnHover(float dTime)
        {
            if (isMouseOver)
            {
                if (enableClickAnimation && Input.GetMouseButton(0))
                    transform.localScale = Vector3.Lerp(transform.localScale, scaleOnClick, dTime * clickSpeed);
                else
                    transform.localScale = Vector3.Lerp(transform.localScale, scaleOnHover, dTime * hoverSpeed);
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, dTime * hoverSpeed);
            }
        }

        private void HandleWaveMotion()
        {
            float rotationAngle = Mathf.Sin(_time * rotationSpeed * Mathf.PI * 2) * rotationAmplitude;
            transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (playSoundOnHover)
            {
                PlayButtonHoverSound();
            }

            isMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;
        }
    }
}