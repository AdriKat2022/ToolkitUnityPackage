using System.Collections;
using UnityEngine;

namespace AdriKat.Toolkit.Animations
{
    public class RotationAnimation : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float duration = 1f;
        [SerializeField] private uint spins = 2;
        [SerializeField] private bool infinite = false;
        [SerializeField] private bool rotateOnAwake = false;
        [SerializeField] private bool restorePosition = false;

        private bool isRotating = false;
        private Quaternion initialRotation;

        private void Awake()
        {
            isRotating = rotateOnAwake;
            initialRotation = transform.rotation;
            if (rotateOnAwake)
            {
                Rotate();
            }
        }

        public void SetDuration(float duration)
        {
            this.duration = duration;
        }

        public void Rotate()
        {
            StopAllCoroutines();
            isRotating = true;
            StartCoroutine(RotateCoroutine());
        }

        public void StopRotation()
        {
            isRotating = false;
        }

        private IEnumerator RotateCoroutine()
        {
            float elapsedTime = 0;
            float speed = spins * 360f / duration;

            while (isRotating && (infinite || elapsedTime < duration))
            {
                transform.Rotate(rotationAxis, speed * Time.deltaTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (restorePosition)
            {
                transform.rotation = initialRotation;
            }
        }
    }
}
