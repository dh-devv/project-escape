using UnityEngine;

namespace Carten
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("=== Player State ===")]
        [SerializeField] private float playerHP = 100f;
        [SerializeField] private int playerPhase = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // =========================================================
        // Player HP
        // =========================================================

        public float PlayerHP
        {
            get => playerHP;
            set => playerHP = Mathf.Max(0f, value);
        }

        // =========================================================
        // Player Phase
        // =========================================================

        public int PlayerPhase
        {
            get => playerPhase;
            set => playerPhase = Mathf.Clamp(value, 1, 3);
        }

        // =========================================================
        // State Save
        // =========================================================

        public void SetPlayerState(float hp, int phase)
        {
            playerHP = Mathf.Max(0f, hp);
            playerPhase = Mathf.Clamp(phase, 1, 3);
        }

        public void ResetPlayerState()
        {
            playerHP = 100f;
            playerPhase = 1;
        }
    }
}