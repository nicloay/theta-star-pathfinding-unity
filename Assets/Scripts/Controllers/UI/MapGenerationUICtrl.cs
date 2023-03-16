using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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

        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<GameState> _gameState;
        [Inject] [UsedImplicitly] private LevelGenerationProgress _progress;

        [SerializeField] private GameObject _mainContainer;
        [SerializeField] private Scrollbar _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;
        
        
        private void Awake()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _gameState.Select(state =>
            {
                Debug.Log("new state = "+state);
                return state == GameState.MagGeneration;
            }).BindToActivity(_mainContainer, token);
            _progress.Progress.BindToSize(_progressBar, token);
            _progress.Progress.Select(f => $"{(int)(f * 100f)}%").BindTo(_progressText, token);
        }
    }
}