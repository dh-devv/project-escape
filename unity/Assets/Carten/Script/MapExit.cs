using UnityEngine;

namespace Carten
{
    public class MapExit : WorldInteractable
    {
        [SerializeField] private string targetSceneName;
        [SerializeField] private string targetSpawnPointName;
        public override string Prompt => "Travel to " + targetSceneName;
        public override bool IsAvailable(PlayerController player) => base.IsAvailable(player) &&
            MapTransitionManager.Instance != null && !MapTransitionManager.Instance.IsTransitioning;

        protected override void Interact(PlayerController player)
        {
            if (string.IsNullOrWhiteSpace(targetSceneName) || string.IsNullOrWhiteSpace(targetSpawnPointName))
            {
                Debug.LogError("[MapExit] Set a destination scene and spawn point.", this);
                return;
            }
            MapTransitionManager.Instance.LoadMap(targetSceneName, targetSpawnPointName);
        }
    }
}
