using UnityEngine;

namespace Carten
{
    public class DamageZone : MonoBehaviour
    {
        [Header("=== Damage ===")]

        [SerializeField]
        private float damage = 10f;

        [SerializeField]
        private float damageInterval = 0.5f;

        private float damageTimer;


        private void Update()
        {
            if (damageTimer > 0f)
            {
                damageTimer -= Time.deltaTime;
            }
        }


        private void OnTriggerStay2D(Collider2D other)
        {
            if (damageTimer > 0f)
                return;

            PlayerController player =
                other.GetComponentInParent<PlayerController>();

            if (player == null)
                return;

            player.TakeDamage(damage);

            damageTimer = damageInterval;
        }
    }
}