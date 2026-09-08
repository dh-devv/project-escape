using UnityEngine;

namespace Carten
{
    public class TestEnemy : EnemyBase
    {
        [Header("=== Test Enemy ===")]
        [SerializeField] private bool destroyOnDeath = false;

        protected override void Awake()
        {
            base.Awake();

            Debug.Log(
                $"[TestEnemy] 생성 → HP: {CurrentHealth:F0}/{MaxHealth:F0}"
            );
        }

        protected override void OnDamaged(float damage)
        {
            base.OnDamaged(damage);

            Debug.Log(
                $"[TestEnemy] 피격 확인 → 받은 데미지: {damage:F1}"
            );
        }

        protected override void Die()
        {
            Debug.Log("[TestEnemy] 사망");

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}