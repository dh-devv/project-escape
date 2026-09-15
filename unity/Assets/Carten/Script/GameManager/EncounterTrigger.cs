using UnityEngine;

namespace Carten
{
    public class EncounterTrigger : MonoBehaviour
    {
        [SerializeField] private EncounterController encounterController;

        [Header("=== One-Time Trigger ===")]
        [SerializeField] private string triggerId;

        private void Start()
        {
            if (encounterController == null)
            {
                encounterController =
                    GetComponentInParent<EncounterController>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (encounterController == null)
            {
                Debug.LogError(
                    "[EncounterTrigger] EncounterController를 찾을 수 없습니다."
                );

                return;
            }

            // GameStateManager가 없으면 기존 방식으로 동작
            if (GameStateManager.Instance != null &&
                !string.IsNullOrEmpty(triggerId))
            {
                // 이미 발동한 트리거라면 다시 실행하지 않음
                if (GameStateManager.Instance.HasTriggered(triggerId))
                    return;
            }

            // 이미 시작했거나 클리어된 Encounter라면 다시 시작하지 않음
            if (encounterController.IsStarted())
                return;

            if (encounterController.IsCleared())
                return;

            // Encounter 시작
            encounterController.StartEncounter();

            // 한 번 발동한 것으로 저장
            if (GameStateManager.Instance != null &&
                !string.IsNullOrEmpty(triggerId))
            {
                GameStateManager.Instance.SetTriggered(triggerId);
            }
        }
    }
}