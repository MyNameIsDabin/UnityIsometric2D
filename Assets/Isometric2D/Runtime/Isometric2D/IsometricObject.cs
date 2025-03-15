using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Isometric2D
{
    [ExecuteAlways]
    public class IsometricObject : MonoBehaviour
    {
        [SerializeField] private Vector2 extends;
        [SerializeField] private float height;
        
        private int _order;
        private bool _isDirty;
        private Vector2? _cachedPosition;
        private Vector2? _cachedExtends;
        private float _cachedHeight;
        
        public float Height => height;
        public Vector2 Extends => extends;

        // Top, Right, Bottom, Left
        private Vector2[] _floorCorners = new Vector2[4];
        
        // Top, Right Top, Right Bottom, Bottom, Left Bottom, Left Top
        public Vector2[] Corners { get; private set; } = new Vector2[6];
        
        public int Order
        {
            get => _order;
            set
            {
                _order = value;
                OnChangeOrder?.Invoke(_order);
            }
        }
        
        public HashSet<IsometricObject> Fronts { get; } = new();
        public HashSet<IsometricObject> Backs { get; } = new();
        
        public Vector3 FloorTopCorner => _floorCorners[0];
        public Vector3 FloorRightCorner => _floorCorners[1];
        public Vector3 FloorBottomCorner => _floorCorners[2];
        public Vector3 FloorLeftCorner => _floorCorners[3];
        public Vector3 FloorCenter => (FloorBottomCorner + FloorTopCorner) * 0.5f;
        public Vector2[] Floors => _floorCorners.Select(c => new Vector2(c.x, c.y)).ToArray();
        
        public event Action<int> OnChangeOrder;
        
        private void OnEnable()
        {
            IsometricWorld.Instance.AddIsometricObject(this);
            _isDirty = true;
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
            if (IsometricWorld.Instance == null || !IsometricWorld.Instance.IsDebugMode) 
                return;
            
            DrawIsometricGizmoDebug();
            DrawIsometricBody();
        }

        private void OnDrawGizmosSelected()
        {
            if (IsometricWorld.Instance != null && IsometricWorld.Instance.IsDebugMode) 
                return;
            
            DrawIsometricBody();
        }

        private void DrawIsometricGizmoDebug()
        {
            var isometricWorld = IsometricWorld.Instance;

            if (isometricWorld == null)
                return;
            
            foreach (var backObj in Backs)
            {
                if (!backObj.gameObject.activeSelf)
                    continue;
                
                var offset = Vector3.up * 0.08f;
                GizmoUtils.DrawVector(backObj.FloorCenter, 
                    FloorCenter,
                    isometricWorld.ArrowColor,
                    $"[{backObj.name} ▶ {gameObject.name}]", offset);
            }
            
            var isRoot = isometricWorld.IsometricSorter is IsometricTopologySorter topologySorter
                         && topologySorter.RootObjects.Contains(this);
            
            GizmoUtils.DrawText(FloorCenter + Vector3.up * 0.2f, Color.yellow, isRoot ? $"{Order} (Root | {gameObject.name})" : $"{Order}");
        }

        private void UpdateCorners(IsometricWorld isometricWorld)
        {
            Corners = isometricWorld.GetIsometricCubeWorldCorners(transform.position, extends, height, transform.lossyScale);

            var virtualHeight = Vector2.up * height;
            
            _floorCorners = new[]
            {
                Corners[0] - virtualHeight,
                Corners[1] - virtualHeight,
                Corners[3],
                Corners[4]
            };
        }

        private void Update()
        {
            var isometricWorld = IsometricWorld.Instance;

            if (isometricWorld == null)
                return;
            
            if (_cachedPosition != transform.position)
            {
                _cachedPosition = transform.position;
                _isDirty = true;
            }

            if (height != _cachedHeight)
            {
                _cachedHeight = height;
                _isDirty = true;
            }

            if (extends != _cachedExtends)
            {
                _cachedExtends = extends;
                _isDirty = true;
            }
            
            if (_isDirty)
                UpdateCorners(isometricWorld);
        }
        
        public void SetBack(IsometricObject isometricObject)
        {
            Backs.Add(isometricObject);
        }

        public void RemoveBack(IsometricObject isometricObject)
        {
            Backs.Remove(isometricObject);
        }
        
        public void SetFront(IsometricObject front)
        {
            Fronts.Add(front);
        }
        
        public void RemoveFront(IsometricObject front)
        {
            Fronts.Remove(front);
        }

        private void DrawIsometricBody()
        {
            var isometricWorld = IsometricWorld.Instance;

            if (isometricWorld == null || _floorCorners == null)
                return;

            var previousColor = Gizmos.color;

            var isConnected = isometricWorld.IsometricObjects.Any(x => x.Fronts.Contains(this))
                || isometricWorld.IsometricObjects.Any(x => x.Backs.Contains(this));
            var defaultColor = isometricWorld.DefaultColor;
            var gizmoColor = isConnected ? isometricWorld.LinkedColor : defaultColor;

            Gizmos.color = gizmoColor;

            if (height > 0)
            {
                var copied = gizmoColor;
                copied.a = defaultColor.a * 0.5f;

                var topCorners = new[]
                {
                    Corners[0], Corners[1], Corners[3] + Vector2.up * height, Corners[4] + Vector2.up * height
                };

                isometricWorld.DrawIsometricTile(_floorCorners, copied, gizmoColor, gizmoColor, copied);
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