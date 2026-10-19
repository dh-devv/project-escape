using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    public class GameClearUI : MonoBehaviour
    {
        [Header("=== Scene Settings ===")]
        [SerializeField] private string mainSceneName = "Main UI"; // 이동할 메인 메뉴 씬 이름 (본인의 메인 씬 이름과 정확히 일치해야 합니다!)

        // 메인 화면으로 돌아가는 버튼에 연결할 함수
        public void GoToMainScene()
        {
            // 혹시 시간 정지(Time.timeScale = 0)를 사용했다면 원래대로 돌려놓습니다.
            Time.timeScale = 1f;

            // 메인 메뉴 씬으로 이동
            SceneManager.LoadScene(mainSceneName);
        }
    }
}
