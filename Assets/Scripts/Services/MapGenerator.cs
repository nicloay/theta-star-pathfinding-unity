using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using Modules.MapGenerator;
using VContainer;
using VContainer.Unity;

namespace Services
{
    [UsedImplicitly]
    public class MapGenerator : IStartable, IDisposable
    {
        private readonly AsyncReactiveProperty<RawMapData> _rawMapData = new(null);
        private CancellationTokenSource _ctx;

        [Inject] [UsedImplicitly] private ScreenSizeMonitor _screenSizeMonitor;
        public IReadOnlyAsyncReactiveProperty<RawMapData> RawMapData => _rawMapData;

        public void Dispose()
        {
            _ctx?.Dispose();
        }

        public void Start()
        {
            _ctx = new CancellationTokenSource();
            _screenSizeMonitor.Resolution.ForEachAsync(
                    resolution => _rawMapData.Value = new RawMapData(resolution.width, resolution.height), _ctx.Token)
                .Forget();
        }
    }
}