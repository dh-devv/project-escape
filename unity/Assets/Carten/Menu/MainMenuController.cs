using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("=== Scene Settings ===")]
        [SerializeField] private string firstSceneName = "Stage1_Map01";
        [SerializeField] private bool showDebugLog = true;

        /// <summary>
        /// 메인 메뉴의 Start 버튼에 연결할 함수
        /// </summary>
        public void OnClickStartButton()
        {
            if (showDebugLog)
            {
                Debug.Log($"[MainMenuController] 게임 시작 버튼 클릭 → {firstSceneName} 씬으로 이동 시도");
            }

            // SceneFadeController가 존재한다면 페이드 아웃 연출과 함께 씬 이동
            if (SceneFadeController.Instance != null)
            {
                SceneFadeController.Instance.FadeToScene(firstSceneName);
            }
            else
            {
                // 페이드 컨트롤러가 없을 경우 비상용 즉시 이동
                SceneManager.LoadScene(firstSceneName);
            }
        }
    }
}
