using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class WorldInteractable : MonoBehaviour
    {
        [SerializeField] private string prompt = "Interact";
        public virtual string Prompt => prompt;
        public virtual bool IsAvailable(PlayerController player) => isActiveAndEnabled && player != null && player.CanAct;

        public bool TryInteract(PlayerController player)
        {
            if (!IsAvailable(player)) return false;
            Interact(player);
            return true;
        }
        protected abstract void Interact(PlayerController player);
    }
}
