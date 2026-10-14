using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        [SerializeField] private float fadeDuration = 1.0f; // 화면이 어두워지는 시간
        [SerializeField] private Image fadeImage; // 화면 암전용 UI 이미지 (선택 사항)
        [SerializeField] private string promptMessage = "[F] 키를 눌러 승강기를 가동하고 최상층(보스룸)으로 이동합니다.";

        private bool isPlayerNearby = false;
        private bool isActivated = false;

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
            Debug.Log("[Elevator Cutscene] ⚙️ 승강기 가동 시작... 최상층으로 하강합니다.");

            // 1. 화면 암전(Fade Out) 연출
            float timer = 0f;
            Color startColor = Color.black;
            startColor.a = 0f;
            Color endColor = Color.black;
            endColor.a = 1f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeDuration;

                if (fadeImage != null)
                {
                    fadeImage.color = Color.Lerp(startColor, endColor, progress);
                }
                yield return null;
            }

            // 2. 다음 씬에서 스폰될 위치 저장
            PlayerPrefs.SetString("LastTargetSpawnPoint", targetSpawnPointName);

            // 3. 최종보스 씬 로드
            SceneManager.LoadScene(targetSceneName);
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
