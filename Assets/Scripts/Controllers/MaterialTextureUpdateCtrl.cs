using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Modules.MapGenerator.MapData;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;
using VContainer;

namespace Controllers
{
    [RequireComponent(typeof(Renderer))]
    public class MaterialTextureUpdateCtrl : MonoBehaviour
    {
        [UsedImplicitly] [Inject] private IReadOnlyAsyncReactiveProperty<IMapData> _mapData;

        private void Awake()
        {
            var renderer = GetComponent<Renderer>();
            Assert.IsNotNull(renderer);
            _mapData.BindMapDataToMainTex(renderer, this.GetCancellationTokenOnDestroy());
        }
    }
    
}