using System.Collections;
using UnityEngine;

namespace Carten
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

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

        private Vector3 shakeOffset = Vector3.zero;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
                return;
            }

            if (target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                }
            }
        }

        private void Start()
        {
            if (target == null || target.gameObject.activeInHierarchy == false)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null || target.gameObject == null || !target.gameObject.activeInHierarchy)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                }
                else
                {
                    return;
                }
            }

            Vector3 targetPosition = target.position;
            float targetX = followX ? targetPosition.x + offsetX : transform.position.x;
            float targetY = followY ? targetPosition.y + offsetY : transform.position.y;

            Vector3 desiredPosition = new Vector3(targetX, targetY, offsetZ);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position - shakeOffset, desiredPosition, followSpeed * Time.deltaTime);

            transform.position = smoothedPosition + shakeOffset;
        }

        public void TriggerShake(float duration = 0.18f, float magnitude = 0.12f)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(CoShake(duration, magnitude));
        }

        private IEnumerator CoShake(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float damper = 1.0f - Mathf.Clamp01(elapsed / duration);

                float offsetX = Random.Range(-1f, 1f) * magnitude * damper;
                float offsetY = Random.Range(-1f, 1f) * magnitude * damper;

                shakeOffset = new Vector3(offsetX, offsetY, 0f);
                yield return null;
            }

            shakeOffset = Vector3.zero;
            shakeCoroutine = null;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
