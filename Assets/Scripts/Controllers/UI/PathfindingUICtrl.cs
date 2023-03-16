using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using Services;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainer;

namespace Controllers.UI
{
    public class PathfindingUICtrl : MonoBehaviour
    {
        [SerializeField] private Button regenerateMapButton;
        [SerializeField] private GameObject mainPanel;

        [UsedImplicitly] [Inject] private GameManager _gameManager;
        [UsedImplicitly] [Inject] private IReadOnlyAsyncReactiveProperty<GameState> _gameState;
        private void Awake()
        {
            regenerateMapButton.onClick.AddListener(_gameManager.RegenerateCurrentMap);
            var token = this.GetCancellationTokenOnDestroy();
            _gameState.Select(state => state == GameState.PathFinding).BindToActivity(mainPanel, token);
        }
    }
}