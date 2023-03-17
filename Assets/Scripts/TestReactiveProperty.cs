using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

public class TestReactiveProperty : MonoBehaviour
{
    private readonly AsyncReactiveProperty<int> width = new(100);

    // Start is called before the first frame update
    private void Start()
    {
        width.ForEachAwaitAsync(async i =>
        {
            await UniTask.Delay(500);
            Debug.Log($"New value = {i}");
        });
    }


    private void Update()
    {
        if (Screen.width != width.Value) width.Value = Screen.width;
    }
}