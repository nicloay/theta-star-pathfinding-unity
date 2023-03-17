using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DataModel;
using JetBrains.Annotations;
using MessagePipe;
using Messages;
using Pathfinding;
using UnityEngine;
using VContainer;

namespace Controllers
{
    /// <summary>
    ///     TODO: rename or SRP
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class PathRenderController : MonoBehaviour
    {
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IGameState> _gameState;

        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IPathInputState> _inputState;
        private LineRenderer _lineRenderer;
        [Inject] [UsedImplicitly] private MapCameraCtrl _mapCamera;

        [Inject] [UsedImplicitly] private IPublisher<MapError> _mapErrorPublisher;
        [Inject] [UsedImplicitly] private IPublisher<VisualMessage> _messenger;

        private IPathFinder _pathFinder;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<PathFindingType> _pathFindingType;


        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            var token = this.GetCancellationTokenOnDestroy();
            _gameState.CombineLatest(_pathFindingType, (data, type) => data)
                .ToReadOnlyAsyncReactiveProperty(token)
                .ForEachAwaitAsync(UpdatePathFinding, token); // combine with pathfinding type to update backed algorithm
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

        private UniTask UpdatePathFinding(IGameState gameState)
        {
            if (gameState is not GameStateMapReady data) return UniTask.CompletedTask;

            if (_pathFindingType.Value == PathFindingType.Fast)
                _pathFinder = new ThetaStarOptimised(data.RawMapData.Map);
            else
                _pathFinder = new ThetaStar(data.RawMapData.Map);
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