using System.Collections;
using UnityEngine;

namespace Carten
{
    public class Enemy3Controller : EnemyBase
    {
        [Header("=== Enemy3 Defense ===")]
        [SerializeField] private float damageReduction = 0.60f;

        [Header("=== Knockback Settings ===")]
        [SerializeField] private float knockbackForce = 2.5f;
        [SerializeField] private float knockbackDuration = 0.12f;

        [Header("=== Death Settings ===")]
        [SerializeField] private float deathDisableDelay = 0.8f;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private Rigidbody2D rb;
        private Animator anim;

        public float DamageReduction => damageReduction;
        public bool IsStopped { get; private set; }
        public bool IsKnockedBack { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();

            if (rb == null)
            {
                Debug.LogError("[Enemy3Controller] Rigidbody2D를 찾을 수 없습니다.");
                return;
            }

            // 회전 방지 및 물리 움직임 안정화
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            IsStopped = false;
            IsKnockedBack = false;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[Enemy3Controller] Enemy3 생성 / " +
                    $"HP: {CurrentHealth:F0}/{MaxHealth:F0} / " +
                    $"Damage Reduction: {damageReduction * 100f:F0}%"
                );
            }
        }

        public override void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0f)
                return;

            // 방어력 적용 (최소 1 피해는 받도록 설정)
            float finalDamage = damage * (1f - damageReduction);
            finalDamage = Mathf.Max(1f, finalDamage);

            currentHealth -= finalDamage;
            currentHealth = Mathf.Max(0f, currentHealth);

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
            else if (anim != null)
            {
                anim.SetTrigger("doDamage");
            }
        }

        public void ApplyKnockback(Vector2 attackerPosition)
        {
            if (IsDead || rb == null) return;
            StopAllCoroutines();
            StartCoroutine(KnockbackRoutine(attackerPosition));
        }

        private IEnumerator KnockbackRoutine(Vector2 attackerPosition)
        {
            IsKnockedBack = true;
            if (anim != null)
            {
                anim.SetBool("isWork", false);
                anim.SetTrigger("doKnockback");
            }

            float direction = transform.position.x >= attackerPosition.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * knockbackForce, rb.linearVelocity.y);

            yield return new WaitForSeconds(knockbackDuration);

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            IsKnockedBack = false;
        }

        public void SetStopped(bool stopped)
        {
            IsStopped = stopped;
            if (stopped) StopHorizontalMovement();
        }

        public void StopHorizontalMovement()
        {
            if (rb == null) return;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        protected override void Die()
        {
            StopAllCoroutines();
            IsKnockedBack = false;
            StopHorizontalMovement();

            if (showDebugLog)
            {
                Debug.Log($"[Enemy3Controller] {gameObject.name} 처치");
            }

            if (anim != null)
            {
                anim.SetBool("isWork", false);
                anim.SetTrigger("doDeath");
                StartCoroutine(DisableAfterDeathRoutine());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator DisableAfterDeathRoutine()
        {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in colliders) col.enabled = false;

            if (rb != null) rb.bodyType = RigidbodyType2D.Static;

            yield return new WaitForSeconds(Mathf.Max(0.1f, deathDisableDelay));
            gameObject.SetActive(false);
        }
    }
}
