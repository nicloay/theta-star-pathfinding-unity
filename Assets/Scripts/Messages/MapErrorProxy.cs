using System;
using JetBrains.Annotations;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace Messages
{
    /// <summary>
    ///     intercept map error and send visual message with it
    /// </summary>
    public class MapErrorProxy : IStartable, IDisposable
    {
        private IDisposable _disposable;
        [UsedImplicitly] [Inject] private ISubscriber<MapError> _mapErrorSubscriber;
        [UsedImplicitly] [Inject] private IPublisher<VisualMessage> _messagePublisher;

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public void Start()
        {
            _disposable = _mapErrorSubscriber.Subscribe(error =>
                _messagePublisher.Publish(new VisualMessage("ERROR: " + error.Error.ToText())));
        }
    }
}