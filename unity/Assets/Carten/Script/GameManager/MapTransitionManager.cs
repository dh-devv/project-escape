using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    public class MapTransitionManager : MonoBehaviour
    {
        public static MapTransitionManager Instance { get; private set; }
        private string targetSpawnPoint;
        private string targetScene;
        private float? carriedHealth;
        private int placedSceneHandle = -1;
        public bool IsTransitioning { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
                new GameObject("MapTransitionManager").AddComponent<MapTransitionManager>();
        }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void LoadMap(string sceneName, string spawnPointName)
        {
            if (IsTransitioning) return;
            if (string.IsNullOrWhiteSpace(sceneName) || string.IsNullOrWhiteSpace(spawnPointName) ||
                !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("[MapTransitionManager] Destination scene/spawn is invalid or not in Build Settings.", this);
                return;
            }
            targetSpawnPoint = spawnPointName;
            targetScene = sceneName;
            GameObject source = GameObject.FindGameObjectWithTag("Player");
            PlayerController controller = source != null ? source.GetComponent<PlayerController>() : null;
            carriedHealth = controller != null && !controller.IsDead ? controller.CurrentHealth : (float?)null;
            IsTransitioning = true;
            try { SceneManager.LoadScene(sceneName); }
            catch (System.Exception error)
            {
                ClearPending();
                Debug.LogException(error, this);
            }
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsTransitioning || scene.name != targetScene) return;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject spawn = GameObject.Find(targetSpawnPoint);
            if (player != null && carriedHealth.HasValue)
                player.GetComponent<PlayerController>()?.RestoreHealthForMap(carriedHealth.Value);
            if (player == null || spawn == null)
                Debug.LogError("[MapTransitionManager] Player or destination spawn was not found.", this);
            else
            {
                player.transform.position = spawn.transform.position;
                placedSceneHandle = scene.handle;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }
            ClearPending();
        }
        private void ClearPending()
        {
            IsTransitioning = false;
            targetScene = null;
            targetSpawnPoint = null;
            carriedHealth = null;
        }
        public bool HasPlacedPlayer(Scene scene) => placedSceneHandle == scene.handle;
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }
    }
}
