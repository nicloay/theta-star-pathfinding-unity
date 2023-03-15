using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Services
{
    [UsedImplicitly]
    public class ScreenSizeMonitor : IStartable, ITickable, IDisposable
    {
        private const int DEBOUNCING_TIMEOUT_MS = 500;
        [Inject] [UsedImplicitly] private readonly IAsyncReactiveProperty<Resolution> _resolution;
        private int _previousHeight;
        private int _previousWidth;

        public void Start()
        {
            _previousHeight = Screen.height;
            _previousWidth = Screen.width;
            SetWidthHeight();
        }

        private CancellationTokenSource _ctx;
        public void Tick()
        {
            if (Screen.width == _previousWidth &&
                Screen.height == _previousHeight) return;
            
            _previousWidth = Screen.width;
            _previousHeight = Screen.height;
            
            _ctx?.Cancel();
            _ctx = new CancellationTokenSource();
            UpdateWithDelay(_ctx.Token).Forget();
        }

        private async UniTaskVoid UpdateWithDelay(CancellationToken token)
        {
            await UniTask.Delay(DEBOUNCING_TIMEOUT_MS, cancellationToken:token);
            if (token.IsCancellationRequested)
            {
                return;
            }
            SetWidthHeight();
            _ctx = null;
        }

        private void SetWidthHeight()
        {
            _resolution.Value = new Resolution
            {
                width = _previousWidth, height = _previousHeight
            };
        }

        public void Dispose()
        {
            _ctx?.Dispose();
        }
    }
}