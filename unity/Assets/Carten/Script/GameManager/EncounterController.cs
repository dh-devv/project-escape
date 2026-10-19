using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class EncounterController : MonoBehaviour
    {
        [Header("=== Trigger ===")]
        [SerializeField] private Collider2D triggerCollider;

        [Header("=== Spawn ===")]
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private Transform[] spawnPoints;

        [Header("=== Enemy Count ===")]
        [SerializeField] private int enemy1Count = 2;
        [SerializeField] private int enemy2Count = 0; // ★ 추가됨
        [SerializeField] private int enemy3Count = 0; // ★ 추가됨

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        private bool encounterStarted = false;
        private bool encounterCleared = false;

        private readonly List<GameObject> encounterEnemies = new List<GameObject>();

        private void Awake()
        {
            if (triggerCollider == null)
            {
                triggerCollider = GetComponentInChildren<Collider2D>();
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawner>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (encounterStarted || encounterCleared)
                return;

            if (other.GetComponentInParent<PlayerController>() == null)
                return;

            EncounterTrigger trigger = GetComponentInChildren<EncounterTrigger>();
            if (trigger != null)
            {
                trigger.TryStartEncounter();
                return;
            }

            StartEncounter();
        }

        public void StartEncounter()
        {
            if (encounterStarted || encounterCleared) return;

            // 총 소환할 적이 0마리면 실행 안 함
            if (enemySpawner == null || spawnPoints == null || spawnPoints.Length == 0 || (enemy1Count + enemy2Count + enemy3Count) <= 0)
            {
                Debug.LogError("[EncounterController] Spawner, spawn points and a positive enemy count are required.", this);
                return;
            }
            foreach (Transform point in spawnPoints)
            {
                if (point != null) continue;
                Debug.LogError("[EncounterController] Spawn points contain an empty entry.", this);
                return;
            }

            SpawnEnemies();
            encounterStarted = encounterEnemies.Count > 0;

            if (showDebugLog && encounterStarted)
            {
                Debug.Log("[EncounterController] 전투 시작!");
            }
        }

        private void SpawnEnemies()
        {
            if (enemySpawner == null || spawnPoints == null || spawnPoints.Length == 0) return;

            int currentSpawnIndex = 0; // 스폰 포인트를 순차적으로 돌아가며 배치하기 위한 인덱스

            // Enemy 1 소환
            for (int i = 0; i < enemy1Count; i++)
            {
                GameObject enemy = enemySpawner.SpawnEnemy1(spawnPoints[currentSpawnIndex % spawnPoints.Length].position);
                if (enemy != null) encounterEnemies.Add(enemy);
                currentSpawnIndex++;
            }

            // Enemy 2 소환
            for (int i = 0; i < enemy2Count; i++)
            {
                GameObject enemy = enemySpawner.SpawnEnemy2(spawnPoints[currentSpawnIndex % spawnPoints.Length].position);
                if (enemy != null) encounterEnemies.Add(enemy);
                currentSpawnIndex++;
            }

            // Enemy 3 소환
            for (int i = 0; i < enemy3Count; i++)
            {
                GameObject enemy = enemySpawner.SpawnEnemy3(spawnPoints[currentSpawnIndex % spawnPoints.Length].position);
                if (enemy != null) encounterEnemies.Add(enemy);
                currentSpawnIndex++;
            }

            if (showDebugLog)
            {
                Debug.Log($"[EncounterController] 총 {encounterEnemies.Count}마리 소환 완료");
            }
        }

        private void Update()
        {
            if (!encounterStarted || encounterCleared) return;
            CheckEncounterClear();
        }

        private void CheckEncounterClear()
        {
            if (encounterEnemies.Count == 0) return;

            for (int i = 0; i < encounterEnemies.Count; i++)
            {
                GameObject enemy = encounterEnemies[i];
                if (enemy == null) continue;

                EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
                if (enemyBase != null && !enemyBase.IsDead)
                {
                    return;
                }
            }

            ClearEncounter();
        }

        private void ClearEncounter()
        {
            encounterCleared = true;
            if (showDebugLog)
            {
                Debug.Log("[EncounterController] 전투 CLEAR!");
            }
        }

        public bool IsStarted() => encounterStarted;
        public bool IsCleared() => encounterCleared;
    }
}
