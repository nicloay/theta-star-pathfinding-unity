using UnityEngine;

namespace Controllers
{
    /// <summary>
    /// Camera which render the map, also convert Vector2Int screen coordinates to global coordinates
    /// </summary>
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