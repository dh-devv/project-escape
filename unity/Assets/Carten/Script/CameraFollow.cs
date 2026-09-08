using UnityEngine;

namespace Carten
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("=== Target ===")]
        [SerializeField] private Transform target;

        [Header("=== Camera Offset ===")]
        [SerializeField] private float offsetX = 0f;
        [SerializeField] private float offsetY = 2f;
        [SerializeField] private float offsetZ = -10f;

        [Header("=== Follow Speed ===")]
        [SerializeField] private float followSpeed = 8f;

        [Header("=== Follow Settings ===")]
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 targetPosition = target.position;

            float targetX = followX
                ? targetPosition.x + offsetX
                : transform.position.x;

            float targetY = followY
                ? targetPosition.y + offsetY
                : transform.position.y;

            Vector3 desiredPosition = new Vector3(
                targetX,
                targetY,
                offsetZ
            );

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                followSpeed * Time.deltaTime
            );
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}