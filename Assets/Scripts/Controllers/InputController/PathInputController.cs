using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DataModel;
using JetBrains.Annotations;
using MessagePipe;
using Messages;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using VContainer;

namespace Controllers
{
    /// <summary>
    ///     State machine which Set InputState used by few modules
    /// 
    ///     We use MonoBehaviour.enable and OnEnable/OnDisable methods here
    ///     to control input (if it's active or not)
    ///     otherwise it's possible to make this class as POCO service
    /// </summary>
    public class PathInputController : MonoBehaviour
    {
        private IDisposable _disposable;
        [Inject] [UsedImplicitly] private EventSystem _eventSystem;
        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IGameState> _gameState;
        [Inject] [UsedImplicitly] private IAsyncReactiveProperty<IInputState> _inputState;
        [Inject] [UsedImplicitly] private IPublisher<MapError> _mapErrorPublisher;

        [Inject] [UsedImplicitly] private IAsyncSubscriber<MapError>
            _mapErrorSubscriber; // ISubscriber and IPublisher doesn't work at the same class, that's why Async here

        private void Awake()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _gameState.Select(state => state is GameStateMapReady).BindToEnableStatus(this, token);
            _disposable = _mapErrorSubscriber.Subscribe((mapError, _) =>
            {
                if (mapError.Error == MapError.ErrorType.PathNotFound) _inputState.Value = new InputIdle();
                return UniTask.CompletedTask;
            });
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

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}