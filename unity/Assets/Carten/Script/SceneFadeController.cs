using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Carten
{
    public class SceneFadeController : MonoBehaviour
    {
        public static SceneFadeController Instance { get; private set; }

        [Header("=== Fade UI ===")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1.0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // 씬이 넘어가도 파괴되지 않음
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (fadeImage == null)
            {
                fadeImage = GetComponentInChildren<Image>();
            }
        }

        private void Start()
        {
            // 시작할 때 오브젝트를 무조건 켜고 Fade In 실행
            gameObject.SetActive(true);
            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(true);
            }
            StartCoroutine(FadeInRoutine());
        }

        public void FadeToScene(string sceneName)
        {
            // 씬이 넘어가기 전에 오브젝트가 확실히 켜져 있도록 보장
            gameObject.SetActive(true);
            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(true);
            }
            StartCoroutine(FadeOutAndLoadRoutine(sceneName));
        }

        private IEnumerator FadeInRoutine()
        {
            if (fadeImage == null) yield break;

            float elapsed = 0f;
            Color color = fadeImage.color;
            color.a = 1f; // 검은 화면으로 시작
            fadeImage.color = color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            color.a = 0f;
            fadeImage.color = color;
        }

        private IEnumerator FadeOutAndLoadRoutine(string sceneName)
        {
            if (fadeImage == null) yield break;

            float elapsed = 0f;
            Color color = fadeImage.color;
            color.a = 0f; // 투명한 상태에서 시작

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            color.a = 1f;
            fadeImage.color = color;

            // 완전히 까매진 후 씬 이동
            SceneManager.LoadScene(sceneName);
        }
    }
}
