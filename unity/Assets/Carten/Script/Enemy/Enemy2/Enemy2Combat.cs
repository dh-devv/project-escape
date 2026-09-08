using UnityEngine;

namespace Carten
{
    public class Enemy2Combat : MonoBehaviour
    {
        // ========================================================
        // References
        // ========================================================

        [Header("=== References ===")]
        [SerializeField] private Transform firePoint;

        [SerializeField] private Enemy2Bullet bulletPrefab;


        // ========================================================
        // Attack
        // ========================================================

        [Header("=== Attack ===")]
        [SerializeField] private float attackDamage = 8f;

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

        private Enemy2Controller enemyController;
        private Enemy2AI enemyAI;

        private float attackTimer;


        // ========================================================
        // Awake
        // ========================================================

        private void Awake()
        {
            enemyController =
                GetComponent<Enemy2Controller>();

            enemyAI =
                GetComponent<Enemy2AI>();

            if (firePoint == null)
            {
                Transform found =
                    transform.Find("FirePoint");

                if (found != null)
                {
                    firePoint =
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
                    "[Enemy2Combat] " +
                    "Enemy2Controller를 찾을 수 없습니다."
                );
            }

            if (enemyAI == null)
            {
                Debug.LogError(
                    "[Enemy2Combat] " +
                    "Enemy2AI를 찾을 수 없습니다."
                );
            }

            if (firePoint == null)
            {
                Debug.LogError(
                    "[Enemy2Combat] " +
                    "FirePoint를 찾을 수 없습니다."
                );
            }

            if (bulletPrefab == null)
            {
                Debug.LogError(
                    "[Enemy2Combat] " +
                    "Bullet Prefab이 설정되지 않았습니다."
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
            if (firePoint == null)
                return;

            if (bulletPrefab == null)
                return;

            Transform target =
                enemyAI.GetTarget();

            if (target == null)
                return;

            float directionX =
                target.position.x -
                firePoint.position.x;

            if (Mathf.Abs(directionX) <
                0.01f)
            {
                directionX =
                    transform.localScale.x >= 0f
                        ? 1f
                        : -1f;
            }

            Vector2 direction =
                new Vector2(
                    Mathf.Sign(directionX),
                    0f
                );

            Enemy2Bullet bullet =
                Instantiate(
                    bulletPrefab,
                    firePoint.position,
                    Quaternion.identity
                );

            bullet.Initialize(
                direction,
                attackDamage,
                playerLayer
            );

            attackTimer =
                attackCooldown;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[ENEMY2 ATTACK]\n" +
                    $"원거리 사격\n" +
                    $"Damage: {attackDamage:F1}\n" +
                    $"Direction: {direction}\n" +
                    $"Cooldown: {attackCooldown:F1}s"
                );
            }
        }


        // ========================================================
        // Gizmo
        // ========================================================

        private void OnDrawGizmosSelected()
        {
            if (firePoint == null)
                return;

            Gizmos.color =
                Color.magenta;

            Gizmos.DrawSphere(
                firePoint.position,
                0.08f
            );

            Gizmos.DrawLine(
                firePoint.position,
                firePoint.position +
                Vector3.right *
                0.6f
            );
        }
    }
}