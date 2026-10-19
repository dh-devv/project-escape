using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Carten
{
    public class GameHUD : MonoBehaviour
    {
        [Header("=== 1. 플레이어 HP & 페이즈 UI ===")]
        [SerializeField] private Image playerHpFill;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private TMP_Text playerPhaseText;

        [Header("=== 2. 플레이어 스킬 & 쿨타임 UI ===")]
        [SerializeField] private Image skillCooldownOverlay;
        [SerializeField] private TMP_Text skillCooldownText;

        [Header("=== 3. 보스 정보 UI (체력 & 페이즈) ===")]
        [SerializeField] private GameObject bossHudPanel;
        [SerializeField] private Image bossHpFill;
        [SerializeField] private TMP_Text bossPhaseText;

        private PlayerController player;
        private BossController currentBoss;

        private void Start()
        {
            // 플레이어 자동 탐색
            player = FindFirstObjectByType<PlayerController>();

            // 보스 자동 탐색
            currentBoss = FindFirstObjectByType<BossController>();

            if (currentBoss != null && !currentBoss.IsDead)
            {
                if (bossHudPanel != null)
                    bossHudPanel.SetActive(currentBoss.HasEncounterStarted);

                currentBoss.EncounterStarted += OnBossEncounterStarted;
                currentBoss.PhaseChanged += OnBossPhaseChanged;
                currentBoss.Defeated += OnBossDefeated;

                OnBossPhaseChanged(currentBoss.CurrentPhase);
            }
            else
            {
                if (bossHudPanel != null)
                    bossHudPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (currentBoss != null)
            {
                currentBoss.EncounterStarted -= OnBossEncounterStarted;
                currentBoss.PhaseChanged -= OnBossPhaseChanged;
                currentBoss.Defeated -= OnBossDefeated;
            }
        }

        private void Update()
        {
            // ★ 매 프레임 플레이어 체력과 페이즈를 직접 확인해서 강제로 UI에 반영!
            if (player == null)
                player = FindFirstObjectByType<PlayerController>();

            if (player != null)
            {
                UpdatePlayerHP(player.CurrentHealth, player.MaxHealth);
                UpdatePlayerPhase((int)player.CurrentPhase + 1);
            }

            // 보스 체력 실시간 업데이트
            if (currentBoss != null && !currentBoss.IsDead)
            {
                if (bossHudPanel != null && !bossHudPanel.activeSelf && currentBoss.HasEncounterStarted)
                {
                    bossHudPanel.SetActive(true);
                }

                if (bossHpFill != null)
                {
                    bossHpFill.fillAmount = currentBoss.HealthPercent / 100f;
                }
            }
        }

        public void UpdatePlayerHP(float currentHp, float maxHp)
        {
            if (playerHpFill != null && maxHp > 0f)
            {
                // 혹시 에디터에서 Filled 설정을 깜빡했더라도 강제로 Filled로 맞춰줍니다!
                if (playerHpFill.type != Image.Type.Filled)
                {
                    playerHpFill.type = Image.Type.Filled;
                    playerHpFill.fillMethod = Image.FillMethod.Horizontal;
                }

                playerHpFill.fillAmount = Mathf.Clamp01(currentHp / maxHp);
            }

            if (playerHpText != null)
                playerHpText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
        }

        public void UpdatePlayerPhase(int phaseNumber)
        {
            if (playerPhaseText != null)
                playerPhaseText.text = $"PHASE {phaseNumber}";
        }

        public void UpdateSkillCooldown(float remainingTime, float totalCooldown)
        {
            if (skillCooldownOverlay != null)
            {
                skillCooldownOverlay.fillAmount = totalCooldown > 0f ? Mathf.Clamp01(remainingTime / totalCooldown) : 0f;
            }

            if (skillCooldownText != null)
            {
                if (remainingTime > 0f)
                {
                    skillCooldownText.gameObject.SetActive(true);
                    skillCooldownText.text = $"{remainingTime:F1}";
                }
                else
                {
                    skillCooldownText.gameObject.SetActive(false);
                }
            }
        }

        private void OnBossEncounterStarted()
        {
            if (bossHudPanel != null)
                bossHudPanel.SetActive(true);

            if (currentBoss != null)
                OnBossPhaseChanged(currentBoss.CurrentPhase);
        }

        private void OnBossPhaseChanged(BossController.BossPhase newPhase)
        {
            if (bossPhaseText != null)
            {
                int phaseNum = (int)newPhase + 1;
                bossPhaseText.text = $"PHASE {phaseNum}";
            }
        }

        private void OnBossDefeated(BossController boss)
        {
            if (bossHudPanel != null)
                bossHudPanel.SetActive(false);
        }
    }
}
