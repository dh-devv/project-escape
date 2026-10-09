using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    [RequireComponent(typeof(LastSparkApiClient))]
    public sealed class LastSparkSession : MonoBehaviour
    {
        public static LastSparkSession Instance { get; private set; }
        [SerializeField, Min(0)] private int existingUserId;
        private LastSparkApiClient api;
        private readonly Queue<PendingResult> pendingResults = new Queue<PendingResult>();
        public int UserId { get; private set; }
        public bool IsConnecting { get; private set; }
        public string Status { get; private set; } = "Offline";
        public LastSparkApiClient Api => api;
        public event Action<int> Connected;
        public int PendingResultCount => pendingResults.Count;

        public sealed class PendingResult
        {
            internal readonly int BossId;
            internal readonly double ClearTime;
            internal readonly int Score;
            internal readonly int MaxPhase;
            public string Status { get; internal set; } = "Result captured offline; connect to save";

            internal PendingResult(int bossId, double clearTime, int score, int maxPhase)
            {
                BossId = bossId;
                ClearTime = clearTime;
                Score = score;
                MaxPhase = maxPhase;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            api = GetComponent<LastSparkApiClient>();
            UserId = existingUserId;
            if (UserId > 0) Status = "Using configured user ID";
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        public void Connect(string username)
        {
            if (IsConnecting || UserId > 0) return;
            if (string.IsNullOrWhiteSpace(username))
            {
                Status = "Enter a username";
                return;
            }
            IsConnecting = true;
            Status = "Connecting";
            api.CreateUser(username.Trim(), response =>
            {
                IsConnecting = false;
                UserId = response.userId;
                Status = "Connected";
                FlushResults();
                Connected?.Invoke(UserId);
            }, error => { IsConnecting = false; Status = error; });
        }

        public PendingResult QueueResult(int bossId, double clearTime, int score, int maxPhase)
        {
            if (bossId <= 0 || clearTime <= 0 || double.IsNaN(clearTime) || double.IsInfinity(clearTime) ||
                score < 0 || maxPhase < 1 || maxPhase > 3)
                throw new ArgumentException("Invalid boss result.");

            PendingResult result = new PendingResult(bossId, clearTime, score, maxPhase);
            pendingResults.Enqueue(result);
            FlushResults();
            return result;
        }

        private void FlushResults()
        {
            if (UserId <= 0) return;
            while (pendingResults.Count > 0)
            {
                PendingResult result = pendingResults.Dequeue();
                result.Status = "Saving";
                api.SaveScore(UserId, result.BossId, result.ClearTime, result.Score, result.MaxPhase,
                    response => { result.Status = "Saved record " + response.recordId; },
                    error =>
                    {
                        result.Status = "Save failed: " + error;
                        Status = result.Status;
                        Debug.LogWarning(result.Status, this);
                    });
            }
        }
    }
}
