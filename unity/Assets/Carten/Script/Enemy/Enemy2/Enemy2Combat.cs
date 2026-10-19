using UnityEngine;

namespace Carten
{
    public class Enemy2Combat : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private Transform firePoint;
        [Tooltip("발사할 총알 프리팹(GameObject)을 여기에 드래그해서 넣으세요.")]
        [SerializeField] private GameObject bulletPrefab;

        [Header("=== Attack ===")]
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("=== Player Layer ===")]
        [SerializeField] private LayerMask playerLayer;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private Enemy2Controller enemyController;
        private Enemy2AI enemyAI;
        private Animator anim;
        private float attackTimer;

        private void Awake()
        {
            enemyController = GetComponent<Enemy2Controller>();
            enemyAI = GetComponent<Enemy2AI>();
            anim = GetComponent<Animator>();

            if (firePoint == null)
            {
                Transform found = transform.Find("FirePoint");
                if (found != null) firePoint = found;
            }

            if (playerLayer.value == 0)
            {
                int playerLayerIndex = LayerMask.NameToLayer("Player");
                if (playerLayerIndex >= 0) playerLayer = 1 << playerLayerIndex;
            }
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
            if (bulletPrefab == null)
            {
                if (showDebugLog) Debug.LogWarning("[Enemy2Combat] Bullet Prefab이 비어 있습니다!");
                return;
            }

            Transform spawnTransform = firePoint != null ? firePoint : transform;
            Transform target = enemyAI != null ? enemyAI.GetTarget() : null;
            if (target == null) return;

            // 공격 애니메이션 실행
            if (anim != null)
            {
                anim.SetTrigger("doAttack");
            }

            float directionX = target.position.x - spawnTransform.position.x;
            if (Mathf.Abs(directionX) < 0.01f)
            {
                directionX = transform.localScale.x >= 0f ? 1f : -1f;
            }

            Vector2 direction = new Vector2(Mathf.Sign(directionX), 0f);

            // 총알 프리팹 생성 및 초기화 (프리팹에 Enemy2Bullet이 없어도 자동 부착)
            GameObject bulletObj = Instantiate(bulletPrefab, spawnTransform.position, Quaternion.identity);
            Enemy2Bullet bullet = bulletObj.GetComponent<Enemy2Bullet>();
            if (bullet == null)
            {
                bullet = bulletObj.AddComponent<Enemy2Bullet>();
            }

            bullet.Initialize(direction, attackDamage, playerLayer);
            attackTimer = attackCooldown;
        }

        private void OnDrawGizmosSelected()
        {
            if (firePoint == null) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(firePoint.position, 0.08f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + Vector3.right * 0.6f);
        }
    }
}
