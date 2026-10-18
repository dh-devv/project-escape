using System.Collections;
using UnityEngine;

namespace Carten
{
    public class Enemy1Controller : EnemyBase
    {
        [Header("=== Knockback Settings ===")]
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float knockbackDuration = 0.15f;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private Rigidbody2D rb;
        private Animator anim;
        private Enemy1AI enemyAI;

        public bool IsStopped { get; private set; }
        public bool IsKnockedBack { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            enemyAI = GetComponent<Enemy1AI>();

            if (rb != null)
            {
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            IsStopped = false;
            IsKnockedBack = false;
        }

        public override void TakeDamage(float damage)
        {
            if (IsDead) return;
            base.TakeDamage(damage);

            if (!IsDead && damage > 0f)
            {
                // 피격 애니메이션 실행
                if (anim != null) anim.SetTrigger("doDamage");
            }
        }

        // ★ 플레이어 공격 등에 맞았을 때 넉백 방향과 함께 호출하는 함수
        public void ApplyKnockback(Vector2 attackerPosition)
        {
            if (IsDead || rb == null) return;
            StartCoroutine(KnockbackRoutine(attackerPosition));
        }

        private IEnumerator KnockbackRoutine(Vector2 attackerPosition)
        {
            IsKnockedBack = true;
            if (enemyAI != null) enemyAI.enabled = false; // 넉백 중 AI 추적 일시 정지

            // 공격자 반대 방향으로 튕겨 나감
            float direction = transform.position.x > attackerPosition.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * knockbackForce, rb.linearVelocity.y);

            yield return new WaitForSeconds(knockbackDuration);

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            IsKnockedBack = false;
            if (enemyAI != null && !IsDead) enemyAI.enabled = true; // AI 재개
        }

        public void SetStopped(bool stopped)
        {
            IsStopped = stopped;
            if (stopped) StopHorizontalMovement();
        }

        public void StopHorizontalMovement()
        {
            if (rb == null) return;
            Vector2 velocity = rb.linearVelocity;
            velocity.x = 0f;
            rb.linearVelocity = velocity;
        }

        protected override void Die()
        {
            if (showDebugLog) Debug.Log($"[Enemy1Controller] {gameObject.name} 처치");

            if (anim != null)
            {
                anim.SetBool("isWork", false);
                anim.SetTrigger("doDeath");
                StartCoroutine(DisableAfterDeath(0.8f));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator DisableAfterDeath(float delay)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;

            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }
    }
}
