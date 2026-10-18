using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class Enemy3Combat : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private Transform attackPoint;

        [Header("=== Attack ===")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackCooldown = 1.8f;
        [SerializeField, Min(0f)] private float hitStunDuration = 0.45f;

        [Header("=== Target ===")]
        [SerializeField] private LayerMask targetLayer;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private Enemy3Controller enemyController;
        private Enemy3AI enemyAI;
        private Animator anim;
        private float attackTimer;

        private void Awake()
        {
            enemyController = GetComponent<Enemy3Controller>();
            enemyAI = GetComponent<Enemy3AI>();
            anim = GetComponent<Animator>();

            if (attackPoint == null)
            {
                Transform found = transform.Find("AttackPoint");
                if (found != null) attackPoint = found;
            }

            if (targetLayer.value == 0)
            {
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer >= 0) targetLayer = 1 << playerLayer;
            }

            if (enemyController == null)
                Debug.LogError("[Enemy3Combat] Enemy3Controller를 찾을 수 없습니다.");
            if (enemyAI == null)
                Debug.LogError("[Enemy3Combat] Enemy3AI를 찾을 수 없습니다.");
            if (attackPoint == null)
                Debug.LogError("[Enemy3Combat] AttackPoint를 찾을 수 없습니다.");
        }

        private void Update()
        {
            if (enemyController == null || enemyController.IsDead || enemyController.IsKnockedBack)
                return;

            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;

            if (enemyAI == null || !enemyAI.IsInAttackRange() || attackTimer > 0f)
                return;

            PerformAttack();
        }

        public void PerformAttack()
        {
            if (enemyController == null || enemyController.IsDead || attackPoint == null)
                return;

            // 공격 애니메이션 실행
            if (anim != null)
            {
                anim.SetTrigger("doAttack");
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayer);
            HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
            int hitCount = 0;

            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.IsDead || damagedTargets.Contains(damageable))
                    continue;

                damagedTargets.Add(damageable);

                if (damageable is PlayerController player)
                    player.TakeDamage(attackDamage, hitStunDuration);
                else
                    damageable.TakeDamage(attackDamage);

                hitCount++;
            }

            attackTimer = attackCooldown;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[ENEMY3 ATTACK]\n" +
                    $"방어형 근접 공격\n" +
                    $"Damage: {attackDamage:F1}\n" +
                    $"Hit: {hitCount}\n" +
                    $"Cooldown: {attackCooldown:F1}s"
                );
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
