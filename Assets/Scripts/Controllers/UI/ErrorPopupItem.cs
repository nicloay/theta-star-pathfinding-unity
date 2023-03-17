using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils;

namespace Controllers.UI
{
    /// <summary>
    /// Error message Instance controller, Spawned through ObjectPool
    /// has reference to text box and contains animation methods
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ErrorPopupItem : MonoBehaviour, ISortingOrder
    {
        private const float DURATION = 3;
        private const float ALPHA_DURATION = 2;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Vector3 offset;
        [SerializeField] private Vector3 scale;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _rectTransform = (RectTransform)transform;
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public int SortingOrder
        {
            set => _canvas.sortingOrder = value;
        }

        public async UniTask ShowError(string error, CancellationToken token)
        {
            _canvasGroup.alpha = 1.0f;
            text.text = error;
            _rectTransform.localPosition = Vector3.zero;
            _rectTransform.localScale = Vector3.one;
            var sequence = DOTween.Sequence()
                .Append(_rectTransform.DOLocalMove(offset, DURATION))
                .Join(_rectTransform.DOScale(scale, DURATION))
                .Insert(DURATION - ALPHA_DURATION, _canvasGroup.DOFade(0f, ALPHA_DURATION));

            await sequence.WithCancellation(token);
        }
    }
}