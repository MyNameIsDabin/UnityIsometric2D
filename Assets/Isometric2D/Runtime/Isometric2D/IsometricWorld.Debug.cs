using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Isometric2D
{
    public partial class IsometricWorld
    {
        [Serializable]
        public class DebugSettings
        {
            public bool DrawDependency = true;
            public bool DrawIsoCorners = true;
            
            public Color DefaultColor = new Color(1f, 1f, 1f, 0.47f);
            public Color LinkedColor = new Color(0f, 1f, 0f, 0.5f);
            public Color ArrowColor = new(1f, 0.92f, 0.02f, 0.9f);
        }

        [Header("Editor Debug Settings")] 
        [SerializeField] private bool debugMode;
        [SerializeField] private DebugSettings debugSettings = new();
        [SerializeField] private int sortedObjectCount;
        
        private int _sortCallCount;
        private float _sortAccElapsed;
        private bool _isEditorDebugSortDirty;

        private static List<IsometricObject> _cachedEditorIsoObjects;
        private static List<IsometricOrderBinder> _cachedEditorIsoOrderBinders;
        
        private void OnDrawGizmos()
        {
            if (debugMode == false && !_isEditorDebugSortDirty)
                return;

            if (debugSettings == null)
                return;

            ForceSortIsoObjectsInEditor(x =>
            {
                if (x == null || x.gameObject == null) 
                    return;

                if (_isEditorDebugSortDirty) 
                    return;
                
                if (debugSettings.DrawDependency)
                    DrawIsometricDependency(x, x.TargetIsoObject);
                
                if (debugSettings.DrawIsoCorners)
                    DrawIsometricObject(x.TargetIsoObject);
            });
            
            _isEditorDebugSortDirty = false;
        }

        public void SetDirtyDebugSort()
        {
            _isEditorDebugSortDirty = true;
        }

        public void ForceSortIsoObjectsInEditor(Action<IsometricOrderBinder> onSort = null)
        {
            _cachedEditorIsoOrderBinders = GetAllIsoOrderBindersInEditor();
            
            SortIsometricObjects();

            foreach (var isoOrderBinder in _cachedEditorIsoOrderBinders)
            {
                if (isoOrderBinder == null || isoOrderBinder.gameObject.activeInHierarchy == false)
                    continue;

                if (isoOrderBinder.TargetIsoObject == null ||
                    isoOrderBinder.TargetIsoObject.gameObject.activeInHierarchy == false)
                    continue;

                var isoObj = isoOrderBinder.TargetIsoObject;

                if (Application.isPlaying == false)
                    isoObj.UpdateCorners(this);

                isoOrderBinder.ChangeOrder(isoObj.Order);
                onSort?.Invoke(isoOrderBinder);
            }
        }
        
        private void DrawIsometricDependency(IsometricOrderBinder binder, IsometricObject target)
        {
            foreach (var backObj in target.Backs)
            {
                if (!backObj.gameObject.activeSelf)
                    continue;
                
                var offset = Vector3.up * 0.08f;
                
                GizmoUtils.DrawVector(backObj.FloorCenter, 
                    target.FloorCenter,
                    debugSettings.ArrowColor,
                    $"[{backObj.name} ▶ {binder.name}]", offset);
            }
            
            var isRoot = IsometricSorter is IsometricTopologySorter topologySorter
                         && topologySorter.RootObjects.Contains(target);
            
            GizmoUtils.DrawText(target.FloorCenter + Vector3.up * 0.2f, Color.yellow, 
                isRoot ? $"{target.Order} (Root | {binder.name})" : $"{target.Order}");
        }
        
        private void DrawIsometricTile(Vector2[] vertices, params Color[] colors)
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

        private void DrawIsometricObject(IsometricObject target)
        {
            var isoObjects = _cachedEditorIsoObjects;
            
            if (isoObjects == null)
                return;
            
            var previousColor = Gizmos.color;

            var isConnected = isoObjects.Any(x => x.Fronts.Contains(target))
                              || isoObjects.Any(x => x.Backs.Contains(target));

            var defaultColor = debugSettings.DefaultColor;
            var gizmoColor = isConnected ? debugSettings.LinkedColor : defaultColor;

            Gizmos.color = gizmoColor;

            var floorCorners = target.Floors;
            
            if (target.Height > 0)
            {
                var copied = gizmoColor;
                copied.a = defaultColor.a * 0.5f;

                var topCorners = new[]
                {
                    target.Corners[0], target.Corners[1], target.Corners[3] + Vector2.up * target.Height, 
                    target.Corners[4] + Vector2.up * target.Height
                };

                DrawIsometricTile(floorCorners, copied, gizmoColor, gizmoColor, copied);
                DrawIsometricTile(topCorners);

                Gizmos.DrawLine(floorCorners[1], topCorners[1]);
                Gizmos.DrawLine(floorCorners[2], topCorners[2]);
                Gizmos.DrawLine(floorCorners[3], topCorners[3]);
            }
            else
            {
                DrawIsometricTile(floorCorners);
            }

            Gizmos.color = previousColor;
        }

        private static List<IsometricOrderBinder> GetAllIsoOrderBindersInEditor()
        {
            _cachedEditorIsoOrderBinders ??= new List<IsometricOrderBinder>();
            _cachedEditorIsoOrderBinders.Clear();
                
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                _cachedEditorIsoOrderBinders.AddRange(root.GetComponentsInChildren<IsometricOrderBinder>());
            
            return _cachedEditorIsoOrderBinders;
        }
        
        private static List<IsometricObject> GetAllIsoObjectsInEditor()
        {
            _cachedEditorIsoObjects ??= new List<IsometricObject>();
            _cachedEditorIsoObjects.Clear();
                
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                _cachedEditorIsoObjects.AddRange(root.GetComponentsInChildren<IsometricObject>());
            
            return _cachedEditorIsoObjects;
        }
    }
}
