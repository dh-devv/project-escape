using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("=== 이동할 첫 번째 스테이지 씬 이름 ===")]
        [SerializeField] private string firstSceneName = "Stage1_Map01";

        [Header("=== 디버그 ===")]
        [SerializeField] private bool showDebugLog = true;

        public void OnClickStartButton()
        {
            // SceneManager.LoadScene(firstSceneName); 대신 아래 코드로 변경!
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFade(firstSceneName);
            }
            else
            {
                SceneManager.LoadScene(firstSceneName);
            }
        }
    }
}
