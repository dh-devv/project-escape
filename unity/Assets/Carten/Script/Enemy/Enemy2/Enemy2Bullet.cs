using UnityEngine;

namespace Carten
{
    public class Enemy2Bullet : MonoBehaviour
    {
        [Header("=== Bullet ===")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField, Min(0f)] private float hitStunDuration = 0.25f;

        [Header("=== Collision ===")]
        [Tooltip("탄환이 부딪히면 사라질 지형/벽 Layer")]
        [SerializeField] private LayerMask obstacleLayer;

        private Rigidbody2D rb;
        private float damage;
        private LayerMask playerLayer;
        private bool consumed;
        private bool initialized;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (obstacleLayer.value == 0)
            {
                int groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer >= 0) obstacleLayer = 1 << groundLayer;
            }
        }

        public void Initialize(Vector2 direction, float bulletDamage, LayerMask targetPlayerLayer)
        {
            damage = bulletDamage;
            playerLayer = targetPlayerLayer;

            Vector2 moveDir = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;

            if (rb != null)
            {
                rb.linearVelocity = moveDir * speed;
            }

            // 총알 방향에 맞춰 이미지 회전/반전
            if (moveDir.x < 0f)
            {
                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            consumed = false;
            initialized = true;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!initialized || consumed || other == null) return;

            // 1. 플레이어 피격 판정
            if (((1 << other.gameObject.layer) & playerLayer.value) != 0 || other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponentInParent<PlayerController>();
                if (player != null && !player.IsDead)
                {
                    consumed = true;
                    player.TakeDamage(damage, hitStunDuration);
                    Destroy(gameObject);
                    return;
                }
            }

            // 2. 벽/바닥 충돌 시 소멸
            if (((1 << other.gameObject.layer) & obstacleLayer.value) != 0)
            {
                consumed = true;
                Destroy(gameObject);
            }
        }
    }
}
