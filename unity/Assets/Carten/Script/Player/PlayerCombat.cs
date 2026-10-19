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

        [Header("=== Hit Feedback Settings ===")]
        [Tooltip("적중 시 멈추는 시간 (추천: 0.05 ~ 0.1초)")]
        [SerializeField] private float hitStopDuration = 0.08f;
        [Tooltip("적에게 공격이 명중했을 때 재생할 타격음 (SFX)")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField, Range(0f, 1f)] private float hitSoundVolume = 1f;

        [Header("=== Input ===")]
        [SerializeField] private KeyCode attackKey = KeyCode.Keypad3;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private PlayerController playerController;
        private AudioSource audioSource;
        private float attackTimer;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            audioSource = GetComponent<AudioSource>();

            if (playerController == null)
            {
                Debug.LogError("[PlayerCombat] PlayerController를 찾을 수 없습니다.");
            }

            if (audioSource == null)
            {
                // AudioSource가 없으면 자동으로 추가해 줍니다.
                audioSource = gameObject.AddComponent<AudioSource>();
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
            // 기본 공격 애니메이션 실행
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

            // 6. 공격 범위 판정 및 데미지/넉백/피격 적용
            Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayer);
            HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

            foreach (Collider2D target in targets)
            {
                IDamageable damageable = target.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.IsDead || hitTargets.Contains(damageable))
                    continue;

                hitTargets.Add(damageable);
                damageable.TakeDamage(finalDamage);

                // 넉백 적용
                Enemy1Controller enemy1 = target.GetComponentInParent<Enemy1Controller>();
                if (enemy1 != null)
                {
                    enemy1.ApplyKnockback(transform.position);
                }
            }

            // 7. 타격 성공 시 피드백 (히트 스톱 + 타격음 SFX)
            if (hitTargets.Count > 0)
            {
                // 히트 스톱 발동
                if (HitStopController.Instance != null)
                {
                    HitStopController.Instance.TriggerHitStop(hitStopDuration);
                }

                // 타격음 재생
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound, hitSoundVolume);
                }
            }

            // 8. 디버그 로그
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
