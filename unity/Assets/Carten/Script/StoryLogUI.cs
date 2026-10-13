using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Carten
{
    /// <summary>
    /// 연구 일지 등 스토리 텍스트를 표시하고 게임을 일시정지하는 UI입니다.
    /// Canvas에 붙이고 Panel, 제목, 본문, 닫기 버튼을 Inspector에서 연결하세요.
    /// </summary>
    public class StoryLogUI : MonoBehaviour
    {
        [Header("UI 연결")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private Button closeButton;

        private float previousTimeScale = 1f;
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (panel != null)
                panel.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        /// <summary>스토리 창을 열고 기존 게임 시간 배율을 저장한 뒤 일시정지합니다.</summary>
        public bool Show(string title, string content)
        {
            if (IsOpen)
                return false;

            if (panel == null || titleText == null || contentText == null)
            {
                Debug.LogError("[StoryLogUI] Panel, Title Text, Content Text를 Inspector에서 연결하세요.", this);
                return false;
            }

            previousTimeScale = Time.timeScale;
            titleText.text = title;
            contentText.text = content;
            panel.SetActive(true);
            IsOpen = true;
            Time.timeScale = 0f;
            return true;
        }

        /// <summary>닫기 버튼에 연결됩니다. 게임을 이전 시간 배율로 되돌립니다.</summary>
        public void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            if (panel != null)
                panel.SetActive(false);

            Time.timeScale = previousTimeScale;
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);

            // 일시정지된 상태에서 UI 오브젝트가 제거되는 경우에도 게임이 멈춰 있지 않게 합니다.
            if (IsOpen)
                Time.timeScale = previousTimeScale;
        }
    }
}
