using System;
using JetBrains.Annotations;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Services
{
    /// <summary>
    ///     log all visual messages to Debug.Log
    /// </summary>
    public class VisualMessageLogger : IStartable, IDisposable
    {
        private IDisposable _disposable;
        [UsedImplicitly] [Inject] private ISubscriber<VisualMessage> _messageReceiver;

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public void Start()
        {
            _disposable = _messageReceiver.Subscribe(message => Debug.Log(message.Message));
        }
    }
}