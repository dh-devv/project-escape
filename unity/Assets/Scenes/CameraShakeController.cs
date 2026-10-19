using System.Collections;
using UnityEngine;

namespace Carten
{
    public class CameraShakeController : MonoBehaviour
    {
        public static CameraShakeController Instance;

        private Coroutine shakeCoroutine;
        private Vector3 currentShakeOffset = Vector3.zero;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        // 기존 카메라 이동 로직이 끝난 직후(LateUpdate)에 흔들림 값을 살짝 얹어줍니다.
        private void LateUpdate()
        {
            if (currentShakeOffset != Vector3.zero)
            {
                transform.position += currentShakeOffset;
            }
        }

        /// <summary>
        /// 화면을 흔듭니다. (게임 방해를 줄이기 위해 시간이 지날수록 흔들림이 부드럽게 줄어듭니다)
        /// </summary>
        /// <param name="duration">흔들리는 시간 (추천: 0.15 ~ 0.25초)</param>
        /// <param name="magnitude">흔들리는 강도 (추천: 0.1 ~ 0.2)</param>
        public void TriggerShake(float duration = 0.15f, float magnitude = 0.12f)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(CoShake(duration, magnitude));
        }

        private IEnumerator CoShake(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // 시간이 지날수록 흔들림 강도가 1 -> 0으로 자연스럽게 줄어듦 (멀미 및 시야 방해 방지)
                float damper = 1.0f - Mathf.Clamp01(elapsed / duration);

                float offsetX = Random.Range(-1f, 1f) * magnitude * damper;
                float offsetY = Random.Range(-1f, 1f) * magnitude * damper;

                currentShakeOffset = new Vector3(offsetX, offsetY, 0f);

                yield return null;
            }

            currentShakeOffset = Vector3.zero;
            shakeCoroutine = null;
        }

        private void OnDisable()
        {
            currentShakeOffset = Vector3.zero;
        }
    }
}
