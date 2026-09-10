using UnityEngine;

namespace Carten
{
    public class EncounterTrigger : MonoBehaviour
    {
        [SerializeField] private EncounterController encounterController;

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

            // 이미 시작했거나 클리어된 Encounter라면 다시 시작하지 않음
            if (encounterController.IsStarted())
                return;

            if (encounterController.IsCleared())
                return;

            encounterController.StartEncounter();
        }
    }
}