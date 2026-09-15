using System.Collections.Generic;
using UnityEngine;
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // =========================================================
    // Player State
    // =========================================================

    [Header("=== Player State ===")]
    [SerializeField] private float playerHP = 100f;
    [SerializeField] private int playerPhase = 1;


    // =========================================================
    // Skill Cooldown
    // =========================================================


    // One-Time Trigger State
    private readonly HashSet<string> triggeredEvents =
        new HashSet<string>();

    public bool HasTriggered(string triggerId)
    {
        if (string.IsNullOrEmpty(triggerId))
            return false;

        return triggeredEvents.Contains(triggerId);
    }

    public void SetTriggered(string triggerId)
    {
        if (string.IsNullOrEmpty(triggerId))
            return;

        triggeredEvents.Add(triggerId);
    }
    public enum SkillCooldownType
    {
        HeavyStrike,
        Defense,
        GravityBoost,

        Overdrive,
        DashSlash,
        BoostExplosion,

        LimitBreak,
        BlinkDash,
        OverloadBlast
    }

    // 다음 사용 가능 시간
    private float heavyStrikeCooldownEndTime;
    private float defenseCooldownEndTime;
    private float gravityBoostCooldownEndTime;

    private float overdriveCooldownEndTime;
    private float dashSlashCooldownEndTime;
    private float boostExplosionCooldownEndTime;

    private float limitBreakCooldownEndTime;
    private float blinkDashCooldownEndTime;
    private float overloadBlastCooldownEndTime;


    // =========================================================
    // Awake
    // =========================================================

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

        set
        {
            playerHP = Mathf.Max(
                0f,
                value
            );
        }
    }


    // =========================================================
    // Player Phase
    // =========================================================

    public int PlayerPhase
    {
        get => playerPhase;

        set
        {
            playerPhase = Mathf.Clamp(
                value,
                1,
                3
            );
        }
    }


    // =========================================================
    // Player State
    // =========================================================

    public void SetPlayerState(
        float hp,
        int phase)
    {
        playerHP =
            Mathf.Max(
                0f,
                hp
            );

        playerPhase =
            Mathf.Clamp(
                phase,
                1,
                3
            );
    }


    // =========================================================
    // Skill Cooldown 저장
    // =========================================================

    public void StartSkillCooldown(
        SkillCooldownType skill,
        float duration)
    {
        float endTime =
            Time.time +
            Mathf.Max(
                0f,
                duration
            );

        switch (skill)
        {
            case SkillCooldownType.HeavyStrike:
                heavyStrikeCooldownEndTime = endTime;
                break;

            case SkillCooldownType.Defense:
                defenseCooldownEndTime = endTime;
                break;

            case SkillCooldownType.GravityBoost:
                gravityBoostCooldownEndTime = endTime;
                break;


            case SkillCooldownType.Overdrive:
                overdriveCooldownEndTime = endTime;
                break;

            case SkillCooldownType.DashSlash:
                dashSlashCooldownEndTime = endTime;
                break;

            case SkillCooldownType.BoostExplosion:
                boostExplosionCooldownEndTime = endTime;
                break;


            case SkillCooldownType.LimitBreak:
                limitBreakCooldownEndTime = endTime;
                break;

            case SkillCooldownType.BlinkDash:
                blinkDashCooldownEndTime = endTime;
                break;

            case SkillCooldownType.OverloadBlast:
                overloadBlastCooldownEndTime = endTime;
                break;
        }
    }


    // =========================================================
    // Skill Cooldown 불러오기
    // =========================================================

    public float GetSkillCooldownRemaining(
        SkillCooldownType skill)
    {
        float endTime = 0f;

        switch (skill)
        {
            case SkillCooldownType.HeavyStrike:
                endTime = heavyStrikeCooldownEndTime;
                break;

            case SkillCooldownType.Defense:
                endTime = defenseCooldownEndTime;
                break;

            case SkillCooldownType.GravityBoost:
                endTime = gravityBoostCooldownEndTime;
                break;


            case SkillCooldownType.Overdrive:
                endTime = overdriveCooldownEndTime;
                break;

            case SkillCooldownType.DashSlash:
                endTime = dashSlashCooldownEndTime;
                break;

            case SkillCooldownType.BoostExplosion:
                endTime = boostExplosionCooldownEndTime;
                break;


            case SkillCooldownType.LimitBreak:
                endTime = limitBreakCooldownEndTime;
                break;

            case SkillCooldownType.BlinkDash:
                endTime = blinkDashCooldownEndTime;
                break;

            case SkillCooldownType.OverloadBlast:
                endTime = overloadBlastCooldownEndTime;
                break;
        }

        return Mathf.Max(
            0f,
            endTime - Time.time
        );
    }


    // =========================================================
    // 모든 쿨타임 초기화
    // =========================================================

    public void ResetSkillCooldowns()
    {
        heavyStrikeCooldownEndTime = 0f;
        defenseCooldownEndTime = 0f;
        gravityBoostCooldownEndTime = 0f;

        overdriveCooldownEndTime = 0f;
        dashSlashCooldownEndTime = 0f;
        boostExplosionCooldownEndTime = 0f;

        limitBreakCooldownEndTime = 0f;
        blinkDashCooldownEndTime = 0f;
        overloadBlastCooldownEndTime = 0f;
    }


    // =========================================================
    // 전체 플레이어 상태 초기화
    // =========================================================

    public void ResetPlayerState()
    {
        playerHP = 100f;
        playerPhase = 1;

        ResetSkillCooldowns();
    }
}