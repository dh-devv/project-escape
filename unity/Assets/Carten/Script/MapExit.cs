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

            // 스폰 포인트 정보 저장 (ConditionalDoor와 동일한 PrefKey 사용)
            PlayerPrefs.SetString("LastTargetSpawnPoint", targetSpawnPointName);
            PlayerPrefs.Save();

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFade(targetSceneName);
            }
            else if (MapTransitionManager.Instance != null)
            {
                MapTransitionManager.Instance.LoadMap(targetSceneName, targetSpawnPointName);
            }
        }
    }
}
