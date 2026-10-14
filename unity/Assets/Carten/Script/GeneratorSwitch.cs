using TMPro;
using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(Collider2D))]
    public class GeneratorSwitch : MonoBehaviour
    {
        [Header("자체 ID 및 기본 설정")]
        [Tooltip("이 오브젝트(발전기 또는 스위치)의 고유 ID입니다.")]
        [SerializeField] private string powerId = "Stage1_Generator_01";
        [SerializeField] private bool startPowered = false;
        [SerializeField] private KeyCode interactKey = KeyCode.F;

        [Header("선행 발전기 조건 (스위치일 경우에만 입력)")]
        [Tooltip("비워두면 조건 없이 바로 작동(발전기)하고, ID를 적으면 해당 발전기가 켜졌을 때만 작동(스위치)합니다.")]
        [SerializeField] private string requiredGeneratorId = "";

        [Header("전원을 공급할 조명 오브젝트")]
        [Tooltip("작동 시 활성화할 Light 2D가 붙은 GameObject들을 넣으세요.")]
        [SerializeField] private GameObject[] lightsToPower;

        [Header("상호작용 안내 (비워두어도 자동으로 찾음)")]
        [SerializeField] private TMP_Text interactionPrompt;
        [SerializeField] private string promptText = "F  작동";
        [SerializeField] private string noPowerPromptText = "전원이 꺼져 있습니다 (발전기 가동 필요)";

        // 씬 내의 모든 발전기/스위치가 공유하는 단일 프롬프트 캐시
        private static TMP_Text sharedPrompt;

        private bool playerInRange;
        private bool isPowered;

        private void Reset()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void Awake()
        {
            isPowered = startPowered;

            GameStateManager stateManager = GameStateManager.Instance;
            if (stateManager != null && !string.IsNullOrWhiteSpace(powerId) && stateManager.HasTriggered(powerId))
                isPowered = true;

            ApplyPowerState();

            // 인스펙터에 직접 연결하지 않았다면 씬에서 "InteractionPrompt"를 자동으로 찾습니다.
            if (interactionPrompt == null)
            {
                interactionPrompt = FindSharedPrompt();
            }

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
        }

        /// <summary>
        /// 씬 안에 꺼져 있는 오브젝트까지 포함하여 "InteractionPrompt"를 딱 한 번 찾아 공유합니다.
        /// </summary>
        private TMP_Text FindSharedPrompt()
        {
            if (sharedPrompt != null)
                return sharedPrompt;

            TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text text in allTexts)
            {
                if (text.name == "InteractionPrompt")
                {
                    sharedPrompt = text;
                    return sharedPrompt;
                }
            }

            return null;
        }

        private void Update()
        {
            // 플레이어가 내 범위 밖이거나 이미 작동된 상태라면 다른 오브젝트의 안내 문구를 건드리지 않음
            if (!playerInRange || isPowered)
                return;

            bool hasGeneratorPower = CheckPrerequisitePower();

            if (interactionPrompt != null)
            {
                if (!interactionPrompt.gameObject.activeSelf)
                    interactionPrompt.gameObject.SetActive(true);

                interactionPrompt.text = hasGeneratorPower ? promptText : noPowerPromptText;
            }

            if (hasGeneratorPower && Input.GetKeyDown(interactKey))
            {
                TurnOnPower();
            }
        }

        private bool CheckPrerequisitePower()
        {
            if (string.IsNullOrWhiteSpace(requiredGeneratorId))
                return true;

            GameStateManager stateManager = GameStateManager.Instance;
            if (stateManager != null)
                return stateManager.HasTriggered(requiredGeneratorId);

            return false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                playerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerInRange = false;

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
        }

        private void TurnOnPower()
        {
            if (isPowered)
                return;

            isPowered = true;
            ApplyPowerState();

            GameStateManager stateManager = GameStateManager.Instance;
            if (stateManager != null && !string.IsNullOrWhiteSpace(powerId))
                stateManager.SetTriggered(powerId);

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);

            Debug.Log($"[GeneratorSwitch] 작동 완료: {powerId}", this);
        }

        private void ApplyPowerState()
        {
            if (lightsToPower == null)
                return;

            foreach (GameObject lightObject in lightsToPower)
            {
                if (lightObject != null)
                    lightObject.SetActive(isPowered);
            }
        }
    }
}
