using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class EnemySpawner : MonoBehaviour
    {
        // ========================================================
        // Enemy Prefabs
        // ========================================================

        [Header("=== Enemy Prefabs ===")]

        [SerializeField]
        private GameObject enemy1Prefab;

        [SerializeField]
        private GameObject enemy2Prefab;

        [SerializeField]
        private GameObject enemy3Prefab;


        // ========================================================
        // Spawn Points
        // ========================================================

        [Header("=== Spawn Points ===")]

        [SerializeField]
        private Transform[] spawnPoints;


        // ========================================================
        // Debug
        // ========================================================

        [Header("=== Debug ===")]

        [SerializeField]
        private bool showDebugLog = true;


        // ========================================================
        // Runtime
        // ========================================================

        private readonly List<GameObject> spawnedEnemies =
            new List<GameObject>();


        // ========================================================
        // Spawn Enemy1
        // ========================================================

        public GameObject SpawnEnemy1(int spawnPointIndex)
        {
            return SpawnEnemy(
                enemy1Prefab,
                spawnPointIndex,
                "Enemy1"
            );
        }


        // ========================================================
        // Spawn Enemy2
        // ========================================================

        public GameObject SpawnEnemy2(int spawnPointIndex)
        {
            return SpawnEnemy(
                enemy2Prefab,
                spawnPointIndex,
                "Enemy2"
            );
        }


        // ========================================================
        // Spawn Enemy3
        // ========================================================

        public GameObject SpawnEnemy3(int spawnPointIndex)
        {
            return SpawnEnemy(
                enemy3Prefab,
                spawnPointIndex,
                "Enemy3"
            );
        }


        // ========================================================
        // Generic Spawn
        // ========================================================

        private GameObject SpawnEnemy(
            GameObject prefab,
            int spawnPointIndex,
            string enemyName)
        {
            if (prefab == null)
            {
                Debug.LogError(
                    $"[EnemySpawner] {enemyName} Prefab이 설정되지 않았습니다."
                );

                return null;
            }

            if (spawnPoints == null ||
                spawnPoints.Length == 0)
            {
                Debug.LogError(
                    "[EnemySpawner] Spawn Point가 없습니다."
                );

                return null;
            }

            if (spawnPointIndex < 0 ||
                spawnPointIndex >= spawnPoints.Length)
            {
                Debug.LogError(
                    $"[EnemySpawner] 잘못된 Spawn Point 번호: " +
                    $"{spawnPointIndex}"
                );

                return null;
            }

            Transform spawnPoint =
                spawnPoints[spawnPointIndex];

            if (spawnPoint == null)
            {
                Debug.LogError(
                    $"[EnemySpawner] Spawn Point " +
                    $"{spawnPointIndex}가 비어 있습니다."
                );

                return null;
            }

            GameObject enemy =
                Instantiate(
                    prefab,
                    spawnPoint.position,
                    Quaternion.identity
                );

            spawnedEnemies.Add(enemy);

            if (showDebugLog)
            {
                Debug.Log(
                    $"[EnemySpawner] {enemyName} 생성 / " +
                    $"Spawn Point: {spawnPointIndex}"
                );
            }

            return enemy;
        }


        // ========================================================
        // Spawn Random Enemy
        // ========================================================

        public GameObject SpawnRandomEnemy(
            int spawnPointIndex)
        {
            int randomType =
                Random.Range(1, 4);

            switch (randomType)
            {
                case 1:
                    return SpawnEnemy1(
                        spawnPointIndex
                    );

                case 2:
                    return SpawnEnemy2(
                        spawnPointIndex
                    );

                case 3:
                    return SpawnEnemy3(
                        spawnPointIndex
                    );
            }

            return null;
        }


        // ========================================================
        // Spawn Specific Wave
        // ========================================================

        public void SpawnWave(
            int enemy1Count,
            int enemy2Count,
            int enemy3Count)
        {
            int pointIndex = 0;

            // Enemy1
            for (int i = 0;
                 i < enemy1Count;
                 i++)
            {
                SpawnEnemy1(
                    pointIndex
                );

                pointIndex++;

                if (pointIndex >= spawnPoints.Length)
                {
                    pointIndex = 0;
                }
            }

            // Enemy2
            for (int i = 0;
                 i < enemy2Count;
                 i++)
            {
                SpawnEnemy2(
                    pointIndex
                );

                pointIndex++;

                if (pointIndex >= spawnPoints.Length)
                {
                    pointIndex = 0;
                }
            }

            // Enemy3
            for (int i = 0;
                 i < enemy3Count;
                 i++)
            {
                SpawnEnemy3(
                    pointIndex
                );

                pointIndex++;

                if (pointIndex >= spawnPoints.Length)
                {
                    pointIndex = 0;
                }
            }

            if (showDebugLog)
            {
                Debug.Log(
                    $"[EnemySpawner] Wave 생성 완료 / " +
                    $"Enemy1: {enemy1Count}, " +
                    $"Enemy2: {enemy2Count}, " +
                    $"Enemy3: {enemy3Count}"
                );
            }
        }


        // ========================================================
        // Clear Spawned Enemies
        // ========================================================

        public void ClearSpawnedEnemies()
        {
            for (int i = spawnedEnemies.Count - 1;
                 i >= 0;
                 i--)
            {
                if (spawnedEnemies[i] != null)
                {
                    Destroy(
                        spawnedEnemies[i]
                    );
                }
            }

            spawnedEnemies.Clear();

            if (showDebugLog)
            {
                Debug.Log(
                    "[EnemySpawner] 모든 생성 적 제거"
                );
            }
        }


        // ========================================================
        // Get Spawned Enemies
        // ========================================================

        public List<GameObject> GetSpawnedEnemies()
        {
            return spawnedEnemies;
        }


        // ========================================================
        // Spawn Point Count
        // ========================================================

        public int GetSpawnPointCount()
        {
            if (spawnPoints == null)
                return 0;

            return spawnPoints.Length;
        }
    }
}