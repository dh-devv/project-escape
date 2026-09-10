using UnityEngine;

namespace Carten
{
    public class PlayerController : MonoBehaviour
    {
        // =========================================================
        // Player Phase
        // =========================================================

        public enum PlayerPhase
        {
            Phase1,
            Phase2,
            Phase3
        }


        // =========================================================
        // HP
        // =========================================================

        [Header("=== HP ===")]

        [SerializeField]
        private float maxHealth = 100f;

        private float currentHealth;

        public float CurrentHealth =>
            currentHealth;

        public float MaxHealth =>
            maxHealth;


        // =========================================================
        // Phase
        // =========================================================

        [Header("=== Phase ===")]

        [SerializeField]
        private float phase2HealthPercent = 70f;

        [SerializeField]
        private float phase3HealthPercent = 30f;

        private PlayerPhase currentPhase;

        public PlayerPhase CurrentPhase =>
            currentPhase;


        // =========================================================
        // Defense
        // =========================================================

        [Header("=== Defense ===")]

        [Range(0f, 1f)]
        [SerializeField]
        private float phase1Defense = 0.6f;

        [Range(0f, 1f)]
        [SerializeField]
        private float phase2Defense = 0.3f;

        [Range(0f, 1f)]
        [SerializeField]
        private float phase3Defense = 0f;


        // 스킬 등에 의해 일시적으로 추가되는 방어율
        private float additionalDamageReduction;


        /// <summary>
        /// 현재 Phase의 기본 방어율 + 추가 방어율
        /// </summary>
        public float DamageReduction
        {
            get
            {
                float baseReduction;

                switch (currentPhase)
                {
                    case PlayerPhase.Phase1:
                        baseReduction = phase1Defense;
                        break;

                    case PlayerPhase.Phase2:
                        baseReduction = phase2Defense;
                        break;

                    case PlayerPhase.Phase3:
                        baseReduction = phase3Defense;
                        break;

                    default:
                        baseReduction = 0f;
                        break;
                }

                return Mathf.Clamp01(
                    baseReduction +
                    additionalDamageReduction
                );
            }
        }


        public float DefensePercent =>
            DamageReduction * 100f;


        /// <summary>
        /// 현재 Phase의 기본 방어율만 반환
        /// </summary>
        public float BaseDamageReduction
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase1:
                        return phase1Defense;

                    case PlayerPhase.Phase2:
                        return phase2Defense;

                    case PlayerPhase.Phase3:
                        return phase3Defense;

