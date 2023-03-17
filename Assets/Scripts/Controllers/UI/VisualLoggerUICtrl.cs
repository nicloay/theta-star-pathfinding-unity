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
    public class VisualLoggerUICtrl : MonoBehaviour
    {
        [SerializeField] private VisualLoggerItem itemPrefab;
        [SerializeField] private Transform logContainer;
        [SerializeField] private int maxLogNumber = 5;
        private readonly Queue<VisualLoggerItem> _currentItems = new();
        private IDisposable _disposable;
        [UsedImplicitly] [Inject] private ISubscriber<VisualMessage> _messageSubscriber;


        private ObjectPool<VisualLoggerItem> _objectPool;

        private CancellationToken _tokenOnDestroy;

        private void Awake()
        {
            _tokenOnDestroy = this.GetCancellationTokenOnDestroy();
            _objectPool = new ObjectPool<VisualLoggerItem>(itemPrefab, logContainer);
            _disposable = _messageSubscriber.Subscribe(HandleMessage);
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void HandleMessage(VisualMessage message)
        {
            var instance = _objectPool.Get();
            instance.Show(message.Message, _tokenOnDestroy).Forget();
            _currentItems.Enqueue(instance);
            while (_currentItems.Count > maxLogNumber)
            {
                var log = _currentItems.Dequeue();
                HideLog(log).Forget();
            }
        }

        private async UniTaskVoid HideLog(VisualLoggerItem visualLoggerItem)
        {
            await visualLoggerItem.Hide(_tokenOnDestroy);
            _objectPool.Release(visualLoggerItem);
        }
    }
}