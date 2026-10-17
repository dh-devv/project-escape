using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class PlayerSkillController : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Transform attackPoint;
        private Rigidbody2D rb;

        [Header("=== Skill Input ===")]
        [SerializeField] private KeyCode skill1Key = KeyCode.Q;
        [SerializeField] private KeyCode skill2Key = KeyCode.W;
        [SerializeField] private KeyCode skill3Key = KeyCode.E;

        [Header("=== Phase 1 : Heavy Strike ===")]
        [SerializeField] private float heavyStrikeDamage = 30f;
        [SerializeField] private float heavyStrikeRange = 1.5f;
        [SerializeField] private float heavyStrikeCooldown = 2f;
        private float heavyStrikeTimer;

        [Header("=== Phase 1 : Defense System ===")]
        [Range(0f, 1f)][SerializeField] private float defenseSkillBonus = 0.2f;
        [SerializeField] private float defenseSkillDuration = 3f;
        [SerializeField] private float defenseSkillCooldown = 8f;
        private float defenseSkillTimer;
        private float defenseSkillDurationTimer;
        private bool isDefenseSkillActive;

        [Header("=== Phase 1 : Gravity Boost ===")]
        [SerializeField] private float gravityBoostHorizontal = 10f;
        [SerializeField] private float gravityBoostVertical = 8f;
        [SerializeField] private float gravityBoostDuration = 0.35f;
        [SerializeField] private float gravityBoostCooldown = 4f;
        private float gravityBoostTimer;
        private float gravityBoostDurationTimer;
        private bool isGravityBoostActive;

        [Header("=== Phase 2 : Overdrive ===")]
        [SerializeField] private float overdriveDamage = 20f;
        [SerializeField] private float overdriveRange = 1.5f;
        [SerializeField] private int overdriveHitCount = 3;
        [SerializeField] private float overdriveHitInterval = 0.12f;
        [SerializeField] private float overdriveCooldown = 4f;
        private float overdriveTimer;

        [Header("=== Phase 2 : Dash Slash ===")]
        [SerializeField] private float dashSlashSpeed = 14f;
        [SerializeField] private float dashSlashDuration = 0.2f;
        [SerializeField] private float dashSlashDamage = 35f;
        [SerializeField] private float dashSlashRange = 1.6f;
        [SerializeField] private float dashSlashCooldown = 3f;
        private float dashSlashTimer;
        private bool isDashSlashActive;

        [Header("=== Phase 2 : Boost Explosion ===")]
        [SerializeField] private float boostExplosionHorizontal = 11f;
        [SerializeField] private float boostExplosionVertical = 10f;
        [SerializeField] private float boostExplosionMoveDuration = 0.3f;
        [SerializeField] private float boostExplosionDamage = 45f;
        [SerializeField] private float boostExplosionRange = 2.5f;
        [SerializeField] private float boostExplosionCooldown = 5f;
        private float boostExplosionTimer;
        private bool isBoostExplosionActive;

        [Header("=== Phase 3 : Limit Break ===")]
        [SerializeField] private float limitBreakDamage = 100f;
        [SerializeField] private float limitBreakRange = 2.5f;
        [SerializeField] private float limitBreakCooldown = 5f;
        private float limitBreakTimer;

        [Header("=== Phase 3 : Blink Dash ===")]
        [SerializeField] private float blinkDashDistance = 4.5f;
        [SerializeField] private float blinkDashDamage = 50f;
        [SerializeField] private float blinkDashRange = 1.8f;
        [SerializeField] private float blinkDashCooldown = 3.5f;
        private float blinkDashTimer;

        [Header("=== Phase 3 : Overload Blast ===")]
        [SerializeField] private float overloadBlastDamage = 90f;
        [SerializeField] private float overloadBlastRange = 3f;
        [SerializeField] private float overloadBlastCooldown = 6f;
        private float overloadBlastTimer;

        [Header("=== Target Layer ===")]
        [SerializeField] private LayerMask targetLayer;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private bool skillExecuting;
        private PlayerController.PlayerPhase previousPhase;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (attackPoint == null)
            {
                Transform foundAttackPoint = transform.Find("AttackPoint");
                if (foundAttackPoint != null) attackPoint = foundAttackPoint;
            }

            if (targetLayer.value == 0)
            {
                int enemyLayer = LayerMask.NameToLayer("Enemy");
                if (enemyLayer >= 0) targetLayer = 1 << enemyLayer;
            }

            previousPhase = playerController != null ? playerController.CurrentPhase : PlayerController.PlayerPhase.Phase1;
        }

        private void Start()
        {
            GameStateManager state = GameStateManager.Instance;
            if (state == null) return;

            heavyStrikeTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.HeavyStrike);
            defenseSkillTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.Defense);
            gravityBoostTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.GravityBoost);
            overdriveTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.Overdrive);
            dashSlashTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.DashSlash);
            boostExplosionTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.BoostExplosion);
            limitBreakTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.LimitBreak);
            blinkDashTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.BlinkDash);
            overloadBlastTimer = state.GetSkillCooldownRemaining(GameStateManager.SkillCooldownType.OverloadBlast);
        }

        private void Update()
        {
            if (playerController == null || playerController.IsDead) return;

            HandleTimers();
            HandlePhaseChange();
            HandleSkillInput();
        }

        private void HandleTimers()
        {
            if (heavyStrikeTimer > 0f) heavyStrikeTimer -= Time.deltaTime;
            if (defenseSkillTimer > 0f) defenseSkillTimer -= Time.deltaTime;
            if (defenseSkillDurationTimer > 0f)
            {
                defenseSkillDurationTimer -= Time.deltaTime;
                if (defenseSkillDurationTimer <= 0f) EndDefenseSkill();
            }
            if (gravityBoostTimer > 0f) gravityBoostTimer -= Time.deltaTime;
            if (gravityBoostDurationTimer > 0f)
            {
                gravityBoostDurationTimer -= Time.deltaTime;
                if (gravityBoostDurationTimer <= 0f) EndGravityBoost();
            }
            if (overdriveTimer > 0f) overdriveTimer -= Time.deltaTime;
            if (dashSlashTimer > 0f) dashSlashTimer -= Time.deltaTime;
            if (boostExplosionTimer > 0f) boostExplosionTimer -= Time.deltaTime;
            if (limitBreakTimer > 0f) limitBreakTimer -= Time.deltaTime;
            if (blinkDashTimer > 0f) blinkDashTimer -= Time.deltaTime;
            if (overloadBlastTimer > 0f) overloadBlastTimer -= Time.deltaTime;
        }

        private void HandlePhaseChange()
        {
            PlayerController.PlayerPhase currentPhase = playerController.CurrentPhase;
            if (currentPhase == previousPhase) return;

            if (isDefenseSkillActive && currentPhase != PlayerController.PlayerPhase.Phase1)
            {
                EndDefenseSkill();
            }
            previousPhase = currentPhase;
        }

        private void OnEnable()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            PlayerHitReaction reaction = GetComponent<PlayerHitReaction>();
            if (reaction == null) reaction = gameObject.AddComponent<PlayerHitReaction>();
            reaction.StunStarted += InterruptForHit;
        }

        private void InterruptForHit()
        {
            StopAllCoroutines();
            if (playerController != null && (isGravityBoostActive || isDashSlashActive || isBoostExplosionActive))
                playerController.EndGravityBoost();
            isGravityBoostActive = false;
            isDashSlashActive = false;
            isBoostExplosionActive = false;
            gravityBoostDurationTimer = 0f;
            skillExecuting = false;
        }

        private void HandleSkillInput()
        {
            if (!playerController.CanAct || skillExecuting) return;

            if (Input.GetKeyDown(skill1Key)) UseSkill1();
            if (Input.GetKeyDown(skill2Key)) UseSkill2();
            if (Input.GetKeyDown(skill3Key)) UseSkill3();
        }

        private void UseSkill1()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1: Phase1Skill1(); break;
                case PlayerController.PlayerPhase.Phase2: Phase2Skill1(); break;
                case PlayerController.PlayerPhase.Phase3: Phase3Skill1(); break;
            }
        }

        private void UseSkill2()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1: Phase1Skill2(); break;
                case PlayerController.PlayerPhase.Phase2: Phase2Skill2(); break;
                case PlayerController.PlayerPhase.Phase3: Phase3Skill2(); break;
            }
        }

        private void UseSkill3()
        {
            switch (playerController.CurrentPhase)
            {
                case PlayerController.PlayerPhase.Phase1: Phase1Skill3(); break;
                case PlayerController.PlayerPhase.Phase2: Phase2Skill3(); break;
                case PlayerController.PlayerPhase.Phase3: Phase3Skill3(); break;
            }
        }

        // Q 스킬들 (1번 스킬 애니메이션 실행)
        private void Phase1Skill1()
        {
            if (heavyStrikeTimer > 0f) return;
            playerController?.TriggerSkillAnimation(1); // ★ Q 스킬 모션 실행[cite: 39]
            PerformAreaDamage(heavyStrikeDamage, heavyStrikeRange);
            heavyStrikeTimer = heavyStrikeCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.HeavyStrike, heavyStrikeCooldown);
        }

        private void Phase2Skill1()
        {
            if (overdriveTimer > 0f || skillExecuting) return;
            playerController?.TriggerSkillAnimation(1); // ★ Q 스킬 모션 실행[cite: 39]
            StartCoroutine(OverdriveRoutine());
        }

        private void Phase3Skill1()
        {
            if (limitBreakTimer > 0f) return;
            playerController?.TriggerSkillAnimation(1); // ★ Q 스킬 모션 실행[cite: 39]
            PerformAreaDamage(limitBreakDamage, limitBreakRange);
            limitBreakTimer = limitBreakCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.LimitBreak, limitBreakCooldown);
        }

        // W 스킬들 (2번 스킬 애니메이션 실행)
        private void Phase1Skill2()
        {
            if (isDefenseSkillActive || defenseSkillTimer > 0f) return;
            playerController?.TriggerSkillAnimation(2); // ★ W 스킬 모션 실행[cite: 40]
            StartDefenseSkill();
        }

        private void Phase2Skill2()
        {
            if (dashSlashTimer > 0f || skillExecuting) return;
            playerController?.TriggerSkillAnimation(2); // ★ W 스킬 모션 실행[cite: 40]
            StartCoroutine(DashSlashRoutine());
        }

        private void Phase3Skill2()
        {
            if (blinkDashTimer > 0f) return;
            playerController?.TriggerSkillAnimation(2); // ★ W 스킬 모션 실행[cite: 40]
            BlinkDash();
        }

        // E 스킬들 (3번 스킬 애니메이션 실행)
        private void Phase1Skill3()
        {
            if (gravityBoostTimer > 0f) return;
            playerController?.TriggerSkillAnimation(3); // ★ E 스킬 모션 실행[cite: 41]
            StartGravityBoostSkill(gravityBoostHorizontal, gravityBoostVertical, gravityBoostDuration);
            gravityBoostTimer = gravityBoostCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.GravityBoost, gravityBoostCooldown);
        }

        private void Phase2Skill3()
        {
            if (boostExplosionTimer > 0f || skillExecuting) return;
            playerController?.TriggerSkillAnimation(3); // ★ E 스킬 모션 실행[cite: 41]
            StartCoroutine(BoostExplosionRoutine());
        }

        private void Phase3Skill3()
        {
            if (overloadBlastTimer > 0f) return;
            playerController?.TriggerSkillAnimation(3); // ★ E 스킬 모션 실행[cite: 41]
            PerformAreaDamage(overloadBlastDamage, overloadBlastRange);
            overloadBlastTimer = overloadBlastCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.OverloadBlast, overloadBlastCooldown);
        }

        private void StartDefenseSkill()
        {
            isDefenseSkillActive = true;
            defenseSkillTimer = defenseSkillCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.Defense, defenseSkillCooldown);
            defenseSkillDurationTimer = defenseSkillDuration;
            playerController.SetAdditionalDamageReduction(defenseSkillBonus);
        }

        private void EndDefenseSkill()
        {
            if (!isDefenseSkillActive) return;
            isDefenseSkillActive = false;
            defenseSkillDurationTimer = 0f;
            playerController.ClearAdditionalDamageReduction();
        }

        private void StartGravityBoostSkill(float horizontal, float vertical, float duration)
        {
            if (playerController == null || isGravityBoostActive) return;
            isGravityBoostActive = true;
            gravityBoostDurationTimer = duration;
            float direction = transform.localScale.x >= 0f ? 1f : -1f;
            playerController.StartGravityBoost(horizontal * direction, vertical);
        }

        private void EndGravityBoost()
        {
            if (!isGravityBoostActive) return;
            isGravityBoostActive = false;
            gravityBoostDurationTimer = 0f;
            playerController.EndGravityBoost();
        }

        private IEnumerator OverdriveRoutine()
        {
            skillExecuting = true;
            overdriveTimer = overdriveCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.Overdrive, overdriveCooldown);

            int hits = Mathf.Max(1, overdriveHitCount);
            for (int i = 0; i < hits; i++)
            {
                PerformAreaDamage(overdriveDamage, overdriveRange);
                yield return new WaitForSeconds(overdriveHitInterval);
            }
            skillExecuting = false;
        }

        private IEnumerator DashSlashRoutine()
        {
            skillExecuting = true;
            isDashSlashActive = true;
            dashSlashTimer = dashSlashCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.DashSlash, dashSlashCooldown);

            float direction = transform.localScale.x >= 0f ? 1f : -1f;
            float duration = Mathf.Max(0.01f, dashSlashDuration);
            playerController.StartGravityBoost(direction * dashSlashSpeed, 0f);
            PerformAreaDamage(dashSlashDamage, dashSlashRange);

            yield return new WaitForSeconds(duration);

            playerController.EndGravityBoost();
            isDashSlashActive = false;
            skillExecuting = false;
        }

        private IEnumerator BoostExplosionRoutine()
        {
            skillExecuting = true;
            isBoostExplosionActive = true;
            boostExplosionTimer = boostExplosionCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.BoostExplosion, boostExplosionCooldown);

            float direction = transform.localScale.x >= 0f ? 1f : -1f;
            playerController.StartGravityBoost(direction * boostExplosionHorizontal, boostExplosionVertical);

            yield return new WaitForSeconds(Mathf.Max(0.01f, boostExplosionMoveDuration));

            playerController.EndGravityBoost();
            PerformAreaDamage(boostExplosionDamage, boostExplosionRange);
            isBoostExplosionActive = false;
            skillExecuting = false;
        }

        private void BlinkDash()
        {
            if (rb == null) return;
            float direction = transform.localScale.x >= 0f ? 1f : -1f;
            Vector2 startPosition = rb.position;
            Vector2 targetPosition = startPosition + new Vector2(direction * blinkDashDistance, 0f);

            RaycastHit2D obstacle = Physics2D.Raycast(startPosition, Vector2.right * direction, blinkDashDistance, LayerMask.GetMask("Ground"));
            if (obstacle.collider != null)
            {
                float safeDistance = Mathf.Max(0f, obstacle.distance - 0.35f);
                targetPosition = startPosition + new Vector2(direction * safeDistance, 0f);
            }

            rb.position = targetPosition;
            PerformAreaDamage(blinkDashDamage, blinkDashRange);
            blinkDashTimer = blinkDashCooldown;
            GameStateManager.Instance?.StartSkillCooldown(GameStateManager.SkillCooldownType.BlinkDash, blinkDashCooldown);
        }

        private void PerformAreaDamage(float damage, float range)
        {
            if (attackPoint == null || targetLayer.value == 0) return;
            Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, range, targetLayer);
            HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

            foreach (Collider2D target in targets)
            {
                if (target == null) continue;
                IDamageable damageable = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.IsDead || hitTargets.Contains(damageable)) continue;

                hitTargets.Add(damageable);
                damageable.TakeDamage(damage);
            }
        }

        public float GetCurrentDefenseReduction() => playerController != null ? playerController.DamageReduction : 0f;
        public float GetCurrentDefensePercent() => GetCurrentDefenseReduction() * 100f;

        public bool IsDefenseSkillActive => isDefenseSkillActive;
        public bool IsSkillExecuting => skillExecuting;
        public bool IsGravityBoostActive => isGravityBoostActive;
        public bool IsDashSlashActive => isDashSlashActive;
        public bool IsBoostExplosionActive => isBoostExplosionActive;

        public float HeavyStrikeCooldownRemaining => Mathf.Max(0f, heavyStrikeTimer);
        public float DefenseCooldownRemaining => Mathf.Max(0f, defenseSkillTimer);
        public float GravityBoostCooldownRemaining => Mathf.Max(0f, gravityBoostTimer);
        public float OverdriveCooldownRemaining => Mathf.Max(0f, overdriveTimer);
        public float DashSlashCooldownRemaining => Mathf.Max(0f, dashSlashTimer);
        public float BoostExplosionCooldownRemaining => Mathf.Max(0f, boostExplosionTimer);
        public float LimitBreakCooldownRemaining => Mathf.Max(0f, limitBreakTimer);
        public float BlinkDashCooldownRemaining => Mathf.Max(0f, blinkDashTimer);
        public float OverloadBlastCooldownRemaining => Mathf.Max(0f, overloadBlastTimer);

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackPoint.position, heavyStrikeRange);
            Gizmos.DrawLine(transform.position, attackPoint.position);
        }

        private void OnDisable()
        {
            PlayerHitReaction reaction = GetComponent<PlayerHitReaction>();
            if (reaction != null) reaction.StunStarted -= InterruptForHit;
            StopAllCoroutines();

            if (playerController != null)
            {
                if (isDefenseSkillActive) playerController.ClearAdditionalDamageReduction();
                if (isGravityBoostActive || isDashSlashActive || isBoostExplosionActive) playerController.EndGravityBoost();
            }

            isDefenseSkillActive = false;
            isGravityBoostActive = false;
            isDashSlashActive = false;
            isBoostExplosionActive = false;
            skillExecuting = false;
        }
    }
}
