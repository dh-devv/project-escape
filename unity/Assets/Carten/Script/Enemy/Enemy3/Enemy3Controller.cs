using UnityEngine;

namespace Carten
{
    public class Enemy3Controller : EnemyBase
    {
        // ========================================================
        // Enemy3 Stats
        // ========================================================

        [Header("=== Enemy3 Defense ===")]
        [SerializeField]
        private float damageReduction = 0.60f;


        // ========================================================
        // Debug
        // ========================================================

        [Header("=== Debug ===")]
        [SerializeField]
        private bool showDebugLog = true;


        // ========================================================
        // Runtime
        // ========================================================

        private Rigidbody2D rb;

        public float DamageReduction
        {
            get
            {
                return damageReduction;
            }
        }


        // ========================================================
        // Awake
        // ========================================================

        protected override void Awake()
        {
            base.Awake();

            rb =
                GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                Debug.LogError(
                    "[Enemy3Controller] " +
                    "Rigidbody2D를 찾을 수 없습니다."
                );

                return;
            }

            // 회전 방지
            rb.constraints |=
                RigidbodyConstraints2D.FreezeRotation;

            // 물리 움직임 안정화
            rb.interpolation =
                RigidbodyInterpolation2D.Interpolate;

            rb.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[Enemy3Controller] " +
                    $"Enemy3 생성 / " +
                    $"HP: {CurrentHealth:F0}/{MaxHealth:F0} / " +
                    $"Damage Reduction: " +
                    $"{damageReduction * 100f:F0}%"
                );
            }
        }


        // ========================================================
        // Damage
        // ========================================================

        public override void TakeDamage(float damage)
        {
            if (IsDead)
                return;

            if (damage <= 0f)
                return;

            // 방어력 적용
            float finalDamage =
                damage *
                (1f - damageReduction);

            // 최소 1 피해는 받도록 설정
            finalDamage =
                Mathf.Max(1f, finalDamage);

            currentHealth -=
                finalDamage;

            currentHealth =
                Mathf.Max(
                    0f,
                    currentHealth
                );

            if (showDebugLog)
            {
                Debug.Log(
                    $"[Enemy3 HIT]\n" +
                    $"받은 공격: {damage:F1}\n" +
                    $"방어 감소: {damageReduction * 100f:F0}%\n" +
                    $"실제 피해: {finalDamage:F1}\n" +
                    $"현재 HP: {currentHealth:F1}/{MaxHealth:F1}"
                );
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }


        // ========================================================
        // Death
        // ========================================================

        protected override void Die()
        {
            if (showDebugLog)
            {
                Debug.Log(
                    $"[Enemy3Controller] " +
                    $"{gameObject.name} 처치"
                );
            }

            gameObject.SetActive(false);
        }
    }
}