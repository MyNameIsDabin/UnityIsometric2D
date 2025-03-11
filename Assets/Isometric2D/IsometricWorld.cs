using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Isometric2D
{
    [ExecuteAlways]
    public class IsometricWorld : MonoBehaviour
    {
        [Header("Isometric Settings")] 
        [SerializeField] private float tileWidth = 1f;
        [SerializeField] private float tileHeight = 0.5f;

        
        [FormerlySerializedAs("isoObjectColor")]
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color linkedColor = Color.green;
        [SerializeField] private Color arrowColor = Color.yellow;
        [SerializeField] private bool debugMode = false;
        
        private Vector3[] _cachedIsometricIdentityCorners;
        private Vector3 _cachedDirectionToRightTop;
        private Vector3 _cachedDirectionToLeftTop;
        private Vector3 _cachedDirectionToLeftBottom;
        private Vector3 _cachedDirectionToRightBottom;

        public Color DefaultColor => defaultColor;
        public Color LinkedColor => linkedColor;
        public Color ArrowColor => arrowColor;

        public List<IsometricObject> IsometricObjects { get; } = new();
        public HashSet<IsometricObject> RootObjects { get; } = new();

        private readonly HashSet<IsometricObject> _visited = new();
        
        private Stack<IsometricObject> Sorted { get; } = new();
        
        public bool IsDebugMode => debugMode;
        
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
        }

        private void Update()
        {
            SortIsometricObjects();
        }

        public void AddIsometricObject(IsometricObject isometricObject)
        {
            if (!IsometricObjects.Contains(isometricObject))
                IsometricObjects.Add(isometricObject);
        }

        public void RemoveIsometricObject(IsometricObject isometricObject)
        {
            IsometricObjects.Remove(isometricObject);
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
        
        public void SortIsometricObjects()
        {
            RootObjects.Clear();
            Sorted.Clear();
            _visited.Clear();

            for (var i = IsometricObjects.Count - 1; i >= 0; i--)
            {
                var isoObj = IsometricObjects[i];
                
                if (isoObj == null)
                {
                    IsometricObjects.Remove(isoObj);
                    continue;
                }
                
                if (!isoObj.gameObject.activeSelf)
                    continue;

                for (var j = IsometricObjects.Count - 1; j >= 0; j--)
                {
                    var otherIsoObj = IsometricObjects[j];

                    if (otherIsoObj == null)
                        IsometricObjects.Remove(otherIsoObj);

                    if (!otherIsoObj.gameObject.activeSelf
                        || isoObj == otherIsoObj
                        || !isoObj.IsOverlap(otherIsoObj)
                        || !isoObj.IsInFrontOf(otherIsoObj))
                    {
                        otherIsoObj.RemoveFront(isoObj);
                        isoObj.RemoveBack(otherIsoObj);
                        continue;
                    }

                    otherIsoObj.SetFront(isoObj);
                    isoObj.SetBack(otherIsoObj);
                }
                
                foreach (var front in isoObj.Fronts.Where(front => front == null).ToList())
                    isoObj.RemoveFront(front);
                
                foreach (var back in isoObj.Backs.Where(back => back == null).ToList())
                    isoObj.RemoveBack(back);
                
                if (isoObj.Backs.Count == 0)
                    RootObjects.Add(isoObj);
            }

            foreach (var rootBackObj in RootObjects)
                InternalSearch(rootBackObj);
            
            var order = 0;
            while (Sorted.TryPop(out var isoObj))
            {
                isoObj.Order = order;
                order++;
            }
        }
        
        private void InternalSearch(IsometricObject isometricObject)
        {
            if (!_visited.Add(isometricObject))
                return;
            
            foreach (var front in isometricObject.Fronts)
                InternalSearch(front);
            
            Sorted.Push(isometricObject);
        }
    }
}
