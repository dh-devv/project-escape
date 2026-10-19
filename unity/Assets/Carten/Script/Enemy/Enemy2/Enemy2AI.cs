using UnityEngine;

namespace Carten
{
    public class Enemy2AI : MonoBehaviour
    {
        [Header("=== Target ===")]
        [SerializeField] private Transform target;

        [Header("=== Movement ===")]
        [SerializeField] private float moveSpeed = 1.2f;
        [Tooltip("이 거리보다 가까우면 뒤로 물러납니다.")]
        [SerializeField] private float minimumAttackDistance = 4f;
        [Tooltip("이 거리보다 멀어지면 플레이어에게 접근합니다.")]
        [SerializeField] private float maximumAttackDistance = 10f;

        [Header("=== Detection ===")]
        [SerializeField] private float detectionDistance = 12f;

        [Header("=== Facing ===")]
        [SerializeField] private bool flipSprite = true;

        private Enemy2Controller enemyController;
        private Rigidbody2D rb;
        private Animator anim;

        private void Awake()
        {
            enemyController = GetComponent<Enemy2Controller>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            FindPlayer();
        }

        private void Update()
        {
            if (enemyController == null || enemyController.IsDead || enemyController.IsKnockedBack)
                return;

            if (target == null)
            {
                FindPlayer();
                return;
            }

            HandleFacing();
        }

        private void FixedUpdate()
        {
            if (enemyController == null || enemyController.IsDead || enemyController.IsKnockedBack || target == null)
                return;

            if (enemyController.IsStopped)
            {
                StopMovement();
                return;
            }

            HandleMovement();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        private void HandleMovement()
        {
            if (rb == null) return;

            float horizontalDistance = Mathf.Abs(target.position.x - transform.position.x);

            // 감지 범위 밖이거나 적정 사격 거리 안에 있으면 정지
            if (horizontalDistance > detectionDistance ||
                (horizontalDistance >= minimumAttackDistance && horizontalDistance <= maximumAttackDistance))
            {
                StopMovement();
                return;
            }

            // 너무 가까우면 뒷걸음질, 너무 멀면 접근
            float direction;
            if (horizontalDistance < minimumAttackDistance)
            {
                direction = target.position.x > transform.position.x ? -1f : 1f;
            }
            else
            {
                direction = target.position.x > transform.position.x ? 1f : -1f;
            }

            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("isWork", true);
        }

        public void StopMovement()
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            if (anim != null)
            {
                anim.SetBool("isWork", false);
            }
        }

        private void HandleFacing()
        {
            if (!flipSprite || target == null) return;

            float direction = target.position.x - transform.position.x;
            if (Mathf.Abs(direction) < 0.01f) return;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
            transform.localScale = scale;
        }

        public bool IsInAttackRange()
        {
            if (target == null) return false;
            float horizontalDistance = Mathf.Abs(target.position.x - transform.position.x);
            return horizontalDistance >= minimumAttackDistance && horizontalDistance <= maximumAttackDistance;
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
        public Transform GetTarget() => target;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, maximumAttackDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minimumAttackDistance);
        }
    }
}
