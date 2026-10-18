using TMPro;
using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(Collider2D))]
    public class GeneratorSwitch : MonoBehaviour
    {
        [Header("자체 ID 및 기본 설정")]
        [Tooltip("이 오브젝트(발전기 또는 스위치)의 고유 ID입니다. 다른 씬의 조명과 연동하려면 같은 ID를 쓰세요.")]
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
            PlayerPrefs.DeleteKey(powerId);

            CheckSavedPowerState();
            ApplyPowerState();

            if (interactionPrompt == null)
            {
                interactionPrompt = FindSharedPrompt();
            }

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
        }

        private void Start()
        {
            // ★ 다른 씬에서 넘어왔을 때 GameStateManager 및 PlayerPrefs 상태를 한 번 더 확인해 조명을 자동으로 켭니다.
            CheckSavedPowerState();
            ApplyPowerState();
        }

        private void CheckSavedPowerState()
        {
            if (startPowered)
            {
                isPowered = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(powerId))
                return;

            GameStateManager stateManager = GameStateManager.Instance;
            if (stateManager != null && stateManager.HasTriggered(powerId))
            {
                isPowered = true;
                return;
            }

            if (PlayerPrefs.GetInt(powerId, 0) == 1)
            {
                isPowered = true;
                if (stateManager != null)
                    stateManager.SetTriggered(powerId);
            }
        }

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
            if (stateManager != null && stateManager.HasTriggered(requiredGeneratorId))
                return true;

            if (PlayerPrefs.GetInt(requiredGeneratorId, 0) == 1)
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

        private void TurnOnPower()
        {
            if (isPowered)
                return;

            isPowered = true;
            ApplyPowerState();

            GameStateManager stateManager = GameStateManager.Instance;
            if (stateManager != null && !string.IsNullOrWhiteSpace(powerId))
                stateManager.SetTriggered(powerId);

            if (!string.IsNullOrWhiteSpace(powerId))
            {
                PlayerPrefs.SetInt(powerId, 1);
                PlayerPrefs.Save();
            }

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
