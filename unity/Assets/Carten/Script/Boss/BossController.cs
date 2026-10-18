using System.Collections;
using UnityEngine;

namespace Carten
{
    public class BossController : EnemyBase
    {
        public enum BossPhase
        {
            Phase1,
            Phase2,
            Phase3
        }

        [Header("=== Boss Phase ===")]
        [SerializeField] private float phase2HealthPercent = 70f;
        [SerializeField] private float phase3HealthPercent = 30f;

        [Header("=== Boss Animators (페이즈별 애니메이터가 다를 경우에만 연결) ===")]
        [SerializeField] private RuntimeAnimatorController phase1Animator;
        [SerializeField] private RuntimeAnimatorController phase2Animator;
        [SerializeField] private RuntimeAnimatorController phase3Animator;
        [SerializeField] private float deathDisableDelay = 1.0f;

        [Header("=== Attack Speed Multiplier ===")]
        [SerializeField] private float phase1AttackSpeedMultiplier = 1f;
        [SerializeField] private float phase2AttackSpeedMultiplier = 1.5f;
        [SerializeField] private float phase3AttackSpeedMultiplier = 2f;

        [Header("=== Damage Multiplier ===")]
        [SerializeField] private float phase1DamageMultiplier = 1f;
        [SerializeField] private float phase2DamageMultiplier = 1.5f;
        [SerializeField] private float phase3DamageMultiplier = 2f;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        public enum BossKind { MiniBoss, FinalBoss }
        [SerializeField] private BossKind bossKind = BossKind.MiniBoss;
        public BossKind Kind => bossKind;
        public bool IsStationary => bossKind == BossKind.FinalBoss;
        public event System.Action<BossPhase> PhaseChanged;
        public event System.Action<BossController> Defeated;
        public event System.Action EncounterStarted;
        public bool HasEncounterStarted { get; private set; }

        public void BeginEncounter()
        {
            if (HasEncounterStarted || IsDead) return;
            HasEncounterStarted = true;
            EncounterStarted?.Invoke();
        }

        private BossPhase currentPhase;
        private Animator anim;

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

        public float AttackSpeedMultiplier
        {
            get
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase2: return phase2AttackSpeedMultiplier;
                    case BossPhase.Phase3: return phase3AttackSpeedMultiplier;
                    default: return phase1AttackSpeedMultiplier;
                }
            }
        }

        public float DamageMultiplier
        {
            get
            {
                switch (currentPhase)
                {
                    case BossPhase.Phase2: return phase2DamageMultiplier;
                    case BossPhase.Phase3: return phase3DamageMultiplier;
                    default: return phase1DamageMultiplier;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            anim = GetComponent<Animator>();
            if (anim != null && phase1Animator == null)
                phase1Animator = anim.runtimeAnimatorController;

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

        public override void TakeDamage(float damage)
        {
            if (IsDead)
                return;

            if (float.IsNaN(damage) || float.IsInfinity(damage)) return;
            if (damage > 0f) BeginEncounter();
            base.TakeDamage(damage);

            if (IsDead)
                return;

            UpdatePhase();
        }

        private void UpdatePhase()
        {
            float healthPercent = HealthPercent;
            BossPhase previousPhase = currentPhase;

            if (IsStationary && healthPercent <= phase3HealthPercent)
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

            ApplyPhaseAnimator();

            if (previousPhase != currentPhase)
            {
                PhaseChanged?.Invoke(currentPhase);
                OnPhaseChanged(previousPhase, currentPhase);
            }
        }

        private void ApplyPhaseAnimator()
        {
            if (anim == null) return;

            RuntimeAnimatorController targetController = phase1Animator;
            if (currentPhase == BossPhase.Phase2 && phase2Animator != null)
                targetController = phase2Animator;
            else if (currentPhase == BossPhase.Phase3)
                targetController = phase3Animator != null ? phase3Animator : (phase2Animator != null ? phase2Animator : phase1Animator);

            if (targetController != null && anim.runtimeAnimatorController != targetController)
                anim.runtimeAnimatorController = targetController;
        }

        private void OnPhaseChanged(BossPhase previousPhase, BossPhase newPhase)
        {
            if (!showDebugLog)
                return;

            Debug.Log(
                $"==============================\n" +
                $"[BOSS PHASE CHANGE]\n" +
                $"보스: {gameObject.name}\n" +
                $"Phase: {previousPhase} → {newPhase}\n" +
                $"현재 HP: {currentHealth:F1}/{maxHealth:F1}\n" +
                $"=============================="
            );
        }

        protected override void OnDamaged(float damage)
        {
            base.OnDamaged(damage);

            // ★ 보스 피격 애니메이션 실행
            if (!IsDead && damage > 0f && anim != null)
            {
                anim.SetTrigger("doDamage");
            }

            if (showDebugLog)
            {
                Debug.Log(
                    $"[BossController] 피격\n" +
                    $"Damage: {damage:F1}\n" +
                    $"HP: {currentHealth:F1}/{maxHealth:F1}\n" +
                    $"Phase: {currentPhase}"
                );
            }
        }

        public void TriggerAttackAnimation(bool isStrongAttack = false)
        {
            if (anim == null || IsDead) return;
            anim.SetTrigger(isStrongAttack ? "doSkill" : "doAttack");
        }

        [Header("=== Clear Condition ===")]
        [SerializeField] private string clearConditionID = "Stage1_MidBoss_Cleared";

        protected override void Die()
        {
            Defeated?.Invoke(this);

            if (!string.IsNullOrWhiteSpace(clearConditionID))
            {
                PlayerPrefs.SetInt(clearConditionID, 1);
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SetTriggered(clearConditionID);
                }
                Debug.Log($"[BossController] 클리어 조건 달성 저장 완료: {clearConditionID}");
            }

            if (showDebugLog)
            {
                Debug.Log(
                    $"================================\n" +
                    $"[BOSS DEFEATED]\n" +
                    $"보스: {gameObject.name}\n" +
                    $"================================"
                );
            }

            // ★ 사망 애니메이션 재생 후 비활성화
            if (anim != null)
            {
                anim.SetBool("isWork", false);
                anim.SetTrigger("doDeath");
                StartCoroutine(DisableAfterDeathRoutine());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator DisableAfterDeathRoutine()
        {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in colliders) col.enabled = false;

            yield return new WaitForSeconds(Mathf.Max(0.1f, deathDisableDelay));
            gameObject.SetActive(false);
        }
    }
}
