using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils;

namespace Controllers.UI
{
    public class VisualLoggerItem : MonoBehaviour, IResetSiblingIndex
    {
        private const float DURATION = 0.25f;
        private const float HIDDEN_OFFSET_X = -500f;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform slidingContainer;

        private CancellationTokenSource _ctx;
        private Vector2 _hiddenPosition;
        private Vector2 _originalPosition;
        private Vector2 _originalSize;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _originalSize = _rectTransform.sizeDelta;
            _originalPosition = _rectTransform.anchoredPosition;
            _hiddenPosition = new Vector2(HIDDEN_OFFSET_X, _originalPosition.y);
        }

        public async UniTaskVoid Show(string messageMessage, CancellationToken token)
        {
            text.text = messageMessage;
            _ctx = CancellationTokenSource.CreateLinkedTokenSource(token);

            _rectTransform.sizeDelta = new Vector2(_originalSize.x, 0);
            //slidingContainer.anchoredPosition = _hiddenPosition;
            var sequence = DOTween.Sequence()
                .Append(_rectTransform.DOSizeDelta(_originalSize, DURATION));
            // .Append(slidingContainer.DOAnchorPos(_originalPosition, DURATION));

            await sequence.WithCancellation(token);
            _ctx = null;
        }

        public async UniTask Hide(CancellationToken token)
        {
            _ctx?.Cancel();
            _ctx = null;
            var target = new Vector2(_originalSize.x, 0);
            await _rectTransform.DOSizeDelta(target, DURATION).ToUniTask(TweenCancelBehaviour.Kill, token);
        }
    }
}