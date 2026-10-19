using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Carten
{
    public class HitFeedbackController : MonoBehaviour
    {
        public static HitFeedbackController Instance { get; private set; }

        [Header("볼륨 참조")]
        [SerializeField] private Volume globalVolume;

        [Header("피격 연출 수치 설정")]
        [SerializeField] private float hitChromaticIntensity = 0.8f; // 피격 시 순간 색수차 강도
        [SerializeField] private float hitVignetteIntensity = 0.55f; // 피격 시 순간 비네트 강도
        [SerializeField] private Color hitVignetteColor = Color.red; // 피격 시 비네트 색상 (붉은색)

        // 평소 기본 수치 (복구용)
        private float defaultChromatic = 0.1f;
        private float defaultVignetteIntensity = 0.35f;
        private Color defaultVignetteColor = Color.black;

        private ChromaticAberration chromaticAberration;
        private Vignette vignette;
        private Coroutine hitRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (globalVolume == null)
                globalVolume = GetComponent<Volume>();

            InitVolumeComponents();
        }

        private void InitVolumeComponents()
        {
            if (globalVolume != null && globalVolume.profile != null)
            {
                if (globalVolume.profile.TryGet(out chromaticAberration))
                {
                    defaultChromatic = chromaticAberration.intensity.value;
                }

                if (globalVolume.profile.TryGet(out vignette))
                {
                    defaultVignetteIntensity = vignette.intensity.value;
                    defaultVignetteColor = vignette.color.value;
                }
            }
        }

        public void TriggerHitEffect(float duration = 0.2f)
        {
            if (hitRoutine != null)
                StopCoroutine(hitRoutine);

            hitRoutine = StartCoroutine(HitEffectRoutine(duration));
        }

        private IEnumerator HitEffectRoutine(float duration)
        {
            if (chromaticAberration == null && vignette == null) yield break;

            // 피격 순간 연출 적용
            if (chromaticAberration != null)
                chromaticAberration.intensity.value = hitChromaticIntensity;

            if (vignette != null)
            {
                vignette.intensity.value = hitVignetteIntensity;
                vignette.color.value = hitVignetteColor;
            }

            // 복구 연산
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (chromaticAberration != null)
                    chromaticAberration.intensity.value = Mathf.Lerp(hitChromaticIntensity, defaultChromatic, t);

                if (vignette != null)
                {
                    vignette.intensity.value = Mathf.Lerp(hitVignetteIntensity, defaultVignetteIntensity, t);
                    vignette.color.value = Color.Lerp(hitVignetteColor, defaultVignetteColor, t);
                }

                yield return null;
            }

            // 최종 복구
            if (chromaticAberration != null)
                chromaticAberration.intensity.value = defaultChromatic;

            if (vignette != null)
            {
                vignette.intensity.value = defaultVignetteIntensity;
                vignette.color.value = defaultVignetteColor;
            }
        }
    }
}
