using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using MessagePipe;
using Messages;
using UnityEngine;
using Utils;
using VContainer;

namespace Controllers.UI
{
    public class ErrorHandlerUICtrl : MonoBehaviour
    {
        [SerializeField] private ErrorPopupItem prefab;
        [SerializeField] private Transform spawnPoint;
        private CancellationToken _cancellationToken;

        private IDisposable _disposable;
        [Inject] [UsedImplicitly] private ISubscriber<MapError> _mapErrorPublisher;
        private ObjectPool<ErrorPopupItem> _pool;


        private void Awake()
        {
            _disposable = _mapErrorPublisher.Subscribe(error => ErrorHandler(error).Forget());
            _pool = new ObjectPool<ErrorPopupItem>(prefab, spawnPoint);
            _cancellationToken = this.GetCancellationTokenOnDestroy();
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }

        private async UniTaskVoid ErrorHandler(MapError error)
        {
            var instance = _pool.Get();
            await instance.ShowError($"ERROR: {error.Error.ToText()}", _cancellationToken);
            if (_cancellationToken.IsCancellationRequested) return;
            _pool.Release(instance);
        }
    }
}