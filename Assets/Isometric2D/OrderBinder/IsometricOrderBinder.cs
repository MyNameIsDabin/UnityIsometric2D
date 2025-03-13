using UnityEngine;

namespace Isometric2D
{
    public abstract class IsometricOrderBinder : MonoBehaviour
    {
        [SerializeField] protected IsometricObject targetIsoObject;
        
        private void OnEnable()
        {
            targetIsoObject.OnChangeOrder += OnChangeOrder;
        }

        private void OnDisable()
        {
            targetIsoObject.OnChangeOrder -= OnChangeOrder;
        }

        protected virtual void OnChangeOrder(int order)
        {
            
        }
    }
}