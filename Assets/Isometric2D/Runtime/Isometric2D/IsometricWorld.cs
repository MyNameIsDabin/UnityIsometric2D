using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Isometric2D
{
    [ExecuteAlways]
    public class IsometricWorld : MonoBehaviour
    {
        [Header("Isometric Settings")] 
        [SerializeField] private float tileWidth = 1f;
        [SerializeField] private float tileHeight = 0.57735f;
        [SerializeField] private IsometricSorterType sorterType = IsometricSorterType.JobSystem;
        [SerializeField] private bool culling;
        
        [Header("Gizmo Settings")] 
        [SerializeField] private float avgTimePer10Calls;
        [SerializeField] private int sortedObjectCount;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color linkedColor = Color.green;
        [SerializeField] private Color arrowColor = Color.yellow;
        [SerializeField] private bool updateOnEditor = true;
        [SerializeField] private bool debugMode = false;
        
        private Vector2[] _cachedIsometricIdentityCorners;
        private Vector2 _cachedDirectionToRightTop;
        private Vector2 _cachedDirectionToLeftTop;
        private Vector2 _cachedDirectionToLeftBottom;
        private Vector2 _cachedDirectionToRightBottom;
        private float _cachedTileWidth;
        private float _cachedTileHeight;
        
        private int _sortCallCount;
        private float _sortAccElapsed;
        private IIsometricSorter _isometricSorter;
        private IsometricSorterType _cachedIsometricSorterType;
        
        private readonly Stopwatch _sortStopWatch = new();
        private readonly List<IsometricObject> _isometricObjects = new();
        
        public Color DefaultColor => defaultColor;
        public Color LinkedColor => linkedColor;
        public Color ArrowColor => arrowColor;
        public bool IsDebugMode => debugMode;
        public bool UpdateOnEditor => updateOnEditor;

        public List<IsometricObject> IsometricObjects => _isometricObjects;
        
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
                #if UNITY_EDITOR
                if (_instance != null && _instance.gameObject.scene.IsValid())
                    return _instance;
                _instance = FindObjectsOfType<IsometricWorld>()
                    .FirstOrDefault(x => x.gameObject.scene.IsValid());
                #else
                if (_instance != null)
                    return _instance;
                _instance = FindObjectOfType<IsometricWorld>();
                #endif

                if (_instance != null)
                    return _instance;

                var inst = new GameObject("Isometric World").AddComponent<IsometricWorld>();
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
            if (Application.isPlaying || updateOnEditor)
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

        public void DrawIsometricTile(Vector2[] vertices, params Color[] colors)
        {
            var previousColor = Gizmos.color;

            for (var i = 0; i < 4; i++)
            {
                Gizmos.color = colors.Length > i ? colors[i] : previousColor;
                var start = vertices[i];
                var end = vertices[(i + 1) % 4];
                Gizmos.DrawLine(start, end);
            }

            Gizmos.color = previousColor;
        }

        public void SortIsometricObjects()
        {
        #if UNITY_EDITOR
            _sortCallCount++;
            _sortStopWatch.Restart();
        #endif

            if (culling)
            {
                var culledObjects = _isometricObjects
                    .Where(x => x.ShouldIgnoreSort == false)
                    .ToList();
                
        #if UNITY_EDITOR
                sortedObjectCount = culledObjects.Count;
        #endif   
                IsometricSorter.SortIsometricObjects(culledObjects);
            }
            else
            {
        #if UNITY_EDITOR
                sortedObjectCount = _isometricObjects.Count;
        #endif
                IsometricSorter.SortIsometricObjects(_isometricObjects);
            }
            
        #if UNITY_EDITOR
            _sortStopWatch.Stop();
            _sortAccElapsed += _sortStopWatch.Elapsed.Milliseconds;

            if (_sortCallCount >= 10)
            {
                avgTimePer10Calls = _sortAccElapsed / _sortCallCount;   
                _sortAccElapsed = 0;
                _sortCallCount = 0;
            }
        #endif
        }
    }
}
