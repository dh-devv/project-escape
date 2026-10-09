using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(BossController), typeof(Rigidbody2D))]
    public class BossAI : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float phase1MoveSpeed = 1.5f;
        [SerializeField] private float phase2MoveSpeed = 2.5f;
        [SerializeField] private float stopDistance = 1.3f;
        [SerializeField] private float detectionDistance = 15f;
        [SerializeField] private bool flipSprite = true;
        private BossController boss;
        private Rigidbody2D rb;
        private RigidbodyConstraints2D originalConstraints;
        private bool anchored;

        public Transform Target => target;
        public bool MovementLocked { get; set; }
        public bool HasLiveTarget => target != null && target.gameObject.activeInHierarchy &&
            (target.GetComponentInParent<PlayerController>() == null ||
             !target.GetComponentInParent<PlayerController>().IsDead);
        public bool IsTargetDetected => HasLiveTarget && Vector2.Distance(transform.position, target.position) <= detectionDistance;

        private void Awake()
        {
            boss = GetComponent<BossController>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!HasLiveTarget)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                target = player != null ? player.transform : null;
            }
            if (!boss.IsDead && IsTargetDetected)
                boss.BeginEncounter();
            if (!IsTargetDetected || boss.IsDead || MovementLocked || boss.IsStationary || !flipSprite)
                return;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (target.position.x >= transform.position.x ? 1f : -1f);
            transform.localScale = scale;
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
                return;
            }
            if (boss.IsDead || MovementLocked || !IsTargetDetected || IsInAttackRange())
            {
                StopMovement();
                return;
            }
            float speed = boss.CurrentPhase == BossController.BossPhase.Phase1
                ? phase1MoveSpeed : phase2MoveSpeed;
            rb.linearVelocity = new Vector2(
                Mathf.Sign(target.position.x - transform.position.x) * speed, rb.linearVelocity.y);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
        public bool IsInAttackRange() => HasLiveTarget &&
            Vector2.Distance(transform.position, target.position) <= stopDistance + 0.3f;
        public void StopMovement()
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        private void OnDisable()
        {
            MovementLocked = false;
            StopMovement();
            if (anchored && rb != null)
                rb.constraints = originalConstraints;
            anchored = false;
        }
    }
}
