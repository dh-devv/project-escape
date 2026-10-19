using System.Collections;
using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(BossController), typeof(Rigidbody2D))]
    public class BossAI : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float phase1MoveSpeed = 1.5f;
        [SerializeField] private float phase2MoveSpeed = 2.5f;

        [Header("=== Distance Settings ===")]
        [Tooltip("보스 덩치(Scale 2)에 맞춰 2.5 ~ 3.0 권장")]
        [SerializeField] private float stopDistance = 2.5f;
        [SerializeField] private float detectionDistance = 15f;
        [SerializeField] private bool flipSprite = true;

        [Header("=== Auto Attack Settings ===")]
        [SerializeField] private bool autoAttackInRange = true;
        [SerializeField] private float baseAttackCooldown = 1.5f;
        [SerializeField] private float attackWindupTime = 0.25f;
        [Tooltip("몇 번째 공격마다 강공격(Skill)을 사용할지 설정")]
        [SerializeField] private int strongAttackEveryN = 3;

        private BossController boss;
        private BossCombat combat;
        private Rigidbody2D rb;
        private Animator anim;
        private Collider2D bossCollider;
        private RigidbodyConstraints2D originalConstraints;

        private bool anchored;
        private bool isAttacking;
        private float attackCooldownTimer;
        private int attackCount;

        public Transform Target => target;
        public bool MovementLocked { get; set; }
        public bool HasLiveTarget => target != null && target.gameObject.activeInHierarchy &&
            (target.GetComponentInParent<PlayerController>() == null ||
             !target.GetComponentInParent<PlayerController>().IsDead);
        public bool IsTargetDetected => HasLiveTarget && Vector2.Distance(transform.position, target.position) <= detectionDistance;

        private void Awake()
        {
            boss = GetComponent<BossController>();
            combat = GetComponent<BossCombat>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            bossCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (attackCooldownTimer > 0f)
                attackCooldownTimer -= Time.deltaTime;

            if (!HasLiveTarget)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                target = player != null ? player.transform : null;
            }
            if (!boss.IsDead && IsTargetDetected)
                boss.BeginEncounter();
            if (!IsTargetDetected || boss.IsDead || MovementLocked || isAttacking || boss.IsStationary)
                return;

            if (flipSprite)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (target.position.x >= transform.position.x ? 1f : -1f);
                transform.localScale = scale;
            }

            // ★ 사거리 안에 들어오면 자동으로 공격 실행!
            if (autoAttackInRange && combat != null && IsInAttackRange() && attackCooldownTimer <= 0f)
            {
                StartCoroutine(AttackRoutine());
            }
        }

        private void FixedUpdate()
        {
            if (boss.IsStationary)
            {
                if (!anchored)
                {
                    originalConstraints = rb.constraints;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    anchored = true;
                }
                rb.linearVelocity = Vector2.zero;
                if (anim != null) anim.SetBool("isWork", false);
                return;
            }
            if (boss.IsDead || MovementLocked || isAttacking || !IsTargetDetected || IsInAttackRange())
            {
                StopMovement();
                return;
            }
            float speed = boss.CurrentPhase == BossController.BossPhase.Phase1
                ? phase1MoveSpeed : phase2MoveSpeed;
            rb.linearVelocity = new Vector2(
                Mathf.Sign(target.position.x - transform.position.x) * speed, rb.linearVelocity.y);

            if (anim != null)
                anim.SetBool("isWork", true);
        }

        private IEnumerator AttackRoutine()
        {
            isAttacking = true;
            StopMovement();

            attackCount++;
            bool useStrongAttack = strongAttackEveryN > 0 && (attackCount % strongAttackEveryN == 0);

            // 1. 공격 애니메이션 먼저 실행
            boss.TriggerAttackAnimation(isStrongAttack: useStrongAttack);

            // 2. 팔을 휘두르는 선딜레이 대기 후 실제 데미지 판정 실행
            yield return new WaitForSeconds(attackWindupTime);

            if (!boss.IsDead && combat != null)
            {
                if (useStrongAttack)
                    combat.PerformAreaAttack();
                else
                    combat.PerformBasicAttack();
            }

            float speedMultiplier = Mathf.Max(0.1f, boss.AttackSpeedMultiplier);
            attackCooldownTimer = baseAttackCooldown / speedMultiplier;
            isAttacking = false;
        }

        public void SetTarget(Transform newTarget) => target = newTarget;

        // ★ 보스와 플레이어의 키(Y축) 차이 때문에 사거리가 안 닿는 현상을 해결하기 위해 가로(X축) 거리와 콜라이더 거리를 함께 체크
        public bool IsInAttackRange()
        {
            if (!HasLiveTarget) return false;

            float horizontalDist = Mathf.Abs(target.position.x - transform.position.x);
            float verticalDist = Mathf.Abs(target.position.y - transform.position.y);

            if (horizontalDist <= stopDistance + 0.3f && verticalDist <= 3.5f)
                return true;

            Collider2D targetCol = target.GetComponentInChildren<Collider2D>();
            if (bossCollider != null && targetCol != null)
            {
                ColliderDistance2D colDist = bossCollider.Distance(targetCol);
                if (colDist.isOverlapped || colDist.distance <= 0.6f)
                    return true;
            }

            return false;
        }

        public void StopMovement()
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null)
                anim.SetBool("isWork", false);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            isAttacking = false;
            MovementLocked = false;
            StopMovement();
            if (anchored && rb != null)
                rb.constraints = originalConstraints;
            anchored = false;
        }
    }
}
