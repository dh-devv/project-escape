using UnityEngine;

namespace Carten
{
    public class Enemy2AI : MonoBehaviour
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
        [SerializeField] private float moveSpeed = 1.2f;

        [Tooltip("이 거리보다 가까우면 뒤로 물러납니다.")]
        [SerializeField] private float minimumAttackDistance = 4f;

        [Tooltip("이 거리보다 멀어지면 플레이어에게 접근합니다.")]
        [SerializeField] private float maximumAttackDistance = 10f;


        // ========================================================
        // Detection
        // ========================================================

        [Header("=== Detection ===")]
        [SerializeField] private float detectionDistance = 12f;


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

        private Enemy2Controller enemyController;
        private Rigidbody2D rb;


        // ========================================================
        // Awake
        // ========================================================

        private void Awake()
        {
            enemyController =
                GetComponent<Enemy2Controller>();

            rb =
                GetComponent<Rigidbody2D>();

            if (enemyController == null)
            {
                Debug.LogError(
                    "[Enemy2AI] " +
                    "Enemy2Controller를 찾을 수 없습니다."
                );
            }

            if (rb == null)
            {
                Debug.LogError(
                    "[Enemy2AI] " +
                    "Rigidbody2D를 찾을 수 없습니다."
                );
            }

            FindPlayer();
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
            {
                FindPlayer();
                return;
            }

            HandleFacing();
        }


        // ========================================================
        // Fixed Update
        // ========================================================

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


        // ========================================================
        // Find Player
        // ========================================================

        private void FindPlayer()
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
                    "[Enemy2AI] " +
                    "Player Tag를 가진 오브젝트를 찾을 수 없습니다."
                );
            }
        }


        // ========================================================
        // Movement
        // ========================================================

        private void HandleMovement()
        {
            if (rb == null)
                return;

            float horizontalDistance =
                Mathf.Abs(
                    target.position.x -
                    transform.position.x
                );

            // 감지 거리 밖
            if (horizontalDistance > detectionDistance)
            {
                StopMovement();
                return;
            }

            // 너무 가까움 → 뒤로 물러남
            if (horizontalDistance <
                minimumAttackDistance)
            {
                MoveAwayFromPlayer();
                return;
            }

            // 적정 사격 거리
            if (horizontalDistance <=
                maximumAttackDistance)
            {
                StopMovement();
                return;
            }

            // 너무 멂 → 접근
            MoveTowardPlayer();
        }


        // ========================================================
        // Move Toward Player
        // ========================================================

        private void MoveTowardPlayer()
        {
            float direction =
                target.position.x >
                transform.position.x
                    ? 1f
                    : -1f;

            SetHorizontalVelocity(
                direction * moveSpeed
            );
        }


        // ========================================================
        // Move Away
        // ========================================================

        private void MoveAwayFromPlayer()
        {
            float direction =
                target.position.x >
                transform.position.x
                    ? -1f
                    : 1f;

            SetHorizontalVelocity(
                direction * moveSpeed
            );
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
        // Set Velocity
        // ========================================================

        private void SetHorizontalVelocity(
            float horizontalVelocity)
        {
            if (rb == null)
                return;

            Vector2 velocity =
                rb.linearVelocity;

            velocity.x =
                horizontalVelocity;

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

            float horizontalDistance =
                Mathf.Abs(
                    target.position.x -
                    transform.position.x
                );

            return horizontalDistance >=
                       minimumAttackDistance &&
                   horizontalDistance <=
                       maximumAttackDistance;
        }


        // ========================================================
        // Target
        // ========================================================

        public void SetTarget(
            Transform newTarget)
        {
            target = newTarget;
        }


        public Transform GetTarget()
        {
            return target;
        }


        // ========================================================
        // Gizmos
        // ========================================================

        private void OnDrawGizmosSelected()
        {
            // 감지 거리
            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                detectionDistance
            );

            // 최대 사격 거리
            Gizmos.color =
                Color.blue;

            Gizmos.DrawWireSphere(
                transform.position,
                maximumAttackDistance
            );

            // 최소 사격 거리
            Gizmos.color =
                Color.red;

            Gizmos.DrawWireSphere(
                transform.position,
                minimumAttackDistance
            );
        }
    }
}