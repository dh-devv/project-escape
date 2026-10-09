using UnityEngine;

namespace Carten
{
    /// <summary>Connects boss defeat to the existing score API without owning combat.</summary>
    [RequireComponent(typeof(BossController))]
    public sealed class BossResultReporter : MonoBehaviour
    {
        [SerializeField] private LastSparkSession session;
        [SerializeField] private PlayerController player;
        [SerializeField, Min(1)] private int bossId = 1;
        [SerializeField, Min(0)] private int clearScore = 1000;
        private BossController boss;
        private float startedAt;
        private int maxPhase = 1;
        private bool captured;
        private LastSparkSession.PendingResult pendingResult;
        private double clearTime;
        private string encounterStatus = "Waiting for encounter";
        public string Status => pendingResult != null ? pendingResult.Status : encounterStatus;

        private void Awake() => boss = GetComponent<BossController>();
        private void OnEnable()
        {
            ResolveReferences();
            boss.EncounterStarted += OnEncounterStarted;
            boss.Defeated += OnDefeated;
        }
        private void ResolveReferences()
        {
            if (LastSparkSession.Instance != null) session = LastSparkSession.Instance;
            if (player == null)
            {
                GameObject found = GameObject.FindGameObjectWithTag("Player");
                if (found != null) player = found.GetComponent<PlayerController>();
            }
        }
        private void Update()
        {
            ResolveReferences();
            if (boss.HasEncounterStarted && player != null && !captured)
                maxPhase = Mathf.Max(maxPhase, (int)player.CurrentPhase + 1);
        }
        private void OnEncounterStarted()
        {
            ResolveReferences();
            startedAt = Time.time;
            maxPhase = player != null ? (int)player.CurrentPhase + 1 : 1;
            encounterStatus = "Encounter active";
        }
        private void OnDefeated(BossController defeated)
        {
            if (captured || !boss.HasEncounterStarted) return;
            ResolveReferences();
            captured = true;
            clearTime = Mathf.Max(0.001f, Time.time - startedAt);
            if (player != null) maxPhase = Mathf.Max(maxPhase, (int)player.CurrentPhase + 1);
            Submit();
        }
        public void Submit()
        {
            if (!captured || pendingResult != null) return;
            ResolveReferences();
            if (session == null)
            {
                encounterStatus = "Result cannot be queued: session is missing";
                Debug.LogWarning(encounterStatus, this);
                return;
            }
            pendingResult = session.QueueResult(bossId, clearTime, clearScore, maxPhase);
        }
        private void OnDisable()
        {
            boss.EncounterStarted -= OnEncounterStarted;
            boss.Defeated -= OnDefeated;
        }
    }
}
