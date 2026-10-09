using UnityEngine;

namespace Carten
{
    public class Enemy2Bullet : MonoBehaviour
    {

        [Header("=== Bullet ===")]
        [SerializeField] private float speed = 8f;

        [SerializeField] private float lifetime = 3f;

        [Header("=== Collision ===")]
        [Tooltip("탄환이 부딪히면 사라질 Layer")]
        [SerializeField] private LayerMask obstacleLayer;

        private Rigidbody2D rb;

        private float damage;
        private LayerMask playerLayer;

        [SerializeField, Min(0f)] private float hitStunDuration = 0.25f;
        private bool consumed;
        private bool initialized;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                Debug.LogError("[Enemy2Bullet] " + "Rigidbody2D를 찾을 수 없습니다.");
            }
        }

        public void Initialize(
            Vector2 direction,
            float bulletDamage,
            LayerMask targetPlayerLayer)
        {
            damage = bulletDamage;

            playerLayer = targetPlayerLayer;

            direction = direction.normalized;

            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }

            consumed = false;
            initialized = true;

            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(
            Collider2D other)
        {
            if (!initialized || consumed)
                return;
            // Player
            if (((1 << other.gameObject.layer) &
                 playerLayer.value) != 0)
            {
                PlayerController player = other.GetComponentInParent<PlayerController>();

                if (player != null)
                {
                    consumed = true;
                    player.TakeDamage(damage, hitStunDuration);

                    Debug.Log($"[Enemy2Bullet] " + $"Player 명중 / " + $"Damage: {damage:F1}");

                    Destroy(gameObject);

                    return;
                }
            }
            // Ground / Wall
            if (((1 << other.gameObject.layer) &
                 obstacleLayer.value) != 0)
            {
                consumed = true;
                Destroy(gameObject);

                return;
            }

            // Enemy 등 다른 오브젝트와는
            // 충돌해도 바로 제거하지 않음.
        }
    }
}
