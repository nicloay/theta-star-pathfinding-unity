using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly Queue<T> _instances = new();
        private readonly T _prefab;
        private readonly Transform _spawnPoint;
        private int _numberOfSpawnedObjects;
        private int _sortingOrder;

        public ObjectPool(T prefab, Transform spawnPoint)
        {
            _prefab = prefab;
            _spawnPoint = spawnPoint;
        }

        public T Get()
        {
            T instance = null;
            if (_instances.Count > 0)
                instance = _instances.Dequeue();
            else
                instance = Object.Instantiate(_prefab, _spawnPoint);

            instance.transform.position = Vector3.zero;
            instance.gameObject.SetActive(true);
            if (instance is ISortingOrder sortingOrderSupport)
            {
                if (_numberOfSpawnedObjects == 0) _sortingOrder = 0;
                sortingOrderSupport.SortingOrder = _sortingOrder++;
            }

            if (instance is IResetSiblingIndex) instance.transform.SetSiblingIndex(0);
            _numberOfSpawnedObjects++;
            return instance;
        }


        public void Release(T instance)
        {
            _numberOfSpawnedObjects--;
            instance.gameObject.SetActive(false);
            _instances.Enqueue(instance);
        }
    }
}