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

        [Inject] [UsedImplicitly] private IAsyncReactiveProperty<IPathInputState> _inputState;

        private void Awake(){
            var token = this.GetCancellationTokenOnDestroy();
            _inputState.Select(status => status is InputBothPointsSet)
                .BindToActivity(runTestButton.gameObject, token);
            _inputState.Select(status => status is not InputBothPointsSet).BindToActivity(hintText.gameObject, token);
        }
    }
}