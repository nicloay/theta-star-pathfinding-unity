using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

public class TestReactiveProperty : MonoBehaviour
{
    private AsyncReactiveProperty<int> width = new AsyncReactiveProperty<int>(100);
    // Start is called before the first frame update
    void Start()
    {
        width.ForEachAwaitAsync(async i =>
        {
            await UniTask.Delay(500);
            Debug.Log($"New value = {i}");
        });
    }

    
    void Update()
    {
        if (Screen.width != width.Value)
        {
            width.Value = Screen.width;
        }
    }
}
