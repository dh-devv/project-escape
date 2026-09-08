using UnityEngine;

namespace Carten
{
    public class Enemy1AI : MonoBehaviour
    {
        // ========================================================
        // Target
        // ========================================================

        [Header("=== Target ===")]
        [SerializeField] private Transform target;


        // ========================================================
        // Movement
        // ========================================================

        [Header("=== Movement ===")]
        [SerializeField] private float moveSpeed = 1.5f;

        [SerializeField] private float stopDistance = 1.1f;


        // ========================================================
        // Detection
        // ========================================================

        [Header("=== Detection ===")]
        [SerializeField] private float detectionDistance = 10f;


        // ========================================================
        // Facing
        // ========================================================

        [Header("=== Facing ===")]
        [SerializeField] private bool flipSprite = true;


        // ========================================================
        // Debug
        // ========================================================

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;


        // ========================================================
        // Runtime
        // ========================================================

        private Enemy1Controller enemyController;
        private Rigidbody2D rb;


        // ========================================================
        // Awake
        // ========================================================

        private void Awake()
        {
            enemyController =
                GetComponent<Enemy1Controller>();

            rb =
                GetComponent<Rigidbody2D>();

            if (enemyController == null)
            {
                Debug.LogError(
                    "[Enemy1AI] " +
                    "Enemy1Controller를 찾을 수 없습니다."
                );
            }

            if (rb == null)
            {
                Debug.LogError(
                    "[Enemy1AI] " +
                    "Rigidbody2D를 찾을 수 없습니다."
                );
            }

            if (target == null)
            {
                GameObject player =
                    GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    target =
                        player.transform;
                }
                else if (showDebugLog)
                {
                    Debug.LogWarning(
                        "[Enemy1AI] " +
                        "Player Tag를 가진 오브젝트를 찾지 못했습니다."
                    );
                }
            }
        }


        // ========================================================
        // Update
        // ========================================================

        private void Update()
        {
            if (enemyController == null)
                return;

            if (enemyController.IsDead)
                return;

            if (target == null)
                return;

            HandleMovement();
            HandleFacing();
        }


        // ========================================================
        // Movement
        // ========================================================

        private void HandleMovement()
        {
            if (rb == null)
                return;

            if (enemyController.IsStopped)
            {
                StopMovement();
                return;
            }

            float distance =
                Vector2.Distance(
                    transform.position,
                    target.position
                );

            // 감지 거리 밖
            if (distance > detectionDistance)
            {
                StopMovement();
                return;
            }

            // 공격 거리 진입
            if (distance <= stopDistance)
            {
                StopMovement();
                return;
            }

            float direction =
                target.position.x >
                transform.position.x
                    ? 1f
                    : -1f;

            Vector2 velocity =
                rb.linearVelocity;

            velocity.x =
                direction * moveSpeed;

            rb.linearVelocity =
                velocity;
        }


        // ========================================================
        // Stop
        // ========================================================

        private void StopMovement()
        {
            if (rb == null)
                return;

            Vector2 velocity =
                rb.linearVelocity;

            velocity.x = 0f;

            rb.linearVelocity =
                velocity;
        }


        // ========================================================
        // Facing
        // ========================================================

        private void HandleFacing()
        {
            if (!flipSprite)
                return;

            float direction =
                target.position.x -
                transform.position.x;

            if (Mathf.Abs(direction) < 0.01f)
                return;

            Vector3 scale =
                transform.localScale;

            scale.x =
                Mathf.Abs(scale.x) *
                Mathf.Sign(direction);

            transform.localScale =
                scale;
        }


        // ========================================================
        // Attack Range
        // ========================================================

        public bool IsInAttackRange()
        {
            if (target == null)
                return false;

            float distance =
                Vector2.Distance(
                    transform.position,
                    target.position
                );

            return distance <=
                   stopDistance + 0.2f;
        }


        // ========================================================
        // Target
        // ========================================================

        public void SetTarget(
            Transform newTarget)
        {
            target = newTarget;
        }


        // ========================================================
        // Gizmos
        // ========================================================

        private void OnDrawGizmosSelected()
        {
            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                detectionDistance
            );

            Gizmos.color =
                Color.blue;

            Gizmos.DrawWireSphere(
                transform.position,
                stopDistance
            );
        }
    }
}