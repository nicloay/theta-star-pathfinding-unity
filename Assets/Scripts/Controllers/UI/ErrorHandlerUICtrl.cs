using System;
using System.Collections.Generic;
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
        [Inject] [UsedImplicitly] private ISubscriber<MapError> _mapErrorPublisher;
        [SerializeField] private ErrorPopupCtrl prefab;
        [SerializeField] private Transform spawnPoint;
        
        private IDisposable _disposable;
        private ObjectPool _pool ;
        private CancellationToken _cancellationToken;

        private static readonly IReadOnlyDictionary<MapError.ErrorType, string> ERROR_DESCRIPTION =
            new Dictionary<MapError.ErrorType, string>()
            {
                { MapError.ErrorType.WrongPosition, "Wrong Position" },
                { MapError.ErrorType.PathNotFound, "Path Not Found" }
            };

        private void Awake()
        {
            _disposable = _mapErrorPublisher.Subscribe(error => ErrorHandler(error).Forget());
            _pool = new ObjectPool(prefab, spawnPoint);
            _cancellationToken = this.GetCancellationTokenOnDestroy();
        }

        private async UniTaskVoid ErrorHandler(MapError error)
        {
            var instance = _pool.Get();
            await instance.ShowError($"ERROR: {ERROR_DESCRIPTION[error.Error]}", _cancellationToken);
            if (_cancellationToken.IsCancellationRequested) return;
            _pool.Release(instance);
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }
    }
}