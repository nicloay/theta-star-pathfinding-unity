using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using VContainer.Unity;

namespace Services
{
    [UsedImplicitly]
    public class ScreenSizeMonitor : IStartable, ITickable
    {
        private readonly AsyncReactiveProperty<Resolution> _resolutionRP = new(Screen.currentResolution);
        private int _height;
        private int _width;

        public ScreenSizeMonitor()
        {
            UpdateResolution();
        }

        public IReadOnlyAsyncReactiveProperty<Resolution> Resolution => _resolutionRP;

        public void Start()
        {
            UpdateResolution();
        }

        public void Tick()
        {
            UpdateResolution();
        }

        private void UpdateResolution()
        {
            if (Screen.width == _width &&
                Screen.height == _height) return;
            _width = Screen.width;
            _height = Screen.height;
            _resolutionRP.Value = new Resolution
            {
                width = _width, height = _height
            };
        }
    }
}