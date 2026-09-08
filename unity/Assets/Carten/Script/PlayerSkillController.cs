using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class PlayerSkillController : MonoBehaviour
    {
        // =========================================================
        // References
        // =========================================================

        [Header("=== References ===")]

        [SerializeField]
        private PlayerController playerController;

        [SerializeField]
        private Transform attackPoint;


        // =========================================================
        // Skill Key
        // =========================================================

        [Header("=== Skill Keys ===")]

        [SerializeField]
        private KeyCode skillQKey = KeyCode.Q;

        [SerializeField]
        private KeyCode skillWKey = KeyCode.W;

        [SerializeField]
        private KeyCode skillEKey = KeyCode.E;


        // =========================================================
        // Phase 1 - Q : Heavy Strike
        // =========================================================

        [Header("=== Phase 1 - Q : Heavy Strike ===")]

        [SerializeField]
        private float phase1QDamage = 30f;

        [SerializeField]
        private float phase1QRange = 1.5f;

        [SerializeField]
        private float phase1QCooldown = 2f;

        private float phase1QCooldownTimer;


        // =========================================================
        // Phase 1 - W : Defense System
        // =========================================================

        [Header("=== Phase 1 - W : Defense System ===")]

        [Tooltip("W 사용 시 추가 피해 감소량")]
        [SerializeField]
        [Range(0f, 1f)]
        private float phase1WDefenseBonus = 0.2f;

        [Tooltip("W 지속 시간")]
        [SerializeField]
        private float phase1WDuration = 3f;

        [Tooltip("W 쿨타임")]
        [SerializeField]
        private float phase1WCooldown = 8f;

        private float phase1WDurationTimer;
        private float phase1WCooldownTimer;

        private bool phase1WActive;


        // =========================================================
        // Phase 1 - E : Gravity Boost
        // =========================================================

        [Header("=== Phase 1 - E : Gravity Boost ===")]

        [Tooltip("E 쿨타임")]
        [SerializeField]
        private float phase1ECooldown = 4f;

        [Tooltip("E 수평 추진 속도")]
        [SerializeField]
        private float phase1EHorizontalVelocity = 10f;

        [Tooltip("E 위쪽 추진 속도")]
        [SerializeField]
        private float phase1EVerticalVelocity = 8f;

        [Tooltip("E 추진 지속 시간")]
        [SerializeField]
        private float phase1EForceDuration = 0.35f;

        private float phase1ECooldownTimer;
        private float phase1EForceTimer;

        private bool phase1EActive;


        // =========================================================
        // Target Layer
        // =========================================================

        [Header("=== Target ===")]

        [SerializeField]
        private LayerMask targetLayer;


        // =========================================================
        // Debug
        // =========================================================

        [Header("=== Debug ===")]

        [SerializeField]
        private bool showDebugLog = true;


        // =========================================================
        // Internal
        // =========================================================

        private readonly HashSet<IDamageable> damagedTargets =
            new HashSet<IDamageable>();


        // =========================================================
        // Awake
        // =========================================================

        private void Awake()
        {
            if (playerController == null)
            {
                playerController =
                    GetComponent<PlayerController>();
            }

            if (attackPoint == null)
            {
                Transform foundAttackPoint =
                    transform.Find("AttackPoint");

                if (foundAttackPoint != null)
                {
                    attackPoint = foundAttackPoint;
                }
            }

            // Enemy Layer 자동 설정
            if (targetLayer.value == 0)
            {
                int enemyLayer =
                    LayerMask.NameToLayer("Enemy");

                if (enemyLayer >= 0)
                {
                    targetLayer =
                        1 << enemyLayer;
                }
            }

            phase1QCooldownTimer = 0f;

            phase1WDurationTimer = 0f;
            phase1WCooldownTimer = 0f;
            phase1WActive = false;

            phase1ECooldownTimer = 0f;
            phase1EForceTimer = 0f;
            phase1EActive = false;
        }


        // =========================================================
        // Update
        // =========================================================

        private void Update()
        {
            if (playerController == null)
                return;

            if (playerController.IsDead)
                return;

            HandleTimers();
            HandleSkillInput();
            HandleWDuration();
            HandleEDuration();
        }


        // =========================================================
        // Timer
        // =========================================================

        private void HandleTimers()
        {
            if (phase1QCooldownTimer > 0f)
            {
                phase1QCooldownTimer -=
                    Time.deltaTime;

                if (phase1QCooldownTimer < 0f)
                {
                    phase1QCooldownTimer = 0f;
                }
            }

            if (phase1WCooldownTimer > 0f)
            {
                phase1WCooldownTimer -=
                    Time.deltaTime;

                if (phase1WCooldownTimer < 0f)
                {
                    phase1WCooldownTimer = 0f;
                }
            }

            if (phase1ECooldownTimer > 0f)
            {
                phase1ECooldownTimer -=
                    Time.deltaTime;

                if (phase1ECooldownTimer < 0f)
                {
                    phase1ECooldownTimer = 0f;
                }
            }
        }


        // =========================================================
        // Skill Input
        // =========================================================

        private void HandleSkillInput()
        {
            // E 추진 중에는 다른 스킬 입력을 받지 않음
            if (phase1EActive)
                return;

            if (Input.GetKeyDown(skillQKey))
            {
                ExecuteQ();
            }

            if (Input.GetKeyDown(skillWKey))
            {
                ExecuteW();
            }

            if (Input.GetKeyDown(skillEKey))
            {
                ExecuteE();
            }
        }


        // =========================================================
        // Q Dispatch
        // =========================================================

        private void ExecuteQ()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1:
                    Phase1SkillQ();
                    break;

                case PlayerController.PlayerPhase.Phase2:
                    DebugSkillLog(
                        "Phase 2",
                        "Q 스킬 미구현"
                    );
                    break;

                case PlayerController.PlayerPhase.Phase3:
                    DebugSkillLog(
                        "Phase 3",
                        "Q 스킬 미구현"
                    );
                    break;
            }
        }


        // =========================================================
        // W Dispatch
        // =========================================================

        private void ExecuteW()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1:
                    Phase1SkillW();
                    break;

                case PlayerController.PlayerPhase.Phase2:
                    DebugSkillLog(
                        "Phase 2",
                        "W 스킬 미구현"
                    );
                    break;

                case PlayerController.PlayerPhase.Phase3:
                    DebugSkillLog(
                        "Phase 3",
                        "W 스킬 미구현"
                    );
                    break;
            }
        }


        // =========================================================
        // E Dispatch
        // =========================================================

        private void ExecuteE()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1:
                    Phase1SkillE();
                    break;

                case PlayerController.PlayerPhase.Phase2:
                    DebugSkillLog(
                        "Phase 2",
                        "E 스킬 미구현"
                    );
                    break;

                case PlayerController.PlayerPhase.Phase3:
                    DebugSkillLog(
                        "Phase 3",
                        "E 스킬 미구현"
                    );
                    break;
            }
        }


        // =========================================================
        // Phase 1 - Q
        // =========================================================

        private void Phase1SkillQ()
        {
            if (phase1QCooldownTimer > 0f)
                return;

            if (attackPoint == null)
            {
                Debug.LogWarning(
                    "[PlayerSkillController] " +
                    "AttackPoint가 없습니다."
                );

                return;
            }

            phase1QCooldownTimer =
                phase1QCooldown;

            PerformHeavyStrike();

            DebugSkillLog(
                "Phase 1",
                "Q - 중타격"
            );
        }


        // =========================================================
        // Phase 1 - W
        // =========================================================

        private void Phase1SkillW()
        {
            if (phase1WActive)
                return;

            if (phase1WCooldownTimer > 0f)
                return;

            StartDefenseSkill();
        }


        // =========================================================
        // Phase 1 - E
        // =========================================================

        private void Phase1SkillE()
        {
            if (phase1EActive)
                return;

            if (phase1ECooldownTimer > 0f)
                return;

            StartGravityBoostSkill();
        }


        // =========================================================
        // Heavy Strike
        // =========================================================

        private void PerformHeavyStrike()
        {
            damagedTargets.Clear();

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    attackPoint.position,
                    phase1QRange,
                    targetLayer
                );

            foreach (Collider2D hit in hits)
            {
                if (hit == null)
                    continue;

                IDamageable damageable =
                    hit.GetComponent<IDamageable>();

                if (damageable == null)
                {
                    damageable =
                        hit.GetComponentInParent<IDamageable>();
                }

                if (damageable == null)
                    continue;

                if (damagedTargets.Contains(damageable))
                    continue;

                damagedTargets.Add(damageable);

                damageable.TakeDamage(
                    phase1QDamage
                );

                if (showDebugLog)
                {
                    Debug.Log(
                        $"[PlayerSkillController] " +
                        $"Q 적중: {hit.name} / " +
                        $"Damage: {phase1QDamage:F1}"
                    );
                }
            }
        }


        // =========================================================
        // W Start
        // =========================================================

        private void StartDefenseSkill()
        {
            phase1WActive = true;

            phase1WDurationTimer =
                phase1WDuration;

            phase1WCooldownTimer =
                phase1WCooldown;

            // PlayerController에 실제 추가 방어력 적용
            playerController.SetAdditionalDamageReduction(
                phase1WDefenseBonus
            );

            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerSkillController] " +
                    $"W - 방어 시스템 활성화 / " +
                    $"추가 방어: " +
                    $"{phase1WDefenseBonus * 100f:F0}% / " +
                    $"지속: {phase1WDuration:F1}s / " +
                    $"현재 방어: " +
                    $"{playerController.DefensePercent:F0}%"
                );
            }
        }


        // =========================================================
        // W Duration
        // =========================================================

        private void HandleWDuration()
        {
            if (!phase1WActive)
                return;

            phase1WDurationTimer -=
                Time.deltaTime;

            if (phase1WDurationTimer <= 0f)
            {
                EndDefenseSkill();
            }
        }


        // =========================================================
        // W End
        // =========================================================

        private void EndDefenseSkill()
        {
            phase1WActive = false;

            phase1WDurationTimer = 0f;

            // 추가 방어력 제거
            playerController.ClearAdditionalDamageReduction();

            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerSkillController] " +
                    $"W - 방어 시스템 종료 / " +
                    $"현재 방어: " +
                    $"{playerController.DefensePercent:F0}%"
                );
            }
        }
        // =========================================================
        // E Start : Gravity Boost
        // =========================================================

        private void StartGravityBoostSkill()
        {
            phase1EActive = true;

            phase1EForceTimer =
                phase1EForceDuration;

            phase1ECooldownTimer =
                phase1ECooldown;

            // 현재 플레이어 방향 확인
            float direction =
                transform.localScale.x >= 0f
                    ? 1f
                    : -1f;

            float horizontalVelocity =
                direction *
                phase1EHorizontalVelocity;

            // PlayerController에 추진 시작 요청
            playerController.StartGravityBoost(
                horizontalVelocity,
                phase1EVerticalVelocity
            );

            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerSkillController] " +
                    $"E - 중력 추진 시작 / " +
                    $"X: {horizontalVelocity:F1} / " +
                    $"Y: {phase1EVerticalVelocity:F1} / " +
                    $"지속: {phase1EForceDuration:F2}s"
                );
            }
        }


        // =========================================================
        // E Duration
        // =========================================================

        private void HandleEDuration()
        {
            if (!phase1EActive)
                return;

            phase1EForceTimer -=
                Time.deltaTime;

            if (phase1EForceTimer <= 0f)
            {
                EndGravityBoostSkill();
            }
        }


        // =========================================================
        // E End : Gravity Boost
        // =========================================================

        private void EndGravityBoostSkill()
        {
            phase1EActive = false;

            phase1EForceTimer = 0f;

            playerController.EndGravityBoost();

            if (showDebugLog)
            {
                Debug.Log(
                    "[PlayerSkillController] " +
                    "E - 중력 추진 종료"
                );
            }
        }


        // =========================================================
        // Defense Getter
        // =========================================================

        public float GetCurrentDefenseReduction()
        {
            if (playerController == null)
                return 0f;

            // PlayerController가 기본 방어 + W 추가 방어를
            // 이미 합산하여 계산하므로 그대로 반환
            return playerController.DamageReduction;
        }


        // =========================================================
        // Skill Status
        // =========================================================

        public bool IsDefenseSkillActive =>
            phase1WActive;

        public bool IsGravityBoostActive =>
            phase1EActive;


        public float DefenseSkillRemainingTime =>
            Mathf.Max(
                0f,
                phase1WDurationTimer
            );

        public float DefenseSkillCooldownRemaining =>
            Mathf.Max(
                0f,
                phase1WCooldownTimer
            );

        public float GravityBoostCooldownRemaining =>
            Mathf.Max(
                0f,
                phase1ECooldownTimer
            );


        // =========================================================
        // Debug Log
        // =========================================================

        private void DebugSkillLog(
            string phase,
            string skillName)
        {
            if (!showDebugLog)
                return;

            Debug.Log(
                $"[PlayerSkillController] " +
                $"{phase} / {skillName}"
            );
        }


        // =========================================================
        // Gizmos
        // =========================================================

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
                return;

            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                attackPoint.position,
                phase1QRange
            );
        }


        // =========================================================
        // Disable / Destroy Safety
        // =========================================================

        private void OnDisable()
        {
            // 플레이어가 비활성화될 때
            // W 추가 방어력이 남지 않도록 정리
            if (playerController != null)
            {
                if (phase1WActive)
                {
                    playerController
                        .ClearAdditionalDamageReduction();
                }

                if (phase1EActive)
                {
                    playerController
                        .EndGravityBoost();
                }
            }

            phase1WActive = false;
            phase1EActive = false;

            phase1WDurationTimer = 0f;
            phase1EForceTimer = 0f;
        }
    }
}