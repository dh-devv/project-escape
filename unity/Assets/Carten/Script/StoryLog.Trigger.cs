using TMPro;
using UnityEngine;

namespace Carten
{
    /// <summary>
    /// 연구 일지 앞에 접근한 뒤 F 키를 눌렀을 때 스토리 UI를 표시합니다.
    /// 문 이동용 트리거와는 별도로 사용합니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StoryLogTrigger : MonoBehaviour
    {
        [Header("일지 정보")]
        [SerializeField] private string storyId = "Stage1_Log_01";
        [SerializeField] private string storyTitle = "연구 일지 01";
        [TextArea(4, 12)]
        [SerializeField] private string storyContent = "이곳에 연구 일지 내용을 입력하세요.";

        [Header("상호작용 설정")]
        [SerializeField] private bool showOnlyOnce = true;
        [SerializeField] private string interactionPromptText = "F  연구 일지 읽기";
        [Tooltip("선택 사항입니다. Canvas 아래에 만든 TextMeshPro 텍스트를 연결하면 가까이 왔을 때 안내 문구가 표시됩니다.")]
        [SerializeField] private TMP_Text interactionPrompt;

        private bool playerInRange;
        private bool hasShownOnce;
        private StoryLogUI storyUI;

        private void Reset()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void Awake()
        {
            storyUI = FindFirstObjectByType<StoryLogUI>();

            if (interactionPrompt != null)
            {
                interactionPrompt.text = interactionPromptText;
                interactionPrompt.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!playerInRange)
                return;

            if (storyUI == null)
                storyUI = FindFirstObjectByType<StoryLogUI>();

            bool alreadyRead = HasAlreadyBeenRead();
            bool canShowPrompt = !alreadyRead && (storyUI == null || !storyUI.IsOpen);

            if (interactionPrompt != null && interactionPrompt.gameObject.activeSelf != canShowPrompt)
                interactionPrompt.gameObject.SetActive(canShowPrompt);

            if (!canShowPrompt || !Input.GetKeyDown(KeyCode.F))
                return;

            if (storyUI == null)
            {
                Debug.LogError("[StoryLogTrigger] StoryLogUI를 찾지 못했습니다. 활성화된 Canvas에 StoryLogUI를 추가하고 UI 참조를 연결하세요.", this);
                return;
            }

            if (!storyUI.Show(storyTitle, storyContent))
                return;

            if (showOnlyOnce)
            {
                hasShownOnce = true;
                GameStateManager stateManager = GameStateManager.Instance;
                if (stateManager != null)
                    stateManager.SetTriggered(storyId);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                playerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;

                if (interactionPrompt != null)
                    interactionPrompt.gameObject.SetActive(false);
            }
        }

        private bool HasAlreadyBeenRead()
        {
            if (!showOnlyOnce)
                return false;

            if (hasShownOnce)
                return true;

            GameStateManager stateManager = GameStateManager.Instance;
            return stateManager != null && stateManager.HasTriggered(storyId);
        }
    }
}
