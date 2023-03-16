using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Controllers.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ErrorPopupCtrl : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Vector3 offset;
        [SerializeField] private Vector3 scale;

        private const float DURATION = 3;
        private const float ALPHA_DURATION = 2;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        
        public int SortingOrder
        {
            set => _canvas.sortingOrder = value;
        }
        
        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _rectTransform = (RectTransform)transform;
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public async UniTask ShowError(string error, CancellationToken token)
        {
            _canvasGroup.alpha = 1.0f;
            text.text = error;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            var sequence = DOTween.Sequence()
                .Append(_rectTransform.DOLocalMove(offset, DURATION))
                .Join(_rectTransform.DOScale(scale, DURATION))
                .Insert(DURATION - ALPHA_DURATION, _canvasGroup.DOFade(0f, ALPHA_DURATION));
            
            await sequence.WithCancellation(token);
        }
    }
}