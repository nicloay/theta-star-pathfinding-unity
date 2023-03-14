using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Services;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;
using VContainer;

namespace Controllers
{
    [RequireComponent(typeof(Renderer))]
    public class MaterialTextureUpdateCtrl : MonoBehaviour
    {
        [UsedImplicitly] [Inject] private MapGenerator _mapGenerator;

        private void Awake()
        {
            var renderer = GetComponent<Renderer>();
            Assert.IsNotNull(renderer);
            _mapGenerator.RawMapData.BindMapDataToMainTex(renderer, this.GetCancellationTokenOnDestroy());
        }
    }
    
}