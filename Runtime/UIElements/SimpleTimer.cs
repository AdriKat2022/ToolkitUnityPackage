using TMPro;
using UnityEngine;

namespace AdriKat.Toolkit.UIElements
{
    public class SimpleTimer : MonoBehaviour
    {
        public float TimerValue => _timerValue;

        [SerializeField] private bool _startOnAwake = true;
        [SerializeField] private TextMeshProUGUI _timerValueText;

        private bool _isRunning = false;
        private float _timerValue = 0f;

        public void ResetTimer()
        {
            _timerValue = 0f;
            _timerValueText.text = "00:00:000";
        }

        public void StartTimer()
        {
            _isRunning = true;
        }

        public void StopTimer()
        {
            _isRunning = false;
        }

        private void Awake()
        {
            if (_timerValueText == null)
            {
                Debug.LogError("Timer: timerValueText is not set!");
                enabled = false;
                return;
            }

            _timerValueText.text = "00:00:000";

            if (_startOnAwake)
            {
                StartTimer();
            }
        }

        private void Update()
        {
            if (!_isRunning) return;

            _timerValue += Time.deltaTime;
            _timerValueText.text = $"{(int)(_timerValue / 60):00}:{(int)(_timerValue % 60):00}:{(int)((_timerValue % 1) * 1000):000}";
        }
    }
}