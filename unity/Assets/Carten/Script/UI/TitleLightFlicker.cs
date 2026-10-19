using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Carten
{
    [RequireComponent(typeof(Light2D))]
    public class TitleLightFlicker : MonoBehaviour
    {
        [Header("조명 수치 설정")]
        [SerializeField] private float minIntensity = 0.2f;
        [SerializeField] private float maxIntensity = 1.8f;
        [SerializeField] private float flickerSpeedMin = 0.03f;
        [SerializeField] private float flickerSpeedMax = 0.15f;

        [Header("전력 끊김(암전) 순간 연출")]
        [SerializeField] private bool allowBlackout = true;
        [Range(0f, 1f)][SerializeField] private float blackoutChance = 0.12f;
        [SerializeField] private float blackoutMaxDuration = 0.3f;

        private Light2D targetLight;

        private void Awake()
        {
            targetLight = GetComponent<Light2D>();
        }

        private void Start()
        {
            StartCoroutine(FlickerRoutine());
        }

        private IEnumerator FlickerRoutine()
        {
            while (true)
            {
                if (targetLight == null) yield break;

                // 전력이 꺼졌다 들어오는 연출
                if (allowBlackout && Random.value < blackoutChance)
                {
                    targetLight.intensity = 0f;
                    yield return new WaitForSeconds(Random.Range(0.05f, blackoutMaxDuration));
                }
                else
                {
                    targetLight.intensity = Random.Range(minIntensity, maxIntensity);
                    yield return new WaitForSeconds(Random.Range(flickerSpeedMin, flickerSpeedMax));
                }
            }
        }
    }
}
