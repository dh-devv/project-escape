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
        private GameObject persistentPlayer;
        private GameObject persistentCamera;
        private SceneHandle placedSceneHandle = SceneHandle.None;
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
            if (controller == null)
            {
                Debug.LogError("[MapTransitionManager] 이동할 플레이어가 없습니다.", this);
                return;
            }
            carriedHealth = controller != null && !controller.IsDead ? controller.CurrentHealth : (float?)null;
            persistentPlayer = source;
            source.transform.SetParent(null);
            DontDestroyOnLoad(source);
            if (Camera.main != null)
            {
                persistentCamera = Camera.main.gameObject;
                persistentCamera.transform.SetParent(null);
                DontDestroyOnLoad(persistentCamera);
            }
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
            if (scene.name == "Main UI")
            {
                if (persistentPlayer != null) Destroy(persistentPlayer);
                if (persistentCamera != null) Destroy(persistentCamera);
                persistentPlayer = null;
                persistentCamera = null;
                ClearPending();
                return;
            }
            if (!IsTransitioning || scene.name != targetScene) return;
            GameObject player = persistentPlayer;
            // Respect a destination scene that supplies its own player/camera.
            foreach (PlayerController candidate in FindObjectsByType<PlayerController>())
            {
                if (candidate.gameObject.scene != scene) continue;
                if (player != null && player != candidate.gameObject) Destroy(player);
                player = candidate.gameObject;
                persistentPlayer = player;
                break;
            }
            foreach (Camera camera in FindObjectsByType<Camera>())
            {
                if (camera.gameObject.scene != scene || !camera.CompareTag("MainCamera")) continue;
                if (persistentCamera != null && persistentCamera != camera.gameObject) Destroy(persistentCamera);
                persistentCamera = camera.gameObject;
                break;
            }
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
                if (persistentCamera != null)
                {
                    CameraFollow follow = persistentCamera.GetComponent<CameraFollow>();
                    if (follow != null) follow.SetTarget(player.transform);
                    persistentCamera.transform.position = new Vector3(player.transform.position.x,
                        player.transform.position.y + 2f, persistentCamera.transform.position.z);
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
        public bool HasPlacedPlayer(Scene scene) =>
            placedSceneHandle != SceneHandle.None && placedSceneHandle == scene.handle;
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }
    }
}
