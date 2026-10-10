using UnityEngine;

namespace GrindHero.Presentation.World
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 17f, -12f);
        [SerializeField, Min(0f)] private float followSharpness = 8f;
        [SerializeField, Range(20f, 80f)] private float fixedPitchDegrees = 55f;

        private Transform _target;

        public void Initialize(Transform target)
        {
            _target = target;
            transform.rotation = Quaternion.Euler(fixedPitchDegrees, 0f, 0f);
            if (_target != null)
            {
                transform.position = _target.position + followOffset;
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            var desiredPosition = _target.position + followOffset;
            var blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
        }
    }
}
