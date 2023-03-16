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
    /// TODO: rename or SRP
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class PathRenderController : MonoBehaviour
    {
        private enum Status
        {
            Pending,
            Point1,
            Point2,
            ShowPath
        }
        
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IMapData> _mapData;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<GameState> _gameState;
        [Inject] [UsedImplicitly] private IPublisher<MapError> _mapErrorPublisher;

        private Vector2Int _startingPoint;
        private Vector2Int _targetPoint;
        private Camera _camera;
        private ThetaStar _pathFinder;
        private LineRenderer _lineRenderer;
        private Status _status;
        
        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _camera = Camera.main;
            _mapData.ForEachAsync(UpdatePathFinding, this.GetCancellationTokenOnDestroy());
            
        }

        private void Update()
        {
            if (_pathFinder == null || _gameState.Value != GameState.PathFinding)
            {
                return;
            }

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
                case Status.Pending:
                    _startingPoint = mapPosition;
                    SetNextStatus(Status.Point1);
                    break;
                case Status.Point1:
                    _targetPoint = mapPosition;
                    SetNextStatus(Status.Point2);
                    break;
                default:
                    break;
            }
        }

        private void SetNextStatus(Status status)
        {
            _status = status;
            switch (status)
            {
                case Status.Pending:
                    //hide all points and path
                    break;
                case Status.Point1:
                    // show start point
                    break;
                case Status.Point2:
                    // shgow targetPoint
                    // set next status = show path
                    break;
                case Status.ShowPath:
                    GeneratePath();
                    // Generate path
                    // visualise
                    break;
            }
        }
        
        private void UpdatePathFinding(IMapData mapData)
        {
            if (mapData is not RawMapData data) return;
            _pathFinder = new ThetaStar(data.Map);
        }

        private void GeneratePath()
        {
            var path = _pathFinder.CalculatePath(_startingPoint, _targetPoint);
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("Can't find the path");
            }

            _lineRenderer.positionCount = path.Count;
            _lineRenderer.widthMultiplier = 0.1f;
            _lineRenderer.SetPositions(path.Select(i => _camera.ScreenToWorldPoint(new Vector3(i.x, i.y, 5f))).ToArray());
        }
    }
}