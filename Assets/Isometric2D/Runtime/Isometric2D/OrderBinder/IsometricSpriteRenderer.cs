using UnityEngine;

namespace Isometric2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteAlways]
    public class IsometricSpriteRenderer : IsometricOrderBinder
    {
        private SpriteRenderer _spriteRenderer;

        private SpriteRenderer SpriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                    _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

                return _spriteRenderer;
            }
        }

        protected override void OnChangeOrder(int order)
        {
            SpriteRenderer.sortingOrder = order;
        }
        
        protected override bool OnShouldIgnoreSort()
        {
            return !gameObject.activeSelf || !SpriteRenderer.isVisible;
        }
    }
}