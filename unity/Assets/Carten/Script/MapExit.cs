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

            // ★ 씬 전환 시 페이드 매니저 연동 확인 후 호출
            if (SceneTransitionManager.Instance != null)
            {
                PlayerPrefs.SetString("LastTargetSpawnPoint", targetSpawnPointName);
                PlayerPrefs.Save();
                SceneTransitionManager.Instance.LoadSceneWithFade(targetSceneName);
            }
            else
            {
                MapTransitionManager.Instance.LoadMap(targetSceneName, targetSpawnPointName);
            }
        }
    }
}
