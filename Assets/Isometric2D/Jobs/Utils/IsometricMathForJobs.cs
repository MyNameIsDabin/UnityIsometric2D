using Isometric2D.Jobs.Structs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Isometric2D
{
    [BurstCompile]
    public static class IsometricMathForJobs
    {
        public static bool IsInFrontOf(Vector3Corners4 obj1, Vector3Corners4 obj2)
        {
            if (obj2[0].x < obj1[2].x)
            {
                var dire1 = math.normalize(obj2[1] - obj2[0]);
                var dire2 = math.normalize(obj1[2] - obj2[0]);
            
                if (Cross(new Vector2(dire1.x, dire1.y), new Vector2(dire2.x, dire2.y)) >= 0)
                    return false;
            }
            else
            {
                var dire1 = math.normalize(obj2[3] - obj2[0]);
                var dire2 = math.normalize(obj1[2] - obj2[0]);
                
                if (Cross(new Vector2(dire1.x, dire1.y), new Vector2(dire2.x, dire2.y)) <= 0)
                    return false;
            }
            
            if (IsPolygonsOverlap(obj1, obj2))
                return obj1.FloorCenter.y < obj2.FloorCenter.y;
            
            return true;
        }

        public static bool IsPolygonsOverlap(Vector3Corners4 polygon1, Vector3Corners4 polygon2)
        {
            if (!IsAABBOverlap(polygon1, polygon2))
                return false;
            
            for (var i = 0; i < Vector3Corners4.Length; i++)
            {
                var p1 = polygon1[i];
                var p2 = polygon1[(i + 1) % Vector3Corners4.Length];
        
                for (var j = 0; j < Vector3Corners4.Length; j++)
                {
                    var q1 = polygon2[j];
                    var q2 = polygon2[(j + 1) % Vector3Corners4.Length];
        
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
        
        private static bool IsAABBOverlap(Vector3Corners4 polygon1, Vector3Corners4 polygon2)
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

        private static Rect GetPolygonBounds(Vector3Corners4 polygon)
        {
            var minX = polygon[0].x;
            var maxX = polygon[0].x;
            var minY = polygon[0].y;
            var maxY = polygon[0].y;
        
            for (var i = 1; i < Vector3Corners4.Length; i++)
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

        private static bool IsPointInPolygon(Vector2 point, Vector3Corners4 polygon)
        {
            var inside = false;
            var n = Vector3Corners4.Length;
        
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