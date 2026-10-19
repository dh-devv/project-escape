using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Carten
{
    public class ComputerMonitorUI : MonoBehaviour
    {
        [Header("=== UI 연결 ===")]
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

        public bool Show(string title, string content)
        {
            if (IsOpen)
                return false;

            if (panel == null || titleText == null || contentText == null)
            {
                Debug.LogError("[ComputerMonitorUI] Panel, Title Text, Content Text가 연결되지 않았습니다.", this);
                return false;
            }

            // ★ 에디터에서 실수로 Scale이 3배(3,3,1)로 되어 있어도 1배(1,1,1)로 자동 보정!
            titleText.rectTransform.localScale = Vector3.one;
            contentText.rectTransform.localScale = Vector3.one;

            // 글상자가 너무 작으면(100x100) 글씨가 넘치므로 적당한 크기로 자동 보정
            if (contentText.rectTransform.sizeDelta.x <= 100f)
            {
                contentText.rectTransform.sizeDelta = new Vector2(340f, 240f);
            }

            // 타이틀과 본문 폰트 크기 개별 적용
            titleText.fontSize = 36f;
            contentText.fontSize = 18f; // 12가 너무 작다면 18 추천 (원하시면 12f로 변경 가능)

            previousTimeScale = Time.timeScale;
            titleText.text = title;
            contentText.text = content;
            panel.SetActive(true);
            IsOpen = true;
            Time.timeScale = 0f;
            return true;
        }

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

            if (IsOpen)
                Time.timeScale = previousTimeScale;
        }
    }
}
