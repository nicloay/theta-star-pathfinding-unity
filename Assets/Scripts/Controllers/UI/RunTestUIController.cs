using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainer;

namespace Controllers.UI
{
    public class RunTestUIController : MonoBehaviour
    {
        [SerializeField] private GameObject hintText;
        [SerializeField] private Button runTestButton;

        [UsedImplicitly] [Inject] private IReadOnlyAsyncReactiveProperty<PathVisibleStatus> _pathVisible;

        private void Awake()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _pathVisible.Select(status => status == PathVisibleStatus.Yes)
                .BindToActivity(runTestButton.gameObject, token);
            _pathVisible.Select(status => status == PathVisibleStatus.No).BindToActivity(hintText.gameObject, token);
        }
    }
}