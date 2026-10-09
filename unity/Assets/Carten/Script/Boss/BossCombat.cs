using System.Collections.Generic;
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

        [Header("=== Field attacks ===")]
        [SerializeField] private Transform arenaCenter;
        [SerializeField] private Vector2 arenaSize = new Vector2(24f, 8f);
        [SerializeField, Min(0f)] private float strongHitStun = 0.5f;
        private readonly List<BossFieldAttack> fields = new List<BossFieldAttack>();
        public Vector2 AttackCenter => attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        private Vector2 ArenaCenter => arenaCenter != null ? (Vector2)arenaCenter.position : (Vector2)transform.position;
        public bool HasActiveFieldAttacks => fields.Exists(field => field != null);

        private BossController bossController;

        private void Awake()
        {
            bossController = GetComponent<BossController>();
            if (attackPoint == null)
                attackPoint = transform.Find("AttackPoint") ?? transform;
            if (playerLayer.value == 0)
                playerLayer = LayerMask.GetMask("Player");

            if (bossController == null)
            {
                Debug.LogError("[BossCombat] BossController를 찾을 수 없습니다.");
            }
        }

        public void PerformBasicAttack()
        {
            if (bossController == null)
                return;

            if (bossController.IsDead)
                return;

            if (attackPoint == null)
            {
                Debug.LogWarning("[BossCombat] AttackPoint가 없습니다.");

                return;
            }

            float attackRange = GetAttackRange();

            Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

            bool hitPlayer = false;

            foreach (Collider2D target in targets)
            {
                PlayerController player = target.GetComponentInParent<PlayerController>();

                if (player == null)
                    continue;

                float finalDamage = baseDamage * bossController.DamageMultiplier;

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
                Debug.Log("[BOSS BASIC ATTACK] 공격했지만 플레이어에게 명중하지 않음");
            }
        }

        public void PerformAreaAttack()
        {
            if (bossController == null)
                return;

            if (bossController.IsDead)
                return;

            if (attackPoint == null)
            {
                Debug.LogWarning("[BossCombat] AttackPoint가 없습니다.");

                return;
            }

            float areaRange = GetAreaAttackRange();

            Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, areaRange, playerLayer);

            bool hitPlayer = false;

            foreach (Collider2D target in targets)
            {
                PlayerController player = target.GetComponentInParent<PlayerController>();

                if (player == null)
                    continue;

                float finalDamage = baseDamage * bossController.DamageMultiplier;

                player.TakeDamage(finalDamage, strongHitStun);

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
                Debug.Log("[BOSS AREA ATTACK] 플레이어 미명중");
            }
        }

        public void PerformTelegraphedMelee(Vector2 center, float warning, bool strong)
        {
            float range = strong ? GetAreaAttackRange() : GetAttackRange();
            SpawnField(center, new Vector2(range * 2f, 2.5f), warning, 0.15f,
                strong ? 1.5f : 1f, strong ? strongHitStun : 0f);
        }

        public void PerformTargetedFieldAttack(Vector2 targetPosition, float warning, int phase)
        {
            float width = phase == 1 ? 2.5f : phase == 2 ? 3.5f : 4.5f;
            float halfWidth = Mathf.Max(width * 0.5f, arenaSize.x * 0.5f);
            float x = Mathf.Clamp(targetPosition.x,
                ArenaCenter.x - halfWidth + width * 0.5f,
                ArenaCenter.x + halfWidth - width * 0.5f);
            SpawnField(new Vector2(x, ArenaCenter.y), new Vector2(width, arenaSize.y),
                warning, 0.35f, 1f, strongHitStun);
        }

        public void PerformLaneAttack(float warning, int phase, int sequence)
        {
            int lanes = 5;
            int safeLane = sequence % lanes;
            float laneWidth = arenaSize.x / lanes;
            for (int lane = 0; lane < lanes; lane++)
            {
                if (lane == safeLane || (phase == 1 && lane % 2 == 0)) continue;
                float x = ArenaCenter.x - arenaSize.x * 0.5f + (lane + 0.5f) * laneWidth;
                SpawnField(new Vector2(x, ArenaCenter.y), new Vector2(laneWidth * 0.9f, arenaSize.y),
                    warning, 0.35f, 1f, strongHitStun);
            }
        }

        private void SpawnField(Vector2 center, Vector2 size, float warning, float active, float multiplier, float stun)
        {
            if (bossController == null || bossController.IsDead || !isActiveAndEnabled) return;
            fields.RemoveAll(field => field == null);
            fields.Add(BossFieldAttack.Create(bossController, center, size, playerLayer,
                baseDamage * bossController.DamageMultiplier * multiplier, stun, warning, active));
        }

        public void CancelFieldAttacks()
        {
            foreach (BossFieldAttack field in fields)
                if (field != null) field.Cancel();
            fields.Clear();
        }

        private void OnDisable() => CancelFieldAttacks();

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

            Gizmos.DrawWireSphere(attackPoint.position, range);
        }
    }
}
