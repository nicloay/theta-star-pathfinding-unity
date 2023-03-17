using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using MapGenerator.MapData;
using MessagePipe;
using Messages;
using Pathfinding;
using Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using VContainer;

namespace Controllers
{
    public enum PathVisibleStatus
    {
        No,
        Yes
    }

    /// <summary>
    ///     TODO: rename or SRP
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class PathRenderController : MonoBehaviour
    {
        [SerializeField] private GameObject pathStartObject;
        [SerializeField] private GameObject pathEndObject;
        private Camera _camera;
        private Vector2Int _endPoint;
        [Inject] [UsedImplicitly] private EventSystem _eventSystem;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<GameState> _gameState;
        [Inject] [UsedImplicitly] private IAsyncReactiveProperty<PathVisibleStatus> _isPathVisible;
        private LineRenderer _lineRenderer;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IMapData> _mapData;

        [Inject] [UsedImplicitly] private IPublisher<MapError> _mapErrorPublisher;
        [Inject] [UsedImplicitly] private IPublisher<VisualMessage> _messenger;

        private IPathFinder _pathFinder;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<PathFindingType> _pathFindingType;
        private Vector2Int _startingPoint;

        private readonly IAsyncReactiveProperty<PathStatus> _status =
            new AsyncReactiveProperty<PathStatus>(PathStatus.Pending);

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _camera = Camera.main;
            var token = this.GetCancellationTokenOnDestroy();
            _mapData.CombineLatest(_pathFindingType, (data, type) => true).ToReadOnlyAsyncReactiveProperty(token)
                .ForEachAwaitAsync(UpdatePathFinding, token);
            _isPathVisible.Select(visible => visible == PathVisibleStatus.Yes).BindToEnableStatus(_lineRenderer, token);
            _status.Select(status => status is PathStatus.Point1 or PathStatus.Point2)
                .BindToActivity(pathStartObject, token);
            _status.Select(status => status is PathStatus.Point2).BindToActivity(pathEndObject, token);
        }

        private void Update()
        {
            if (_pathFinder == null || _gameState.Value != GameState.PathFinding) return;

            if (!Input.GetMouseButtonDown(0)) return;

            if (_eventSystem.IsPointerOverGameObject()) return;

            var mousePosition = Input.mousePosition;
            if (mousePosition.x < 0 || mousePosition.y < 0 || mousePosition.x >= Screen.width ||
                mousePosition.y >= Screen.height)
                return;


            var mapPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);
            if (!_pathFinder.IsPassable(mapPosition))
            {
                _mapErrorPublisher.Publish(new MapError(MapError.ErrorType.WrongPosition));
                return;
            }

            switch (_status.Value)
            {
                case PathStatus.Point2:
                case PathStatus.Pending:
                    _startingPoint = mapPosition;
                    SetNextStatus(PathStatus.Point1);
                    break;
                case PathStatus.Point1:
                    _endPoint = mapPosition;
                    SetNextStatus(PathStatus.Point2);
                    break;
            }
        }

        private void SetNextStatus(PathStatus pathStatus)
        {
            _status.Value = pathStatus;
            switch (pathStatus)
            {
                case PathStatus.Pending:
                    _isPathVisible.Value = PathVisibleStatus.No;
                    break;
                case PathStatus.Point1:
                    _isPathVisible.Value = PathVisibleStatus.No;
                    pathStartObject.transform.position = GetGlobalPosition(_startingPoint);
                    break;
                case PathStatus.Point2:
                    pathEndObject.transform.position = GetGlobalPosition(_endPoint);
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var path = _pathFinder.CalculatePath(_startingPoint, _endPoint);
                    stopWatch.Stop();

                    if (path == null || path.Count == 0)
                    {
                        _mapErrorPublisher.Publish(new MapError(MapError.ErrorType.PathNotFound));
                        SetNextStatus(PathStatus.Pending);
                    }
                    else
                    {
                        _messenger.Publish(
                            new VisualMessage($"Path found in {stopWatch.ElapsedMilliseconds} milliseconds"));
                        UpdateLineRenderer(path);
                    }

                    break;
            }
        }

        private UniTask UpdatePathFinding(bool _)
        {
            if (_mapData.Value is not RawMapData data) return UniTask.CompletedTask;

            if (_pathFindingType.Value == PathFindingType.Fast)
                _pathFinder = new ThetaStarOptimised(data.Map);
            else
                _pathFinder = new ThetaStar(data.Map);
            SetNextStatus(PathStatus.Pending);
            return UniTask.CompletedTask;
        }

        private void UpdateLineRenderer(List<Vector2Int> path)
        {
            _isPathVisible.Value = PathVisibleStatus.Yes;
            _lineRenderer.positionCount = path.Count;
            _lineRenderer.widthMultiplier = 0.1f;
            _lineRenderer.SetPositions(path.Select(GetGlobalPosition).ToArray());
        }

        private Vector3 GetGlobalPosition(Vector2Int vector)
        {
            return _camera.ScreenToWorldPoint(new Vector3(vector.x, vector.y, 5f));
        }

        private enum PathStatus
        {
            Pending,
            Point1,
            Point2
        }
    }
}