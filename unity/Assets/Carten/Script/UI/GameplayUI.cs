using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Carten
{
    [DisallowMultipleComponent, RequireComponent(typeof(PlayerController))]
    public sealed class GameplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset font;
        private PlayerController player;
        private GameObject canvasRoot;
        private RectTransform healthFill;
        private TMP_Text healthText;
        private TMP_Text bossText;
        private BossController boss;
        private float nextBossSearch;
        private GameObject deathPanel;
        public bool IsReady => canvasRoot != null && healthFill != null;
        public static TMP_Text InteractionPrompt { get; private set; }

        private void Start()
        {
            player = GetComponent<PlayerController>();
            canvasRoot = new GameObject("Gameplay UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasRoot);
            var canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            if (EventSystem.current == null)
            {
                canvasRoot.AddComponent<EventSystem>();
                canvasRoot.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            Transform hud = Panel("Suit Status", canvasRoot.transform, new Vector2(.02f, .86f), new Vector2(.25f, .97f), new Color(.025f, .045f, .05f, .94f)).transform;
            healthText = Label("Health", hud, new Vector2(.05f, .39f), new Vector2(.95f, .94f), 24);
            Transform track = Panel("Health Track", hud, new Vector2(.06f, .16f), new Vector2(.94f, .3f), new Color(.17f, .2f, .2f)).transform;
            healthFill = (RectTransform)Panel("Health Fill", track, Vector2.zero, Vector2.one, new Color(.68f, .8f, .7f)).transform;
            bossText = Label("Boss Status", canvasRoot.transform, new Vector2(.34f, .89f), new Vector2(.76f, .97f), 27);
            Label("Controls", canvasRoot.transform, new Vector2(.02f, .02f), new Vector2(.98f, .07f), 20).text = "이동 A/D  ·  점프 SPACE  ·  공격 NUM 3  ·  스킬 Q/W/E  ·  상호작용 F";
            InteractionPrompt = Label("InteractionPrompt", canvasRoot.transform, new Vector2(.2f, .09f), new Vector2(.8f, .15f), 26);
            InteractionPrompt.alignment = TextAlignmentOptions.Center;
            InteractionPrompt.gameObject.SetActive(false);

            GameObject log = Panel("Research Log", canvasRoot.transform, new Vector2(.16f, .16f), new Vector2(.84f, .84f), new Color(.025f, .04f, .045f, .98f));
            TMP_Text title = Label("Log Title", log.transform, new Vector2(.06f, .78f), new Vector2(.94f, .94f), 34);
            TMP_Text content = Label("Log Content", log.transform, new Vector2(.06f, .2f), new Vector2(.94f, .76f), 25);
            Button close = Button("Close Log", log.transform, new Vector2(.73f, .05f), new Vector2(.94f, .15f), "닫기");
            canvasRoot.AddComponent<StoryLogUI>().Bind(log, title, content, close);

            deathPanel = Panel("Signal Lost", canvasRoot.transform, new Vector2(.26f, .32f), new Vector2(.74f, .68f), new Color(.035f, .025f, .025f, .97f));
            Label("Death Title", deathPanel.transform, new Vector2(.08f, .5f), new Vector2(.92f, .9f), 38).text = "신호 소실";
            Button("Return To Menu", deathPanel.transform, new Vector2(.22f, .13f), new Vector2(.78f, .37f), "시작 화면으로").onClick.AddListener(ReturnToMenu);
            deathPanel.SetActive(false);
            player.Died += ShowDeath;
        }

        private void ShowDeath()
        {
            if (!IsReady) return;
            healthFill.anchorMax = new Vector2(0f, 1f);
            healthText.text = "HP 0 / " + Mathf.CeilToInt(player.MaxHealth);
            deathPanel.SetActive(true);
        }

        private void Update()
        {
            if (!IsReady || player == null) return;
            healthFill.anchorMax = new Vector2(Mathf.Clamp01(player.CurrentHealth / Mathf.Max(1f, player.MaxHealth)), 1f);
            healthText.text = "HP " + Mathf.CeilToInt(player.CurrentHealth) + " / " + Mathf.CeilToInt(player.MaxHealth) + "    PHASE " + ((int)player.CurrentPhase + 1);
            deathPanel.SetActive(player.IsDead);
            if (Time.unscaledTime >= nextBossSearch)
            {
                nextBossSearch = Time.unscaledTime + .5f;
                boss = FindAnyObjectByType<BossController>();
            }
            bossText.text = boss == null ? "" : (boss.Kind == BossController.BossKind.FinalBoss ? "NEXUS" : "SENTINEL") + "    " + Mathf.CeilToInt(boss.CurrentHealth) + " / " + Mathf.CeilToInt(boss.MaxHealth);
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Main UI");
        }

        private static void Layout(RectTransform rect, Transform parent, Vector2 min, Vector2 max)
        {
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private GameObject Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            Layout((RectTransform)go.transform, parent, min, max);
            go.GetComponent<Image>().color = color;
            go.GetComponent<Image>().raycastTarget = false;
            return go;
        }

        private TMP_Text Label(string name, Transform parent, Vector2 min, Vector2 max, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            Layout((RectTransform)go.transform, parent, min, max);
            var text = go.GetComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = size;
            text.color = new Color(.85f, .87f, .81f);
            text.raycastTarget = false;
            return text;
        }

        private Button Button(string name, Transform parent, Vector2 min, Vector2 max, string caption)
        {
            GameObject go = Panel(name, parent, min, max, new Color(.19f, .25f, .24f));
            go.GetComponent<Image>().raycastTarget = true;
            var button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            var label = Label("Caption", go.transform, Vector2.zero, Vector2.one, 24);
            label.alignment = TextAlignmentOptions.Center;
            label.text = caption;
            return button;
        }

        private void OnDestroy()
        {
            if (player != null) player.Died -= ShowDeath;
            if (canvasRoot != null && InteractionPrompt != null && InteractionPrompt.transform.IsChildOf(canvasRoot.transform))
                InteractionPrompt = null;
            if (canvasRoot != null) Destroy(canvasRoot);
        }
    }
}
