using UnityEngine;

namespace Carten
{
    public class Stage1Manager : MonoBehaviour
    {
        [Header("=== Player ===")]
        [SerializeField] private GameObject player;
        [SerializeField] private Transform playerStart;

        [Header("=== Enemy ===")]
        [SerializeField] private EnemySpawner enemySpawner;

        [SerializeField] private int firstEncounterEnemyCount = 2;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;


        private void Start()
        {
            InitializeStage();
        }


        // ========================================================
        // Stage 1 초기화
        // ========================================================

        private void InitializeStage()
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawner>();
            }

            if (player == null)
            {
                Debug.LogError(
                    "[Stage1Manager] Player를 찾을 수 없습니다."
                );

                return;
            }

            if (playerStart == null)
            {
                Debug.LogError(
                    "[Stage1Manager] PlayerStart가 설정되지 않았습니다."
                );

                return;
            }

            if (enemySpawner == null)
            {
                Debug.LogError(
                    "[Stage1Manager] EnemySpawner를 찾을 수 없습니다."
                );

                return;
            }

            // Player 위치 초기화
            player.transform.position = playerStart.position;

            // Player Rigidbody 속도 초기화
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
                    "[Stage1Manager] Player 시작 위치 설정 완료"
                );

                Debug.Log(
                    $"[Stage1Manager] 시작 위치: {playerStart.position}"
                );
            }

            // 첫 번째 전투 시작
            StartFirstEncounter();
        }


        // ========================================================
        // 첫 번째 전투
        // ========================================================

        private void StartFirstEncounter()
        {
            if (showDebugLog)
            {
                Debug.Log(
                    "[Stage1Manager] Stage 1 첫 번째 전투 시작"
                );
            }

            for (int i = 0; i < firstEncounterEnemyCount; i++)
            {
                enemySpawner.SpawnEnemy1(i);
            }

            if (showDebugLog)
            {
                Debug.Log(
                    $"[Stage1Manager] Enemy1 {firstEncounterEnemyCount}마리 소환"
                );
            }
        }
    }
}