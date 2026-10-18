using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    [RequireComponent(typeof(Collider2D))]
    public class ConditionalDoor : MonoBehaviour
    {
        private const string SpawnPointPrefKey = "LastTargetSpawnPoint";
        private static bool sceneHookRegistered;

        [Header("=== 이동할 목적지 설정 ===")]
        [SerializeField] private string targetSceneName = "Stage1_Map04";
        [SerializeField] private string targetSpawnPointName = "Spawn_FromMap02";

        [Header("=== 잠금 해제 조건 설정 ===")]
        [Tooltip("비워두면 조건 없이 항상 열리는 일반 문으로 작동합니다.")]
        [SerializeField] private string requiredConditionID = "Stage1_Generator_01";
        [SerializeField] private string lockedMessage = "전력 공급이 차단되어 격벽이 잠겼습니다. 지하 발전실(Map03)을 먼저 가동하세요.";

        [Header("=== 조작 및 시각 연출 ===")]
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [SerializeField] private GameObject lockedRedLight;
        [SerializeField] private GameObject unlockedGreenLight;

        private bool isPlayerNearby = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHook()
        {
            if (sceneHookRegistered) return;
            SceneManager.sceneLoaded += OnSceneLoadedMovePlayer;
            sceneHookRegistered = true;
        }

        private static void OnSceneLoadedMovePlayer(Scene scene, LoadSceneMode mode)
        {
            if (!PlayerPrefs.HasKey(SpawnPointPrefKey)) return;

            string spawnName = PlayerPrefs.GetString(SpawnPointPrefKey, "");
            PlayerPrefs.DeleteKey(SpawnPointPrefKey);

            if (string.IsNullOrWhiteSpace(spawnName)) return;

            GameObject spawnObj = GameObject.Find(spawnName);
            if (spawnObj == null) return;

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                player.transform.position = spawnObj.transform.position;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = spawnObj.transform.position;
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }

        private void Awake()
        {
            RegisterSceneHook();
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Start()
        {
            UpdateDoorVisuals();
        }

        private void Update()
        {
            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                TryUseDoor();
            }
        }

        private void TryUseDoor()
        {
            if (IsDoorUnlocked())
            {
                Debug.Log($"[ConditionalDoor] 격벽 개방 완료! {targetSceneName} 씬으로 이동합니다.");

                PlayerPrefs.SetString(SpawnPointPrefKey, targetSpawnPointName);
                PlayerPrefs.Save();

                // ★ SceneTransitionManager를 통한 페이드 전환 적용
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadSceneWithFade(targetSceneName);
                }
                else
                {
                    SceneManager.LoadScene(targetSceneName);
                }
            }
            else
            {
                Debug.LogWarning($"[보안 격벽 잠김] {lockedMessage} (필요 조건: {requiredConditionID})");
            }
        }

        public bool IsDoorUnlocked()
        {
            if (string.IsNullOrEmpty(requiredConditionID)) return true;

            if (GameStateManager.Instance != null && GameStateManager.Instance.HasTriggered(requiredConditionID))
                return true;

            if (PlayerPrefs.GetInt(requiredConditionID, 0) == 1)
                return true;

            return false;
        }

        public void UpdateDoorVisuals()
        {
            bool unlocked = IsDoorUnlocked();
            if (lockedRedLight != null) lockedRedLight.SetActive(!unlocked);
            if (unlockedGreenLight != null) unlockedGreenLight.SetActive(unlocked);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                isPlayerNearby = true;
                UpdateDoorVisuals();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                isPlayerNearby = false;
            }
        }
    }
}
