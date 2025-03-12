using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Isometric2D.Jobs.Structs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Debug = UnityEngine.Debug;

namespace Isometric2D
{
    [ExecuteAlways]
    public class IsometricWorld : MonoBehaviour
    {
        [Header("Isometric Settings")] 
        [SerializeField] private float tileWidth = 1f;
        [SerializeField] private float tileHeight = 0.5f;
        [SerializeField] private bool useJobSystem = true;
        
        [Header("Gizmo Settings")] 
        [SerializeField] private float sortAvgTime;
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
        
        private Stopwatch _sortStopWatch = new Stopwatch();
        private int _sortCallCount = 0;
        private float _sortAccElapsed = 0;
        
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
            
            Application.targetFrameRate = 60;
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
            foreach (var back in isometricObject.Backs)
                back.RemoveFront(isometricObject);
            
            foreach (var front in isometricObject.Fronts)
                front.RemoveBack(isometricObject);

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
        #if UNITY_EDITOR
            _sortCallCount++;
            _sortStopWatch.Restart();
        #endif

            RootObjects.Clear();
            Sorted.Clear();
            _visited.Clear();
            
            if (!useJobSystem)
            {
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

                    // Editor 환경에서 플레이 중이 아닐 때 수동으로 오브젝트를 제거한 경우를 대응하기 위해 수동으로 null 참조를 정리.
                    if (!Application.isPlaying)
                    {
                        foreach (var front in isoObj.Fronts.Where(front => front == null).ToList())
                            isoObj.RemoveFront(front);
                
                        foreach (var back in isoObj.Backs.Where(back => back == null).ToList())
                            isoObj.RemoveBack(back);                    
                    }

                    if (isoObj.Backs.Count == 0)
                        RootObjects.Add(isoObj);
                    
                    foreach (var rootBackObj in RootObjects)
                        InternalSearch(rootBackObj);
                }
                
                var order = 0;
                while (Sorted.TryPop(out var isoObj))
                {
                    isoObj.Order = order;
                    order++;
                }
            }
            else
            {
                var safetyIsoObjects = new List<IsometricObject>(IsometricObjects.Count);
                
                foreach (var isoObject in IsometricObjects)
                {
                    if (isoObject == null)
                        continue;

                    isoObject.Fronts.Clear();
                    isoObject.Backs.Clear();
                    
                    if (!isoObject.gameObject.activeSelf)
                        continue;
                    
                    safetyIsoObjects.Add(isoObject);
                }
                
                var isoObjFloorCorners = new NativeArray<Vector3Corners4>(safetyIsoObjects.Count, Allocator.TempJob);
                var isoObjIsoCorners = new NativeArray<Vector2Corners6>(safetyIsoObjects.Count, Allocator.TempJob);
                var isoObjTransformAccessArray = new TransformAccessArray(safetyIsoObjects.Count);
                
                var frontResults = new NativeMultiHashMap<int, int>(safetyIsoObjects.Count*2, Allocator.TempJob);
                var backResults = new NativeMultiHashMap<int, int>(safetyIsoObjects.Count*2, Allocator.TempJob);
                
                for (var i = 0; i < safetyIsoObjects.Count; i++)
                {
                    isoObjFloorCorners[i] = new Vector3Corners4
                    {
                        v0 = safetyIsoObjects[i].FloorTopCorner,
                        v1 = safetyIsoObjects[i].FloorRightCorner,
                        v2 = safetyIsoObjects[i].FloorBottomCorner,
                        v3 = safetyIsoObjects[i].FloorLeftCorner,
                    };
                    
                    isoObjIsoCorners[i] = new Vector2Corners6
                    {
                        v0 = safetyIsoObjects[i].Corners[0],
                        v1 = safetyIsoObjects[i].Corners[1],
                        v2 = safetyIsoObjects[i].Corners[2],
                        v3 = safetyIsoObjects[i].Corners[3],
                        v4 = safetyIsoObjects[i].Corners[4],
                        v5 = safetyIsoObjects[i].Corners[5],
                    };
                    
                    isoObjTransformAccessArray.Add(safetyIsoObjects[i].transform);
                }
                
                var isometricParallelJob = new IsometricParallelJob
                {
                    floorCorners = isoObjFloorCorners,
                    isoCorners = isoObjIsoCorners,
                    fronts = frontResults.AsParallelWriter(),
                    backs = backResults.AsParallelWriter()
                };
                
                var jobHandle = isometricParallelJob.Schedule(isoObjTransformAccessArray);
                jobHandle.Complete();
                
                foreach (var keyValue in frontResults)
                {
                    var key = keyValue.Key;
                    var values = frontResults.GetValuesForKey(key);
                    
                    foreach (var value in values)
                        safetyIsoObjects[key].SetFront(safetyIsoObjects[value]);
                }
                
                foreach (var keyValue in backResults)
                {
                    var key = keyValue.Key;
                    var values = backResults.GetValuesForKey(key);

                    foreach (var value in values)
                        safetyIsoObjects[key].SetBack(safetyIsoObjects[value]);
                }

                frontResults.Dispose();
                backResults.Dispose();
                isoObjTransformAccessArray.Dispose();
                isoObjIsoCorners.Dispose();
                isoObjFloorCorners.Dispose();
                
                foreach (var isometricObject in safetyIsoObjects.Where(x => x.Backs.Count == 0))
                    RootObjects.Add(isometricObject);
                
                foreach (var rootBackObj in RootObjects)
                    InternalSearch(rootBackObj);
                
                var order = 0;
                while (Sorted.TryPop(out var isoObj))
                {
                    isoObj.Order = order;
                    order++;
                }
            }

        #if UNITY_EDITOR
            _sortStopWatch.Stop();
            _sortAccElapsed += _sortStopWatch.Elapsed.Milliseconds;

            if (_sortCallCount >= 10)
            {
                sortAvgTime = _sortAccElapsed / _sortCallCount;   
                _sortAccElapsed = 0;
                _sortCallCount = 0;
            }
        #endif
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
