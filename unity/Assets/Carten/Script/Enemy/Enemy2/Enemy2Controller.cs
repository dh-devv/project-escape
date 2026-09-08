using UnityEngine;

namespace Carten
{
    public class Enemy2Controller : EnemyBase
    {
        // ========================================================
        // Debug
        // ========================================================

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;


        // ========================================================
        // Runtime
        // ========================================================

        private Rigidbody2D rb;

        public bool IsStopped { get; private set; }


        // ========================================================
        // Awake
        // ========================================================

        protected override void Awake()
        {
            base.Awake();

            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                Debug.LogError(
                    "[Enemy2Controller] " +
                    "Rigidbody2D를 찾을 수 없습니다."
                );

                return;
            }

            rb.constraints |=
                RigidbodyConstraints2D.FreezeRotation;

            rb.interpolation =
                RigidbodyInterpolation2D.Interpolate;

            rb.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            IsStopped = false;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[Enemy2Controller] Enemy2 생성 / " +
                    $"HP: {CurrentHealth:F0}/{MaxHealth:F0}"
                );
            }
        }


        // ========================================================
        // Stop State
        // ========================================================

        public void SetStopped(bool stopped)
        {
            IsStopped = stopped;

            if (stopped)
            {
                StopHorizontalMovement();
            }
        }


        // ========================================================
        // Stop Horizontal Movement
        // ========================================================

        public void StopHorizontalMovement()
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
        // Death
        // ========================================================

        protected override void Die()
        {
            if (showDebugLog)
            {
                Debug.Log(
                    $"[Enemy2Controller] " +
                    $"{gameObject.name} 처치"
                );
            }

            gameObject.SetActive(false);
        }
    }
}