using UnityEngine;

namespace Carten
{
    public class BossCombat : MonoBehaviour
    {
        [Header("=== Attack Point ===")]
        [SerializeField] private Transform attackPoint;

        [Header("=== Attack Range ===")]
        [SerializeField] private float phase1AttackRange = 1.5f;
        [SerializeField] private float phase2AttackRange = 1.7f;
        [SerializeField] private float phase3AttackRange = 2.0f;

        [Header("=== Damage ===")]
        [SerializeField] private float baseDamage = 10f;

        [Header("=== Player Layer ===")]
        [SerializeField] private LayerMask playerLayer;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private BossController bossController;

        private void Awake()
        {
            bossController = GetComponent<BossController>();

            if (bossController == null)
            {
                Debug.LogError(
                    "[BossCombat] BossController를 찾을 수 없습니다."
                );
            }
        }

        // ========================================================
        // Public Attack
        // ========================================================

        public void PerformBasicAttack()
        {
            if (bossController == null)
                return;

            if (bossController.IsDead)
                return;

            if (attackPoint == null)
            {
                Debug.LogWarning(
                    "[BossCombat] AttackPoint가 없습니다."
                );

                return;
            }

            float attackRange = GetAttackRange();

            Collider2D[] targets =
                Physics2D.OverlapCircleAll(
                    attackPoint.position,
                    attackRange,
                    playerLayer
                );

            bool hitPlayer = false;

            foreach (Collider2D target in targets)
            {
                PlayerController player =
                    target.GetComponentInParent<PlayerController>();

                if (player == null)
                    continue;

                float finalDamage =
                    baseDamage *
                    bossController.DamageMultiplier;

                player.TakeDamage(finalDamage);

                hitPlayer = true;

                if (showDebugLog)
                {
                    Debug.Log(
                        $"[BOSS BASIC ATTACK]\n" +
                        $"Phase: {bossController.CurrentPhase}\n" +
                        $"Damage: {finalDamage:F1}\n" +
                        $"Target: {player.gameObject.name}"
                    );
                }

                break;
            }

            if (!hitPlayer && showDebugLog)
            {
                Debug.Log(
                    "[BOSS BASIC ATTACK] 공격했지만 플레이어에게 명중하지 않음"
                );
            }
        }

        // ========================================================
        // Area Attack
        // ========================================================

        public void PerformAreaAttack()
        {
            if (bossController == null)
                return;

            if (bossController.IsDead)
                return;

            if (attackPoint == null)
            {
                Debug.LogWarning(
                    "[BossCombat] AttackPoint가 없습니다."
                );

                return;
            }

            float areaRange = GetAreaAttackRange();

            Collider2D[] targets =
                Physics2D.OverlapCircleAll(
                    attackPoint.position,
                    areaRange,
                    playerLayer
                );

            bool hitPlayer = false;

            foreach (Collider2D target in targets)
            {
                PlayerController player =
                    target.GetComponentInParent<PlayerController>();

                if (player == null)
                    continue;

                float finalDamage =
                    baseDamage *
                    bossController.DamageMultiplier;

                player.TakeDamage(finalDamage);

                hitPlayer = true;

                if (showDebugLog)
                {
                    Debug.Log(
                        $"[BOSS AREA ATTACK]\n" +
                        $"Phase: {bossController.CurrentPhase}\n" +
                        $"Damage: {finalDamage:F1}\n" +
                        $"Range: {areaRange:F1}"
                    );
                }

                break;
            }

            if (!hitPlayer && showDebugLog)
            {
                Debug.Log(
                    "[BOSS AREA ATTACK] 플레이어 미명중"
                );
            }
        }

        private float GetAreaAttackRange()
        {
            switch (bossController.CurrentPhase)
            {
                case BossController.BossPhase.Phase3:
                    return 4f;

                case BossController.BossPhase.Phase2:
                    return 3f;

                default:
                    return 2.5f;
            }
        }

        // ========================================================
        // Attack Range
        // ========================================================

        private float GetAttackRange()
        {
            switch (bossController.CurrentPhase)
            {
                case BossController.BossPhase.Phase2:
                    return phase2AttackRange;

                case BossController.BossPhase.Phase3:
                    return phase3AttackRange;

                default:
                    return phase1AttackRange;
            }
        }

        // ========================================================
        // Gizmo
        // ========================================================

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
                return;

            float range = phase1AttackRange;

            if (bossController != null)
            {
                switch (bossController.CurrentPhase)
                {
                    case BossController.BossPhase.Phase2:
                        range = phase2AttackRange;
                        break;

                    case BossController.BossPhase.Phase3:
                        range = phase3AttackRange;
                        break;
                }
            }

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(
                attackPoint.position,
                range
            );
        }
    }
}