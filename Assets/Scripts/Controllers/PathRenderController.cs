using System.Collections.Generic;
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
using VContainer;

namespace Controllers
{
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
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<GameState> _gameState;
        private LineRenderer _lineRenderer;

        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IMapData> _mapData;
        [Inject] [UsedImplicitly] private IPublisher<MapError> _mapErrorPublisher;
        private ThetaStar _pathFinder;


        private Vector2Int _startingPoint;
        private Status _status;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _camera = Camera.main;
            var token = this.GetCancellationTokenOnDestroy();
            _mapData.ForEachAwaitAsync(UpdatePathFinding, token);
            _gameState.ForEachAwaitAsync(OnGameStateChange, token);
        }

        private void Update()
        {
            if (_pathFinder == null || _gameState.Value != GameState.PathFinding) return;

            if (!Input.GetMouseButtonDown(0)) return;


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

            switch (_status)
            {
                case Status.Point2:
                case Status.Pending:
                    _startingPoint = mapPosition;
                    SetNextStatus(Status.Point1);
                    break;
                case Status.Point1:
                    _endPoint = mapPosition;
                    SetNextStatus(Status.Point2);
                    break;
            }
        }

        private UniTask OnGameStateChange(GameState gameState)
        {
            if (gameState != GameState.PathFinding)
            {
                pathStartObject.SetActive(false);
                pathEndObject.SetActive(false);
            }

            return UniTask.CompletedTask;
        }

        private void SetNextStatus(Status status)
        {
            _status = status;
            switch (status)
            {
                case Status.Pending:
                    _lineRenderer.enabled = false;
                    pathStartObject.SetActive(false);
                    pathEndObject.SetActive(false);
                    //hide all points and path
                    break;
                case Status.Point1:
                    _lineRenderer.enabled = false;
                    pathStartObject.SetActive(true);
                    pathEndObject.SetActive(false);
                    pathStartObject.transform.position = GetGlobalPosition(_startingPoint);
                    break;
                case Status.Point2:
                    pathEndObject.SetActive(true);
                    pathEndObject.transform.position = GetGlobalPosition(_endPoint);
                    var path = _pathFinder.CalculatePath(_startingPoint, _endPoint);
                    if (path == null || path.Count == 0)
                    {
                        _mapErrorPublisher.Publish(new MapError(MapError.ErrorType.PathNotFound));
                        SetNextStatus(Status.Pending);
                    }
                    else
                    {
                        UpdateLineRenderer(path);
                    }

                    break;
            }
        }

        private UniTask UpdatePathFinding(IMapData mapData)
        {
            if (mapData is not RawMapData data) return UniTask.CompletedTask;
            _pathFinder = new ThetaStar(data.Map);
            SetNextStatus(Status.Pending);
            return UniTask.CompletedTask;
        }

        private void UpdateLineRenderer(List<Vector2Int> path)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.positionCount = path.Count;
            _lineRenderer.widthMultiplier = 0.1f;
            _lineRenderer.SetPositions(path.Select(GetGlobalPosition).ToArray());
        }

        private Vector3 GetGlobalPosition(Vector2Int vector)
        {
            return _camera.ScreenToWorldPoint(new Vector3(vector.x, vector.y, 5f));
        }

        private enum Status
        {
            Pending,
            Point1,
            Point2
        }
    }
}