using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DataModel;
using JetBrains.Annotations;
using MapGenerator.MapData;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Services
{
    public class GameManager : IStartable, IDisposable
    {
        private readonly CancellationTokenSource _selfTokenSource = new();
        [UsedImplicitly] [Inject] private IAsyncReactiveProperty<IGameState> _gameState;

        [UsedImplicitly] [Inject] private IReadOnlyAsyncReactiveProperty<Resolution> _resolution;

        private CancellationTokenSource mapGeneratorCTX;

        public void Dispose()
        {
            _selfTokenSource.Dispose();
            mapGeneratorCTX?.Dispose();
        }

        public void Start()
        {
            _resolution.ForEachAwaitAsync(async resolution => RegenerateMap(resolution), _selfTokenSource.Token);
        }

        public void RegenerateCurrentMap()
        {
            RegenerateMap(_resolution.Value);
        }

        private void RegenerateMap(Resolution resolution)
        {
            if (mapGeneratorCTX != null) mapGeneratorCTX.Cancel();
            mapGeneratorCTX = new CancellationTokenSource();
            GenerateNewMap(resolution.width, resolution.height, mapGeneratorCTX.Token).Forget();
        }

        private async UniTaskVoid GenerateNewMap(int width, int height, CancellationToken token)
        {
            Debug.Log("start new map generation");
            var progressState = new GameStateMapGeneration();
            _gameState.Value = progressState;

            var mapData = await RawMapData.Create(width, height, progressState.Progress, mapGeneratorCTX.Token);
            await UniTask.Delay(100, cancellationToken: token);
            _gameState.Value = token.IsCancellationRequested ? new GameStateNan() : new GameStateMapReady(mapData);
            mapGeneratorCTX = null;
        }
    }
}