using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Isometric2D
{
    public class IsometricDefaultSorter : IsometricTopologySorter
    {
        protected override void TopologySort(List<IsometricObject> isometricObjects, ref HashSet<IsometricObject> rootObjects)
        {
            rootObjects.Clear();
            
            for (var i = isometricObjects.Count - 1; i >= 0; i--)
            {
                var isoObj = isometricObjects[i];
                
                if (isoObj == null)
                {
                    isometricObjects.Remove(isoObj);
                    continue;
                }
                
                if (!isoObj.gameObject.activeSelf)
                    continue;

                for (var j = isometricObjects.Count - 1; j >= 0; j--)
                {
                    var otherIsoObj = isometricObjects[j];

                    if (otherIsoObj == null)
                        isometricObjects.Remove(otherIsoObj);

                    if (!otherIsoObj.gameObject.activeSelf
                        || isoObj == otherIsoObj
                        || !isoObj.IsOverlap(otherIsoObj)
                        || !isoObj.IsInFrontOf(otherIsoObj))
                    {
                        otherIsoObj.RemoveFront(isoObj);
                        isoObj.RemoveBack(otherIsoObj);
                        continue;
                    }

                    otherIsoObj.SetFront(isoObj);
                    isoObj.SetBack(otherIsoObj);
                }

                // Editor 환경에서 플레이 중이 아닐 때 수동으로 오브젝트를 제거한 경우를 대응하기 위해 수동으로 null 참조를 정리. 런타임엔 발생하는 상황이 아님
                if (!Application.isPlaying)
                {
                    foreach (var front in isoObj.Fronts.Where(front => front == null).ToList())
                        isoObj.RemoveFront(front);
            
                    foreach (var back in isoObj.Backs.Where(back => back == null).ToList())
                        isoObj.RemoveBack(back);                    
                }

                if (isoObj.Backs.Count == 0)
                    rootObjects.Add(isoObj);
            }
        }
    }
}