                    default:
                        return 0f;
                }
            }
        }


        /// <summary>
        /// 현재 추가 방어율
        /// </summary>
        public float AdditionalDamageReduction =>
            additionalDamageReduction;


        /// <summary>
        /// 일시적인 추가 피해 감소율 설정
        /// </summary>
        public void SetAdditionalDamageReduction(
            float reduction)
        {
            additionalDamageReduction =
                Mathf.Clamp(
                    reduction,
                    0f,
                    1f
                );

            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerController] " +
                    $"추가 방어율 설정 → " +
                    $"+{additionalDamageReduction * 100f:F0}% / " +
                    $"최종 방어율: {DefensePercent:F0}%"
                );
            }
        }


        /// <summary>
        /// 추가 피해 감소율 제거
        /// </summary>
        public void ClearAdditionalDamageReduction()
        {
            additionalDamageReduction = 0f;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerController] " +
                    $"추가 방어율 제거 → " +
                    $"최종 방어율: {DefensePercent:F0}%"
                );
            }
        }


        // =========================================================
        // Movement
        // =========================================================

        [Header("=== Movement ===")]

        [SerializeField]
        private float phase1MoveSpeed = 2.5f;

        [SerializeField]
        private float phase2MoveSpeed = 5f;

        [SerializeField]
        private float phase3MoveSpeed = 9f;


        public float CurrentMoveSpeed
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2:
                        return phase2MoveSpeed;

                    case PlayerPhase.Phase3:
                        return phase3MoveSpeed;

                    default:
                        return phase1MoveSpeed;
                }
            }
        }


        // =========================================================
        // Jump
        // =========================================================

        [Header("=== Jump ===")]

        [SerializeField]
        private float phase1JumpForce = 6f;

        [SerializeField]
        private float phase2JumpForce = 10f;

        [SerializeField]
        private float phase3JumpForce = 10f;


        [SerializeField]
        private int phase1MaxJumps = 1;

        [SerializeField]
        private int phase2MaxJumps = 2;

        [SerializeField]
        private int phase3MaxJumps = 3;


        public float CurrentJumpForce
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2:
                        return phase2JumpForce;

                    case PlayerPhase.Phase3:
                        return phase3JumpForce;

                    default:
                        return phase1JumpForce;
                }
            }
        }


        public int MaxJumps
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2:
                        return phase2MaxJumps;

                    case PlayerPhase.Phase3:
                        return phase3MaxJumps;

                    default:
                        return phase1MaxJumps;
                }
            }
        }


        // =========================================================
        // Gravity
        // =========================================================

        [Header("=== Gravity ===")]

        [SerializeField]
        private float baseGravityScale = 3f;

        [SerializeField]
        private float fallGravityMultiplier = 1.8f;

        [SerializeField]
        private float riseGravityMultiplier = 1f;


        // =========================================================
        // Dash
        // =========================================================

        [Header("=== Dash ===")]

        [SerializeField]
        private float phase2DashSpeed = 12f;

        [SerializeField]
        private float phase3DashSpeed = 18f;

        [SerializeField]
        private float dashDuration = 0.15f;

        [SerializeField]
        private float dashCooldown = 0.5f;


        public bool CanDash =>
            currentPhase == PlayerPhase.Phase2 ||
            currentPhase == PlayerPhase.Phase3;


        public float CurrentDashSpeed
        {
            get
            {
                if (currentPhase ==
                    PlayerPhase.Phase3)
                {
                    return phase3DashSpeed;
                }

                return phase2DashSpeed;
            }
        }


        // =========================================================
        // Attack
        // =========================================================

        [Header("=== Attack ===")]

        [SerializeField]
        private float phase1AttackSpeedMultiplier = 1f;

        [SerializeField]
        private float phase2AttackSpeedMultiplier = 1.5f;

        [SerializeField]
        private float phase3AttackSpeedMultiplier = 5f;


        [SerializeField]
        private float phase1CriticalMultiplier = 1f;

        [SerializeField]
        private float phase2CriticalMultiplier = 1f;

        [SerializeField]
        private float phase3CriticalMultiplier = 5f;


        public float AttackSpeedMultiplier
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2:
                        return phase2AttackSpeedMultiplier;

                    case PlayerPhase.Phase3:
                        return phase3AttackSpeedMultiplier;

                    default:
                        return phase1AttackSpeedMultiplier;
                }
            }
        }


        public float CriticalMultiplier
        {
            get
            {
                switch (currentPhase)
                {
                    case PlayerPhase.Phase2:
                        return phase2CriticalMultiplier;

                    case PlayerPhase.Phase3:
                        return phase3CriticalMultiplier;

                    default:
                        return phase1CriticalMultiplier;
                }
            }
        }


        // =========================================================
        // Ground Check
        // =========================================================

        [Header("=== Ground Check ===")]

        [SerializeField]
        private Transform groundCheck;

        [SerializeField]
        private float groundCheckRadius = 0.2f;

        [Tooltip("일반 바닥 Layer")]
        [SerializeField]
        private LayerMask groundLayer;

        [Tooltip("밟을 수 있는 오브젝트 Layer")]
        [SerializeField]
        private LayerMask jumpableLayer;


        // =========================================================
        // Debug
        // =========================================================

        [Header("=== Debug ===")]

        [SerializeField]
        private bool showDebugLog = true;


        // =========================================================
        // Internal
        // =========================================================

        private Rigidbody2D rb;

        private int jumpCount;

        private float dashTimer;
        private float dashCooldownTimer;

        private bool isDashing;
        private bool isDead;

        // 스킬 강제 이동 중 일반 이동 잠금
        private bool isSkillMovementLocked;

        // Update에서 입력을 받고 FixedUpdate에서 점프 실행
        private bool jumpRequested;

        // Update에서 대시 입력을 받아 FixedUpdate에서 실제 대시 실행
        private bool dashRequested;


        public bool IsDead =>
            isDead;

        public bool IsSkillMovementLocked =>
            isSkillMovementLocked;


        // =========================================================
        // Awake        Private void HandleJump()
        // =========================================================

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                Debug.LogError(
                    "[PlayerController] " +
                    "Rigidbody2D를 찾을 수 없습니다."
                );

                return;
            }


            // 회전 방지
            rb.constraints |=
                RigidbodyConstraints2D.FreezeRotation;


            // 물리 이동 안정화
            rb.interpolation =
                RigidbodyInterpolation2D.Interpolate;

            rb.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;


            currentHealth =
                maxHealth;

            additionalDamageReduction =
                0f;

            isSkillMovementLocked =
                false;

            jumpRequested =
                false;

            dashRequested =
                false;

            UpdatePhase();


            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerController] " +
                    $"플레이어 생성\n" +
                    $"HP: {currentHealth:F0}/" +
                    $"{maxHealth:F0}\n" +
                    $"Phase: {currentPhase}\n" +
                    $"기본 방어율: " +
                    $"{BaseDamageReduction * 100f:F0}%\n" +
                    $"최종 방어율: " +
                    $"{DefensePercent:F0}%"
                );
            }
        }


        // =========================================================
        // Update
        // =========================================================

        private void Update()
        {
            if (isDead)
                return;

            HandleTimers();

            HandleJumpInput();
            HandleDashInput();
        }


        // =========================================================
        // FixedUpdate
        // =========================================================

        private void FixedUpdate()
        {
            if (isDead)
                return;

            HandleMovement();

            HandleJump();

            HandleDash();

            ApplyGravity();
        }


        // =========================================================
        // Timer
        // =========================================================

        private void HandleTimers()
        {
            // dashTimer는 실제 대시 진행 시간을 FixedUpdate에서만 감소시킴
            // Update + FixedUpdate에서 중복 차감되지 않도록 함

            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -=
                    Time.deltaTime;

                if (dashCooldownTimer < 0f)
                {
                    dashCooldownTimer = 0f;
                }
            }
        }


        // =========================================================
        // Movement
        // =========================================================

        private void HandleMovement()
        {
            if (rb == null)
                return;

            if (isDashing)
                return;

            if (isSkillMovementLocked)
                return;


            float horizontal =
                Input.GetAxisRaw(
                    "Horizontal"
                );


            rb.linearVelocity =
                new Vector2(
                    horizontal *
                    CurrentMoveSpeed,
                    rb.linearVelocity.y
                );


            // 이동 방향에 따라 플레이어 반전
            if (horizontal != 0f)
            {
                Vector3 scale =
                    transform.localScale;

                scale.x =
                    Mathf.Abs(scale.x) *
                    Mathf.Sign(horizontal);

                transform.localScale =
                    scale;
            }
        }


        // =========================================================
        // Jump Input
        // =========================================================

        private void HandleJumpInput()
        {
            if (isSkillMovementLocked)
                return;

            if (!Input.GetButtonDown("Jump"))
                return;

            jumpRequested = true;
        }


        // =========================================================
        // Jump
        // =========================================================

        private void HandleJump()
        {
            if (rb == null)
                return;

            if (isSkillMovementLocked)
            {
                jumpRequested = false;
                return;
            }

            bool isGrounded = CheckGround();

            // 바닥 또는 밟을 수 있는 오브젝트에 닿아 있으면
            // 점프 횟수를 초기화한다.
            if (isGrounded)
            {
                jumpCount = 0;
            }

            if (!jumpRequested)
                return;

            jumpRequested = false;

            if (jumpCount >= MaxJumps)
                return;

            rb.linearVelocity =
                new Vector2(
                    rb.linearVelocity.x,
                    CurrentJumpForce
                );

            jumpCount++;
        }


        // =========================================================
        // Ground Check
        // =========================================================

        private bool CheckGround()
        {
            if (groundCheck == null)
                return false;


            // 일반 바닥 확인
            if (groundLayer.value != 0)
            {
                Collider2D groundHit =
                    Physics2D.OverlapCircle(
                        groundCheck.position,
                        groundCheckRadius,
                        groundLayer
                    );

                if (groundHit != null)
                    return true;
            }


            // 밟을 수 있는 오브젝트 확인
            if (jumpableLayer.value != 0)
            {
                Collider2D jumpableHit =
                    Physics2D.OverlapCircle(
                        groundCheck.position,
                        groundCheckRadius,
                        jumpableLayer
                    );

                if (jumpableHit != null)
                    return true;
            }


            return false;
        }


        // =========================================================
        // Dash Input
        // =========================================================

        private void HandleDashInput()
        {
            if (!CanDash)
                return;

            if (isSkillMovementLocked)
                return;

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                dashRequested = true;
            }
        }


        // =========================================================
        // Dash
        // =========================================================

        private void HandleDash()
        {
            if (isDashing)
            {
                dashTimer -=
                    Time.fixedDeltaTime;

                if (dashTimer <= 0f)
                {
                    dashTimer = 0f;
                    isDashing = false;
                }

                return;
            }

            if (!CanDash)
            {
                dashRequested = false;
                return;
            }

            if (isSkillMovementLocked)
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

            dashTimer =
                dashDuration;

            dashCooldownTimer =
                dashCooldown;


            float direction =
                transform.localScale.x >= 0f
                    ? 1f
                    : -1f;


            rb.linearVelocity =
                new Vector2(
                    direction *
                    CurrentDashSpeed,
                    0f
                );
        }


        // =========================================================
        // Gravity
        // =========================================================

        private void ApplyGravity()
        {
            if (rb == null)
                return;


            // Dash 중에는 기존 중력 상태 유지
            if (isDashing)
                return;


            // 스킬 강제 이동 중에는
            // Skill 쪽에서 중력을 제어한다.
            if (isSkillMovementLocked)
                return;


            if (rb.linearVelocity.y < 0f)
            {
                rb.gravityScale =
                    baseGravityScale *
                    fallGravityMultiplier;
            }
            else
            {
                rb.gravityScale =
                    baseGravityScale *
                    riseGravityMultiplier;
            }
        }


        // =========================================================
        // Skill Movement
        // =========================================================

        public void StartGravityBoost(
            float horizontalVelocity,
            float verticalVelocity)
        {
            if (rb == null)
                return;


            isSkillMovementLocked =
                true;


            rb.gravityScale = 0f;


            rb.linearVelocity =
                new Vector2(
                    horizontalVelocity,
                    verticalVelocity
                );
        }


        public void EndGravityBoost()
        {
            if (rb == null)
                return;


            rb.gravityScale =
                baseGravityScale;


            rb.linearVelocity =
                new Vector2(
                    0f,
                    rb.linearVelocity.y
                );


            isSkillMovementLocked =
                false;
        }
        // =========================================================
        // Damage
        // =========================================================

        public void TakeDamage(float damage)
        {
            if (isDead)
                return;


            damage =
                Mathf.Max(
                    0f,
                    damage
                );


            // -----------------------------------------------------
            // 현재 방어율
            // -----------------------------------------------------

            float damageReduction =
                DamageReduction;


            // -----------------------------------------------------
            // 최종 피해 계산
            // -----------------------------------------------------

            float finalDamage =
                damage *
                (1f - damageReduction);


            // -----------------------------------------------------
            // HP 감소
            // -----------------------------------------------------

            currentHealth -=
                finalDamage;


            currentHealth =
                Mathf.Max(
                    0f,
                    currentHealth
                );


            // -----------------------------------------------------
            // Debug
            // -----------------------------------------------------

            if (showDebugLog)
            {
                Debug.Log(
                    $"[PlayerController] 피격\n" +
                    $"Phase: {currentPhase}\n" +
                    $"원본 공격력: {damage:F1}\n" +
                    $"기본 방어율: " +
                    $"{BaseDamageReduction * 100f:F0}%\n" +
                    $"추가 방어율: " +
                    $"{AdditionalDamageReduction * 100f:F0}%\n" +
                    $"최종 방어율: " +
                    $"{DefensePercent:F0}%\n" +
                    $"최종 피해: {finalDamage:F1}\n" +
                    $"HP: {currentHealth:F1}/" +
                    $"{maxHealth:F1}"
                );
            }


            // -----------------------------------------------------
            // Phase 재판정
            // -----------------------------------------------------

            UpdatePhase();


            // -----------------------------------------------------
            // 사망
            // -----------------------------------------------------

            if (currentHealth <= 0f)
            {
                Die();
            }
        }


        // =========================================================
        // Phase Update
        // =========================================================

        private void UpdatePhase()
        {
            float healthPercent =
                maxHealth > 0f
                    ? (currentHealth /
                       maxHealth) * 100f
                    : 0f;


            PlayerPhase previousPhase =
                currentPhase;


            if (healthPercent <=
                phase3HealthPercent)
            {
                currentPhase =
                    PlayerPhase.Phase3;
            }
            else if (healthPercent <=
                     phase2HealthPercent)
            {
                currentPhase =
                    PlayerPhase.Phase2;
            }
            else
            {
                currentPhase =
                    PlayerPhase.Phase1;
            }


            if (previousPhase !=
                currentPhase)
            {
                OnPhaseChanged(
                    previousPhase,
                    currentPhase
                );
            }
        }


        // =========================================================
        // Phase Changed
        // =========================================================

        private void OnPhaseChanged(
            PlayerPhase previousPhase,
            PlayerPhase newPhase)
        {
            if (!showDebugLog)
                return;


            Debug.Log(
                $"================================\n" +
                $"[SUIT PHASE CHANGE]\n" +
                $"상태 변화: " +
                $"{previousPhase} → {newPhase}\n" +
                $"현재 HP: " +
                $"{currentHealth:F1}/{maxHealth:F1}\n" +
                $"HP: " +
                $"{GetHealthPercent():F1}%\n" +
                $"기본 방어율: " +
                $"{BaseDamageReduction * 100f:F0}%\n" +
                $"추가 방어율: " +
                $"{AdditionalDamageReduction * 100f:F0}%\n" +
                $"최종 방어율: " +
                $"{DefensePercent:F0}%\n" +
                $"이동속도: " +
                $"{CurrentMoveSpeed:F1}\n" +
                $"점프 횟수: " +
                $"{MaxJumps}\n" +
                $"공격속도 배율: " +
                $"x{AttackSpeedMultiplier:F1}\n" +
                $"크리티컬 배율: " +
                $"x{CriticalMultiplier:F1}\n" +
                $"================================"
            );
        }


        // =========================================================
        // Death
        // =========================================================

        private void Die()
        {
            if (isDead)
                return;


            isDead = true;


            if (rb != null)
            {
                rb.linearVelocity =
                    Vector2.zero;

                rb.gravityScale =
                    baseGravityScale;
            }


            additionalDamageReduction =
                0f;

            isSkillMovementLocked =
                false;

            jumpRequested =
                false;

            dashRequested =
                false;


            if (showDebugLog)
            {
                Debug.Log(
                    "[PlayerController] " +
                    "플레이어 사망"
                );
            }


            gameObject.SetActive(false);
        }


        // =========================================================
        // Utility
        // =========================================================

        public float GetHealthPercent()
        {
            if (maxHealth <= 0f)
                return 0f;


            return
                (currentHealth /
                 maxHealth) * 100f;
        }


        // =========================================================
        // Gizmos
        // =========================================================

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
                return;


            Gizmos.color =
                Color.green;


            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}