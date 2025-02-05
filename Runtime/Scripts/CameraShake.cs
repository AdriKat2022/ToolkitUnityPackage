using UnityEngine;

namespace AdriKat.AnimationScripts
{
    public class CameraShake : MonoBehaviour
    {
        [Header("Max offsets")]
        [SerializeField]
        private Vector2 maxOffset;
        [SerializeField]
        private float maxAngle;

        [Header("Stress")]
        [SerializeField]
        private float maxStress = 1;
        [SerializeField]
        private float stressRate = 1;

        [Header("Object")]
        [SerializeField]
        private GameObject objectToShake;
        [SerializeField]
        private bool useInitialPosition = true;

        private bool drainStress = true;
        private bool canShake = false;
        private float stress;

        private Vector3 initialPosition;

#if UNITY_EDITOR

        /*private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, maxOffset);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                transform.position + (Vector3)deadZoneOffset,
                new Vector3(deadZoneFromCenter.x, deadZoneFromCenter.y));
        }*/


#endif

        public void DrainStress(bool drain) => drainStress = drain;
        public void CanShake(bool can) => canShake = can;

        public void AddStress(float stress)
        {
            this.stress += stress;
            this.stress = Mathf.Clamp(stress, 0, 1);
        }

        public void AddStress(ShakeStrength stress)
        {
            this.stress += (int)stress / 10f;
            this.stress = Mathf.Clamp(this.stress, 0, 1);
        }

        public void StopShake()
        {
            stress = 0;
        }

        private void Start()
        {
            initialPosition = objectToShake.transform.localPosition;
            canShake = true;
        }

        private void Update()
        {
            if (!canShake) return;

            ShakeCamera(stress * stress);

            if (drainStress) stress -= Time.deltaTime * stressRate;

            stress = Mathf.Clamp(stress, 0, 1);
        }

        private void ShakeCamera(float strength)
        {
            Vector3 computedPosition = useInitialPosition ? initialPosition : Vector3.zero;

            computedPosition.x += Random.Range(-maxOffset.x * strength, maxOffset.x * strength);
            computedPosition.y += Random.Range(-maxOffset.y * strength, maxOffset.y * strength);

            Quaternion computedRotation = Quaternion.Euler(0, 0, Random.Range(-maxAngle * strength, maxAngle * strength));

            objectToShake.transform.SetLocalPositionAndRotation(computedPosition, computedRotation);
        }

        public enum ShakeStrength
        {
            None = 0,
            Small = 1,
            Medium = 2,
            Moderate = 3,
            Strong = 4,
            Extreme = 5
        }
    }
}
