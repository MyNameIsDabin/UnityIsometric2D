using UnityEngine;
using UnityEngine.Rendering;

namespace Isometric2D
{
    [RequireComponent(typeof(SortingGroup))]
    [ExecuteAlways]
    public class IsometricSortingGroup : IsometricOrderBinder
    {
        private SortingGroup _sortingGroup;

        private SortingGroup SortingGroup
        {
            get
            {
                if (_sortingGroup == null)
                    _sortingGroup = gameObject.GetComponent<SortingGroup>();

                return _sortingGroup;
            }
        }
        
        protected override void OnChangeOrder(int order)
        {
            SortingGroup.sortingOrder = order;
        }
    }
}