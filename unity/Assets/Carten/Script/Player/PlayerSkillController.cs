using System.Collections;
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

        private Rigidbody2D rb;


        // =========================================================
        // Skill Input
        // =========================================================

        [Header("=== Skill Input ===")]

        [SerializeField]
        private KeyCode skill1Key = KeyCode.Q;

        [SerializeField]
        private KeyCode skill2Key = KeyCode.W;

        [SerializeField]
        private KeyCode skill3Key = KeyCode.E;


        // =========================================================
        // Phase 1 - Q : Heavy Strike
        // =========================================================

        [Header("=== Phase 1 : Heavy Strike ===")]

        [SerializeField]
        private float heavyStrikeDamage = 30f;

        [SerializeField]
        private float heavyStrikeRange = 1.5f;

        [SerializeField]
        private float heavyStrikeCooldown = 2f;

        private float heavyStrikeTimer;


        // =========================================================
        // Phase 1 - W : Defense System
        // =========================================================

        [Header("=== Phase 1 : Defense System ===")]

        [Range(0f, 1f)]
        [SerializeField]
        private float defenseSkillBonus = 0.2f;

        [SerializeField]
        private float defenseSkillDuration = 3f;

        [SerializeField]
        private float defenseSkillCooldown = 8f;

        private float defenseSkillTimer;
        private float defenseSkillDurationTimer;

        private bool isDefenseSkillActive;


        // =========================================================
        // Phase 1 - E : Gravity Boost
        // =========================================================

        [Header("=== Phase 1 : Gravity Boost ===")]

        [SerializeField]
        private float gravityBoostHorizontal = 10f;

        [SerializeField]
        private float gravityBoostVertical = 8f;

        [SerializeField]
        private float gravityBoostDuration = 0.35f;

        [SerializeField]
        private float gravityBoostCooldown = 4f;

        private float gravityBoostTimer;
        private float gravityBoostDurationTimer;

        private bool isGravityBoostActive;


        // =========================================================
        // Phase 2 - Q : Overdrive
        // =========================================================

        [Header("=== Phase 2 : Overdrive ===")]

        [SerializeField]
        private float overdriveDamage = 20f;

        [SerializeField]
        private float overdriveRange = 1.5f;

        [SerializeField]
        private int overdriveHitCount = 3;

        [SerializeField]
        private float overdriveHitInterval = 0.12f;

        [SerializeField]
        private float overdriveCooldown = 4f;

        private float overdriveTimer;


        // =========================================================
        // Phase 2 - W : Dash Slash
        // =========================================================

        [Header("=== Phase 2 : Dash Slash ===")]

        [SerializeField]
        private float dashSlashSpeed = 14f;

        [SerializeField]
        private float dashSlashDuration = 0.2f;

        [SerializeField]
        private float dashSlashDamage = 35f;

        [SerializeField]
        private float dashSlashRange = 1.6f;

        [SerializeField]
        private float dashSlashCooldown = 3f;

        private float dashSlashTimer;

        private bool isDashSlashActive;


        // =========================================================
        // Phase 2 - E : Boost Explosion
        // =========================================================

        [Header("=== Phase 2 : Boost Explosion ===")]

        [SerializeField]
        private float boostExplosionHorizontal = 11f;

        [SerializeField]
        private float boostExplosionVertical = 10f;

        [SerializeField]
        private float boostExplosionMoveDuration = 0.3f;

        [SerializeField]
        private float boostExplosionDamage = 45f;

        [SerializeField]
        private float boostExplosionRange = 2.5f;

        [SerializeField]
        private float boostExplosionCooldown = 5f;

        private float boostExplosionTimer;

        private bool isBoostExplosionActive;


        // =========================================================
        // Phase 3 - Q : Limit Break
        // =========================================================

        [Header("=== Phase 3 : Limit Break ===")]

        [SerializeField]
        private float limitBreakDamage = 100f;

        [SerializeField]
        private float limitBreakRange = 2.5f;

        [SerializeField]
        private float limitBreakCooldown = 5f;

        private float limitBreakTimer;


        // =========================================================
        // Phase 3 - W : Blink Dash
        // =========================================================

        [Header("=== Phase 3 : Blink Dash ===")]

        [SerializeField]
        private float blinkDashDistance = 4.5f;

        [SerializeField]
        private float blinkDashDamage = 50f;

        [SerializeField]
        private float blinkDashRange = 1.8f;

        [SerializeField]
        private float blinkDashCooldown = 3.5f;

        private float blinkDashTimer;


        // =========================================================
        // Phase 3 - E : Overload Blast
        // =========================================================

        [Header("=== Phase 3 : Overload Blast ===")]

        [SerializeField]
        private float overloadBlastDamage = 90f;

        [SerializeField]
        private float overloadBlastRange = 3f;

        [SerializeField]
        private float overloadBlastCooldown = 6f;

        private float overloadBlastTimer;


        // =========================================================
        // Target Layer
        // =========================================================

        [Header("=== Target Layer ===")]

        [SerializeField]
        private LayerMask targetLayer;


        // =========================================================
        // Debug
        // =========================================================

        [Header("=== Debug ===")]

        [SerializeField]
        private bool showDebugLog = true;


        // =========================================================
        // Internal State
        // =========================================================

        private bool skillExecuting;

        private PlayerController.PlayerPhase previousPhase;


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

            if (rb == null)
            {
                rb =
                    GetComponent<Rigidbody2D>();
            }

            if (attackPoint == null)
            {
                Transform foundAttackPoint =
                    transform.Find("AttackPoint");

                if (foundAttackPoint != null)
                {
                    attackPoint =
                        foundAttackPoint;
                }
            }

            // Enemy Layer 자동 탐색
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

            if (playerController == null)
            {
                Debug.LogError(
                    "[PlayerSkillController] " +
                    "PlayerController를 찾을 수 없습니다."
                );
            }

            if (rb == null)
            {
                Debug.LogError(
                    "[PlayerSkillController] " +
                    "Rigidbody2D를 찾을 수 없습니다."
                );
            }

            if (attackPoint == null)
            {
                Debug.LogError(
                    "[PlayerSkillController] " +
                    "AttackPoint를 찾을 수 없습니다."
                );
            }

            previousPhase =
                playerController != null
                    ? playerController.CurrentPhase
                    : PlayerController.PlayerPhase.Phase1;
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
            HandlePhaseChange();
            HandleSkillInput();
        }


        // =========================================================
        // Timers
        // =========================================================

        private void HandleTimers()
        {
            if (heavyStrikeTimer > 0f)
                heavyStrikeTimer -= Time.deltaTime;

            if (defenseSkillTimer > 0f)
                defenseSkillTimer -= Time.deltaTime;

            if (defenseSkillDurationTimer > 0f)
            {
                defenseSkillDurationTimer -=
                    Time.deltaTime;

                if (defenseSkillDurationTimer <= 0f)
                {
                    EndDefenseSkill();
                }
            }

            if (gravityBoostTimer > 0f)
                gravityBoostTimer -= Time.deltaTime;

            if (gravityBoostDurationTimer > 0f)
            {
                gravityBoostDurationTimer -=
                    Time.deltaTime;

                if (gravityBoostDurationTimer <= 0f)
                {
                    EndGravityBoost();
                }
            }

            if (overdriveTimer > 0f)
                overdriveTimer -= Time.deltaTime;

            if (dashSlashTimer > 0f)
                dashSlashTimer -= Time.deltaTime;

            if (boostExplosionTimer > 0f)
                boostExplosionTimer -= Time.deltaTime;

            if (limitBreakTimer > 0f)
                limitBreakTimer -= Time.deltaTime;

            if (blinkDashTimer > 0f)
                blinkDashTimer -= Time.deltaTime;

            if (overloadBlastTimer > 0f)
                overloadBlastTimer -= Time.deltaTime;
        }


        // =========================================================
        // Phase Change Safety
        // =========================================================

        private void HandlePhaseChange()
        {
            PlayerController.PlayerPhase currentPhase =
                playerController.CurrentPhase;

            if (currentPhase == previousPhase)
                return;

            // Phase 1에서 W 사용 중 Phase가 바뀌면
            // 추가 방어력 제거
            if (isDefenseSkillActive &&
                currentPhase != PlayerController.PlayerPhase.Phase1)
            {
                EndDefenseSkill();
            }

            previousPhase =
                currentPhase;
        }


        // =========================================================
        // Input
        // =========================================================

        private void HandleSkillInput()
        {
            if (skillExecuting)
                return;

            if (Input.GetKeyDown(skill1Key))
            {
                UseSkill1();
            }

            if (Input.GetKeyDown(skill2Key))
            {
                UseSkill2();
            }

            if (Input.GetKeyDown(skill3Key))
            {
                UseSkill3();
            }
        }


        // =========================================================
        // Skill 1
        // =========================================================

        private void UseSkill1()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1:
                    Phase1Skill1();
                    break;

                case PlayerController.PlayerPhase.Phase2:
                    Phase2Skill1();
                    break;

                case PlayerController.PlayerPhase.Phase3:
                    Phase3Skill1();
                    break;
            }
        }


        // =========================================================
        // Skill 2
        // =========================================================

        private void UseSkill2()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1:
                    Phase1Skill2();
                    break;

                case PlayerController.PlayerPhase.Phase2:
                    Phase2Skill2();
                    break;

                case PlayerController.PlayerPhase.Phase3:
                    Phase3Skill2();
                    break;
            }
        }


        // =========================================================
        // Skill 3
        // =========================================================

        private void UseSkill3()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1:
                    Phase1Skill3();
                    break;

                case PlayerController.PlayerPhase.Phase2:
                    Phase2Skill3();
                    break;

                case PlayerController.PlayerPhase.Phase3:
                    Phase3Skill3();
                    break;
            }
        }


        // =========================================================
        // Phase 1
        // =========================================================

        private void Phase1Skill1()
        {
            if (heavyStrikeTimer > 0f)
                return;

            PerformAreaDamage(
                heavyStrikeDamage,
                heavyStrikeRange
            );

            heavyStrikeTimer =
                heavyStrikeCooldown;

            DebugSkillLog(
                "Phase 1",
                "Q - 중장갑 강타"
            );
        }


        private void Phase1Skill2()
        {
            if (isDefenseSkillActive)
                return;

            if (defenseSkillTimer > 0f)
                return;

            StartDefenseSkill();
        }


        private void Phase1Skill3()
        {
            if (gravityBoostTimer > 0f)
                return;

            StartGravityBoostSkill(
                gravityBoostHorizontal,
                gravityBoostVertical,
                gravityBoostDuration
            );

            gravityBoostTimer =
                gravityBoostCooldown;

            DebugSkillLog(
                "Phase 1",
                "E - 중력 추진"
            );
        }


        // =========================================================
        // Phase 1 - W
        // =========================================================

        private void StartDefenseSkill()
        {
            isDefenseSkillActive =
                true;

            defenseSkillTimer =
                defenseSkillCooldown;

            defenseSkillDurationTimer =
                defenseSkillDuration;

            // 실제 PlayerController 방어력 적용
            playerController
                .SetAdditionalDamageReduction(
                    defenseSkillBonus
                );

            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerSkillController] " +
                    $"Phase 1 W 활성화 / " +
                    $"추가 방어: " +
                    $"{defenseSkillBonus * 100f:F0}% / " +
                    $"현재 방어: " +
                    $"{playerController.DefensePercent:F0}%"
                );
            }
        }


        private void EndDefenseSkill()
        {
            if (!isDefenseSkillActive)
                return;

            isDefenseSkillActive =
                false;

            defenseSkillDurationTimer =
                0f;

            playerController
                .ClearAdditionalDamageReduction();

            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerSkillController] " +
                    $"Phase 1 W 종료 / " +
                    $"현재 방어: " +
                    $"{playerController.DefensePercent:F0}%"
                );
            }
        }


        // =========================================================
        // Phase 1 - E Gravity Boost
        // =========================================================

        private void StartGravityBoostSkill(
            float horizontal,
            float vertical,
            float duration)
        {
            if (playerController == null)
                return;

            if (isGravityBoostActive)
                return;

            isGravityBoostActive =
                true;

            gravityBoostDurationTimer =
                duration;

            float direction =
                transform.localScale.x >= 0f
                    ? 1f
                    : -1f;

            playerController.StartGravityBoost(
                horizontal * direction,
                vertical
            );
        }


        private void EndGravityBoost()
        {
            if (!isGravityBoostActive)
                return;

            isGravityBoostActive =
                false;

            gravityBoostDurationTimer =
                0f;

            playerController.EndGravityBoost();
        }


        // =========================================================
        // Phase 2
        // =========================================================

        private void Phase2Skill1()
        {
            if (overdriveTimer > 0f)
                return;

            if (skillExecuting)
                return;

            StartCoroutine(
                OverdriveRoutine()
            );
        }


        private void Phase2Skill2()
        {
            if (dashSlashTimer > 0f)
                return;

            if (skillExecuting)
                return;

            StartCoroutine(
                DashSlashRoutine()
            );
        }


        private void Phase2Skill3()
        {
            if (boostExplosionTimer > 0f)
                return;

            if (skillExecuting)
                return;

            StartCoroutine(
                BoostExplosionRoutine()
            );
        }


        // =========================================================
        // Phase 2 - Q Overdrive
        // =========================================================

        private IEnumerator OverdriveRoutine()
        {
            skillExecuting =
                true;

            overdriveTimer =
                overdriveCooldown;

            if (showDebugLog)
            {
                Debug.Log(
                    "[PlayerSkillController] " +
                    "Phase 2 Q - 오버드라이브 시작"
                );
            }

            int hits =
                Mathf.Max(
                    1,
                    overdriveHitCount
                );

            for (int i = 0; i < hits; i++)
            {
                PerformAreaDamage(
                    overdriveDamage,
                    overdriveRange
                );

                yield return new WaitForSeconds(
                    overdriveHitInterval
                );
            }

            if (showDebugLog)
            {
                Debug.Log(
                    "[PlayerSkillController] " +
                    "Phase 2 Q - 오버드라이브 종료"
                );
            }

            skillExecuting =
                false;
        }


        // =========================================================
        // Phase 2 - W Dash Slash
        // =========================================================

        private IEnumerator DashSlashRoutine()
        {
            skillExecuting =
                true;

            isDashSlashActive =
                true;

            dashSlashTimer =
                dashSlashCooldown;

            float direction =
                transform.localScale.x >= 0f
                    ? 1f
                    : -1f;

            float duration =
                Mathf.Max(
                    0.01f,
                    dashSlashDuration
                );

            playerController.StartGravityBoost(
                direction * dashSlashSpeed,
                0f
            );

            // 돌진 중에도 적에게 공격
            PerformAreaDamage(
                dashSlashDamage,
                dashSlashRange
            );

            yield return new WaitForSeconds(
                duration
            );

            playerController.EndGravityBoost();

            isDashSlashActive =
                false;

            skillExecuting =
                false;

            DebugSkillLog(
                "Phase 2",
                "W - 대시 슬래시"
            );
        }
        // =========================================================
        // Phase 2 - E Boost Explosion
        // =========================================================

        private IEnumerator BoostExplosionRoutine()
        {
            skillExecuting =
                true;

            isBoostExplosionActive =
                true;

            boostExplosionTimer =
                boostExplosionCooldown;

            float direction =
                transform.localScale.x >= 0f
                    ? 1f
                    : -1f;

            playerController.StartGravityBoost(
                direction * boostExplosionHorizontal,
                boostExplosionVertical
            );

            if (showDebugLog)
            {
                Debug.Log(
                    "[PlayerSkillController] " +
                    "Phase 2 E - 추진 시작"
                );
            }

            yield return new WaitForSeconds(
                Mathf.Max(
                    0.01f,
                    boostExplosionMoveDuration
                )
            );

            playerController.EndGravityBoost();

            // 추진 종료 지점에서 폭발
            PerformAreaDamage(
                boostExplosionDamage,
                boostExplosionRange
            );

            if (showDebugLog)
            {
                Debug.Log(
                    "[PlayerSkillController] " +
                    "Phase 2 E - 추진 폭발"
                );
            }

            isBoostExplosionActive =
                false;

            skillExecuting =
                false;
        }


        // =========================================================
        // Phase 3
        // =========================================================

        private void Phase3Skill1()
        {
            if (limitBreakTimer > 0f)
                return;

            PerformAreaDamage(
                limitBreakDamage,
                limitBreakRange
            );

            limitBreakTimer =
                limitBreakCooldown;

            DebugSkillLog(
                "Phase 3",
                "Q - 리미트 브레이크"
            );
        }


        private void Phase3Skill2()
        {
            if (blinkDashTimer > 0f)
                return;

            BlinkDash();
        }


        private void Phase3Skill3()
        {
            if (overloadBlastTimer > 0f)
                return;

            PerformAreaDamage(
                overloadBlastDamage,
                overloadBlastRange
            );

            overloadBlastTimer =
                overloadBlastCooldown;

            DebugSkillLog(
                "Phase 3",
                "E - 과부하 방출"
            );
        }


        // =========================================================
        // Phase 3 - W Blink Dash
        // =========================================================

        private void BlinkDash()
        {
            if (rb == null)
                return;

            float direction =
                transform.localScale.x >= 0f
                    ? 1f
                    : -1f;

            Vector2 startPosition =
                rb.position;

            Vector2 targetPosition =
                startPosition +
                new Vector2(
                    direction *
                    blinkDashDistance,
                    0f
                );

            // 벽이나 장애물에 박히는 것을 어느 정도 방지
            RaycastHit2D obstacle =
                Physics2D.Raycast(
                    startPosition,
                    Vector2.right * direction,
                    blinkDashDistance,
                    LayerMask.GetMask("Ground")
                );

            if (obstacle.collider != null)
            {
                float safeDistance =
                    Mathf.Max(
                        0f,
                        obstacle.distance - 0.35f
                    );

                targetPosition =
                    startPosition +
                    new Vector2(
                        direction *
                        safeDistance,
                        0f
                    );
            }

            rb.position =
                targetPosition;

            // 이동 직후 목적지 공격
            PerformAreaDamage(
                blinkDashDamage,
                blinkDashRange
            );

            blinkDashTimer =
                blinkDashCooldown;

            DebugSkillLog(
                "Phase 3",
                "W - 블링크 대시"
            );
        }


        // =========================================================
        // Area Damage
        // =========================================================

        private void PerformAreaDamage(
            float damage,
            float range)
        {
            if (attackPoint == null)
                return;

            if (targetLayer.value == 0)
                return;

            Collider2D[] targets =
                Physics2D.OverlapCircleAll(
                    attackPoint.position,
                    range,
                    targetLayer
                );

            HashSet<IDamageable> hitTargets =
                new HashSet<IDamageable>();

            foreach (Collider2D target in targets)
            {
                if (target == null)
                    continue;

                IDamageable damageable =
                    target.GetComponent<IDamageable>();

                if (damageable == null)
                {
                    damageable =
                        target.GetComponentInParent<IDamageable>();
                }

                if (damageable == null)
                    continue;

                if (damageable.IsDead)
                    continue;

                if (hitTargets.Contains(damageable))
                    continue;

                hitTargets.Add(
                    damageable
                );

                damageable.TakeDamage(
                    damage
                );

                if (showDebugLog)
                {
                    Debug.Log(
                        $"[PlayerSkillController] " +
                        $"스킬 적중: " +
                        $"{target.gameObject.name} / " +
                        $"Damage: {damage:F1}"
                    );
                }
            }
        }


        // =========================================================
        // Defense
        // =========================================================

        public float GetCurrentDefenseReduction()
        {
            if (playerController == null)
                return 0f;

            // PlayerController가
            // 기본 방어 + 추가 방어를 계산
            return playerController.DamageReduction;
        }


        public float GetCurrentDefensePercent()
        {
            return
                GetCurrentDefenseReduction() *
                100f;
        }


        public bool IsDefenseSkillActive =>
            isDefenseSkillActive;


        // =========================================================
        // Skill Status
        // =========================================================

        public bool IsSkillExecuting =>
            skillExecuting;

        public bool IsGravityBoostActive =>
            isGravityBoostActive;

        public bool IsDashSlashActive =>
            isDashSlashActive;

        public bool IsBoostExplosionActive =>
            isBoostExplosionActive;


        // =========================================================
        // Cooldown Getters
        // =========================================================

        public float HeavyStrikeCooldownRemaining =>
            Mathf.Max(
                0f,
                heavyStrikeTimer
            );

        public float DefenseCooldownRemaining =>
            Mathf.Max(
                0f,
                defenseSkillTimer
            );

        public float GravityBoostCooldownRemaining =>
            Mathf.Max(
                0f,
                gravityBoostTimer
            );

        public float OverdriveCooldownRemaining =>
            Mathf.Max(
                0f,
                overdriveTimer
            );

        public float DashSlashCooldownRemaining =>
            Mathf.Max(
                0f,
                dashSlashTimer
            );

        public float BoostExplosionCooldownRemaining =>
            Mathf.Max(
                0f,
                boostExplosionTimer
            );

        public float LimitBreakCooldownRemaining =>
            Mathf.Max(
                0f,
                limitBreakTimer
            );

        public float BlinkDashCooldownRemaining =>
            Mathf.Max(
                0f,
                blinkDashTimer
            );

        public float OverloadBlastCooldownRemaining =>
            Mathf.Max(
                0f,
                overloadBlastTimer
            );


        // =========================================================
        // Debug
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
                Color.cyan;

            Gizmos.DrawWireSphere(
                attackPoint.position,
                heavyStrikeRange
            );

            Gizmos.DrawLine(
                transform.position,
                attackPoint.position
            );
        }


        // =========================================================
        // Disable Safety
        // =========================================================

        private void OnDisable()
        {
            StopAllCoroutines();

            if (playerController != null)
            {
                // W 방어력 정리
                if (isDefenseSkillActive)
                {
                    playerController
                        .ClearAdditionalDamageReduction();
                }

                // 중력 추진 정리
                if (isGravityBoostActive)
                {
                    playerController
                        .EndGravityBoost();
                }

                // 대시 슬래시 / 추진 폭발 도중이면
                // 중력 제어를 원상복구
                if (isDashSlashActive ||
                    isBoostExplosionActive)
                {
                    playerController
                        .EndGravityBoost();
                }
            }

            isDefenseSkillActive = false;
            isGravityBoostActive = false;
            isDashSlashActive = false;
            isBoostExplosionActive = false;

            skillExecuting = false;

            defenseSkillDurationTimer = 0f;
            gravityBoostDurationTimer = 0f;
        }
    }
}