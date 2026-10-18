using System.Collections;
using UnityEngine;

namespace Carten
{
    public class Enemy2Controller : EnemyBase
    {
        [Header("=== Knockback Settings ===")]
        [SerializeField] private float knockbackForce = 4f;
        [SerializeField] private float knockbackDuration = 0.15f;

        [Header("=== Death Settings ===")]
        [SerializeField] private float deathDisableDelay = 0.8f;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private Rigidbody2D rb;
        private Animator anim;

        public bool IsStopped { get; private set; }
        public bool IsKnockedBack { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();

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

            if (!IsDead && damage > 0f && anim != null)
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
                Debug.Log($"[Enemy2Controller] {gameObject.name} 처치");
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
