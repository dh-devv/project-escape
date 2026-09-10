using UnityEngine;

namespace Carten
{
    public class Stage1Manager : MonoBehaviour
    {
        [Header("=== Player ===")]
        [SerializeField] private GameObject player;
        [SerializeField] private Transform playerStart;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private void Start()
        {
            InitializeStage();
        }

        private void InitializeStage()
        {
            // Player 자동 찾기
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            // Player 확인
            if (player == null)
            {
                Debug.LogError(
                    "[Stage1Manager] Player를 찾을 수 없습니다."
                );

                return;
            }

            // PlayerStart 확인
            if (playerStart == null)
            {
                Debug.LogError(
                    "[Stage1Manager] PlayerStart가 설정되지 않았습니다."
                );

                return;
            }

            // Player 시작 위치 설정
            player.transform.position = playerStart.position;

            // Rigidbody 초기화
            Rigidbody2D playerRb =
                player.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }

            if (showDebugLog)
            {
                Debug.Log(
                    "[Stage1Manager] Stage 1 초기화 완료"
                );

                Debug.Log(
                    $"[Stage1Manager] Player 시작 위치: {playerStart.position}"
                );
            }
        }
    }
}