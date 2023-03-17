using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DataModel;
using JetBrains.Annotations;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainer;

namespace Controllers.UI
{
    public class MapGenerationUICtrl : MonoBehaviour
    {
        [SerializeField] private GameObject _mainContainer;
        [SerializeField] private Scrollbar _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;

        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IGameState> _gameState;


        private void Awake()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _gameState.Select(state => state is GameStateMapGeneration).BindToActivity(_mainContainer, token);
            _gameState.ForEachAwaitWithCancellationAsync(BindProgressToUI, token);
        }


        private UniTask BindProgressToUI(IGameState gameState, CancellationToken token)
        {
            if (gameState is GameStateMapGeneration gameStateProgress)
            {
                gameStateProgress.Progress.Progress.BindToSize(_progressBar, token);
                gameStateProgress.Progress.Progress.Select(f => $"{(int)(f * 100f)}%").BindTo(_progressText, token);
            }
            return UniTask.CompletedTask;
        }
    }
}