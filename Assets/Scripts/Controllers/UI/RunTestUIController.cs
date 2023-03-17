using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DataModel;
using JetBrains.Annotations;
using MessagePipe;
using Messages;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainer;
using System.Linq;
using System.Threading;

namespace Controllers.UI
{
    public class RunTestUIController : MonoBehaviour
    {
        [SerializeField] private GameObject hintText;
        [SerializeField] private Button runTestButton;

        [Inject] [UsedImplicitly] private IReadOnlyAsyncReactiveProperty<IGameState> _gameState;
        [Inject] [UsedImplicitly] private IAsyncReactiveProperty<IPathInputState> _inputState;
        [Inject] [UsedImplicitly] private IPublisher<VisualMessage> _messenger;

        private const int TEST_NUMBER = 16;
        
        private void Awake(){
            var token = this.GetCancellationTokenOnDestroy();
            _inputState.Select(status => status is InputBothPointsSet)
                .BindToActivity(runTestButton.gameObject, token);
            _inputState.Select(status => status is not InputBothPointsSet).BindToActivity(hintText.gameObject, token);
            
            runTestButton.onClick.AddListener(()=>
            {
                _ctx = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy()); 
                RunTest(_ctx.Token).Forget();
            });
        }

        private CancellationTokenSource _ctx;

        private async UniTaskVoid RunTest(CancellationToken token)
        {
            var bothPoints = (InputBothPointsSet)_inputState.Value;
            
            var stopWatch = new Stopwatch();
            var slow = new ThetaStar((_gameState.Value as GameStateMapReady)!.RawMapData.Map);
            var fast = new ThetaStarOptimised((_gameState.Value as GameStateMapReady)!.RawMapData.Map);
            var slowResults =await RunOn("SLOW", slow, bothPoints.FirstPoint, bothPoints.SecondPoint, stopWatch, token);
            if (token.IsCancellationRequested)return;
            var fastResult =await RunOn("FAST", fast, bothPoints.FirstPoint, bothPoints.SecondPoint, stopWatch, token);
            if (token.IsCancellationRequested)return;
            var avgSlow = slowResults.OrderBy(l => l).Skip(3).Take(10).Average(l => l);
            var avgFast = fastResult.OrderBy(l => l).Skip(3).Take(10).Average(l => l);
            _messenger.Publish(new VisualMessage($"Average SLOW: {avgSlow}"));
            _messenger.Publish(new VisualMessage($"Average FAST: {avgFast}"));
            _ctx = null;
        }

        private async UniTask<long[]> RunOn(string prefix, IPathFinder pathFinder, Vector2Int from, Vector2Int to, Stopwatch stopWatch, CancellationToken token)
        {
            var result = new long[TEST_NUMBER];
            for (int i = 0; i < TEST_NUMBER; i++)
            {
                await UniTask.Delay(1000, cancellationToken:token);
                if (token.IsCancellationRequested) break;
                stopWatch.Reset();
                stopWatch.Start();
                pathFinder.CalculatePath(from, to);
                if (token.IsCancellationRequested) break;
                stopWatch.Stop();
                _messenger.Publish(new VisualMessage($"{prefix} {i} = {stopWatch.ElapsedMilliseconds}"));
                result[i] = stopWatch.ElapsedMilliseconds;
            }

            return result;
        }
    }
}