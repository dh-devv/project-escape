using TMPro;
using UnityEngine;

namespace Carten
{
    /// <summary>
    /// 플레이어가 가까이에서 F 키를 누르면 발전기를 켜고 연결된 조명 오브젝트를 활성화합니다.
    /// Light2D 타입을 직접 참조하지 않으므로 URP 2D 어셈블리 참조 문제를 피할 수 있습니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GeneratorSwitch : MonoBehaviour
    {
        [Header("발전기 설정")]
        [SerializeField] private string powerId = "Stage1_Generator_01";
        [SerializeField] private bool startPowered = false;
        [SerializeField] private KeyCode interactKey = KeyCode.F;

        [Header("전원을 공급할 조명 오브젝트")]
        [Tooltip("Light 2D 컴포넌트가 붙은 GameObject 자체를 넣으세요.")]
        [SerializeField] private GameObject[] lightsToPower;

        [Header("상호작용 안내 (선택 사항)")]
        [SerializeField] private TMP_Text interactionPrompt;
        [SerializeField] private string promptText = "F  발전기 작동";

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

            if (interactionPrompt != null)
            {
                interactionPrompt.text = promptText;
                interactionPrompt.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            bool canInteract = playerInRange && !isPowered;

            if (interactionPrompt != null && interactionPrompt.gameObject.activeSelf != canInteract)
                interactionPrompt.gameObject.SetActive(canInteract);

            if (!canInteract || !Input.GetKeyDown(interactKey))
                return;

            TurnOnGenerator();
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

        private void TurnOnGenerator()
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

            Debug.Log($"[GeneratorSwitch] 발전기 작동 완료: {powerId}", this);
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
