using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    public class MapTransitionManager : MonoBehaviour
    {
        public static MapTransitionManager Instance { get; private set; }

        private string targetSpawnPoint;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadMap(string sceneName, string spawnPointName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError(
                    "[MapTransitionManager] Scene 이름이 비어 있습니다."
                );
                return;
            }

            targetSpawnPoint = spawnPointName;

            Debug.Log(
                $"[MapTransitionManager] 맵 이동: {sceneName} / Spawn: {spawnPointName}"
            );

            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.LoadScene(sceneName);
        }

        private void OnSceneLoaded(
            Scene scene,
            LoadSceneMode mode
        )
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (string.IsNullOrEmpty(targetSpawnPoint))
                return;

            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError(
                    "[MapTransitionManager] Player를 찾을 수 없습니다."
                );
                return;
            }

            GameObject spawnPoint =
                GameObject.Find(targetSpawnPoint);

            if (spawnPoint == null)
            {
                Debug.LogError(
                    $"[MapTransitionManager] SpawnPoint를 찾을 수 없습니다: {targetSpawnPoint}"
                );
                return;
            }

            player.transform.position =
                spawnPoint.transform.position;

            Rigidbody2D rb =
                player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            Debug.Log(
                $"[MapTransitionManager] Player 배치 완료: {targetSpawnPoint}"
            );

            targetSpawnPoint = null;
        }
    }
}