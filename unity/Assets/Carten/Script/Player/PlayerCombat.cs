using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("=== Attack Point ===")]
        [SerializeField] private Transform attackPoint;

        [Header("=== Attack Settings ===")]
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 0.5f;

        [Header("=== Target Layer ===")]
        [SerializeField] private LayerMask targetLayer;

        [Header("=== Input ===")]
        [SerializeField] private KeyCode attackKey = KeyCode.Keypad3;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private PlayerController playerController;
        private float attackTimer;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            if (playerController == null)
            {
                Debug.LogError("[PlayerCombat] PlayerController를 찾을 수 없습니다.");
            }
        }

        private void Update()
        {
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }

            HandleAttack();
        }

        private void HandleAttack()
        {
            if (playerController == null || !playerController.CanAct)
                return;
            if (!Input.GetKeyDown(attackKey))
                return;

            if (attackTimer > 0f)
            {
                if (showDebugLog)
                {
                    Debug.Log($"[PlayerCombat] 공격 쿨타임 중 → {attackTimer:F2}초");
                }
                return;
            }

            PerformAttack();
        }

        private void PerformAttack()
        {
            // ★ 기본 공격 애니메이션(Player_P1_Attack) 실행!
            playerController?.TriggerAttackAnimation();

            // 1. 현재 Phase 확인
            float attackSpeedMultiplier = 1f;
            float criticalMultiplier = 1f;

            if (playerController != null)
            {
                attackSpeedMultiplier = playerController.AttackSpeedMultiplier;
                criticalMultiplier = playerController.CriticalMultiplier;
            }

            // 2. 공격 쿨타임 계산
            attackTimer = attackCooldown / Mathf.Max(0.01f, attackSpeedMultiplier);

            // 3. 크리티컬 판정
            float criticalChance = 0f;
            if (playerController != null &&
                playerController.CurrentPhase == PlayerController.PlayerPhase.Phase3)
            {
                criticalChance = 100f;
            }

            bool isCritical = Random.Range(0f, 100f) < criticalChance;

            // 4. 최종 데미지 계산
            float finalDamage = attackDamage;
            if (isCritical)
            {
                finalDamage *= criticalMultiplier;
            }

            // 5. Attack Point 확인
            if (attackPoint == null)
            {
                Debug.LogWarning("[PlayerCombat] AttackPoint가 설정되지 않았습니다.");
                return;
            }

            // 6. 공격 범위 판정
            Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayer);
            HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

            foreach (Collider2D target in targets)
            {
                IDamageable damageable = target.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.IsDead || hitTargets.Contains(damageable))
                    continue;

                hitTargets.Add(damageable);
                damageable.TakeDamage(finalDamage);
            }

            // 7. 디버그 로그
            if (showDebugLog)
            {
                string criticalText = isCritical ? "★ CRITICAL ★" : "일반 공격";
                Debug.Log(
                    $"[PlayerCombat] 공격\n" +
                    $"타입: {criticalText}\n" +
                    $"Phase: {GetCurrentPhaseName()}\n" +
                    $"공격력: {finalDamage:F1}\n" +
                    $"명중 대상: {hitTargets.Count}"
                );
            }
        }

        private string GetCurrentPhaseName()
        {
            return playerController == null ? "Unknown" : playerController.CurrentPhase.ToString();
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            Gizmos.DrawLine(transform.position, attackPoint.position);
        }
    }
}
