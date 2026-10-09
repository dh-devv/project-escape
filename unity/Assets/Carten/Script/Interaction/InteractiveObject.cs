using UnityEngine;
using UnityEngine.Events;

namespace Carten
{
    /// <summary>Switches, doors and single-use rewards without coupling to combat.</summary>
    public sealed class InteractiveObject : WorldInteractable
    {
        public enum InteractionMode { Activate, Toggle, Deactivate }
        [SerializeField] private InteractionMode mode = InteractionMode.Toggle;
        [SerializeField] private GameObject[] targets = new GameObject[0];
        [SerializeField] private bool singleUse;
        [SerializeField, Min(0f)] private float cooldown = 0.3f;
        [SerializeField] private UnityEvent<PlayerController> onInteracted = new UnityEvent<PlayerController>();
        private bool used;
        private float availableAt;
        public bool HasBeenUsed => used;
        public UnityEvent<PlayerController> OnInteracted => onInteracted;

        public override bool IsAvailable(PlayerController player) => base.IsAvailable(player) && (!singleUse || !used) && Time.time >= availableAt;

        protected override void Interact(PlayerController player)
        {
            used = true;
            availableAt = Time.time + cooldown;
            foreach (GameObject target in targets)
            {
                if (target == null || target == gameObject) continue;
                target.SetActive(mode == InteractionMode.Activate ||
                    (mode == InteractionMode.Toggle && !target.activeSelf));
            }
            onInteracted.Invoke(player);
        }
    }
}
