using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Utils;
using VContainer;

namespace Controllers
{
    /// <summary>
    /// Synchronise map scale with screen aspect through resolution property
    /// </summary>
    public class ResolutionToScaleCtrl : MonoBehaviour
    {
        [Inject]
        [UsedImplicitly]
        private void InjectResolution(IAsyncReactiveProperty<Resolution> resolution)
        {
            resolution.BindToScale(transform, this.GetCancellationTokenOnDestroy());
        }
    }
}