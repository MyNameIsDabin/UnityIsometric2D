using System;
using UnityEngine;

namespace Isometric2D
{
    public abstract class IsometricOrderBinder : MonoBehaviour
    {
        [SerializeField] protected IsometricObject targetIsoObject;

        private void Awake()
        {
            targetIsoObject.OnShouldIgnoreSort = OnShouldIgnoreSort;
        }

        private void OnEnable()
        {
            targetIsoObject.OnChangeOrder += OnChangeOrder;
            targetIsoObject.OnUpdateCorners += OnUpdateCorners;
        }

        private void OnDisable()
        {
            targetIsoObject.OnChangeOrder -= OnChangeOrder;
            targetIsoObject.OnUpdateCorners -= OnUpdateCorners;
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