using Cysharp.Threading.Tasks;
using DataModel;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;
using VContainer;

namespace Controllers
{
    /// <summary>
    /// Controller which request and update texture on the screen from the raw map data when it's ready
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class MaterialTextureUpdateCtrl : MonoBehaviour
    {
        [UsedImplicitly] [Inject] private IReadOnlyAsyncReactiveProperty<IGameState> _gameState;

        private void Awake()
        {
            var r = GetComponent<Renderer>();
            Assert.IsNotNull(r);
            _gameState.BindMapDataToMainTex(r, this.GetCancellationTokenOnDestroy());
        }
    }
}