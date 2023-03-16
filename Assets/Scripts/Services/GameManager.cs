using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using MapGenerator.MapData;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Unity;

namespace Services
{
    public enum GameState
    {
        Unknown,
        MagGeneration,
        PathFinding
    }
    
    public class GameManager : IStartable, IDisposable
    {
        [UsedImplicitly] [Inject] private IAsyncReactiveProperty<IMapData> _rawMapData;
        [UsedImplicitly] [Inject] private IAsyncReactiveProperty<GameState> _gameState ;
        [UsedImplicitly] [Inject] private IReadOnlyAsyncReactiveProperty<Resolution> _resolution;
        [UsedImplicitly] [Inject] private LevelGenerationProgress _progress;

        private CancellationTokenSource mapGeneratorCTX;
        private readonly CancellationTokenSource _selfTokenSource = new CancellationTokenSource();
        public void Start()
        {
            _resolution.ForEachAwaitAsync(async resolution => RegenerateMap(resolution), _selfTokenSource.Token);
        }

        private void  RegenerateMap(Resolution resolution)
        {
            if (mapGeneratorCTX != null)
            {
                mapGeneratorCTX.Cancel();
            }
            mapGeneratorCTX = new CancellationTokenSource();
            GenerateNewMap(resolution.width, resolution.height, mapGeneratorCTX.Token).Forget();
        }

        private async UniTaskVoid GenerateNewMap(int width, int height, CancellationToken token)
        {
            Debug.Log("start new map generation");
            _gameState.Value = GameState.MagGeneration;
            
            _rawMapData.Value = await RawMapData.Create(width, height, _progress, mapGeneratorCTX.Token);
            await UniTask.Delay(500, cancellationToken: token);
            _gameState.Value = token.IsCancellationRequested ? GameState.Unknown : GameState.PathFinding;
            mapGeneratorCTX = null;
        }
        
        public void Dispose()
        {
            _selfTokenSource.Dispose();
            mapGeneratorCTX?.Dispose();
        }
    }
}