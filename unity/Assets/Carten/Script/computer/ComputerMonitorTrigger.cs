using UnityEngine;
using TMPro;

namespace Carten
{
    [RequireComponent(typeof(Collider2D))]
    public class ComputerMonitorTrigger : MonoBehaviour
    {
        [Header("=== 모니터 내용 설정 ===")]
        [SerializeField] private string monitorTitle = "개발 일지 01";
        [TextArea(4, 12)]
        [SerializeField] private string monitorContent = "여기에 모니터에 표시될 내용을 입력하세요.";

        [Header("=== 발전기/스위치 연동 설정 ===")]
        [Tooltip("3번 씬의 발전기(Stage1_Generator_01) 또는 조명 스위치(Stage1_Light_All)의 ID를 적으세요.")]
        [SerializeField] private string requiredPowerId = "Stage1_Light_All";
        [SerializeField] private string powerOffPrompt = "전력이 들어오지 않는다";
        [SerializeField] private string powerOnPrompt = "[F] 모니터 확인하기";

        [Header("=== 모니터 스프라이트 변경 ===")]
        [Tooltip("꺼져 있을 때의 모니터 이미지 (비워두면 현재 이미지 자동 사용)")]
        [SerializeField] private Sprite powerOffSprite;
        [Tooltip("전력이 들어왔을 때 바뀔 '불 켜진 모니터' 스프라이트를 넣으세요!")]
        [SerializeField] private Sprite powerOnSprite;

        [Header("=== UI 연결 (비워두어도 자동으로 찾음) ===")]
        [SerializeField] private TMP_Text interactionPrompt;

        private SpriteRenderer spriteRenderer;
        private bool playerInRange;
        private ComputerMonitorUI monitorUI;
        private bool lastPowerState;

        private void Reset()
        {
            Collider2D triggerCol = GetComponent<Collider2D>();
            if (triggerCol != null)
                triggerCol.isTrigger = true;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && powerOffSprite == null)
            {
                powerOffSprite = spriteRenderer.sprite;
            }

            monitorUI = Object.FindAnyObjectByType<ComputerMonitorUI>();

            if (interactionPrompt == null)
                interactionPrompt = FindPromptText();

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
        }

        private void Start()
        {
            lastPowerState = CheckPowerState();
            UpdateMonitorVisuals(lastPowerState);
        }

        private TMP_Text FindPromptText()
        {
            TMP_Text[] allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text text in allTexts)
            {
                if (text.name == "InteractionPrompt")
                    return text;
            }
            return null;
        }

        private void Update()
        {
            bool isPowered = CheckPowerState();

            // 전력 상태가 바뀌면 즉시 모니터 이미지 교체
            if (isPowered != lastPowerState)
            {
                lastPowerState = isPowered;
                UpdateMonitorVisuals(isPowered);
            }

            if (!playerInRange)
                return;

            if (monitorUI == null)
                monitorUI = Object.FindAnyObjectByType<ComputerMonitorUI>();

            if (interactionPrompt == null)
                interactionPrompt = FindPromptText();

            if (interactionPrompt != null)
            {
                if (interactionPrompt.rectTransform.sizeDelta.x < 600f)
                    interactionPrompt.rectTransform.sizeDelta = new Vector2(600f, 50f);

                // ★ 전기가 안 들어왔을 땐 "전력이 들어오지 않는다", 들어왔을 땐 "[F] 모니터 확인하기" 표시!
                interactionPrompt.text = isPowered ? powerOnPrompt : powerOffPrompt;

                bool canShowPrompt = (monitorUI == null || !monitorUI.IsOpen);
                if (interactionPrompt.gameObject.activeSelf != canShowPrompt)
                    interactionPrompt.gameObject.SetActive(canShowPrompt);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!isPowered)
                {
                    Debug.Log($"[ComputerMonitor] 전력이 들어오지 않습니다. (필요한 ID: {requiredPowerId})");
                    return;
                }

                if (monitorUI != null)
                {
                    monitorUI.Show(monitorTitle, monitorContent);
                }
            }
        }

        private void UpdateMonitorVisuals(bool isPowered)
        {
            if (spriteRenderer == null)
                return;

            if (isPowered && powerOnSprite != null)
                spriteRenderer.sprite = powerOnSprite;
            else if (!isPowered && powerOffSprite != null)
                spriteRenderer.sprite = powerOffSprite;
        }

        private bool CheckPowerState()
        {
            if (string.IsNullOrWhiteSpace(requiredPowerId))
                return true;

            GameStateManager stateManager = GameStateManager.Instance;
            if (stateManager != null && stateManager.HasTriggered(requiredPowerId))
                return true;

            if (PlayerPrefs.GetInt(requiredPowerId, 0) == 1)
                return true;

            return false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
                playerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerController>() == null)
                return;

            playerInRange = false;

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
        }
    }
}
