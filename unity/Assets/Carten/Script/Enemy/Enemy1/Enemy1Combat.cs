using UnityEngine;

namespace Carten
{
    public class Enemy1Combat : MonoBehaviour
    {
        // ========================================================
        // Attack Point
        // ========================================================

        [Header("=== Attack Point ===")]
        [SerializeField] private Transform attackPoint;


        // ========================================================
        // Attack
        // ========================================================

        [Header("=== Attack ===")]
        [SerializeField] private float attackRange = 1.3f;

        [SerializeField] private float attackDamage = 10f;

        [SerializeField] private float attackCooldown = 1.5f;


        // ========================================================
        // Player Layer
        // ========================================================

        [Header("=== Player Layer ===")]
        [SerializeField] private LayerMask playerLayer;


        // ========================================================
        // Debug
        // ========================================================

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;


        // ========================================================
        // Runtime
        // ========================================================

        private Enemy1Controller enemyController;
        private Enemy1AI enemyAI;

        private float attackTimer;


        // ========================================================
        // Awake
        // ========================================================

        private void Awake()
        {
            enemyController =
                GetComponent<Enemy1Controller>();

            enemyAI =
                GetComponent<Enemy1AI>();

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

            // Player Layer 자동 탐색
            if (playerLayer.value == 0)
            {
                int playerLayerIndex =
                    LayerMask.NameToLayer("Player");

                if (playerLayerIndex >= 0)
                {
                    playerLayer =
                        1 << playerLayerIndex;
                }
            }

            if (enemyController == null)
            {
                Debug.LogError(
                    "[Enemy1Combat] " +
                    "Enemy1Controller를 찾을 수 없습니다."
                );
            }

            if (enemyAI == null)
            {
                Debug.LogError(
                    "[Enemy1Combat] " +
                    "Enemy1AI를 찾을 수 없습니다."
                );
            }

            if (attackPoint == null)
            {
                Debug.LogError(
                    "[Enemy1Combat] " +
                    "AttackPoint를 찾을 수 없습니다."
                );
            }

            if (playerLayer.value == 0)
            {
                Debug.LogError(
                    "[Enemy1Combat] " +
                    "Player Layer가 설정되지 않았습니다."
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
            if (enemyController == null)
                return;

            if (enemyController.IsDead)
                return;

            if (attackPoint == null)
                return;

            Collider2D[] targets =
                Physics2D.OverlapCircleAll(
                    attackPoint.position,
                    attackRange,
                    playerLayer
                );

            bool hitPlayer = false;

            foreach (Collider2D target in targets)
            {
                if (target == null)
                    continue;

                PlayerController player =
                    target.GetComponentInParent<PlayerController>();

                if (player == null)
                    continue;

                player.TakeDamage(
                    attackDamage
                );

                hitPlayer = true;

                if (showDebugLog)
                {
                    Debug.Log(
                        $"[ENEMY1 ATTACK]\n" +
                        $"Damage: {attackDamage:F1}\n" +
                        $"Target: {player.gameObject.name}"
                    );
                }

                break;
            }

            attackTimer =
                attackCooldown;

            if (!hitPlayer && showDebugLog)
            {
                Debug.Log(
                    "[ENEMY1 ATTACK] " +
                    "공격했지만 플레이어에게 명중하지 않았습니다."
                );
            }
        }


        // ========================================================
        // Gizmo
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