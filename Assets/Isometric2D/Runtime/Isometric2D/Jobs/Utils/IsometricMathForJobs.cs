using Isometric2D.Jobs.Structs;
using Unity.Burst;
using UnityEngine;

namespace Isometric2D
{
    [BurstCompile]
    public static class IsometricMathForJobs
    {
        public static bool IsInFrontOf(Vector2Corners4 obj1, Vector2Corners4 obj2)
        {
            var range = (obj1[0].y - obj1[2].y) + (obj2[0].y - obj2[2].y);
            var downVector = Vector2.down * range;
            
            if (obj1[3].x > obj2[3].x && obj1[1].x < obj2[1].x)
            {
                var leftCorner = obj1[3];
                var bottomCorner = obj1[2];
                var rightCorner = obj1[1];

                var rightToTop = obj2[0] - obj2[1];
                var leftToTop = obj2[0] - obj2[3];
                
                if (LineSegmentsIntersect(leftCorner, leftCorner + downVector, obj2[1], obj2[1] + rightToTop)
                    || LineSegmentsIntersect(leftCorner, leftCorner + downVector, obj2[3], obj2[3] + leftToTop))
                {
                    return false;
                }
            
                if (LineSegmentsIntersect(bottomCorner, bottomCorner + downVector, obj2[1], obj2[1] + rightToTop)
                    || LineSegmentsIntersect(bottomCorner, bottomCorner + downVector, obj2[3], obj2[3] + leftToTop))
                {
                    return false;
                }
            
                if (LineSegmentsIntersect(rightCorner, rightCorner + downVector, obj2[1], obj2[1] + rightToTop)
                    || LineSegmentsIntersect(rightCorner, rightCorner + downVector, obj2[3], obj2[3] + leftToTop))
                {
                    return false;
                }
            }
            else
            {
                var rightFaceCorners = new Vector2Corners4
                {
                    v0 = obj1[2],
                    v1 = obj1[1],
                    v2 = obj1[1] + Vector2.down * range,
                    v3 = obj1[2] + Vector2.down * range,
                };
            
                if (IsPointInPolygon(obj2[3], rightFaceCorners)
                    || IsPointInPolygon(obj2[1], rightFaceCorners)
                    || IsPointInPolygon(obj2[0], rightFaceCorners)
                    || IsPointInPolygon(obj2[2], rightFaceCorners))
                    return false;
            
                var leftFaceCorners = new Vector2Corners4
                {
                    v0 = obj1[3],
                    v1 = obj1[2],
                    v2 = obj1[2] + Vector2.down * range,
                    v3 = obj1[3] + Vector2.down * range,
                };
            
                if (IsPointInPolygon(obj2[3], leftFaceCorners)
                    || IsPointInPolygon(obj2[1], leftFaceCorners)
                    || IsPointInPolygon(obj2[2], leftFaceCorners)
                    || IsPointInPolygon(obj2[0], leftFaceCorners))
                    return false;   
            }

            return true;
        }

        public static bool IsPolygonsOverlap(Vector2Corners4 polygon1, Vector2Corners4 polygon2)
        {
            if (!IsAABBOverlap(polygon1, polygon2))
                return false;
            
            for (var i = 0; i < Vector2Corners4.Length; i++)
            {
                var p1 = polygon1[i];
                var p2 = polygon1[(i + 1) % Vector2Corners4.Length];
        
                for (var j = 0; j < Vector2Corners4.Length; j++)
                {
                    var q1 = polygon2[j];
                    var q2 = polygon2[(j + 1) % Vector2Corners4.Length];
        
                    if (LineSegmentsIntersect(p1, p2, q1, q2))
                        return true;
                }
            }
        
            return IsPointInPolygon(polygon1[0], polygon2) || IsPointInPolygon(polygon2[0], polygon1);
        }
        
