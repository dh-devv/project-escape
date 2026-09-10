using System.Collections;
using UnityEngine;

namespace Carten
{
    public class WaveManager : MonoBehaviour
    {
        // ========================================================
        // WaveManager
        // ========================================================

        [Header("=== References ===")]
        [SerializeField] private EnemySpawner enemySpawner;

        [Header("=== Wave Settings ===")]
        [SerializeField] private float nextWaveDelay = 2f;

        [SerializeField] private bool startAutomatically = true;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;


        // ========================================================
        // Wave Data
        // ========================================================

        [System.Serializable]
        public class WaveData
        {
            public int enemy1Count;
            public int enemy2Count;
            public int enemy3Count;

            public WaveData(
                int enemy1,
                int enemy2,
                int enemy3)
            {
                enemy1Count = enemy1;
                enemy2Count = enemy2;
                enemy3Count = enemy3;
            }
        }


        // ========================================================
        // 현재 테스트용 Wave
        // ========================================================

        private readonly WaveData[] waves =
        {
            // Wave 1
            new WaveData(2, 0, 0),

            // Wave 2
            new WaveData(1, 1, 0),

            // Wave 3
            new WaveData(1, 1, 1)
        };


        // ========================================================
        // State
        // ========================================================

        private int currentWaveIndex = -1;

        private bool isWaveRunning = false;
        private bool stageClear = false;


        // ========================================================
        // Unity
        // ========================================================

        private void Start()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawner>();
            }

            if (enemySpawner == null)
            {
                Debug.LogError(
                    "[WaveManager] EnemySpawner를 찾을 수 없습니다."
                );

                return;
            }

            if (startAutomatically)
            {
                StartStage();
            }
        }


        private void Update()
        {
            if (!isWaveRunning)
                return;

            CheckWaveClear();
        }


        // ========================================================
        // Stage Start
        // ========================================================

        public void StartStage()
        {
            if (enemySpawner == null)
            {
                Debug.LogError(
                    "[WaveManager] EnemySpawner가 없습니다."
                );

                return;
            }

            currentWaveIndex = -1;
            stageClear = false;

            if (showDebugLog)
            {
                Debug.Log(
                    "[WaveManager] Stage 시작"
                );
            }

            StartNextWave();
        }


        // ========================================================
        // 다음 Wave
        // ========================================================

        private void StartNextWave()
        {
            currentWaveIndex++;

            // 모든 Wave 종료
            if (currentWaveIndex >= waves.Length)
            {
                StageClear();
                return;
            }

            WaveData wave = waves[currentWaveIndex];

            if (showDebugLog)
            {
                Debug.Log(
                    $"[WaveManager] Wave {currentWaveIndex + 1} 시작"
                );

                Debug.Log(
                    $"Enemy1: {wave.enemy1Count} / " +
                    $"Enemy2: {wave.enemy2Count} / " +
                    $"Enemy3: {wave.enemy3Count}"
                );
            }

            enemySpawner.SpawnWave(
                wave.enemy1Count,
                wave.enemy2Count,
                wave.enemy3Count
            );

            isWaveRunning = true;
        }


        // ========================================================
        // Wave 클리어 확인
        // ========================================================

        private void CheckWaveClear()
        {
            var spawnedEnemies = enemySpawner.GetSpawnedEnemies();

            if (spawnedEnemies == null ||
                spawnedEnemies.Count == 0)
            {
                return;
            }

            bool allEnemiesDead = true;

            for (int i = 0; i < spawnedEnemies.Count; i++)
            {
                GameObject enemyObject = spawnedEnemies[i];

                if (enemyObject == null)
                    continue;

                EnemyBase enemy =
                    enemyObject.GetComponent<EnemyBase>();

                if (enemy == null)
                    continue;

                // 살아있는 적이 하나라도 있으면
                if (!enemy.IsDead &&
                    enemyObject.activeSelf)
                {
                    allEnemiesDead = false;
                    break;
                }
            }

            if (allEnemiesDead)
            {
                WaveClear();
            }
        }


        // ========================================================
        // Wave 클리어
        // ========================================================

        private void WaveClear()
        {
            if (!isWaveRunning)
                return;

            isWaveRunning = false;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[WaveManager] Wave {currentWaveIndex + 1} 클리어"
                );
            }

            StartCoroutine(StartNextWaveAfterDelay());
        }


        // ========================================================
        // 다음 Wave 대기
        // ========================================================

        private IEnumerator StartNextWaveAfterDelay()
        {
            yield return new WaitForSeconds(nextWaveDelay);

            enemySpawner.ClearSpawnedEnemies();

            StartNextWave();
        }


        // ========================================================
        // Stage Clear
        // ========================================================

        private void StageClear()
        {
            if (stageClear)
                return;

            stageClear = true;
            isWaveRunning = false;

            if (showDebugLog)
            {
                Debug.Log(
                    "================================="
                );

                Debug.Log(
                    "[WaveManager] STAGE CLEAR!"
                );

                Debug.Log(
                    "================================="
                );
            }
        }


        // ========================================================
        // 외부 확인용
        // ========================================================

        public int GetCurrentWave()
        {
            return currentWaveIndex + 1;
        }


        public int GetTotalWaves()
        {
            return waves.Length;
        }


        public bool IsStageClear()
        {
            return stageClear;
        }


        public bool IsWaveRunning()
        {
            return isWaveRunning;
        }
    }
}