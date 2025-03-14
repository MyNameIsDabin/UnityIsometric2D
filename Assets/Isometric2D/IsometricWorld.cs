using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Isometric2D
{
    [ExecuteAlways]
    public class IsometricWorld : MonoBehaviour
    {
        [Header("Isometric Settings")] 
        [SerializeField] private float tileWidth = 1f;
        [SerializeField] private float tileHeight = 0.5f;
        [SerializeField] private IsometricSorterType sorterType = IsometricSorterType.JobSystem;
        
        [Header("Gizmo Settings")] 
        [SerializeField] private float avgTimePer10Calls;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color linkedColor = Color.green;
        [SerializeField] private Color arrowColor = Color.yellow;
        [SerializeField] private bool debugMode = false;
        
        private Vector3[] _cachedIsometricIdentityCorners;
        private Vector3 _cachedDirectionToRightTop;
        private Vector3 _cachedDirectionToLeftTop;
        private Vector3 _cachedDirectionToLeftBottom;
        private Vector3 _cachedDirectionToRightBottom;
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
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<IsometricWorld>();

                if (_instance != null)
                    return _instance;

                var inst = new GameObject("Isometric World").AddComponent<IsometricWorld>();

                Debug.Log("Isometric World Created.", inst);
                return inst;
            }
        }

        private void Awake()
        {
            _instance = this;
            
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
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

        public Vector3[] IsometricIdentityCorners
        {
            get
            {
                if (_cachedIsometricIdentityCorners == default
                    || _cachedIsometricIdentityCorners.Length == 0)
                {
                    _cachedIsometricIdentityCorners = new[]
                    {
                        new Vector3(0, tileHeight / 2f, 0), // Top
                        new Vector3(tileWidth / 2f, 0, 0), // Right
                        new Vector3(0, -tileHeight / 2f, 0), // Bottom
                        new Vector3(-tileWidth / 2f, 0, 0) // Left
                    };
                }

                return _cachedIsometricIdentityCorners;
            }
        }

        public Vector3 IsoIdentityDirectionToRightTop
        {
            get
            {
                if (_cachedDirectionToRightTop == default)
                    _cachedDirectionToRightTop = (IsometricIdentityCorners[0] - IsometricIdentityCorners[3]);
                return _cachedDirectionToRightTop;
            }
        }

        public Vector3 IsoIdentityDirectionToLeftTop
        {
            get
            {
                if (_cachedDirectionToLeftTop == default)
                    _cachedDirectionToLeftTop = (IsometricIdentityCorners[0] - IsometricIdentityCorners[1]);
                return _cachedDirectionToLeftTop;
            }
        }

        public Vector3 IsoIdentityDirectionToLeftBottom
        {
            get
            {
                if (_cachedDirectionToLeftBottom == default)
                    _cachedDirectionToLeftBottom = (IsometricIdentityCorners[2] - IsometricIdentityCorners[1]);
                return _cachedDirectionToLeftBottom;
            }
        }

        public Vector3 IsoIdentityDirectionToRightBottom
        {
            get
            {
                if (_cachedDirectionToRightBottom == default)
                    _cachedDirectionToRightBottom = (IsometricIdentityCorners[1] - IsometricIdentityCorners[0]);
                return _cachedDirectionToRightBottom;
            }
        }

        public Vector3[] GetIsometricCorners(Vector3 worldPosition, Vector2 extends)
        {
            var extendToRightTop = IsoIdentityDirectionToRightTop * (extends[0] - 1.0f);
            var extendToLeftTop = IsoIdentityDirectionToLeftTop * (extends[1] - 1.0f);
            var identity = IsometricIdentityCorners;

            return new[]
            {
                identity[0] + worldPosition + extendToRightTop + extendToLeftTop, // Top
                identity[1] + worldPosition + extendToRightTop, // Right
                identity[2] + worldPosition, // Bottom
                identity[3] + worldPosition + extendToLeftTop // Left
            };
        }

        public void DrawIsometricTile(Vector3[] vertices, params Color[] colors)
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

        private void SortIsometricObjects()
        {
        #if UNITY_EDITOR
            _sortCallCount++;
            _sortStopWatch.Restart();
        #endif
            
            IsometricSorter.SortIsometricObjects(_isometricObjects);
 
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
