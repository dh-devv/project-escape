using System.Collections;
using UnityEngine;

namespace Carten
{
    public class BossPattern : MonoBehaviour
    {
        // ========================================================
        // Pattern Timing
        // ========================================================

        [Header("=== Pattern Timing ===")]
        [SerializeField] private float phase1AttackInterval = 1.5f;
        [SerializeField] private float phase2AttackInterval = 1.0f;
        [SerializeField] private float phase3AttackInterval = 0.5f;

        // ========================================================
        // Phase 2 Pattern
        // ========================================================

        [Header("=== Phase 2 Pattern ===")]
        [SerializeField] private float phase2ChargeChance = 0.35f;

        // ========================================================
        // Phase 3 Pattern
        // ========================================================

        [Header("=== Phase 3 Pattern ===")]
        [SerializeField] private float phase3ChargeChance = 0.45f;
        [SerializeField] private float phase3AreaChance = 0.35f;

        // ========================================================
        // Debug
        // ========================================================

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        // ========================================================
        // Runtime
        // ========================================================

        private BossController bossController;
        private BossCombat bossCombat;
        private BossAI bossAI;

        private float patternTimer;
        private bool isExecutingPattern;

        // ========================================================
        // Awake
        // ========================================================

        private void Awake()
        {
            bossController = GetComponent<BossController>();
            bossCombat = GetComponent<BossCombat>();
            bossAI = GetComponent<BossAI>();

            if (bossController == null)
            {
                Debug.LogError(
                    "[BossPattern] BossController를 찾을 수 없습니다."
                );
            }

            if (bossCombat == null)
            {
                Debug.LogError(
                    "[BossPattern] BossCombat을 찾을 수 없습니다."
                );
            }

            if (bossAI == null)
            {
                Debug.LogError(
                    "[BossPattern] BossAI를 찾을 수 없습니다."
                );
            }
        }

        // ========================================================
        // Update
        // ========================================================

        private void Update()
        {
            if (bossController == null)
                return;

            if (bossCombat == null)
                return;

            if (bossController.IsDead)
                return;

            if (isExecutingPattern)
                return;

            patternTimer -= Time.deltaTime;

            if (patternTimer > 0f)
                return;

            // 공격 범위 안에 있을 때만 패턴 실행
            if (bossAI != null)
            {
                if (!bossAI.IsInAttackRange())
                    return;
            }

            StartCoroutine(ExecutePattern());
        }

        // ========================================================
        // Execute Pattern
        // ========================================================

        private IEnumerator ExecutePattern()
        {
            isExecutingPattern = true;

            switch (bossController.CurrentPhase)
            {
                case BossController.BossPhase.Phase1:

                    ExecutePhase1Pattern();

                    break;

                case BossController.BossPhase.Phase2:

                    ExecutePhase2Pattern();

                    break;

                case BossController.BossPhase.Phase3:

                    yield return ExecutePhase3Pattern();

                    break;
            }

            patternTimer = GetAttackInterval();

            isExecutingPattern = false;
        }

        // ========================================================
        // Phase 1
        // ========================================================

        private void ExecutePhase1Pattern()
        {
            if (showDebugLog)
            {
                Debug.Log(
                    "[BOSS PATTERN] Phase 1 → 기본 공격"
                );
            }

            bossCombat.PerformBasicAttack();
        }

        // ========================================================
        // Phase 2
        // ========================================================

        private void ExecutePhase2Pattern()
        {
            float random =
                Random.Range(0f, 1f);

            if (random < phase2ChargeChance)
            {
                if (showDebugLog)
                {
                    Debug.Log(
                        "[BOSS PATTERN] Phase 2 → 돌진 공격"
                    );
                }

                // 현재 돌진 공격 시스템과 연결하기 전까지
                // 기본 공격으로 임시 처리
                bossCombat.PerformBasicAttack();
            }
            else
            {
                if (showDebugLog)
                {
                    Debug.Log(
                        "[BOSS PATTERN] Phase 2 → 기본 공격"
                    );
                }

                bossCombat.PerformBasicAttack();
            }
        }

        // ========================================================
        // Phase 3
        // ========================================================

        private IEnumerator ExecutePhase3Pattern()
        {
            float random =
                Random.Range(0f, 1f);

            // ----------------------------------------------------
            // Area Attack
            // ----------------------------------------------------

            if (random < phase3AreaChance)
            {
                if (showDebugLog)
                {
                    Debug.Log(
                        "[BOSS PATTERN] Phase 3 → 광역 공격 준비"
                    );
                }

                // 공격 전조
                yield return new WaitForSeconds(0.5f);

                if (bossController.IsDead)
                    yield break;

                bossCombat.PerformAreaAttack();

                // 후딜레이
                yield return new WaitForSeconds(0.3f);
            }

            // ----------------------------------------------------
            // Combo Attack
            // ----------------------------------------------------

            else if (
                random <
                phase3AreaChance + phase3ChargeChance
            )
            {
                if (showDebugLog)
                {
                    Debug.Log(
                        "[BOSS PATTERN] Phase 3 → 연속 공격"
                    );
                }

                bossCombat.PerformBasicAttack();

                yield return new WaitForSeconds(0.12f);

                if (bossController.IsDead)
                    yield break;

                bossCombat.PerformBasicAttack();

                yield return new WaitForSeconds(0.12f);

                if (bossController.IsDead)
                    yield break;

                bossCombat.PerformBasicAttack();
            }

            // ----------------------------------------------------
            // Basic Attack
            // ----------------------------------------------------

            else
            {
                if (showDebugLog)
                {
                    Debug.Log(
                        "[BOSS PATTERN] Phase 3 → 기본 공격"
                    );
                }

                bossCombat.PerformBasicAttack();
            }
        }

        // ========================================================
        // Attack Interval
        // ========================================================

        private float GetAttackInterval()
        {
            switch (bossController.CurrentPhase)
            {
                case BossController.BossPhase.Phase2:
                    return phase2AttackInterval;

                case BossController.BossPhase.Phase3:
                    return phase3AttackInterval;

                default:
                    return phase1AttackInterval;
            }
        }
    }
}