        public static bool IsPolygonsOverlap(Vector2Corners6 polygon1, Vector2Corners6 polygon2)
        {
            if (!IsAABBOverlap(polygon1, polygon2))
                return false;
            
            for (var i = 0; i < Vector2Corners6.Length; i++)
            {
                var p1 = polygon1[i];
                var p2 = polygon1[(i + 1) % Vector2Corners6.Length];
        
                for (var j = 0; j < Vector2Corners6.Length; j++)
                {
                    var q1 = polygon2[j];
                    var q2 = polygon2[(j + 1) % Vector2Corners6.Length];
        
                    if (LineSegmentsIntersect(p1, p2, q1, q2))
                        return true;
                }
            }
        
            return IsPointInPolygon(polygon1[0], polygon2) || IsPointInPolygon(polygon2[0], polygon1);
        }
        
        private static bool IsAABBOverlap(Vector2Corners4 polygon1, Vector2Corners4 polygon2)
        {
            var bounds1 = GetPolygonBounds(polygon1);
            var bounds2 = GetPolygonBounds(polygon2);
        
            return bounds1.Overlaps(bounds2);
        }
        
        private static bool IsAABBOverlap(Vector2Corners6 polygon1, Vector2Corners6 polygon2)
        {
            var bounds1 = GetPolygonBounds(polygon1);
            var bounds2 = GetPolygonBounds(polygon2);
        
            return bounds1.Overlaps(bounds2);
        }

        private static Rect GetPolygonBounds(Vector2Corners4 polygon)
        {
            var minX = polygon[0].x;
            var maxX = polygon[0].x;
            var minY = polygon[0].y;
            var maxY = polygon[0].y;
        
            for (var i = 1; i < Vector2Corners4.Length; i++)
            {
                var x = polygon[i].x;
                var y = polygon[i].y;
        
                if (x < minX)
                    minX = x;
                else if (x > maxX)
                    maxX = x;
        
                if (y < minY)
                    minY = y;
                else if (y > maxY)
                    maxY = y;
            }
        
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        
        private static Rect GetPolygonBounds(Vector2Corners6 polygon)
        {
            var minX = polygon[0].x;
            var maxX = polygon[0].x;
            var minY = polygon[0].y;
            var maxY = polygon[0].y;
        
            for (var i = 1; i < Vector2Corners6.Length; i++)
            {
                var x = polygon[i].x;
                var y = polygon[i].y;
        
                if (x < minX)
                    minX = x;
                else if (x > maxX)
                    maxX = x;
        
                if (y < minY)
                    minY = y;
                else if (y > maxY)
                    maxY = y;
            }
        
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        
        private static bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            var r = p2 - p1;
            var s = q2 - q1;
        
            var rxs = Cross(r, s);
            var qp = q1 - p1;
            var qpxr = Cross(qp, r);
        
            if (Mathf.Approximately(rxs, 0))
            {
                if (!Mathf.Approximately(qpxr, 0)) 
                    return false;
                
                var t0 = Vector2.Dot(qp, r) / Vector2.Dot(r, r);
                var t1 = t0 + Vector2.Dot(s, r) / Vector2.Dot(r, r);
                    
                return t0 is >= 0 and <= 1 || t1 is >= 0 and <= 1 || !(t0 <= 0) || !(t1 >= 1);
            }
        
            var t = Cross(qp, s) / rxs;
            var u = Cross(qp, r) / rxs;
        
            return t is >= 0 and <= 1 && u is >= 0 and <= 1;
        }

        private static bool IsPointInPolygon(Vector2 point, Vector2Corners4 polygon)
        {
            var inside = false;
            var n = Vector2Corners4.Length;
        
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
            }
        
            return inside;
        }
        
        private static bool IsPointInPolygon(Vector3 point, Vector2Corners6 polygon)
        {
            var inside = false;
            var n = Vector2Corners6.Length;
        
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
            }
        
            return inside;
        }
        
        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
    }
}