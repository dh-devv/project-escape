using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        // 페이드 없이 즉시 씬을 부드럽고 안전하게 로드합니다.
        public void LoadSceneWithFade(string sceneName)
        {
            Debug.Log($"[SceneTransitionManager] 씬 이동: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }
}
