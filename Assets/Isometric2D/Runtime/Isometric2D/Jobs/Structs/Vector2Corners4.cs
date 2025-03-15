using UnityEngine;

namespace Isometric2D.Jobs.Structs
{
    [System.Serializable]
    public struct Vector2Corners4
    {
        public Vector2 v0;
        public Vector2 v1;
        public Vector2 v2;
        public Vector2 v3;
        
        public const int Length = 4;
        
        public Vector2 FloorCenter => (v2 + v1) * 0.5f;
        
        public Vector2[] Corners => new[]
        {
            v0, v1, v2, v3
        };
        
        public Vector2 this[int index]
        {
            get
            {
                switch(index)
                {
                    case 0: return v0;
                    case 1: return v1;
                    case 2: return v2;
                    case 3: return v3;
                    default: throw new System.IndexOutOfRangeException("Index must be between 0 and 3.");
                }
            }
            set
            {
                switch(index)
                {
                    case 0: v0 = value; break;
                    case 1: v1 = value; break;
                    case 2: v2 = value; break;
                    case 3: v3 = value; break;
                    default: throw new System.IndexOutOfRangeException("Index must be between 0 and 3.");
                }
            }
        }
    }
}