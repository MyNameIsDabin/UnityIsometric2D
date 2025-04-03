using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Isometric2D
{
    [RequireComponent(typeof(SortingGroup))]
    [ExecuteAlways]
    public class IsometricSortingGroup : IsometricOrderBinder
    {
        private SortingGroup _sortingGroup;

        public SortingGroup SortingGroup
        {
            get
            {
                if (_sortingGroup == null)
                    _sortingGroup = gameObject.GetComponent<SortingGroup>();

                return _sortingGroup;
            }
        }

        private SpriteRenderer[] _spriteRenderers; 

        private void Awake()
        {
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
        protected override void OnChangeOrder(int order)
        {
            SortingGroup.sortingOrder = order;
        }

        protected override bool OnShouldIgnoreSort()
        {
            var isNotVisible = _spriteRenderers.Length > 0 && _spriteRenderers.All(x => !x.isVisible);
            
            return !gameObject.activeSelf || isNotVisible;
        }
    }
}