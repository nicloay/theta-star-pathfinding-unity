using System;
using Cysharp.Threading.Tasks;

namespace Utils
{
    public class LevelGenerationProgress : IProgress<float> , IDisposable
    {
        private readonly AsyncReactiveProperty<float> _progress = new(0.0f);
        public IReadOnlyAsyncReactiveProperty<float> Progress => _progress;

        public void Report(float value)
        {
            _progress.Value = value;
        }

        public void Dispose()
        {
            _progress?.Dispose();
        }
    }
}