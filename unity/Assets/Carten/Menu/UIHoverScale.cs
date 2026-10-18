using UnityEngine;
using UnityEngine.EventSystems;

namespace Carten
{
    public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("=== Scale Settings ===")]
        [SerializeField] private float hoverScale = 1.1f; // 마우스 올렸을 때 커질 크기 (1.1배)
        [SerializeField] private float transitionSpeed = 10f; // 부드러운 속도

        private Vector3 originalScale;
        private Vector3 targetScale;

        private void Awake()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
        }

        private void Update()
        {
            // 부드럽게 크기 보간 (Lerp)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = originalScale;
        }
    }
}
