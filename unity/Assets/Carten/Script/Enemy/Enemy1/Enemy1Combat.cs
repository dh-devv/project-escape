using UnityEngine;

namespace Carten
{
    public class Enemy1Combat : MonoBehaviour
    {
        [Header("=== Attack Point ===")]
        [SerializeField] private Transform attackPoint;

        [Header("=== Attack ===")]
        [SerializeField] private float attackRange = 1.3f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("=== Player Layer ===")]
        [SerializeField] private LayerMask playerLayer;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private Enemy1Controller enemyController;
        private Enemy1AI enemyAI;
        private Animator anim;
        private float attackTimer;

        private void Awake()
        {
            enemyController = GetComponent<Enemy1Controller>();
            enemyAI = GetComponent<Enemy1AI>();
            anim = GetComponent<Animator>();

            if (attackPoint == null)
            {
                Transform found = transform.Find("AttackPoint");
                if (found != null) attackPoint = found;
            }

            if (playerLayer.value == 0)
            {
                int playerLayerIndex = LayerMask.NameToLayer("Player");
                if (playerLayerIndex >= 0) playerLayer = 1 << playerLayerIndex;
            }
        }

        private void Update()
        {
            if (enemyController == null || enemyController.IsDead) return;

            if (attackTimer > 0f) attackTimer -= Time.deltaTime;
            if (enemyAI == null || !enemyAI.IsInAttackRange() || attackTimer > 0f) return;

            PerformAttack();
        }

        public void PerformAttack()
        {
            if (enemyController == null || enemyController.IsDead || attackPoint == null) return;

            if (anim != null) anim.SetTrigger("doAttack");

            Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
            bool hitPlayer = false;

            foreach (Collider2D target in targets)
            {
                if (target == null) continue;
                PlayerController player = target.GetComponentInParent<PlayerController>();
                if (player == null) continue;

                player.TakeDamage(attackDamage);
                hitPlayer = true;
                break;
            }

            attackTimer = attackCooldown;
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
