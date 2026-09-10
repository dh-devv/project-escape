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
        [SerializeField] private Transform[] spawnPoints;               //private void StartEncounter()

        [Header("=== Enemy Count ===")]
        [SerializeField] private int enemy1Count = 2;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;


        private bool encounterStarted = false;
        private bool encounterCleared = false;

        private readonly List<GameObject> encounterEnemies =
            new List<GameObject>();


        private void Start()
        {
            if (triggerCollider == null)
            {
                triggerCollider =
                    GetComponentInChildren<Collider2D>();
            }

            if (enemySpawner == null)
            {
                enemySpawner =
                    FindFirstObjectByType<EnemySpawner>();
            }
        }


        // ========================================================
        // Trigger
        // ========================================================

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (encounterStarted)
                return;

            if (encounterCleared)
                return;

            if (!other.CompareTag("Player"))
                return;

            StartEncounter();
        }


        // ========================================================
        // Encounter 시작
        // ========================================================

        public void StartEncounter()
        {
            encounterStarted = true;

            if (showDebugLog)
            {
                Debug.Log(
                    "[EncounterController] Encounter01 시작"
                );
            }

            SpawnEnemies();
        }


        private void SpawnEnemies()
        {
            if (enemySpawner == null)
            {
                Debug.LogError(
                    "[EncounterController] EnemySpawner가 없습니다."
                );

                return;
            }

            if (spawnPoints == null ||
                spawnPoints.Length == 0)
            {
                Debug.LogError(
                    "[EncounterController] SpawnPoint가 없습니다."
                );

                return;
            }


            for (int i = 0; i < enemy1Count; i++)
            {
                int pointIndex =
                    i % spawnPoints.Length;

                GameObject enemy =
                    enemySpawner.SpawnEnemy1(
                        spawnPoints[pointIndex].position
                    );

                if (enemy != null)
                {
                    encounterEnemies.Add(enemy);
                }
            }

            if (showDebugLog)
            {
                Debug.Log(
                    $"[EncounterController] Enemy1 {enemy1Count}마리 소환"
                );
            }
        }


        // ========================================================
        // 전투 확인
        // ========================================================

        private void Update()
        {
            if (!encounterStarted)
                return;

            if (encounterCleared)
                return;

            CheckEncounterClear();
        }


        private void CheckEncounterClear()
        {
            if (encounterEnemies.Count == 0)
                return;

            for (int i = 0; i < encounterEnemies.Count; i++)
            {
                GameObject enemy =
                    encounterEnemies[i];

                if (enemy == null)
                    continue;

                EnemyBase enemyBase =
                    enemy.GetComponent<EnemyBase>();

                if (enemyBase == null)
                    continue;

                if (!enemyBase.IsDead)
                {
                    return;
                }
            }

            ClearEncounter();
        }


        // ========================================================
        // Encounter Clear
        // ========================================================

        private void ClearEncounter()
        {
            encounterCleared = true;

            if (showDebugLog)
            {
                Debug.Log(
                    "[EncounterController] Encounter01 CLEAR!"
                );
            }
        }


        public bool IsStarted()
        {
            return encounterStarted;
        }


        public bool IsCleared()
        {
            return encounterCleared;
        }
    }
}