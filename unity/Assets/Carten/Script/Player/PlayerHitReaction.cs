using System;
using UnityEngine;

namespace Carten
{
    /// <summary>Owns hit stun only; health and movement remain in PlayerController.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHitReaction : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float maximumStunDuration = 1.2f;
        private float stunUntil;

        public event Action StunStarted;
        public bool IsStunned => isActiveAndEnabled && Time.time < stunUntil;
        public float RemainingStun => IsStunned ? stunUntil - Time.time : 0f;

        public void ApplyStun(float duration)
        {
            if (!isActiveAndEnabled || float.IsNaN(duration) || duration <= 0f)
                return;

            bool wasStunned = IsStunned;
            stunUntil = Mathf.Max(stunUntil, Time.time + Mathf.Min(duration, maximumStunDuration));
            if (!wasStunned && IsStunned)
                StunStarted?.Invoke();
        }

        public void ClearStun() => stunUntil = 0f;
        private void OnDisable() => ClearStun();
    }
}
