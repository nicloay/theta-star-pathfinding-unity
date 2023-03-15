using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using MapGenerator.MapData;
using Modules.MapGenerator;
using Modules.MapGenerator.MapData;
using Pathfinding;
using Services;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Controllers
{
    [RequireComponent(typeof(LineRenderer))]
    public class PathRenderController : MonoBehaviour
    {
        private readonly Vector2Int[] _screenPositions = new Vector2Int[3];
        private Camera _camera;
        private int _currentPosition = -1;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IMapData> _mapData;
        private ThetaStar _pathFinder;
        private LineRenderer _lineRenderer;
        
        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _camera = Camera.main;
            _mapData.ForEachAsync(SyncPathFinding, this.GetCancellationTokenOnDestroy());
        }

        private void Update()
        {
            if (_pathFinder == null)
            {
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                var mousePosition = Input.mousePosition;
                if (mousePosition.x < 0 || mousePosition.y < 0 || mousePosition.x >= Screen.width ||
                    mousePosition.y >= Screen.height)
                    return;

                _currentPosition++;
                _screenPositions[_currentPosition] = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);
                
                if (_currentPosition == 1)
                {
                    GeneratePath();
                    _currentPosition = -1;
                }
            }
        }

        private void SyncPathFinding(IMapData mapData)
        {
            if (mapData is not RawMapData data) return;
            _pathFinder = new ThetaStar(data.Map);
            _currentPosition = -1;
        }

        private void GeneratePath()
        {
            var path = _pathFinder.CalculatePath(_screenPositions[0], _screenPositions[1]);
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("Can't find the path");
            }

            _lineRenderer.positionCount = path.Count;
            _lineRenderer.widthMultiplier = 0.1f;
            _lineRenderer.SetPositions(path.Select(i => _camera.ScreenToWorldPoint(new Vector3(i.x, i.y, 5f))).ToArray());
        }

        private void OnDrawGizmos()
        {
            if (_camera == null)
            {
                return;
            }
            var from = new Vector3(_screenPositions[0].x, _screenPositions[0].y, 5);
            var to = new Vector3(_screenPositions[1].x, _screenPositions[1].y, 5);
            Gizmos.DrawSphere(_camera.ScreenToWorldPoint(from), 0.2f);
            Gizmos.DrawSphere(_camera.ScreenToWorldPoint(to), 0.2f);
        }
    }
}