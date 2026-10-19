using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    public class BossCombat : MonoBehaviour
    {
        [Header("=== Attack Point ===")]
        [SerializeField] private Transform attackPoint;

        [Header("=== Attack Range ===")]
        [SerializeField] private float phase1AttackRange = 1.5f;
        [SerializeField] private float phase2AttackRange = 1.7f;
        [SerializeField] private float phase3AttackRange = 2.0f;

        [Header("=== Damage ===")]
        [SerializeField] private float baseDamage = 10f;

        [Header("=== Player Layer ===")]
        [SerializeField] private LayerMask playerLayer;

        [Header("=== Debug ===")]
        [SerializeField] private bool showDebugLog = true;

        [Header("=== Field attacks (아레나 범위 설정) ===")]
        [SerializeField] private Transform arenaCenter;
        [SerializeField] private Vector2 arenaSize = new Vector2(24f, 8f);
        [SerializeField, Min(0f)] private float strongHitStun = 0.5f;

        [Header("=== Boss Sound Effects (보스 공격 사운드) ===")]
        [Tooltip("기 모을 때 / 장판 경고 시 재생할 사운드")]
        [SerializeField] private AudioClip warningSound;
        [Tooltip("내려칠 때 / 공격 폭발 시 재생할 사운드")]
        [SerializeField] private AudioClip attackSound;
        [Tooltip("1.0 이상으로 올리면 소리가 강제로 증폭됩니다! (기본 추천: 2.0 ~ 3.0)")]
        [SerializeField, Range(0f, 5f)] private float soundVolume = 2.5f;

        [Header("=== Screen Shake Settings (화면 흔들림 설정) ===")]
        [Tooltip("공격 시 화면이 흔들리는 시간 (추천: 0.15 ~ 0.2초)")]
        [SerializeField] private float shakeDuration = 0.18f;
        [Tooltip("화면이 흔들리는 세기 (시야 방해 없으려면 0.1 ~ 0.15 추천)")]
        [SerializeField, Range(0f, 0.5f)] private float shakeMagnitude = 0.12f;

        [Header("=== Field Attack Assets (패턴 에셋 넣는 곳) ===")]
        [Tooltip("경고 단계에 표시할 프리팹 (예: VFX_Common_AttackWarning)")]
        [SerializeField] private GameObject warningPrefab;
        [Tooltip("실제 공격 판정 시 표시할 폭발/레이저 프리팹")]
        [SerializeField] private GameObject attackPrefab;
        [Tooltip("프리팹 대신 일반 스프라이트 이미지를 넣을 경우 (경고용)")]
        [SerializeField] private Sprite warningSprite;
        [Tooltip("프리팹 대신 일반 스프라이트 이미지를 넣을 경우 (공격용)")]
        [SerializeField] private Sprite attackSprite;
        [Tooltip("에셋 크기를 장판 판정 범위(Size)에 맞춰 자동으로 늘릴지 여부")]
        [SerializeField] private bool scaleAssetToFitZone = true;
        [Tooltip("기본 노란색/빨간색 테두리 선 표시 여부")]
        [SerializeField] private bool showWarningOutline = true;

        private readonly List<BossFieldAttack> fields = new List<BossFieldAttack>();
        public Vector2 AttackCenter => attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        private Vector2 ArenaCenter => arenaCenter != null ? (Vector2)arenaCenter.position : (Vector2)transform.position;
        public bool HasActiveFieldAttacks => fields.Exists(field => field != null);

        private BossController bossController;
        private AudioSource audioSource;

        private void Awake()
        {
            bossController = GetComponent<BossController>();
            if (attackPoint == null)
                attackPoint = transform.Find("AttackPoint") ?? transform;
            if (playerLayer.value == 0)
                playerLayer = LayerMask.GetMask("Player");

            if (bossController == null)
            {
                Debug.LogError("[BossCombat] BossController를 찾을 수 없습니다.");
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
        }

        public void PerformBasicAttack()
        {
            if (bossController == null || bossController.IsDead) return;

            bossController.TriggerAttackAnimation(isStrongAttack: false);

            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound, soundVolume);
            }

            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.TriggerShake(shakeDuration * 0.8f, shakeMagnitude * 0.7f);
            }

            if (attackPoint == null) return;

            float attackRange = GetAttackRange();
            Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

            foreach (Collider2D target in targets)
            {
                PlayerController player = target.GetComponentInParent<PlayerController>();
                if (player == null) continue;

                float finalDamage = baseDamage * bossController.DamageMultiplier;
                player.TakeDamage(finalDamage);
                break;
            }
        }

        public void PerformAreaAttack()
        {
            if (bossController == null || bossController.IsDead) return;

            bossController.TriggerAttackAnimation(isStrongAttack: true);

            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound, soundVolume);
            }

            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.TriggerShake(shakeDuration, shakeMagnitude);
            }

            if (attackPoint == null) return;

            float areaRange = GetAreaAttackRange();
            Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, areaRange, playerLayer);

            foreach (Collider2D target in targets)
            {
                PlayerController player = target.GetComponentInParent<PlayerController>();
                if (player == null) continue;

                float finalDamage = baseDamage * bossController.DamageMultiplier;
                player.TakeDamage(finalDamage, strongHitStun);
                break;
            }
        }

        public void PerformTelegraphedMelee(Vector2 center, float warning, bool strong)
        {
            bossController?.TriggerAttackAnimation(isStrongAttack: strong);

            float range = strong ? GetAreaAttackRange() : GetAttackRange();
            SpawnField(center, new Vector2(range * 2f, 2.5f), warning, 0.15f,
                strong ? 1.5f : 1f, strong ? strongHitStun : 0f, playFeedback: true);
        }

        public void PerformTargetedFieldAttack(Vector2 targetPosition, float warning, int phase)
        {
            bossController?.TriggerAttackAnimation(isStrongAttack: true);

            float width = phase == 1 ? 2.5f : phase == 2 ? 3.5f : 4.5f;
            float halfWidth = Mathf.Max(width * 0.5f, arenaSize.x * 0.5f);
            float x = Mathf.Clamp(targetPosition.x,
                ArenaCenter.x - halfWidth + width * 0.5f,
                ArenaCenter.x + halfWidth - width * 0.5f);
            SpawnField(new Vector2(x, ArenaCenter.y), new Vector2(width, arenaSize.y),
                warning, 0.35f, 1f, strongHitStun, playFeedback: true);
        }

        public void PerformLaneAttack(float warning, int phase, int sequence)
        {
            bossController?.TriggerAttackAnimation(isStrongAttack: true);

            int lanes = 5;
            int safeLane = sequence % lanes;
            float laneWidth = arenaSize.x / lanes;
            bool firstFeedback = true;

            for (int lane = 0; lane < lanes; lane++)
            {
                if (lane == safeLane || (phase == 1 && lane % 2 == 0)) continue;
                float x = ArenaCenter.x - arenaSize.x * 0.5f + (lane + 0.5f) * laneWidth;
                SpawnField(new Vector2(x, ArenaCenter.y), new Vector2(laneWidth * 0.9f, arenaSize.y),
                    warning, 0.35f, 1f, strongHitStun, playFeedback: firstFeedback);
                firstFeedback = false;
            }
        }

        private void SpawnField(Vector2 center, Vector2 size, float warning, float active, float multiplier, float stun, bool playFeedback = true)
        {
            if (bossController == null || bossController.IsDead || !isActiveAndEnabled) return;
            fields.RemoveAll(field => field == null);
            fields.Add(BossFieldAttack.Create(
                bossController, center, size, playerLayer,
                baseDamage * bossController.DamageMultiplier * multiplier, stun, warning, active,
                warningPrefab, attackPrefab, warningSprite, attackSprite, scaleAssetToFitZone, showWarningOutline,
                playFeedback ? warningSound : null, playFeedback ? attackSound : null, soundVolume,
                playFeedback, shakeDuration, shakeMagnitude));
        }

        public void CancelFieldAttacks()
        {
            foreach (BossFieldAttack field in fields)
                if (field != null) field.Cancel();
            fields.Clear();
        }

        private void OnDisable() => CancelFieldAttacks();

        private float GetAreaAttackRange()
        {
            switch (bossController.CurrentPhase)
            {
                case BossController.BossPhase.Phase3: return 4f;
                case BossController.BossPhase.Phase2: return 3f;
                default: return 2.5f;
            }
        }

        private float GetAttackRange()
        {
            switch (bossController.CurrentPhase)
            {
                case BossController.BossPhase.Phase2: return phase2AttackRange;
                case BossController.BossPhase.Phase3: return phase3AttackRange;
                default: return phase1AttackRange;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            float range = phase1AttackRange;
            if (bossController != null)
            {
                switch (bossController.CurrentPhase)
                {
                    case BossController.BossPhase.Phase2: range = phase2AttackRange; break;
                    case BossController.BossPhase.Phase3: range = phase3AttackRange; break;
                }
            }
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, range);
        }
    }
}
