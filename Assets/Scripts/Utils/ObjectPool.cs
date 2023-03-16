using System.Collections.Generic;
using Controllers.UI;
using UnityEngine;

namespace Utils
{
    public class ObjectPool
    {
        private readonly ErrorPopupCtrl _prefab;
        private readonly Transform _spawnPoint;
        private readonly Queue<ErrorPopupCtrl> _instances = new();

        public ObjectPool(ErrorPopupCtrl prefab, Transform spawnPoint)
        {
            _prefab = prefab;
            _spawnPoint = spawnPoint;
        }
        private int numberOfSpawnedObjects = 0;
        private int sortingOrder = 0;
        public ErrorPopupCtrl Get()
        {
            if (numberOfSpawnedObjects == 0)
            {
                sortingOrder = 0;
            }
            
            ErrorPopupCtrl instance = null;
            if (_instances.Count > 0)
            {
                instance = _instances.Dequeue();
            }
            else
            {
                instance = Object.Instantiate(_prefab, _spawnPoint);
            }

            instance.transform.position = Vector3.zero;
            instance.gameObject.SetActive(true);
            instance.SortingOrder = sortingOrder++;
            numberOfSpawnedObjects++;
            return instance;
        }


        public void Release(ErrorPopupCtrl instance)
        {
            numberOfSpawnedObjects--;
            instance.gameObject.SetActive(false);
            _instances.Enqueue(instance);
        }
    }
}