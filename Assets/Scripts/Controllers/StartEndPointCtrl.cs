using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;

namespace Controllers
{
    [RequireComponent(typeof(Renderer))]
    public class StartEndPointCtrl : MonoBehaviour
    {
        [UsedImplicitly]
        public enum PointType
        {
            Start,
            End
        }

        [SerializeField] private PointType type;

        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IPathInputState> _inputState;
        [Inject] [UsedImplicitly] private MapCameraCtrl _mapCamera;

        private Renderer _renderer;

        private void Awake()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _renderer = GetComponent<Renderer>();
            _inputState.ForEachAwaitWithCancellationAsync(UpdatePosition, token);
        }

        private UniTask UpdatePosition(IPathInputState mapPosition, CancellationToken token)
        {
            if (type == PointType.End)
            {
                _renderer.enabled = mapPosition is InputBothPointsSet;
                if (mapPosition is InputBothPointsSet bothPoints)
                    transform.position = _mapCamera.GetGlobalPosition(bothPoints.SecondPoint);
            }
            else
            {
                _renderer.enabled = mapPosition is not InputIdle;
                if (mapPosition is InputFirstPointSet firstPoint)
                    transform.position = _mapCamera.GetGlobalPosition(firstPoint.FirstPoint);
            }

            return UniTask.CompletedTask;
        }
    }
}