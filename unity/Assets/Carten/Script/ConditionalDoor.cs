using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    [RequireComponent(typeof(Collider2D))]
    public class ConditionalDoor : MonoBehaviour
    {
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

        private void Awake()
        {
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

                // 다음 씬에서 플레이어가 스폰될 위치 이름 저장
                PlayerPrefs.SetString("LastTargetSpawnPoint", targetSpawnPointName);

                // 목적지 씬 로드 (기존 MapTransitionManager의 전환 함수가 있다면 그곳으로 연결 가능)
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                // 잠겨있을 때 안내 메시지 출력
                Debug.LogWarning($"[보안 격벽 잠김] {lockedMessage} (필요 조건: {requiredConditionID})");
            }
        }

        public bool IsDoorUnlocked()
        {
            // 조건 ID가 비어있으면 무조건 열림 (일반 문)
            if (string.IsNullOrEmpty(requiredConditionID)) return true;

            // PlayerPrefs를 통해 스위치/발전기 작동 여부 확인 (1이면 작동 완료)
            if (PlayerPrefs.GetInt(requiredConditionID, 0) == 1)
            {
                return true;
            }

            return false;
        }

        public void UpdateDoorVisuals()
        {
            bool unlocked = IsDoorUnlocked();
            if (lockedRedLight != null) lockedRedLight.SetActive(!unlocked);
            if (unlockedGreenLight != null) unlockedGreenLight.SetActive(unlocked);
        }

        // 테스트용: 인스펙터에서 컴포넌트 우클릭 메뉴로 잠금/해제 상태를 즉시 테스트할 수 있습니다.
        [ContextMenu("테스트: 조건 강제 달성 (문 열기)")]
        public void DebugUnlockDoor()
        {
            PlayerPrefs.SetInt(requiredConditionID, 1);
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
