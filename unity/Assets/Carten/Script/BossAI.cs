using UnityEngine;

namespace Carten
{
    public class BossAI : MonoBehaviour
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
        [SerializeField] private float phase1MoveSpeed = 1.5f;
        [SerializeField] private float phase2MoveSpeed = 2.5f;
        [SerializeField] private float phase3MoveSpeed = 4f;

        [SerializeField] private float stopDistance = 1.3f;

        // ========================================================
        // Detection
        // ========================================================

        [Header("=== Detection ===")]
        [SerializeField] private float detectionDistance = 15f;

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

        private BossController bossController;
        private Rigidbody2D rb;

        private void Awake()
        {
            bossController = GetComponent<BossController>();
            rb = GetComponent<Rigidbody2D>();

            if (bossController == null)
            {
                Debug.LogError(
                    "[BossAI] BossController를 찾을 수 없습니다."
                );
            }

            if (rb == null)
            {
                Debug.LogError(
                    "[BossAI] Rigidbody2D를 찾을 수 없습니다."
                );
            }
        }

        private void Update()
        {
            if (bossController == null)
                return;

            if (bossController.IsDead)
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
            float distance =
                Vector2.Distance(
                    transform.position,
                    target.position
                );

            if (distance > detectionDistance)
            {
                StopMovement();
                return;
            }

            if (distance <= stopDistance)
            {
                StopMovement();
                return;
            }

            float direction =
                target.position.x > transform.position.x
                    ? 1f
                    : -1f;

            float moveSpeed = GetMoveSpeed();

            Vector2 velocity = rb.linearVelocity;

            velocity.x = direction * moveSpeed;

            rb.linearVelocity = velocity;
        }

        // ========================================================
        // Stop
        // ========================================================

        private void StopMovement()
        {
            Vector2 velocity = rb.linearVelocity;

            velocity.x = 0f;

            rb.linearVelocity = velocity;
        }

        // ========================================================
        // Move Speed
        // ========================================================

        private float GetMoveSpeed()
        {
            switch (bossController.CurrentPhase)
            {
                case BossController.BossPhase.Phase2:
                    return phase2MoveSpeed;

                case BossController.BossPhase.Phase3:
                    return phase3MoveSpeed;

                default:
                    return phase1MoveSpeed;
            }
        }

        // ========================================================
        // Facing
        // ========================================================

        private void HandleFacing()
        {
            if (!flipSprite)
                return;

            if (target.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }


        public bool IsInAttackRange()
        {
            if (target == null)
                return false;

            float distance =
                Vector2.Distance(
                    transform.position,
                    target.position
                );

            return distance <= stopDistance + 0.3f;
        }

        // ========================================================
        // Gizmos
        // ========================================================

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                detectionDistance
            );

            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(
                transform.position,
                stopDistance
            );
        }
    }
}