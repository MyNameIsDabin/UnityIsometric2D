using UnityEngine;

namespace Isometric2D
{
    public abstract class IsometricOrderBinder : MonoBehaviour
    {
        [SerializeField] protected IsometricObject targetIsoObject;
        
        private void OnEnable()
        {
            targetIsoObject.OnChangeOrder += OnChangeOrder;
            targetIsoObject.OnUpdateCorners += OnUpdateCorners;
            targetIsoObject.OnShouldIgnoreSort = OnShouldIgnoreSort;
        }

        private void OnDisable()
        {
            targetIsoObject.OnChangeOrder -= OnChangeOrder;
            targetIsoObject.OnUpdateCorners -= OnUpdateCorners;
            targetIsoObject.OnShouldIgnoreSort = null;
        }

        protected virtual void OnChangeOrder(int order)
        {
            
        }

        protected virtual void OnUpdateCorners()
        {
            
        }
        
        protected virtual bool OnShouldIgnoreSort()
        {
            return true;
        }
    }
}