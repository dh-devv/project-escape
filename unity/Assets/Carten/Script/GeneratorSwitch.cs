using System.Collections;
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
        [Tooltip("Spot Light 2D, Light_Room_01 등 발전기가 켜질 때 함께 작동할 GameObject들을 모두 등록하세요.")]
        [SerializeField] private GameObject[] lightsToPower;

        [Header("전력 가동 연출 설정")]
        [Tooltip("체크하면 발전기를 켤 때 조명이 치직거리며 켜지는 깜빡임 연출이 들어갑니다.")]
        [SerializeField] private bool useFlickerEffect = true;

        [Header("테스트 설정")]
        [Tooltip("체크하면 게임 실행 시 이전 저장 기록을 지우고 꺼진 상태로 강제 시작합니다 (테스트용).")]
        [SerializeField] private bool resetPowerStateOnStart = false;

        [Header("상호작용 안내 (비워두어도 자동으로 찾음)")]
        [SerializeField] private TMP_Text interactionPrompt;
        [SerializeField] private string promptText = "F  작동";
        [SerializeField] private string noPowerPromptText = "전원이 꺼져 있습니다 (발전기 가동 필요)";

        private bool playerInRange;
        private bool isPowered;
        private bool lastPrerequisiteState;

        private void Reset()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        private void Awake()
        {
            if (resetPowerStateOnStart && !string.IsNullOrWhiteSpace(powerId))
            {
                PlayerPrefs.DeleteKey(powerId);
            }

            CheckSavedPowerState();

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
            ApplyPowerState(animate: false);
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
                bool currentPrereq = CheckPrerequisitePower();
                if (currentPrereq != lastPrerequisiteState)
                {
                    lastPrerequisiteState = currentPrereq;
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

            ApplyPowerState(animate: useFlickerEffect);

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);

            Debug.Log($"[GeneratorSwitch] 발전기 가동 완료: {powerId}", this);
        }

        private void ApplyPowerState(bool animate = false)
        {
            if (lightsToPower == null) return;

            if (animate && isPowered && gameObject.activeInHierarchy)
            {
                StartCoroutine(FlickerPowerOnRoutine());
            }
            else
            {
                SetLightsActive(isPowered);
            }
        }

        private IEnumerator FlickerPowerOnRoutine()
        {
            for (int i = 0; i < 3; i++)
            {
                SetLightsActive(true);
                yield return new WaitForSeconds(Random.Range(0.04f, 0.08f));
                SetLightsActive(false);
                yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
            }
            SetLightsActive(true);
        }

        private void SetLightsActive(bool active)
        {
            if (lightsToPower == null) return;
            foreach (GameObject lightObject in lightsToPower)
            {
                if (lightObject != null)
                    lightObject.SetActive(active);
            }
        }

        // 인스펙터 스크립트 컴포넌트 우클릭 메뉴로 저장 상태 초기화 가능
        [ContextMenu("Reset Power State (Delete Key)")]
        public void ResetPowerStateInEditor()
        {
            if (!string.IsNullOrWhiteSpace(powerId))
            {
                PlayerPrefs.DeleteKey(powerId);
                Debug.Log($"[GeneratorSwitch] {powerId} 키가 초기화되었습니다.");
            }
        }
    }
}
