using System.Collections.Generic;

namespace Isometric2D
{
    public abstract class IsometricTopologySorter : IIsometricSorter
    {
        private readonly HashSet<IsometricObject> _visited = new();
        private readonly Stack<IsometricObject> _sorted = new();
        private HashSet<IsometricObject> _rootObjects = new ();
        
        public HashSet<IsometricObject> RootObjects => _rootObjects;
        
        public void SortIsometricObjects(List<IsometricObject> isometricObjects)
        {
            _rootObjects.Clear();
            _visited.Clear();
            _sorted.Clear();

            TopologySort(isometricObjects, ref _rootObjects);
            
            foreach (var rootBackObj in _rootObjects)
                InternalSearch(rootBackObj);
            
            var order = 0;
            
            while (_sorted.TryPop(out var isoObj))
            {
                isoObj.Order = order;
                order++;
            }
        }

        protected abstract void TopologySort(List<IsometricObject> isometricObjects, ref HashSet<IsometricObject> rootObjects);

        private void InternalSearch(IsometricObject isometricObject)
        {
            if (!_visited.Add(isometricObject))
                return;
            
            foreach (var front in isometricObject.Fronts)
                InternalSearch(front);
            
            _sorted.Push(isometricObject);
        }
    }
}