using UnityEngine;

namespace Carten
{
    public class BossController : EnemyBase
    {
        // ========================================================
        // Boss Phase
        // ========================================================

        public enum BossPhase
        {
            Phase1,
            Phase2,
            Phase3
        }

        // ========================================================
        // Phase Settings
        // ========================================================

        [Header("=== Boss Phase ===")]
        [SerializeField] private float phase2HealthPercent = 70f;
        [SerializeField] private float phase3HealthPercent = 30f;

        // ========================================================
        // Attack Speed
        // ========================================================

        [Header("=== Attack Speed Multiplier ===")]
        [SerializeField] private float phase1AttackSpeedMultiplier = 1f;
        [SerializeField] private float phase2AttackSpeedMultiplier = 1.5f;
        [SerializeField] private float phase3AttackSpeedMultiplier = 2f;

        // ========================================================
        // Damage
        // ========================================================

        [Header("=== Damage Multiplier ===")]
        [SerializeField] private float phase1DamageMultiplier = 1f;
        [SerializeField] private float phase2DamageMultiplier = 1.5f;
        [SerializeField] private float phase3DamageMultiplier = 2f;

        // ========================================================
        // Debug
        // ========================================================

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        // ========================================================
        // Runtime
        // ========================================================

        private BossPhase currentPhase;

        public BossPhase CurrentPhase => currentPhase;

        public float HealthPercent
        {
            get
            {
                if (maxHealth <= 0f)
                    return 0f;

                return (currentHealth / maxHealth) * 100f;
            }
        }

        // ========================================================
        // Current Attack Speed
        // ========================================================

        public float AttackSpeedMultiplier
        {
            get
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase2:
                        return phase2AttackSpeedMultiplier;

                    case BossPhase.Phase3:
                        return phase3AttackSpeedMultiplier;

                    default:
                        return phase1AttackSpeedMultiplier;
                }
            }
        }

        // ========================================================
        // Current Damage
        // ========================================================

        public float DamageMultiplier
        {
            get
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase2:
                        return phase2DamageMultiplier;

                    case BossPhase.Phase3:
                        return phase3DamageMultiplier;

                    default:
                        return phase1DamageMultiplier;
                }
            }
        }

        // ========================================================
        // Awake
        // ========================================================

        protected override void Awake()
        {
            base.Awake();

            UpdatePhase();

            if (showDebugLog)
            {
                Debug.Log(
                    $"[BossController] 보스 생성\n" +
                    $"HP: {currentHealth:F0}/{maxHealth:F0}\n" +
                    $"Phase: {currentPhase}"
                );
            }
        }

        // ========================================================
        // Take Damage
        // ========================================================

        public override void TakeDamage(float damage)
        {
            if (IsDead)
                return;

            base.TakeDamage(damage);

            if (IsDead)
                return;

            UpdatePhase();
        }

        // ========================================================
        // Phase Update
        // ========================================================

        private void UpdatePhase()
        {
            float healthPercent = HealthPercent;

            BossPhase previousPhase = currentPhase;

            if (healthPercent <= phase3HealthPercent)
            {
                currentPhase = BossPhase.Phase3;
            }
            else if (healthPercent <= phase2HealthPercent)
            {
                currentPhase = BossPhase.Phase2;
            }
            else
            {
                currentPhase = BossPhase.Phase1;
            }

            if (previousPhase != currentPhase)
            {
                OnPhaseChanged(previousPhase, currentPhase);
            }
        }

        // ========================================================
        // Phase Changed
        // ========================================================

        private void OnPhaseChanged(
            BossPhase previousPhase,
            BossPhase newPhase)
        {
            if (!showDebugLog)
                return;

            Debug.Log(
                $"==============================\n" +
                $"[BOSS PHASE CHANGE]\n" +
                $"보스: {gameObject.name}\n" +
                $"Phase: {previousPhase} → {newPhase}\n" +
                $"현재 HP: {currentHealth:F1}/{maxHealth:F1}\n" +
                $"HP: {HealthPercent:F1}%\n" +
                $"공격속도 배율: x{AttackSpeedMultiplier:F1}\n" +
                $"데미지 배율: x{DamageMultiplier:F1}\n" +
                $"=============================="
            );
        }

        // ========================================================
        // Damaged
        // ========================================================

        protected override void OnDamaged(float damage)
        {
            base.OnDamaged(damage);

            if (showDebugLog)
            {
                Debug.Log(
                    $"[BossController] 피격\n" +
                    $"Damage: {damage:F1}\n" +
                    $"HP: {currentHealth:F1}/{maxHealth:F1}\n" +
                    $"HP: {HealthPercent:F1}%\n" +
                    $"Phase: {currentPhase}"
                );
            }
        }

        // ========================================================
        // Death
        // ========================================================

        protected override void Die()
        {
            if (showDebugLog)
            {
                Debug.Log(
                    $"================================\n" +
                    $"[BOSS DEFEATED]\n" +
                    $"보스: {gameObject.name}\n" +
                    $"================================"
                );
            }

            gameObject.SetActive(false);
        }
    }
}