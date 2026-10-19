using UnityEngine;

namespace Carten
{
    public class Enemy1AI : MonoBehaviour
    {
        [Header("=== Target ===")]
        [SerializeField] private Transform target;

        [Header("=== Movement ===")]
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private float stopDistance = 1.1f;

        [Header("=== Detection ===")]
        [SerializeField] private float detectionDistance = 10f;

        [Header("=== Facing ===")]
        [SerializeField] private bool flipSprite = true;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private Enemy1Controller enemyController;
        private Rigidbody2D rb;
        private Animator anim;

        private void Awake()
        {
            enemyController = GetComponent<Enemy1Controller>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();

            if (enemyController == null && showDebugLog)
                Debug.LogError("[Enemy1AI] Enemy1Controller를 찾을 수 없습니다.");
            if (rb == null && showDebugLog)
                Debug.LogError("[Enemy1AI] Rigidbody2D를 찾을 수 없습니다.");

            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else if (showDebugLog)
                {
                    Debug.LogWarning("[Enemy1AI] Player Tag를 가진 오브젝트를 찾지 못했습니다.");
                }
            }
        }

        private void Update()
        {
            if (enemyController == null || enemyController.IsDead || enemyController.IsKnockedBack || target == null)
                return;

            HandleMovement();
            HandleFacing();
        }

        private void HandleMovement()
        {
            if (rb == null)
                return;

            if (enemyController.IsStopped)
            {
                StopMovement();
                return;
            }

            float distance = Vector2.Distance(transform.position, target.position);

            if (distance > detectionDistance || distance <= stopDistance)
            {
                StopMovement();
                return;
            }

            float direction = target.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            if (anim != null)
                anim.SetBool("isWork", true);
        }

        private void StopMovement()
        {
            if (rb == null)
                return;

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (anim != null)
                anim.SetBool("isWork", false);
        }

        private void HandleFacing()
        {
            if (!flipSprite)
                return;

            float direction = target.position.x - transform.position.x;
            if (Mathf.Abs(direction) < 0.01f)
                return;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
            transform.localScale = scale;
        }

        public bool IsInAttackRange()
        {
            if (target == null)
                return false;

            float distance = Vector2.Distance(transform.position, target.position);
            return distance <= stopDistance + 0.2f;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
    }
}
