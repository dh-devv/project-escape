using System.Collections;
using UnityEngine;

namespace Carten
{
    public class HitStopController : MonoBehaviour
    {
        public static HitStopController Instance;
        private bool isWaiting = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 지정한 시간(초) 동안 게임을 잠시 멈춥니다. (타격감 연출용)
        /// </summary>
        /// <param name="duration">멈출 시간 (추천: 0.05 ~ 0.1)</param>
        public void TriggerHitStop(float duration)
        {
            if (isWaiting) return;
            StartCoroutine(CoHitStop(duration));
        }

        private IEnumerator CoHitStop(float duration)
        {
            isWaiting = true;
            Time.timeScale = 0f; // 게임 시간 정지

            // 타임스케일이 0이어도 정상적으로 시간이 흐르도록 Realtime 사용
            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = 1f; // 게임 시간 복구
            isWaiting = false;
        }
    }
}
