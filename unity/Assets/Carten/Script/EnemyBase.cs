using UnityEngine;

namespace Carten
{
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("=== HP ===")]
        [SerializeField] protected float maxHealth = 100f;

        protected float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0f;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(float damage)
        {
            if (IsDead || float.IsNaN(damage) || float.IsInfinity(damage))
                return;

            damage = Mathf.Max(0f, damage);

            currentHealth -= damage;
            currentHealth = Mathf.Max(0f, currentHealth);

            OnDamaged(damage);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        protected virtual void OnDamaged(float damage)
        {
            Debug.Log(
                $"[{gameObject.name}] 피격 → Damage: {damage:F1} / HP: {currentHealth:F1} / {maxHealth:F1}"
            );
        }

        protected virtual void Die()
        {
            Debug.Log($"[{gameObject.name}] 사망");

            gameObject.SetActive(false);
        }
    }
}
