using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using MapGenerator.MapData;
using MessagePipe;
using Messages;
using Pathfinding;
using Services;
using UnityEngine;
using Utils;
using VContainer;

namespace Controllers
{
    /// <summary>
    ///     TODO: rename or SRP
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class PathRenderController : MonoBehaviour
    {
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<GameState> _gameState;
        private LineRenderer _lineRenderer;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IMapData> _mapData;

        [Inject] [UsedImplicitly] private IPublisher<MapError> _mapErrorPublisher;
        [Inject] [UsedImplicitly] private IPublisher<VisualMessage> _messenger;

        private IPathFinder _pathFinder;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<PathFindingType> _pathFindingType;

        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IPathInputState> _inputState;
        [Inject] [UsedImplicitly] private MapCameraCtrl _mapCamera;
        
        
        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            var token = this.GetCancellationTokenOnDestroy();
            _mapData.CombineLatest(_pathFindingType, (data, type) => true).ToReadOnlyAsyncReactiveProperty(token)
                .ForEachAwaitAsync(UpdatePathFinding, token);
            _inputState.ForEachAwaitWithCancellationAsync(CalculatePathAndUpdateRenderer, token);
        }

        private UniTask CalculatePathAndUpdateRenderer(IPathInputState pathState, CancellationToken token)
        {
            if (pathState is InputBothPointsSet bothPoints)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var path = _pathFinder.CalculatePath(bothPoints.FirstPoint, bothPoints.SecondPoint);
                stopWatch.Stop();

                if (path == null || path.Count == 0)
                {
                    _mapErrorPublisher.Publish(new MapError(MapError.ErrorType.PathNotFound));
                }
                else
                {
                    _messenger.Publish(
                        new VisualMessage($"Path found in {stopWatch.ElapsedMilliseconds} milliseconds"));
                    UpdateLineRenderer(path);
                    return UniTask.CompletedTask;
                }
            }

            _lineRenderer.enabled = false;
            return UniTask.CompletedTask;
        }

        private UniTask UpdatePathFinding(bool _)
        {
            if (_mapData.Value is not RawMapData data) return UniTask.CompletedTask;

            if (_pathFindingType.Value == PathFindingType.Fast)
                _pathFinder = new ThetaStarOptimised(data.Map);
            else
                _pathFinder = new ThetaStar(data.Map);
            return UniTask.CompletedTask;
        }

        private void UpdateLineRenderer(List<Vector2Int> path)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.positionCount = path.Count;
            _lineRenderer.widthMultiplier = 0.1f;
            _lineRenderer.SetPositions(path.Select(_mapCamera.GetGlobalPosition).ToArray());
        }
    }
}