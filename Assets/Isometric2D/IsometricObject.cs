using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Isometric2D
{
    [ExecuteAlways]
    public class IsometricObject : MonoBehaviour
    {
        [SerializeField] private Vector2 extends;
        [SerializeField] private float height;
        
        private Color _gizmosColor = Color.white;

        // Top, Right, Bottom, Left
        private Vector3[] _floorCorners = new Vector3[4];

        public float Height => height;
        public Vector3 FloorTopCorner => _floorCorners[0];
        public Vector3 FloorRightCorner => _floorCorners[1];
        public Vector3 FloorBottomCorner => _floorCorners[2];
        public Vector3 FloorLeftCorner => _floorCorners[3];

        public Vector2[] Corners { get; private set; } = new Vector2[6];

        private void OnEnable()
        {
            IsometricWorld.Instance.AddIsometricObject(this);
        }

        private void OnValidate()
        {
            IsometricWorld.Instance.AddIsometricObject(this);
        }

        private void OnDisable()
        {
            IsometricWorld.Instance.RemoveIsometricObject(this);
        }

        private void OnDrawGizmos()
        {
            DrawIsometricBody();
        }

        private void Update()
        {
            var isometricWorld = IsometricWorld.Instance;

            if (isometricWorld == null)
                return;
            
            _floorCorners = isometricWorld.GetIsometricCorners(transform.position, extends);

            var virtualHeight = Vector3.up * height;
            
            Corners[0] = _floorCorners[0] + virtualHeight; // Top
            Corners[1] = _floorCorners[1] + virtualHeight; // Right Top
            Corners[2] = _floorCorners[1]; // Right Bottom
            Corners[3] = _floorCorners[2]; // Bottom
            Corners[4] = _floorCorners[3]; // Left Bottom
            Corners[5] = _floorCorners[3] + virtualHeight; // Left Top
            
            var contacted = false;
            
            foreach (var isometricObject in isometricWorld.IsometricObjects)
            {
                if (isometricObject == this)
                    continue;
                
                if (!isometricObject.gameObject.activeSelf)
                    continue;
                
                if (isometricObject.IsOverlap(this))
                {
                    _gizmosColor = Color.green;
                    contacted = true;
                    break;
                }
            }

            if (!contacted)
                _gizmosColor = isometricWorld.TileColor;
        }

        private void DrawIsometricBody()
        {
            var isometricWorld = IsometricWorld.Instance;

            if (isometricWorld == null || _floorCorners == null)
                return;

            var previousColor = Gizmos.color;

            Gizmos.color = _gizmosColor;

            if (height > 0)
            {
                var topCorners = isometricWorld.GetIsometricCorners(transform.position + Vector3.up * height, extends);
                isometricWorld.DrawIsometricTile(_floorCorners, Color.grey, _gizmosColor, _gizmosColor, Color.grey);
                isometricWorld.DrawIsometricTile(topCorners);

                Gizmos.DrawLine(_floorCorners[1], topCorners[1]);
                Gizmos.DrawLine(_floorCorners[2], topCorners[2]);
                Gizmos.DrawLine(_floorCorners[3], topCorners[3]);
            }
            else
            {
                isometricWorld.DrawIsometricTile(_floorCorners);
            }

            Gizmos.color = previousColor;
        }
    }
}