using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Isometric2D
{
    public partial class IsometricWorld : MonoBehaviour
    {
        [Header("Isometric Settings")] 
        [SerializeField] private float tileWidth = 1f;
        [SerializeField] private float tileHeight = 0.57735f;
        [SerializeField] private IsometricSorterType sorterType = IsometricSorterType.JobSystem;
        [SerializeField] private bool culling = true;
        
        private Vector2[] _cachedIsometricIdentityCorners;
        private Vector2 _cachedDirectionToRightTop;
        private Vector2 _cachedDirectionToLeftTop;
        private Vector2 _cachedDirectionToLeftBottom;
        private Vector2 _cachedDirectionToRightBottom;
        private float _cachedTileWidth;
        private float _cachedTileHeight;
        
        private IIsometricSorter _isometricSorter;
        private IsometricSorterType _cachedIsometricSorterType;
        
        private readonly List<IsometricObject> _isometricObjects = new();
        
        public IIsometricSorter IsometricSorter
        {
            get
            {
                var willBeUpdated = _isometricSorter == null || _cachedIsometricSorterType != sorterType;
                
                if (willBeUpdated)
                {
                    _cachedIsometricSorterType = sorterType;
                    _isometricSorter = IsometricSorterAttribute.CreateSorter(sorterType);
                }
                
                return _isometricSorter;
            }
        }
        
        private static IsometricWorld _instance;

        public static IsometricWorld Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                
                _instance = FindFirstObjectByType<IsometricWorld>();

                if (_instance != null)
                    return _instance;

                var inst = new GameObject(nameof(IsometricWorld)).AddComponent<IsometricWorld>();
                return inst;
            }
        }
        
        public static bool HasInstance => _instance != null;

        private void Awake()
        {
            if ((_instance != null && _instance != this))
            {
                DestroyImmediate(gameObject);
                return;
            }
            
            _instance = this;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void Update()
        {
            if (Application.isPlaying == false)
                return;
            
            SortIsometricObjects();
        }

        public void AddIsometricObject(IsometricObject isometricObject)
        {
            if (!_isometricObjects.Contains(isometricObject))
                _isometricObjects.Add(isometricObject);
        }

        public void RemoveIsometricObject(IsometricObject isometricObject)
        {
            foreach (var back in isometricObject.Backs)
                back.RemoveFront(isometricObject);
            
            foreach (var front in isometricObject.Fronts)
                front.RemoveBack(isometricObject);

            _isometricObjects.Remove(isometricObject);
        }

        public Vector2[] IsometricIdentityCorners
        {
            get
            {
                if (_cachedIsometricIdentityCorners == default
                    || _cachedIsometricIdentityCorners.Length == 0
                    || _cachedTileWidth != tileWidth
                    || _cachedTileHeight != tileHeight)
                {
                    _cachedTileWidth = tileWidth;
                    _cachedTileHeight = tileHeight;
                    _cachedDirectionToRightTop = default;
                    _cachedDirectionToLeftTop = default;
                    _cachedDirectionToLeftBottom = default;
                    _cachedDirectionToRightBottom = default;
                    
                    _cachedIsometricIdentityCorners = new[]
                    {
                        new Vector2(0, tileHeight / 2f), // Top
                        new Vector2(tileWidth / 2f, 0), // Right
                        new Vector2(0, -tileHeight / 2f), // Bottom
                        new Vector2(-tileWidth / 2f, 0) // Left
                    };
                }

                return _cachedIsometricIdentityCorners;
            }
        }

        public Vector2 IsoIdentityDirectionToRightTop
        {
            get
            {
                if (_cachedDirectionToRightTop == default)
                    _cachedDirectionToRightTop = (IsometricIdentityCorners[0] - IsometricIdentityCorners[3]);
                return _cachedDirectionToRightTop;
            }
        }

        public Vector2 IsoIdentityDirectionToLeftTop
        {
            get
            {
                if (_cachedDirectionToLeftTop == default)
                    _cachedDirectionToLeftTop = (IsometricIdentityCorners[0] - IsometricIdentityCorners[1]);
                return _cachedDirectionToLeftTop;
            }
        }

        public Vector2 IsoIdentityDirectionToLeftBottom
        {
            get
            {
                if (_cachedDirectionToLeftBottom == default)
                    _cachedDirectionToLeftBottom = (IsometricIdentityCorners[2] - IsometricIdentityCorners[1]);
                return _cachedDirectionToLeftBottom;
            }
        }

        public Vector2 IsoIdentityDirectionToRightBottom
        {
            get
            {
                if (_cachedDirectionToRightBottom == default)
                    _cachedDirectionToRightBottom = (IsometricIdentityCorners[1] - IsometricIdentityCorners[0]);
                return _cachedDirectionToRightBottom;
            }
        }
        
        public Vector2[] GetIsometricCubeWorldCorners(Vector2 worldPosition, Vector2 extends, float height, Vector2? lossyScale = null)
        {
            var extendToRightTop = IsoIdentityDirectionToRightTop * (extends[0] - 1.0f);
            var extendToLeftTop = IsoIdentityDirectionToLeftTop * (extends[1] - 1.0f);
            var identity = IsometricIdentityCorners;

            // Floor
            var floorTop = identity[0] + extendToRightTop + extendToLeftTop;
            var floorRight = identity[1] + extendToRightTop;
            var floorBottom = identity[2];
            var floorLeft = identity[3] + extendToLeftTop;
            
            height = lossyScale.HasValue ? Mathf.Abs(lossyScale.Value.y) * height : height;
            
            // Cube
            var virtualHeight = Vector2.up * height;
            
            if (lossyScale.HasValue)
            {
                floorTop *= lossyScale.Value;
                floorRight *= lossyScale.Value;
                floorLeft *= lossyScale.Value;
                floorBottom *= lossyScale.Value;

                if (floorRight.x < floorLeft.x)
                    (floorLeft, floorRight) = (floorRight, floorLeft);
            }
            
            var top = worldPosition + floorTop + virtualHeight;
            var rightTop =  worldPosition + floorRight + virtualHeight;
            var rightBottom = worldPosition + floorRight;
            var bottom = worldPosition + floorBottom;
            var leftBottom =  worldPosition + floorLeft;
            var leftTop =  worldPosition + floorLeft + virtualHeight;

            return new[]
            {
                top,
                rightTop,
                rightBottom,
                bottom,
                leftBottom,
                leftTop,
            };
        }

        public List<IsometricObject> GetIsometricObjects(bool isCull = false)
        {
#if UNITY_EDITOR
            var isoObjList = Application.isPlaying ? _isometricObjects : GetAllIsoObjectsInEditor();
#else
            var isoObjList = _isometricObjects;
#endif
            if (!isCull) 
                return isoObjList;
            
            var culledObjects = isoObjList
                .Where(x => x.ShouldIgnoreSort == false)
                .ToList();

            return culledObjects;
        }

        public void SortIsometricObjects()
        {
            var isometricObjects = GetIsometricObjects(culling);
#if UNITY_EDITOR
            sortedObjectCount = isometricObjects.Count;
#endif
            IsometricSorter.SortIsometricObjects(isometricObjects);
        }
    }
}
