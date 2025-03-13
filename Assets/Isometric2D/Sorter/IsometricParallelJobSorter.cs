using System.Collections.Generic;
using System.Linq;
using Isometric2D.Jobs.Structs;
using Unity.Collections;
using UnityEngine.Jobs;

namespace Isometric2D
{
    public class IsometricParallelJobSorter : IsometricTopologySorter
    {
        protected override void TopologySort(List<IsometricObject> isometricObjects, ref HashSet<IsometricObject> rootObjects)
        {
            var safetyIsoObjects = new List<IsometricObject>(isometricObjects.Count);
            
            foreach (var isoObject in isometricObjects)
            {
                if (isoObject == null)
                    continue;

                isoObject.Fronts.Clear();
                isoObject.Backs.Clear();
                
                if (!isoObject.gameObject.activeSelf)
                    continue;
                
                safetyIsoObjects.Add(isoObject);
            }
            
            var isoObjFloorCorners = new NativeArray<Vector3Corners4>(safetyIsoObjects.Count, Allocator.TempJob);
            var isoObjIsoCorners = new NativeArray<Vector2Corners6>(safetyIsoObjects.Count, Allocator.TempJob);
            var isoObjTransformAccessArray = new TransformAccessArray(safetyIsoObjects.Count);
            
            var frontResults = new NativeMultiHashMap<int, int>(safetyIsoObjects.Count, Allocator.TempJob);
            var backResults = new NativeMultiHashMap<int, int>(safetyIsoObjects.Count, Allocator.TempJob);
            
            for (var i = 0; i < safetyIsoObjects.Count; i++)
            {
                isoObjFloorCorners[i] = new Vector3Corners4
                {
                    v0 = safetyIsoObjects[i].FloorTopCorner,
                    v1 = safetyIsoObjects[i].FloorRightCorner,
                    v2 = safetyIsoObjects[i].FloorBottomCorner,
                    v3 = safetyIsoObjects[i].FloorLeftCorner,
                };
                
                isoObjIsoCorners[i] = new Vector2Corners6
                {
                    v0 = safetyIsoObjects[i].Corners[0],
                    v1 = safetyIsoObjects[i].Corners[1],
                    v2 = safetyIsoObjects[i].Corners[2],
                    v3 = safetyIsoObjects[i].Corners[3],
                    v4 = safetyIsoObjects[i].Corners[4],
                    v5 = safetyIsoObjects[i].Corners[5],
                };
                
                isoObjTransformAccessArray.Add(safetyIsoObjects[i].transform);
            }
            
            var isometricParallelJob = new IsometricParallelJob
            {
                floorCorners = isoObjFloorCorners,
                isoCorners = isoObjIsoCorners,
                fronts = frontResults.AsParallelWriter(),
                backs = backResults.AsParallelWriter()
            };
            
            var jobHandle = isometricParallelJob.Schedule(isoObjTransformAccessArray);
            jobHandle.Complete();
            
            foreach (var keyValue in frontResults)
            {
                var key = keyValue.Key;
                var values = frontResults.GetValuesForKey(key);
                
                foreach (var value in values)
                    safetyIsoObjects[key].SetFront(safetyIsoObjects[value]);
            }
            
            foreach (var keyValue in backResults)
            {
                var key = keyValue.Key;
                var values = backResults.GetValuesForKey(key);

                foreach (var value in values)
                    safetyIsoObjects[key].SetBack(safetyIsoObjects[value]);
            }

            frontResults.Dispose();
            backResults.Dispose();
            isoObjTransformAccessArray.Dispose();
            isoObjIsoCorners.Dispose();
            isoObjFloorCorners.Dispose();
            
            foreach (var isometricObject in safetyIsoObjects.Where(x => x.Backs.Count == 0))
                rootObjects.Add(isometricObject);
        }
    }
}