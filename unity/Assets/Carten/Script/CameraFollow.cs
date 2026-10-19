using UnityEngine;

namespace Carten
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

        public Transform target;
        public float offsetX = 0f;
        public float offsetY = 2f;
        public float offsetZ = -10f;
        public float followSpeed = 8f;
        public bool followX = true;
        public bool followY = true;

        // 카메라 흔들림(Shake) 관련 변수
        private float shakeTimer = 0f;
        private float currentShakeMagnitude = 0.2f;

        private void Awake()
        {
            // ★ Destroy(gameObject) 제거: 새 씬이 로드되면 이 카메라가 현재 인스턴스가 됨
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;
            FindAndSnapToTarget();
        }

        private void Start()
        {
            FindAndSnapToTarget();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindAndSnapToTarget();
                if (target == null) return;
            }

            Vector3 targetPosition = new Vector3(
                followX ? target.position.x + offsetX : transform.position.x,
                followY ? target.position.y + offsetY : transform.position.y,
                target.position.z + offsetZ
            );

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            // 카메라 흔들림 연산
            if (shakeTimer > 0)
            {
                Vector2 randomCircle = Random.insideUnitCircle * currentShakeMagnitude;
                smoothedPosition += new Vector3(randomCircle.x, randomCircle.y, 0f);
                shakeTimer -= Time.deltaTime;
            }

            transform.position = smoothedPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void TriggerShake(float duration = 0.2f, float magnitude = 0.2f)
        {
            shakeTimer = duration;
            currentShakeMagnitude = magnitude;
        }

        public void FindAndSnapToTarget()
        {
            if (target == null)
            {
                PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target != null)
            {
                transform.position = new Vector3(
                    followX ? target.position.x + offsetX : transform.position.x,
                    followY ? target.position.y + offsetY : transform.position.y,
                    target.position.z + offsetZ
                );
            }
        }
    }
}
