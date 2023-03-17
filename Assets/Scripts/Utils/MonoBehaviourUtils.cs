using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public static class MonoBehaviourUtils
    {
        public static IEnumerable<T> FindComponents<T>(this Scene scene) where T : Component
        {
            if (!scene.IsValid())
            {
                Debug.LogError($"Wrong scene {scene} provided");
                return null;
            }

            return scene.GetRootGameObjects()
                .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<T>(true));
        }
    }
}