using UnityEngine;

namespace Carten
{
    public class MapExit : MonoBehaviour
    {
        [Header("=== Target Map ===")]
        [SerializeField] private string targetSceneName;

        [Header("=== Target Spawn ===")]
        [SerializeField] private string targetSpawnPointName;

        [Header("=== Interaction ===")]
        [SerializeField] private KeyCode interactKey = KeyCode.F;

        private bool playerInside = false;

        private void Update()
        {
            if (!playerInside)
                return;

            if (Input.GetKeyDown(interactKey))
            {
                Transition();
            }
        }

        private void Transition()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError(
                    "[MapExit] 이동할 Scene 이름이 설정되지 않았습니다."
                );
                return;
            }

            if (string.IsNullOrEmpty(targetSpawnPointName))
            {
                Debug.LogError(
                    "[MapExit] 이동할 SpawnPoint 이름이 설정되지 않았습니다."
                );
                return;
            }

            if (MapTransitionManager.Instance == null)
            {
                Debug.LogError(
                    "[MapExit] MapTransitionManager를 찾을 수 없습니다."
                );
                return;
            }

            MapTransitionManager.Instance.LoadMap(
                targetSceneName,
                targetSpawnPointName
            );
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerInside = true;

            Debug.Log(
                $"[MapExit] F키를 눌러 {targetSceneName} 이동"
            );
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerInside = false;
        }
    }
}