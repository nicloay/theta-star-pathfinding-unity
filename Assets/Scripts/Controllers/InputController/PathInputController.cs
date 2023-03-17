using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DataModel;
using JetBrains.Annotations;
using MapGenerator.MapData;
using MessagePipe;
using Messages;
using Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using VContainer;

namespace Controllers
{
    /// <summary>
    ///     We use MonoBehaviour.enable and OnEnable/OnDisable methods here
    ///     to control input (if it's active or not)
    ///     otherwise it's possible to make this class as POCO service
    /// </summary>
    public class PathInputController : MonoBehaviour
    {
        [Inject] [UsedImplicitly] private EventSystem _eventSystem;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IGameState> _gameState;
        [Inject] [UsedImplicitly] private IAsyncReactiveProperty<IPathInputState> _inputState;

        [Inject] [UsedImplicitly] private IAsyncSubscriber<MapError> _mapErrorSubscriber; // ISubscriber and IPublisher doesn't work at the same class, that's why Async here
        [Inject] [UsedImplicitly] private IPublisher<MapError> _mapErrorPublisher;

        private IDisposable _disposable;
        private void Awake()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _gameState.Select(state => state is GameStateMapReady).BindToEnableStatus(this, token);
            _disposable = _mapErrorSubscriber.Subscribe( (mapError, _) => 
            {
                if (mapError.Error == MapError.ErrorType.PathNotFound)
                {
                    _inputState.Value = new InputIdle();
                }
                return UniTask.CompletedTask;
            });
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            if (_eventSystem.IsPointerOverGameObject()) return;

            var mousePosition = Input.mousePosition;
            if (mousePosition.x < 0 || mousePosition.y < 0 || mousePosition.x >= Screen.width ||
                mousePosition.y >= Screen.height)
                return;


            var mapPosition = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);
            if (!((GameStateMapReady)_gameState.Value)!.RawMapData.IsPassable(mapPosition))
            {
                _mapErrorPublisher.Publish(new MapError(MapError.ErrorType.WrongPosition));
                return;
            }

            if (_inputState.Value is InputIdle or InputBothPointsSet)
                _inputState.Value = new InputFirstPointSet(mapPosition);
            else if (_inputState.Value is InputFirstPointSet firstPointSet)
                _inputState.Value = new InputBothPointsSet(firstPointSet, mapPosition);
        }

        private void OnEnable()
        {
            _inputState.Value = new InputIdle();
        }

        private void OnDisable()
        {
            _inputState.Value = new InputIdle();
        }
    }
}