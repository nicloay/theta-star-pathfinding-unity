using System;
using UnityEngine;

namespace Controllers
{
    [RequireComponent(typeof(Camera))]
    public class MapCameraCtrl : MonoBehaviour
    {
        private Camera _camera;
        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        public Vector3 GetGlobalPosition(Vector2Int vector)
        {
            return _camera.ScreenToWorldPoint(new Vector3(vector.x, vector.y, 5f));
        }
    }
}