using UnityEngine;

namespace Carten
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        public enum PlayerPhase
        {
            Phase1,
            Phase2,
            Phase3
        }

        [Header("=== HP ===")]
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        [Header("=== Phase ===")]
        [SerializeField] private float phase2HealthPercent = 70f;
        [SerializeField] private float phase3HealthPercent = 30f;
        private PlayerPhase currentPhase;
        public PlayerPhase CurrentPhase => currentPhase;

        [Header("=== Defense ===")]
        [Range(0f, 1f)][SerializeField] private float phase1Defense = 0.6f;
        [Range(0f, 1f)][SerializeField] private float phase2Defense = 0.3f;
        [Range(0f, 1f)][SerializeField] private float phase3Defense = 0f;

        private float additionalDamageReduction;

        public float DamageReduction
        {
            get
            {
                float baseReduction;
                switch (currentPhase)
                {
                    case PlayerPhase.Phase1: baseReduction = phase1Defense; break;
                    case PlayerPhase.Phase2: baseReduction = phase2Defense; break;
                    case PlayerPhase.Phase3: baseReduction = phase3Defense; break;
                    default: baseReduction = 0f; break;
                }
                return Mathf.Clamp01(baseReduction + additionalDamageReduction);
            }
        }

        public float DefensePercent => DamageReduction * 100f;

        public float BaseDamageReduction
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase1: return phase1Defense;
                    case PlayerPhase.Phase2: return phase2Defense;
                    case PlayerPhase.Phase3: return phase3Defense;
                    default: return 0f;
                }
            }
        }

        public float AdditionalDamageReduction => additionalDamageReduction;

        public void SetAdditionalDamageReduction(float reduction)
        {
            additionalDamageReduction = Mathf.Clamp(reduction, 0f, 1f);
        }

        public void ClearAdditionalDamageReduction()
        {
            additionalDamageReduction = 0f;
        }

        [Header("=== Movement ===")]
        [SerializeField] private float phase1MoveSpeed = 2.5f;
        [SerializeField] private float phase2MoveSpeed = 5f;
        [SerializeField] private float phase3MoveSpeed = 9f;

        public float CurrentMoveSpeed
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2: return phase2MoveSpeed;
                    case PlayerPhase.Phase3: return phase3MoveSpeed;
                    default: return phase1MoveSpeed;
                }
            }
        }

        [Header("=== Jump ===")]
        [SerializeField] private float phase1JumpForce = 6f;
        [SerializeField] private float phase2JumpForce = 10f;
        [SerializeField] private float phase3JumpForce = 10f;
        [SerializeField] private int phase1MaxJumps = 1;
        [SerializeField] private int phase2MaxJumps = 2;
        [SerializeField] private int phase3MaxJumps = 3;

        public float CurrentJumpForce
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2: return phase2JumpForce;
                    case PlayerPhase.Phase3: return phase3JumpForce;
                    default: return phase1JumpForce;
                }
            }
        }

        public int MaxJumps
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2: return phase2MaxJumps;
                    case PlayerPhase.Phase3: return phase3MaxJumps;
                    default: return phase1MaxJumps;
                }
            }
        }

        [Header("=== Gravity ===")]
        [SerializeField] private float baseGravityScale = 3f;
        [SerializeField] private float fallGravityMultiplier = 1.8f;
        [SerializeField] private float riseGravityMultiplier = 1f;

        [Header("=== Dash ===")]
        [SerializeField] private float phase2DashSpeed = 12f;
        [SerializeField] private float phase3DashSpeed = 18f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.5f;

        public bool CanDash => currentPhase == PlayerPhase.Phase2 || currentPhase == PlayerPhase.Phase3;
        public float CurrentDashSpeed => currentPhase == PlayerPhase.Phase3 ? phase3DashSpeed : phase2DashSpeed;

        [Header("=== Attack ===")]
        [SerializeField] private float phase1AttackSpeedMultiplier = 1f;
        [SerializeField] private float phase2AttackSpeedMultiplier = 1.5f;
        [SerializeField] private float phase3AttackSpeedMultiplier = 5f;
        [SerializeField] private float phase1CriticalMultiplier = 1f;
        [SerializeField] private float phase2CriticalMultiplier = 1f;
        [SerializeField] private float phase3CriticalMultiplier = 5f;

        public float AttackSpeedMultiplier
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2: return phase2AttackSpeedMultiplier;
                    case PlayerPhase.Phase3: return phase3AttackSpeedMultiplier;
                    default: return phase1AttackSpeedMultiplier;
                }
            }
        }

        public float CriticalMultiplier
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2: return phase2CriticalMultiplier;
                    case PlayerPhase.Phase3: return phase3CriticalMultiplier;
                    default: return phase1CriticalMultiplier;
                }
            }
        }

        [Header("=== Ground Check ===")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask jumpableLayer;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private PlayerHitReaction hitReaction;
        public PlayerHitReaction HitReaction => hitReaction;
        public bool IsStunned => hitReaction != null && hitReaction.IsStunned;
        public bool CanAct => isActiveAndEnabled && !isDead && !IsStunned;

        private Rigidbody2D rb;
        private Animator anim;

        private int jumpCount;
        private float dashTimer;
        private float dashCooldownTimer;

        private bool isDashing;
        private bool isDead;
        private bool isSkillMovementLocked;
        private bool jumpRequested;
        private bool dashRequested;
        private bool suppressDamageAnimOnce;

        public bool IsDead => isDead;
        public bool IsSkillMovementLocked => isSkillMovementLocked;

        private void Awake()
        {
            hitReaction = GetComponent<PlayerHitReaction>();
            if (hitReaction == null)
                hitReaction = gameObject.AddComponent<PlayerHitReaction>();
            hitReaction.StunStarted += InterruptMovement;
            if (GetComponent<PlayerInteractor>() == null)
                gameObject.AddComponent<PlayerInteractor>();

            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();

            if (rb == null)
            {
                Debug.LogError("[PlayerController] Rigidbody2D를 찾을 수 없습니다.");
                return;
            }

            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            currentHealth = maxHealth;
            additionalDamageReduction = 0f;
            isSkillMovementLocked = false;
            jumpRequested = false;
            dashRequested = false;

            UpdatePhase(playAnimation: false);
        }

        private void Start()
        {
            GameStateManager state = GameStateManager.Instance;
            if (state == null || float.IsNaN(state.PlayerHP) || float.IsInfinity(state.PlayerHP))
                return;

            currentHealth = Mathf.Clamp(state.PlayerHP, 0f, maxHealth);
            UpdatePhase(playAnimation: false);
            if (currentHealth <= 0f)
                Die();
        }

        private void SaveGameState()
        {
            GameStateManager.Instance?.SetPlayerState(currentHealth, (int)currentPhase + 1);
        }

        private void Update()
        {
            if (isDead)
                return;

            HandleTimers();
            if (!CanAct)
                return;

            HandleJumpInput();
            HandleDashInput();
        }

        private void FixedUpdate()
        {
            if (isDead)
                return;

            bool isGrounded = CheckGround();
            if (anim != null)
            {
                anim.SetBool("isGrounded", isGrounded);
            }

            if (IsStunned)
            {
                if (rb != null)
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                ApplyGravity();
                return;
            }

            HandleMovement();
            HandleJump(isGrounded);
            HandleDash();
            ApplyGravity();
        }

        private void HandleTimers()
        {
            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
                if (dashCooldownTimer < 0f)
                    dashCooldownTimer = 0f;
            }
        }

        private void HandleMovement()
        {
            if (rb == null)
                return;

            if (isDashing || isSkillMovementLocked)
            {
                if (anim != null) anim.SetBool("isWork", false);
                return;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(horizontal * CurrentMoveSpeed, rb.linearVelocity.y);

            if (horizontal != 0f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontal);
                transform.localScale = scale;
            }

            if (anim != null)
            {
                anim.SetBool("isWork", horizontal != 0f);
            }
        }

        private void HandleJumpInput()
        {
            if (isSkillMovementLocked)
                return;

            if (!Input.GetButtonDown("Jump"))
                return;

            jumpRequested = true;
        }

        private void HandleJump(bool isGrounded)
        {
            if (rb == null)
                return;

            if (isSkillMovementLocked)
            {
                jumpRequested = false;
                return;
            }

            if (isGrounded && rb.linearVelocity.y <= 0.05f)
            {
                jumpCount = 0;
            }

            if (!jumpRequested)
                return;

            jumpRequested = false;

            if (jumpCount >= MaxJumps)
                return;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, CurrentJumpForce);
            jumpCount++;

            if (anim != null)
            {
                anim.SetBool("isGrounded", false);
                anim.SetTrigger("doJump");
            }
        }

        private bool CheckGround()
        {
            if (groundCheck == null)
                return false;

            if (groundLayer.value != 0)
            {
                Collider2D groundHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
                if (groundHit != null)
                    return true;
            }

            if (jumpableLayer.value != 0)
            {
                Collider2D jumpableHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, jumpableLayer);
                if (jumpableHit != null)
                    return true;
            }

            return false;
        }

        private void HandleDashInput()
        {
            if (!CanDash || isSkillMovementLocked)
                return;

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                dashRequested = true;
            }
        }

        private void HandleDash()
        {
            if (isDashing)
            {
                dashTimer -= Time.fixedDeltaTime;
                if (dashTimer <= 0f)
                {
                    dashTimer = 0f;
                    isDashing = false;
                }
                return;
            }

            if (!CanDash || isSkillMovementLocked)
            {
                dashRequested = false;
                return;
            }

            if (!dashRequested)
                return;

            dashRequested = false;

            if (dashCooldownTimer > 0f)
                return;

            StartDash();
        }

        private void StartDash()
        {
            if (rb == null)
                return;

            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            float direction = transform.localScale.x >= 0f ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * CurrentDashSpeed, 0f);
        }

        private void ApplyGravity()
        {
            if (rb == null || isDashing || isSkillMovementLocked)
                return;

            if (rb.linearVelocity.y < 0f)
                rb.gravityScale = baseGravityScale * fallGravityMultiplier;
            else
                rb.gravityScale = baseGravityScale * riseGravityMultiplier;
        }

        public void StartGravityBoost(float horizontalVelocity, float verticalVelocity)
        {
            if (rb == null || !CanAct)
                return;

            isSkillMovementLocked = true;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);
        }

        public void EndGravityBoost()
        {
            if (rb == null)
                return;

            rb.gravityScale = baseGravityScale;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            isSkillMovementLocked = false;
        }

        // ★ 기본 공격 및 스킬 1, 2, 3 애니메이션 실행 함수들
        public void TriggerAttackAnimation()
        {
            if (anim != null && !isDead)
                anim.SetTrigger("doAttack");
        }

        public void TriggerSkillAnimation(int skillNumber)
        {
            if (anim == null || isDead)
                return;

            switch (skillNumber)
            {
                case 1: anim.SetTrigger("doSkill1"); break; // HeavyStrike 등 Q스킬
                case 2: anim.SetTrigger("doSkill2"); break; // Defense 등 W스킬
                case 3: anim.SetTrigger("doSkill3"); break; // GravityBoost 등 E스킬
            }
        }

        internal void RestoreHealthForMap(float health)
        {
            if (isDead || health <= 0f || float.IsNaN(health) || float.IsInfinity(health))
                return;

            currentHealth = Mathf.Min(health, maxHealth);
            UpdatePhase(playAnimation: false);
            SaveGameState();
        }

        // ★ 보스 장판 등 스턴(강공격)을 동반한 피격 시 Knockback 재생!
        public void TakeDamage(float damage, float stunDuration)
        {
            float previousHealth = currentHealth;
            bool isStrongHit = stunDuration > 0f;
            suppressDamageAnimOnce = isStrongHit;

            TakeDamage(damage);
            suppressDamageAnimOnce = false;

            if (!isDead && currentHealth < previousHealth && isStrongHit)
            {
                hitReaction?.ApplyStun(stunDuration);
                if (anim != null)
                {
                    anim.SetTrigger("doKnockback");
                }
            }
        }

        private void InterruptMovement()
        {
            isDashing = false;
            dashTimer = 0f;
            jumpRequested = false;
            dashRequested = false;
            EndGravityBoost();

            if (anim != null) anim.SetBool("isWork", false);
        }

        private void OnDisable()
        {
            hitReaction?.ClearStun();
            InterruptMovement();
        }

        private void OnDestroy()
        {
            if (hitReaction != null)
                hitReaction.StunStarted -= InterruptMovement;
        }

        public void TakeDamage(float damage)
        {
            if (isDead || !isActiveAndEnabled || float.IsNaN(damage) || float.IsInfinity(damage))
                return;

            damage = Mathf.Max(0f, damage);
            float damageReduction = DamageReduction;
            float finalDamage = damage * (1f - damageReduction);
            currentHealth -= finalDamage;
            currentHealth = Mathf.Max(0f, currentHealth);

            PlayerPhase beforePhase = currentPhase;
            UpdatePhase(playAnimation: true);
            bool phaseChangedNow = beforePhase != currentPhase;

            // ★ 페이즈 전환이나 넉백이 아닐 때 일반 피격(doDamage) 실행
            if (!suppressDamageAnimOnce && !phaseChangedNow && finalDamage > 0f && currentHealth > 0f && anim != null)
            {
                anim.SetTrigger("doDamage");
            }

            SaveGameState();

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void UpdatePhase(bool playAnimation = false)
        {
            float healthPercent = maxHealth > 0f ? (currentHealth / maxHealth) * 100f : 0f;
            PlayerPhase previousPhase = currentPhase;

            if (healthPercent <= phase3HealthPercent)
                currentPhase = PlayerPhase.Phase3;
            else if (healthPercent <= phase2HealthPercent)
                currentPhase = PlayerPhase.Phase2;
            else
                currentPhase = PlayerPhase.Phase1;

            if (previousPhase != currentPhase)
            {
                // ★ 페이즈가 바뀔 때 PhaseBreak 애니메이션 실행! (Player_P1_PhaseBreak)
                if (playAnimation && currentHealth > 0f && anim != null)
                {
                    anim.SetTrigger("doPhaseBreak");
                }
                OnPhaseChanged(previousPhase, currentPhase);
            }
        }

        private void OnPhaseChanged(PlayerPhase previousPhase, PlayerPhase newPhase)
        {
            if (!showDebugLog)
                return;

            Debug.Log($"[SUIT PHASE CHANGE] {previousPhase} → {newPhase} (HP: {currentHealth:F1}/{maxHealth:F1})");
        }

        private void Die()
        {
            isDead = true;
            if (showDebugLog)
                Debug.Log("[PlayerController] 플레이어 사망");

            if (anim != null) anim.SetBool("isWork", false);
            gameObject.SetActive(false);
        }

        public float GetHealthPercent() => maxHealth <= 0f ? 0f : (currentHealth / maxHealth) * 100f;

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
                return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
