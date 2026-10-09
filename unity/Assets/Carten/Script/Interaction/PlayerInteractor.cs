using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(PlayerController))]
    [DisallowMultipleComponent]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float interactionRadius = 1.75f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private bool showPrompt = true;
        private PlayerController player;
        public WorldInteractable FocusedObject { get; private set; }

        private void Awake() => player = GetComponent<PlayerController>();
        private void Update()
        {
            FocusedObject = FindNearest();
            if (FocusedObject != null && Input.GetKeyDown(interactKey))
                FocusedObject.TryInteract(player);
        }
        public WorldInteractable FindNearest()
        {
            if (player == null || !player.CanAct) return null;
            WorldInteractable nearest = null;
            float nearestDistance = float.PositiveInfinity;
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactionLayers))
            {
                WorldInteractable candidate = collider.GetComponentInParent<WorldInteractable>();
                if (candidate == null || !candidate.IsAvailable(player)) continue;
                float distance = ((Vector2)transform.position - collider.ClosestPoint(transform.position)).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }
            return nearest;
        }
        private void OnGUI()
        {
            if (showPrompt && FocusedObject != null && player.CanAct)
                GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height - 70f, 300f, 40f),
                    $"[{interactKey}] {FocusedObject.Prompt}");
        }
        private void OnDisable() => FocusedObject = null;
    }
}
