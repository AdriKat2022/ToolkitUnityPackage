using System.Collections;
using UnityEngine;

namespace AdriKat.AnimationScripts.Camera
{
    public class CameraShake : MonoBehaviour
    {
        #region Variables
        [SerializeField] private GameObject _objectToShake;

        [Header("Max offsets")]
        [SerializeField] private Vector3 _maxOffset;
        [SerializeField] private float _maxAngle;

        [Header("Stress")]
        [SerializeField] private float _maxStress = 1;
        [Tooltip("The default rate at which the stress is drained.\n" +
            "Can be overriden momentarily (without being overwritten) with ShakeTimeWithStress to control the duration of a shake.")]
        [SerializeField] private float _defaultStressRate = 1;
        private bool _drainStress = true;
        [Tooltip("The relation between the stress and the shake strength.")]
        [SerializeField] private StrengthStressRelation _strengthStressRelation = StrengthStressRelation.Linear;
        [Tooltip("Used only if strengthStressRelation is set to custom.")]
        [SerializeField] private AnimationCurve _customStrengthStressRelation = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Relative position")]
        [Tooltip("If true, the object will shake from its initial position. If false, it will shake from the base of the parent.")]
        [SerializeField] private bool _useInitialPosition = true;
        [SerializeField] private bool _updateInitialPositionOnEnable = true;

        [Header("Edge behaviours")]
        [Tooltip("If true, resets stress, position and running coroutines when disabled.")]
        [SerializeField] private bool _resetAllOnDisable = true;

        [Header("Debug")]
        [SerializeField] private bool _showGizmos = true;

        private bool _canShake = false;
        private float _stress;
        private float _currentStressRate;
        private Vector3 _initialPosition;

        #endregion

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (_objectToShake == null || !_showGizmos) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_objectToShake.transform.position, new Vector3(_maxOffset.x, _maxOffset.y, _maxOffset.z));
        }

#endif

        #region Public methods
        /// <summary>
        /// Decide if the stress should be drained or not.
        /// </summary>
        public void CanStressDrain(bool canStressDrain)
        {
            _drainStress = canStressDrain;
        }
        /// <summary>
        /// Add stress to the shake. Resulting stress will be clamped between 0 and maxStress.<br/>
        /// Best way to make the object shake.<br/>
        /// </summary>
        /// <param name="additionalStress">0 is no shake. maxStress (usually 1) is strongest shake.</param>
        public void AddStress(float additionalStress)
        {
            _stress += additionalStress;
            _stress = Mathf.Clamp(_stress, 0, _maxStress);
        }
        /// <summary>
        /// Add stress to the shake. Resulting stress will be clamped between 0 and maxStress.<br/>
        /// Best way to make the object shake.<br/>
        /// </summary>
        public void AddStress(ShakeStrength additionalStressPreset)
        {
            _stress += (int)additionalStressPreset / 10f;
            _stress = Mathf.Clamp(_stress, 0, _maxStress);
        }
        /// <summary>
        /// Shake the object for a certain duration ignoring stress completely.<br/>
        /// Bypasses the canShake variable.<br/>
        /// This won't result in any smoothing in constrast to the stress, so not recommended if smoothness is priority.
        /// </summary>
        public void ShakeTime(float duration, float strength)
        {
            StartCoroutine(ShakeForDuration(duration, strength));
        }
        /// <summary>
        /// Shake the object for a certain duration..<br/>
        /// Bypasses the canShake variable.<br/>
        /// Uses the smoothing strategy from the strengthStressRelation variable.<br/>
        /// OVERWRITES the current stress.
        /// </summary>
        /// <param name="duration">Duration of the shake.</param>
        /// <param name="stress">Similar to the strength.</param>
        public void ShakeTimeWithStress(float duration, float stress)
        {
            _stress = stress;
            StartCoroutine(OverrideStressRate(duration, stress));
        }
        /// <summary>
        /// Decide if the object can shake or not.
        /// </summary>
        public void CanShake(bool can)
        {
            _canShake = can;
        }
        /// <summary>
        /// Stop immediately the shake, and resets the stress to 0.<br/>
        /// Optionnally resets the position of the object to its initial position.
        /// </summary>
        public void StopShake(bool resetPosition)
        {
            StopAllCoroutines();
            _stress = 0;

            if (resetPosition)
            {
                _objectToShake.transform.localPosition = _initialPosition;
            }
        }
        #endregion

        #region MonoBehaviour
        private void OnEnable()
        {
            if (_updateInitialPositionOnEnable)
            {
                _initialPosition = _objectToShake.transform.localPosition;
            }
        }

        private void Start()
        {
            _currentStressRate = _defaultStressRate;
            _initialPosition = _objectToShake.transform.localPosition;
            _canShake = true;
        }

        private void Update()
        {
            if (!_canShake) return;

            switch (_strengthStressRelation)
            {
                case StrengthStressRelation.Linear:
                    ShakeCamera(_stress);
                    break;
                case StrengthStressRelation.Quadratic:
                    ShakeCamera(_stress * _stress);
                    break;
                case StrengthStressRelation.Custom:
                    ShakeCamera(_customStrengthStressRelation.Evaluate(_stress));
                    break;
            }

            ManageStress();
        }

        private void OnDisable()
        {
            if (_resetAllOnDisable)
            {
                StopShake(true);
            }
        }

        #endregion

        #region Internal methods
        private void ManageStress()
        {
            if (_drainStress) _stress -= Time.deltaTime * _currentStressRate;

            _stress = Mathf.Clamp(_stress, 0, 1);
        }

        private void ShakeCamera(float strength)
        {
            Vector3 computedPosition = _useInitialPosition ? _initialPosition : Vector3.zero;

            computedPosition.x += Random.Range(-_maxOffset.x * strength, _maxOffset.x * strength);
            computedPosition.y += Random.Range(-_maxOffset.y * strength, _maxOffset.y * strength);
            computedPosition.z += Random.Range(-_maxOffset.z * strength, _maxOffset.z * strength);

            Quaternion computedRotation = Quaternion.Euler(0, 0, Random.Range(-_maxAngle * strength, _maxAngle * strength));

            _objectToShake.transform.SetLocalPositionAndRotation(computedPosition, computedRotation);
        }

        private IEnumerator ShakeForDuration(float duration, float strength)
        {
            float time = 0;
            while (time < duration)
            {
                ShakeCamera(strength);
                time += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator OverrideStressRate(float durationToMatch, float stress)
        {
            float time = 0;
            float oldRate = _currentStressRate;

            // Override the stress rate so the shake falls to 0 at the end of the duration.
            _currentStressRate = stress / durationToMatch;

            while (time < durationToMatch)
            {
                time += Time.deltaTime;
                yield return null;
            }
            _currentStressRate = oldRate;
        }

        #endregion

        public enum ShakeStrength
        {
            None = 0,
            Small = 1,
            Medium = 2,
            Moderate = 3,
            Strong = 4,
            Extreme = 5
        }

        public enum StrengthStressRelation
        {
            Linear,
            Quadratic,
            Custom
        }
    }
}
