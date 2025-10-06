using Isometric2D.Jobs.Structs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

namespace Isometric2D
{
    [BurstCompile]
    public struct IsometricParallelJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<Vector2Corners4> floorCorners;
        [ReadOnly]
        public NativeArray<Vector2Corners6> isoCorners;
        
        public NativeParallelMultiHashMap<int, int>.ParallelWriter fronts;
        public NativeParallelMultiHashMap<int, int>.ParallelWriter backs;
        
        public void Execute(int index, TransformAccess transform)
        {
            var floors = floorCorners[index];
            var corners = isoCorners[index];
                
            for (var j = 0; j < floorCorners.Length; j++)
            {
                if (index == j)
                    continue;
                    
                var otherFloors = floorCorners[j];
                var otherCorners = isoCorners[j];

                if (!IsometricMathForJobs.IsPolygonsOverlap(corners, otherCorners)
                    || !IsometricMathForJobs.IsInFrontOf(floors, otherFloors)) 
                    continue;
                    
                fronts.Add(j, index);
                backs.Add(index, j);
            }
        }
    }
}