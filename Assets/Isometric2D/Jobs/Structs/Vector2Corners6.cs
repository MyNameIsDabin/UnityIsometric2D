using UnityEngine;

namespace Isometric2D.Jobs.Structs
{
    [System.Serializable]
    public struct Vector2Corners6
    {
        public Vector2 v0;
        public Vector2 v1;
        public Vector2 v2;
        public Vector2 v3;
        public Vector2 v4;
        public Vector2 v5;
        
        public const int Length = 6;
        
        public Vector2[] Corners => new[]
        {
            v0, v1, v2, v3, v4, v5
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
                    case 4: return v4;
                    case 5: return v5;
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
                    case 4: v4 = value; break;
                    case 5: v5 = value; break;
                    default: throw new System.IndexOutOfRangeException("Index must be between 0 and 3.");
                }
            }
        }
    }
}