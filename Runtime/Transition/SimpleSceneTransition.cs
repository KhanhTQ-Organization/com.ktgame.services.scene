using UnityEngine;

namespace com.ktgame.services.scene
{
    public class SimpleSceneTransition : SceneTransition
    {
        [SerializeField] private TransitionType _type;
        [SerializeField] private float _delay;
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private EaseType _easeType = EaseType.Linear;
        [SerializeField] private float _beforeAlpha = 1.0f;
        [SerializeField] private float _afterAlpha = 1.0f;
        [SerializeField] private RectTransform _rectTransform;

        public override TransitionType Type => _type;
        public override float Duration => _delay + _duration;

        private CanvasGroup _canvasGroup;

        protected override void OnInitialize()
        {
            if (!_rectTransform.gameObject.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup = _rectTransform.gameObject.AddComponent<CanvasGroup>();
            }

            _canvasGroup = canvasGroup;
            _rectTransform.gameObject.SetActive(Type == TransitionType.Enter);
        }

        protected override void OnStart()
        {
            _rectTransform.gameObject.SetActive(true);
        }

        protected override void OnUpdate(float time)
        {
            time = Mathf.Max(0.0f, time - _delay);
            var progress = _duration <= 0.0f ? 1.0f : Mathf.Clamp01(time / _duration);
            progress = Easings.Interpolate(progress, _easeType);
            var alpha = Mathf.Lerp(_beforeAlpha, _afterAlpha, progress);
            _canvasGroup.alpha = alpha;
        }

        protected override void OnComplete()
        {
            _rectTransform.gameObject.SetActive(Type == TransitionType.Exit);
        }
    }
}