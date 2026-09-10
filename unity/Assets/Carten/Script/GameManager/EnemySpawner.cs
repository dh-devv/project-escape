using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("=== Enemy Prefabs ===")]
        [SerializeField] private GameObject enemy1Prefab;
        [SerializeField] private GameObject enemy2Prefab;
        [SerializeField] private GameObject enemy3Prefab;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private readonly List<GameObject> spawnedEnemies =
            new List<GameObject>();


        // ========================================================
        // Enemy Spawn
        // ========================================================

        public GameObject SpawnEnemy1(Vector3 spawnPosition)
        {
            return SpawnEnemy(
                enemy1Prefab,
                spawnPosition,
                "Enemy1"
            );
        }


        public GameObject SpawnEnemy2(Vector3 spawnPosition)
        {
            return SpawnEnemy(
                enemy2Prefab,
                spawnPosition,
                "Enemy2"
            );
        }


        public GameObject SpawnEnemy3(Vector3 spawnPosition)
        {
            return SpawnEnemy(
                enemy3Prefab,
                spawnPosition,
                "Enemy3"
            );
        }


        private GameObject SpawnEnemy(
            GameObject prefab,
            Vector3 spawnPosition,
            string enemyName)
        {
            if (prefab == null)
            {
                Debug.LogError(
                    $"[EnemySpawner] {enemyName} Prefab이 설정되지 않았습니다."
                );

                return null;
            }

            GameObject enemy = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            spawnedEnemies.Add(enemy);

            if (showDebugLog)
            {
                Debug.Log(
                    $"[EnemySpawner] {enemyName} 생성 / 위치: {spawnPosition}"
                );
            }

            return enemy;
        }


        // ========================================================
        // 관리
        // ========================================================

        public List<GameObject> GetSpawnedEnemies()
        {
            return spawnedEnemies;
        }


        public void ClearSpawnedEnemies()
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] != null)
                {
                    Destroy(spawnedEnemies[i]);
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
    }
}