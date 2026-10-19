using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Carten
{
    [DisallowMultipleComponent]
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private string firstScene = "Stage1_Map01";
        private bool loading;
        private Button startButton;

        private void Awake()
        {
            Time.timeScale = 1f;
            var scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (background == null) return;
            background.raycastTarget = false;
            var aspect = background.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio = background.sprite != null ? background.sprite.rect.width / background.sprite.rect.height : 16f / 9f;

            // The approved title artwork contains the button label; this is its click target.
            var buttonObject = new GameObject("Start Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(background.transform, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(0.153f, 0.401f);
            rect.anchorMax = new Vector2(0.346f, 0.469f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = buttonObject.GetComponent<Image>();
            image.color = Color.clear;
            startButton = buttonObject.GetComponent<Button>();
            var colors = startButton.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = new Color(1f, 0.65f, 0.2f, 0.14f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(1f, 0.65f, 0.2f, 0.28f);
            startButton.colors = colors;
            startButton.onClick.AddListener(StartGame);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) StartGame();
        }

        public void StartGame()
        {
            if (loading) return;
            if (!Application.CanStreamedLevelBeLoaded(firstScene))
            {
                Debug.LogError("[MainMenuUI] 시작 맵이 빌드에 없습니다: " + firstScene, this);
                return;
            }
            loading = true;
            if (startButton != null) startButton.interactable = false;
            GameStateManager.Instance?.ResetRunState();
            Time.timeScale = 1f;
            SceneManager.LoadSceneAsync(firstScene);
        }
    }
}
