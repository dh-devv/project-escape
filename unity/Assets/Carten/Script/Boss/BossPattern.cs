using System.Collections;
using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(BossController), typeof(BossCombat), typeof(BossAI))]
    public class BossPattern : MonoBehaviour
    {
        [SerializeField] private float phase1AttackInterval = 1.5f;
        [SerializeField] private float phase2AttackInterval = 1f;
        [SerializeField] private float phase3AttackInterval = 0.5f;
        [SerializeField, Range(0f, 1f)] private float phase2ChargeChance = 0.35f;
        [SerializeField, Min(0.1f)] private float attackWarning = 0.65f;
        [SerializeField, Min(0f)] private float recoveryTime = 0.35f;
        private BossController boss;
        private BossCombat combat;
        private BossAI ai;
        private float timer;
        private bool executing;
        private int patternIndex;
        public bool IsExecutingPattern => executing;

        private void Awake()
        {
            boss = GetComponent<BossController>();
            combat = GetComponent<BossCombat>();
            ai = GetComponent<BossAI>();
        }
        private void OnEnable()
        {
            boss.PhaseChanged += OnPhaseChanged;
            timer = phase1AttackInterval;
        }
        private void Update()
        {
            if (boss.IsDead || executing || !ai.IsTargetDetected)
                return;
            timer -= Time.deltaTime;
            if (timer > 0f || (!boss.IsStationary && !ai.IsInAttackRange()))
                return;
            boss.BeginEncounter();
            StartCoroutine(ExecutePattern());
        }

        private IEnumerator ExecutePattern()
        {
            executing = true;
            ai.MovementLocked = true;
            ai.StopMovement();
            if (boss.IsStationary)
            {
                int phase = (int)boss.CurrentPhase + 1;
                // Alternate aimed impact and arena lanes; Phase 3 adds a delayed follow-up.
                if (patternIndex++ % 2 == 0)
                    combat.PerformTargetedFieldAttack(ai.Target.position, attackWarning, phase);
                else
                    combat.PerformLaneAttack(attackWarning, phase, patternIndex);
                yield return new WaitForSeconds(attackWarning + 0.4f);
                if (CanContinue() && phase == 3)
                {
                    combat.PerformTargetedFieldAttack(ai.Target.position, attackWarning, phase);
                    yield return new WaitForSeconds(attackWarning + 0.4f);
                }
            }
            else
            {
                bool strong = boss.CurrentPhase == BossController.BossPhase.Phase2 &&
                    Random.value < phase2ChargeChance;
                Vector2 lockedCenter = combat.AttackCenter;
                combat.PerformTelegraphedMelee(lockedCenter, attackWarning, strong);
                yield return new WaitForSeconds(attackWarning + 0.2f);
                if (CanContinue() && !strong && boss.CurrentPhase == BossController.BossPhase.Phase2)
                {
                    combat.PerformTelegraphedMelee(combat.AttackCenter, attackWarning, false);
                    yield return new WaitForSeconds(attackWarning + 0.2f);
                }
            }
            yield return new WaitForSeconds(recoveryTime);
            ai.MovementLocked = false;
            executing = false;
            float interval = boss.CurrentPhase == BossController.BossPhase.Phase1
                ? phase1AttackInterval : boss.CurrentPhase == BossController.BossPhase.Phase2
                ? phase2AttackInterval : phase3AttackInterval;
            timer = Mathf.Max(0.15f, interval / Mathf.Max(0.1f, boss.AttackSpeedMultiplier));
        }

        private bool CanContinue() => !boss.IsDead && ai.IsTargetDetected;
        private void OnPhaseChanged(BossController.BossPhase phase)
        {
            StopAllCoroutines();
            combat.CancelFieldAttacks();
            ai.MovementLocked = false;
            executing = false;
            timer = Mathf.Max(attackWarning, recoveryTime);
        }
        private void OnDisable()
        {
            if (boss != null) boss.PhaseChanged -= OnPhaseChanged;
            StopAllCoroutines();
            if (combat != null) combat.CancelFieldAttacks();
            if (ai != null) ai.MovementLocked = false;
            executing = false;
        }
    }
}
