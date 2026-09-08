using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class Enemy3Combat : MonoBehaviour
    {
        // ========================================================
        // References
        // ========================================================

        [Header("=== References ===")]
        [SerializeField]
        private Transform attackPoint;


        // ========================================================
        // Attack
        // ========================================================

        [Header("=== Attack ===")]
        [SerializeField]
        private float attackRange = 1.5f;

        [SerializeField]
        private float attackDamage = 15f;

        [SerializeField]
        private float attackCooldown = 1.8f;


        // ========================================================
        // Target
        // ========================================================

        [Header("=== Target ===")]
        [SerializeField]
        private LayerMask targetLayer;


        // ========================================================
        // Debug
        // ========================================================

        [Header("=== Debug ===")]
        [SerializeField]
        private bool showDebugLog = true;


        // ========================================================
        // Runtime
        // ========================================================

        private Enemy3Controller enemyController;
        private Enemy3AI enemyAI;

        private float attackTimer;


        // ========================================================
        // Awake
        // ========================================================

        private void Awake()
        {
            enemyController =
                GetComponent<Enemy3Controller>();

            enemyAI =
                GetComponent<Enemy3AI>();

            if (attackPoint == null)
            {
                Transform found =
                    transform.Find("AttackPoint");

                if (found != null)
                {
                    attackPoint =
                        found;
                }
            }

            // Enemy Layer 자동 탐색
            if (targetLayer.value == 0)
            {
                int playerLayer =
                    LayerMask.NameToLayer("Player");

                if (playerLayer >= 0)
                {
                    targetLayer =
                        1 << playerLayer;
                }
            }

            if (enemyController == null)
            {
                Debug.LogError(
                    "[Enemy3Combat] " +
                    "Enemy3Controller를 찾을 수 없습니다."
                );
            }

            if (enemyAI == null)
            {
                Debug.LogError(
                    "[Enemy3Combat] " +
                    "Enemy3AI를 찾을 수 없습니다."
                );
            }

            if (attackPoint == null)
            {
                Debug.LogError(
                    "[Enemy3Combat] " +
                    "AttackPoint를 찾을 수 없습니다."
                );
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

            if (attackTimer > 0f)
            {
                attackTimer -=
                    Time.deltaTime;
            }

            if (enemyAI == null)
                return;

            if (!enemyAI.IsInAttackRange())
                return;

            if (attackTimer > 0f)
                return;

            PerformAttack();
        }


        // ========================================================
        // Attack
        // ========================================================

        public void PerformAttack()
        {
            if (attackPoint == null)
                return;

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    attackPoint.position,
                    attackRange,
                    targetLayer
                );

            HashSet<IDamageable>
                damagedTargets =
                    new HashSet<IDamageable>();

            int hitCount = 0;

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable =
                    hit.GetComponentInParent<IDamageable>();

                if (damageable == null)
                    continue;

                if (damagedTargets.Contains(
                    damageable))
                {
                    continue;
                }

                if (damageable.IsDead)
                    continue;

                damagedTargets.Add(
                    damageable
                );

                damageable.TakeDamage(
                    attackDamage
                );

                hitCount++;
            }

            attackTimer =
                attackCooldown;

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


        // ========================================================
        // Gizmos
        // ========================================================

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
                return;

            Gizmos.color =
                Color.red;

            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRange
            );
        }
    }
}