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
        [SerializeField] private string targetSceneName = "Stage1_Map04"; // 이동할 씬 이름
        [SerializeField] private string targetSpawnPointName = "Spawn_FromMap02"; // 도착 위치 이름

        [Header("=== 잠금 해제 조건 설정 ===")]
        [Tooltip("비워두면 조건 없이 항상 열리는 일반 문으로 작동합니다.")]
        [SerializeField] private string requiredConditionID = "Stage1_Generator_01"; // 필요한 조건 ID
        [SerializeField] private string lockedMessage = "전력 공급이 차단되어 격벽이 잠겼습니다. 지하 발전실(Map03)을 먼저 가동하세요.";

        [Header("=== 조작 및 시각 연출 ===")]
        [SerializeField] private KeyCode interactKey = KeyCode.F; // 상호작용 키
        [SerializeField] private GameObject lockedRedLight;   // 잠겼을 때 켜질 붉은 조명/표시 (선택)
        [SerializeField] private GameObject unlockedGreenLight; // 열렸을 때 켜질 초록 조명/표시 (선택)

        private bool isPlayerNearby = false;

        // ★ 게임 시작 시 씬 이동 후 스폰 위치 이동 이벤트를 자동으로 등록합니다.
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
            if (spawnObj == null)
            {
                Debug.LogWarning($"[ConditionalDoor] '{scene.name}' 씬에서 스폰 위치 '{spawnName}' 오브젝트를 찾을 수 없습니다.");
                return;
            }

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
                Debug.Log($"[ConditionalDoor] 플레이어를 지정 스폰 위치 '{spawnName}'(으)로 이동시켰습니다.");
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
                Debug.Log($"[ConditionalDoor] 격벽 개방 완료! {targetSceneName} 씬({targetSpawnPointName})으로 이동합니다.");

                // 다음 씬에서 플레이어가 스폰될 위치 이름 저장
                PlayerPrefs.SetString(SpawnPointPrefKey, targetSpawnPointName);
                PlayerPrefs.Save();

                SceneManager.LoadScene(targetSceneName);
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

        [ContextMenu("테스트: 조건 강제 달성 (문 열기)")]
        public void DebugUnlockDoor()
        {
            PlayerPrefs.SetInt(requiredConditionID, 1);
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetTriggered(requiredConditionID);
            UpdateDoorVisuals();
            Debug.Log($"[테스트] '{requiredConditionID}' 조건이 달성 처리되었습니다! 이제 문이 열립니다.");
        }

        [ContextMenu("테스트: 조건 초기화 (문 잠그기)")]
        public void DebugLockDoor()
        {
            PlayerPrefs.SetInt(requiredConditionID, 0);
            UpdateDoorVisuals();
            Debug.Log($"[테스트] '{requiredConditionID}' 조건이 초기화되어 문이 다시 잠겼습니다.");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                isPlayerNearby = true;
                UpdateDoorVisuals();
                if (IsDoorUnlocked())
                    Debug.Log($"[문 접근] [{interactKey}] 키를 눌러 이동하세요.");
                else
                    Debug.Log($"[잠긴 격벽 접근] [{interactKey}] 키를 눌러 상태를 확인하세요.");
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
