using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    [RequireComponent(typeof(Collider2D))]
    public class ElevatorTransition : MonoBehaviour
    {
        [Header("=== 이동할 최종보스 씬 설정 ===")]
        [SerializeField] private string targetSceneName = "Stage1_Map08";
        [SerializeField] private string targetSpawnPointName = "Spawn_FromMap07";

        [Header("=== 연출 및 컷씬 설정 ===")]
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [SerializeField] private string promptMessage = "[F] 키를 눌러 승강기를 가동하고 최상층(보스룸)으로 이동합니다.";

        private bool isPlayerNearby = false;
        private bool isActivated = false;

        private static string pendingSpawnPointName;

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Update()
        {
            if (isPlayerNearby && !isActivated && Input.GetKeyDown(interactKey))
            {
                StartCoroutine(RunElevatorCutsceneSequence());
            }
        }

        private IEnumerator RunElevatorCutsceneSequence()
        {
            isActivated = true;
            Debug.Log("[Elevator Cutscene] 승강기 가동 시작... 최상층으로 이동합니다.");

            // 1. 다음 씬에서 스폰될 위치 저장 및 씬 로드 이벤트 등록
            pendingSpawnPointName = targetSpawnPointName;
            PlayerPrefs.SetString("LastTargetSpawnPoint", targetSpawnPointName);
            PlayerPrefs.Save();

            SceneManager.sceneLoaded -= OnBossSceneLoaded;
            SceneManager.sceneLoaded += OnBossSceneLoaded;

            // 2. SceneTransitionManager를 통한 부드러운 페이드 씬 전환 호출
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFade(targetSceneName);
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }

            yield return null;
        }

        private static void OnBossSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnBossSceneLoaded;

            // 중복 플레이어 제거 로직
            PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            PlayerController activePlayer = null;

            if (players.Length > 1)
            {
                foreach (PlayerController p in players)
                {
                    if (p.gameObject.scene.name == "DontDestroyOnLoad")
                    {
                        activePlayer = p;
                        break;
                    }
                }

                if (activePlayer == null) activePlayer = players[0];

                foreach (PlayerController p in players)
                {
                    if (p != activePlayer)
                    {
                        Destroy(p.gameObject);
                    }
                }
            }
            else if (players.Length == 1)
            {
                activePlayer = players[0];
            }

            // 스폰 위치로 플레이어 이동
            if (activePlayer != null && !string.IsNullOrWhiteSpace(pendingSpawnPointName))
            {
                GameObject spawnPoint = GameObject.Find(pendingSpawnPointName);
                if (spawnPoint != null)
                {
                    activePlayer.transform.position = spawnPoint.transform.position;
                    Rigidbody2D rb = activePlayer.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.position = spawnPoint.transform.position;
                        rb.linearVelocity = Vector2.zero;
                    }
                }
            }

            pendingSpawnPointName = null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                isPlayerNearby = true;
                Debug.Log(promptMessage);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null)
            {
                isPlayerNearby = false;
            }
        }
    }
}
