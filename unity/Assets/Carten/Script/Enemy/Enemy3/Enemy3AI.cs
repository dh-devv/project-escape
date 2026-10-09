using UnityEngine;

namespace Carten
{
    public class Enemy3AI : MonoBehaviour
    {
        // Target
        [Header("=== Target ===")]
        [SerializeField]
        private Transform target;

        [Header("=== Movement ===")]
        [SerializeField]
        private float moveSpeed = 0.7f;

        [SerializeField]
        private float detectionDistance = 10f;

        [SerializeField]
        private float attackDistance = 1.5f;
        // Facing
        [Header("=== Facing ===")]
        [SerializeField]
        private bool flipSprite = true;

        [Header("=== Debug ===")]
        [SerializeField]
        private bool showDebugLog = true;

        private Enemy3Controller enemyController;
        private Rigidbody2D rb;

        private void Awake()
        {
            enemyController = GetComponent<Enemy3Controller>();

            rb = GetComponent<Rigidbody2D>();

            if (enemyController == null)
            {
                Debug.LogError("[Enemy3AI] " + "Enemy3Controller를 찾을 수 없습니다.");
            }

            if (rb == null)
            {
                Debug.LogError("[Enemy3AI] " + "Rigidbody2D를 찾을 수 없습니다.");
            }

            FindPlayer();
        }

        private void Update()
        {
            if (enemyController == null)
                return;

            if (enemyController.IsDead)
                return;

            if (target == null)
            {
                FindPlayer();
                return;
            }

            HandleFacing();
        }

        private void FixedUpdate()
        {
            if (enemyController == null)
                return;

            if (enemyController.IsDead)
                return;

            if (target == null)
                return;

            HandleMovement();
        }
        // Find Player
        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                target = player.transform;
            }
            else if (showDebugLog)
            {
                Debug.LogWarning("[Enemy3AI] " + "Player Tag를 찾을 수 없습니다.");
            }
        }

        private void HandleMovement()
        {
            if (rb == null)
                return;

            float distance = Mathf.Abs(target.position.x - transform.position.x);

            // 감지 거리 밖
            if (distance > detectionDistance)
            {
                StopMovement();
                return;
            }

            // 공격 거리 안
            if (distance <= attackDistance)
            {
                StopMovement();
                return;
            }

            // 플레이어에게 천천히 접근
            float direction = target.position.x > transform.position.x ? 1f : -1f;

            Vector2 velocity = rb.linearVelocity;

            velocity.x = direction * moveSpeed;

            rb.linearVelocity = velocity;
        }
        // Stop
        private void StopMovement()
        {
            if (rb == null)
                return;

            Vector2 velocity = rb.linearVelocity;

            velocity.x = 0f;

            rb.linearVelocity = velocity;
        }
        // Facing
        private void HandleFacing()
        {
            if (!flipSprite)
                return;

            float direction = target.position.x - transform.position.x;

            if (Mathf.Abs(direction) < 0.01f)
                return;

            Vector3 scale = transform.localScale;

            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);

            transform.localScale = scale;
        }

        public bool IsInAttackRange()
        {
            if (target == null)
                return false;

            float distance = Mathf.Abs(target.position.x - transform.position.x);

            return distance <=
                   attackDistance;
        }
        // Target
        public Transform GetTarget()
        {
            return target;
        }

        public void SetTarget(
            Transform newTarget)
        {
            target = newTarget;
        }

        private void OnDrawGizmosSelected()
        {
            // Detection
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(transform.position, detectionDistance);

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, attackDistance);
        }
    }
}
