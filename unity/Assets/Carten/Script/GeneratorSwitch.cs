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

        [Header("스프라이트(이미지) 변경 설정")]
        [Tooltip("꺼져 있을 때의 기본 스프라이트 (비워두면 현재 SpriteRenderer의 이미지를 자동 사용)")]
        [SerializeField] private Sprite offSprite;
        [Tooltip("(스위치 전용, 선택사항) 발전기는 켜졌지만 스위치는 아직 안 눌렀을 때 보여줄 스프라이트")]
        [SerializeField] private Sprite standbySprite;
        [Tooltip("F를 눌러 완전히 작동시켰을 때 바뀔 스프라이트!")]
        [SerializeField] private Sprite onSprite;

        [Header("전원을 공급할 조명 오브젝트")]
        [Tooltip("작동 시 활성화할 Light 2D가 붙은 GameObject들을 넣으세요.")]
        [SerializeField] private GameObject[] lightsToPower;

        [Header("상호작용 안내 (비워두어도 자동으로 찾음)")]
        [SerializeField] private TMP_Text interactionPrompt;
        [SerializeField] private string promptText = "F  작동";
        [SerializeField] private string noPowerPromptText = "전원이 꺼져 있습니다 (발전기 가동 필요)";

        private SpriteRenderer spriteRenderer;
        private bool playerInRange;
        private bool isPowered;
        private bool lastPrerequisiteState;

        // 에디터에서 게임을 처음 재생할 때 한 번만 이전 PlayerPrefs 캐시를 깨끗하게 지워줍니다. (씬 이동 시에는 안 지워짐!)
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
                PlayerPrefs.DeleteAll(); // 게임 최초 시작 시에만 이전 테스트 기록 초기화
            }

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && offSprite == null)
            {
                offSprite = spriteRenderer.sprite;
            }

            // SpriteRenderer가 없는 투명 오브젝트(예: LightReceive)는 자동으로 수신 전용으로 취급
            if (spriteRenderer == null && lightsToPower != null && lightsToPower.Length > 0 && string.IsNullOrWhiteSpace(requiredGeneratorId))
            {
                // 만약 직접 상호작용하지 않는 수신기라면 인스펙터에서 isReceiverOnly를 켜거나 자동으로 동작
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
            // 아직 안 켜졌더라도 다른 곳에서 전원이 들어왔는지 확인 (LightReceive 연동 및 스위치 대기 이미지 갱신)
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

            // LightReceive처럼 조명만 켜주는 수신 전용 오브젝트이거나 이미 켜졌으면 안내 문구를 띄우지 않음
            if (isReceiverOnly || !playerInRange || isPowered)
                return;

            if (interactionPrompt == null)
                interactionPrompt = FindPromptText();

            bool hasGeneratorPower = CheckPrerequisitePower();

            if (interactionPrompt != null)
            {
                // 글씨가 잘리지 않게 텍스트 박스 크기 자동 확보
                if (interactionPrompt.rectTransform.sizeDelta.x < 600f)
                    interactionPrompt.rectTransform.sizeDelta = new Vector2(600f, 50f);

                if (!interactionPrompt.gameObject.activeSelf)
                    interactionPrompt.gameObject.SetActive(true);

                // 발전기가 켜졌으면 "F 작동", 안 켜졌으면 "전원이 꺼져 있습니다 (발전기 가동 필요)" 표시
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
            // 1. 조명 켜기/끄기
            if (lightsToPower != null)
            {
                foreach (GameObject lightObject in lightsToPower)
                {
                    if (lightObject != null)
                        lightObject.SetActive(isPowered);
                }
            }

            // 2. 발전기 및 스위치 스프라이트(이미지) 변경!
            if (spriteRenderer != null)
            {
                if (isPowered)
                {
                    // 작동 완료되었을 때 이미지
                    if (onSprite != null)
                        spriteRenderer.sprite = onSprite;
                }
                else
                {
                    // 아직 작동 안 시켰을 때: 선행 발전기가 켜졌고 대기 이미지(standbySprite)가 있다면 교체, 아니면 꺼진 이미지(offSprite)
                    bool hasGeneratorPower = CheckPrerequisitePower();
                    if (hasGeneratorPower && standbySprite != null)
                        spriteRenderer.sprite = standbySprite;
                    else if (offSprite != null)
                        spriteRenderer.sprite = offSprite;
                }
            }
        }
    }
}
