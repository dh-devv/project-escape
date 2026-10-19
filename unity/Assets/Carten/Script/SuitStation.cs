using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(Collider2D))]
    public class SuitStation : MonoBehaviour
    {
        [Header("=== 회복 설정 ===")]
        [SerializeField] private float healAmount = 100f; // 회복량 (넉넉하게 설정 시 풀회복)
        [SerializeField] private KeyCode interactKey = KeyCode.F; // 상호작용 키

        [Header("=== 시각 연출 (선택사항) ===")]
        [SerializeField] private GameObject healEffectLight; // 작동 시 켜질 조명이나 이펙트

        private PlayerController nearbyPlayer;

        private void Awake()
        {
            // 충돌체가 트리거(통과 가능)로 작동하도록 자동 설정
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Update()
        {
            // 플레이어가 거치대 앞에 있고, 살아있으며, F키를 눌렀을 때
            if (nearbyPlayer != null && !nearbyPlayer.IsDead && Input.GetKeyDown(interactKey))
            {
                RepairSuit();
            }
        }

        private void RepairSuit()
        {
            // ★ PlayerController에 있는 정식 Heal 함수 호출! (체력 회복 + 페이즈/애니메이터 복구 + HUD 갱신까지 자동 처리됨)
            nearbyPlayer.Heal(healAmount);

            Debug.Log($"[SuitStation] 슈트 수리 및 체력 회복 완료! (+{healAmount})");

            if (healEffectLight != null)
            {
                healEffectLight.SetActive(true);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                nearbyPlayer = player;
                Debug.Log("[SuitStation] 슈트 거치대 접근: [F] 키를 눌러 슈트를 수리하세요.");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null && nearbyPlayer == player)
            {
                nearbyPlayer = null;
            }
        }
    }
}
