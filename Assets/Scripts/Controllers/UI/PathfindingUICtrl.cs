using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using Pathfinding;
using Services;
using TMPro;
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
        [SerializeField] private Button pathFindingFastButton;
        [SerializeField] private Button pathFindingSlowButton;
        [SerializeField] private TextMeshProUGUI pathfindingText;

        [UsedImplicitly] [Inject] private GameManager _gameManager;
        [UsedImplicitly] [Inject] private IReadOnlyAsyncReactiveProperty<GameState> _gameState;

        [UsedImplicitly] [Inject] private IAsyncReactiveProperty<PathFindingType> _pathFindingType;

        private void Awake()
        {
            regenerateMapButton.onClick.AddListener(_gameManager.RegenerateCurrentMap);
            var token = this.GetCancellationTokenOnDestroy();
            _gameState.Select(state => state == GameState.PathFinding).BindToActivity(mainPanel, token);

            _pathFindingType.ForEachAwaitAsync(async type =>
            {
                pathFindingFastButton.interactable = type == PathFindingType.Slow;
                pathFindingSlowButton.interactable = type == PathFindingType.Fast;
                // pathfindingText.text = "Pathfinding - " + (type == PathFindingType.Fast ? "Fast" : "Slow"); // you can do it this or see below
            }, token);
            _pathFindingType.Select(type => $"Pathfinding - {(type == PathFindingType.Fast ? "Fast" : "Slow")}")
                .BindTo(pathfindingText, token); // or this

            pathFindingFastButton.onClick.AddListener(() => _pathFindingType.Value = PathFindingType.Fast);
            pathFindingSlowButton.onClick.AddListener(() => _pathFindingType.Value = PathFindingType.Slow);
        }
    }
}