using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Isometric2D
{
    public class IsometricObject : MonoBehaviour
    {
        [SerializeField] private Vector2 extends;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float height;
        
        private int _order;
        private bool _isDirty;
        private Vector2? _cachedOffset;
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

        public bool ShouldIgnoreSort => OnShouldIgnoreSort?.Invoke() ?? true;
        
        public event Action<int> OnChangeOrder;
        public event Action OnUpdateCorners;

        public Func<bool> OnShouldIgnoreSort;
        
        private void OnEnable()
        {
            IsometricWorld.Instance.AddIsometricObject(this);
            _isDirty = true;
        }

        private void OnDisable()
        {
            if (IsometricWorld.HasInstance)
                IsometricWorld.Instance.RemoveIsometricObject(this);
        }

        public void UpdateCorners(IsometricWorld isometricWorld)
        {
            Corners = isometricWorld.GetIsometricCubeWorldCorners(transform.position + new Vector3(offset.x, offset.y), extends, height, transform.lossyScale);

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

            if (_cachedOffset != offset)
            {
                _cachedOffset = offset;
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
            {
                _isDirty = false;
                UpdateCorners(isometricWorld);
                OnUpdateCorners?.Invoke();
            }
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
    }
}