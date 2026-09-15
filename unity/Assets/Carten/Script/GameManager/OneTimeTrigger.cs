using UnityEngine;

public class OneTimeTrigger : MonoBehaviour
{
    [Header("=== 설정 ===")]
    [SerializeField] private bool destroyAfterTrigger = false;

    private bool hasTriggered = false;

    /// <summary>
    /// 아직 발동되지 않았다면 true를 반환하고,
    /// 이미 발동했다면 false를 반환한다.
    /// </summary>
    public bool TryTrigger()
    {
        if (hasTriggered)
            return false;

        hasTriggered = true;
        return true;
    }

    /// <summary>
    /// 이미 발동했는지 확인
    /// </summary>
    public bool HasTriggered => hasTriggered;

    /// <summary>
    /// 트리거 상태 초기화
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    /// <summary>
    /// 트리거 발동 후 오브젝트를 제거하고 싶을 때
    /// </summary>
    public void TriggerAndDestroy()
    {
        if (!TryTrigger())
            return;

        Destroy(gameObject);
    }
}