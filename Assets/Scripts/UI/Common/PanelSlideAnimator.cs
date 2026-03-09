using UnityEngine;

namespace LAMENT
{
    /// <summary>
    /// RectTransform을 좌→우로 슬라이드(오프스크린→온스크린) + CanvasGroup 페이드.
    /// 모든 애니메이션은 Time.unscaledDeltaTime 사용(슬로모션 영향 X).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class PanelSlideAnimator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;   // Optional
        [SerializeField] private float duration = 0.18f;
        [SerializeField] private float offscreenX = -600f;
        [SerializeField] private float onscreenX = 0f;

        private RectTransform _rt;
        private Coroutine _co;

        private void Awake()
        {
            EnsureInit();
        }

        private void EnsureInit()
        {
            if (_rt == null) _rt = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void InstantHide()
        {
            EnsureInit();                      // ← 추가
            if (_co != null) StopCoroutine(_co);
            var p = _rt.anchoredPosition; 
            p.x = offscreenX; 
            _rt.anchoredPosition = p;
            if (canvasGroup) canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void Show()
        {
            EnsureInit();                      // ← 추가
            if (_co != null) StopCoroutine(_co);
            gameObject.SetActive(true);
            _co = StartCoroutine(CoSlide(offscreenX, onscreenX, 0f, 1f));
        }

        public void Hide()
        {
            EnsureInit();                      // ← 추가
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(CoSlide(_rt.anchoredPosition.x, offscreenX, canvasGroup ? canvasGroup.alpha : 1f, 0f, 
                () => gameObject.SetActive(false)));
        }

        public void NudgeTo(float x)
        {
            EnsureInit();                      // ← 추가
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(CoSlide(_rt.anchoredPosition.x, x, canvasGroup ? canvasGroup.alpha : 1f, 1f));
        }

        private System.Collections.IEnumerator CoSlide(float fromX, float toX, float fromA, float toA, System.Action onEnd = null)
        {
            EnsureInit();                      // ← 추가
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / duration), 3f);
                var p = _rt.anchoredPosition; 
                p.x = Mathf.Lerp(fromX, toX, k); 
                _rt.anchoredPosition = p;
                if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(fromA, toA, k);
                yield return null;
            }
            var end = _rt.anchoredPosition; end.x = toX; _rt.anchoredPosition = end;
            if (canvasGroup) canvasGroup.alpha = toA;
            onEnd?.Invoke();
            _co = null;
        }
    }
}