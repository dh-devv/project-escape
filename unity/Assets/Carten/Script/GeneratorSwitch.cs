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

        [Header("수신 전용 설정 (LightReceive 같은 조명 수신기용)")]
        [Tooltip("체크하면 플레이어가 F로 직접 켤 수 없고, 다른 씬의 스위치가 켜졌을 때 조명만 자동으로 켜주는 수신기 역할을 합니다.")]
        [SerializeField] private bool isReceiverOnly = false;

        [Header("선행 발전기 조건 (스위치일 경우에만 입력)")]
        [Tooltip("비워두면 조건 없이 바로 작동(발전기)하고, ID를 적으면 해당 발전기가 켜졌을 때만 작동(스위치)합니다.")]
        [SerializeField] private string requiredGeneratorId = "";

        [Header("전원을 공급할 조명 및 이펙트 오브젝트")]
        [Tooltip("작동 시 활성화할 Light 2D나 빛나는 파티클 등의 GameObject들을 넣으세요.")]
        [SerializeField] private GameObject[] lightsToPower;

        [Header("상호작용 안내 (비워두어도 자동으로 찾음)")]
        [SerializeField] private TMP_Text interactionPrompt;
        [SerializeField] private string promptText = "F  작동";
        [SerializeField] private string noPowerPromptText = "전원이 꺼져 있습니다 (발전기 가동 필요)";

        private bool playerInRange;
        private bool isPowered;
        private bool lastPrerequisiteState;

        private static bool sessionInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetSessionFlag()
        {
            sessionInitialized = false;
        }

        private void Reset()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void Awake()
        {
            if (!sessionInitialized)
            {
                sessionInitialized = true;
                PlayerPrefs.DeleteAll();
            }

            CheckSavedPowerState();
            ApplyPowerState();

            if (interactionPrompt == null)
            {
                interactionPrompt = FindPromptText();
            }

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
        }

        private void Start()
        {
            CheckSavedPowerState();
            lastPrerequisiteState = CheckPrerequisitePower();
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

        private TMP_Text FindPromptText()
        {
            TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include);
            foreach (TMP_Text text in allTexts)
            {
                if (text.name == "InteractionPrompt")
                    return text;
            }
            return null;
        }

        private void Update()
        {
            if (!isPowered)
            {
                CheckSavedPowerState();
                bool currentPrereq = CheckPrerequisitePower();
                if (isPowered || currentPrereq != lastPrerequisiteState)
                {
                    lastPrerequisiteState = currentPrereq;
                    ApplyPowerState();
                }
            }

            if (isReceiverOnly || !playerInRange || isPowered)
                return;

            if (interactionPrompt == null)
                interactionPrompt = FindPromptText();

            bool hasGeneratorPower = CheckPrerequisitePower();

            if (interactionPrompt != null)
            {
                if (interactionPrompt.rectTransform.sizeDelta.x < 600f)
                    interactionPrompt.rectTransform.sizeDelta = new Vector2(600f, 50f);

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
            if (isReceiverOnly)
                return;

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

            GameStateManager stateManager = GameStateManager.Instance;
            if (stateManager != null && !string.IsNullOrWhiteSpace(powerId))
                stateManager.SetTriggered(powerId);

            if (!string.IsNullOrWhiteSpace(powerId))
            {
                PlayerPrefs.SetInt(powerId, 1);
                PlayerPrefs.Save();
            }

            ApplyPowerState();

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);

            Debug.Log($"[GeneratorSwitch] 작동 완료: {powerId}", this);
        }

        private void ApplyPowerState()
        {
            // 연결된 조명 및 이펙트 오브젝트 켜기/끄기
            if (lightsToPower != null)
            {
                foreach (GameObject lightObject in lightsToPower)
                {
                    if (lightObject != null)
                        lightObject.SetActive(isPowered);
                }
            }
        }
    }
}
