using UnityEngine;

namespace Isometric2D
{
    public static class IsometricMath
    {
        public static bool IsInFrontOf(this IsometricObject obj1, IsometricObject obj2)
        {
            if (obj2.FloorTopCorner.x < obj1.FloorBottomCorner.x)
            {
                var topToRight = obj2.FloorRightCorner - obj2.FloorTopCorner;
                var topToObj1Left = obj1.FloorLeftCorner - obj2.FloorTopCorner;
            
                // 오른쪽 모서리 뒤쪽에 위치한 벡터는 뒤로 판단 
                if (Cross(topToRight.normalized, topToObj1Left.normalized) >= 0)
                    return false;
            }
            else
            {
                var topToLeft = obj2.FloorLeftCorner - obj2.FloorTopCorner;
                var topToObj1Right = obj1.FloorRightCorner - obj2.FloorTopCorner;
                
                // 왼쪽 모서리 뒤쪽에 위치한 벡터는 뒤로 판단
                if (Cross(topToLeft.normalized, topToObj1Right.normalized) <= 0)
                    return false;
            }
            
            // 바닥 안쪽 면으로 들어왔는지 한번 더 확인
            if (IsPolygonsOverlap(obj1.Floors, obj2.Floors))
                return obj1.FloorCenter.y < obj2.FloorCenter.y;

            return true;
        }
        
        public static bool IsOverlap(this IsometricObject obj1, IsometricObject obj2)
        {
            return IsPolygonsOverlap(obj1.Corners, obj2.Corners);
        }
        
        public static bool IsPolygonsOverlap(Vector2[] polygon1, Vector2[] polygon2)
        {
            // AABB를 이용한 빠른 겹침 검사
            if (!IsAABBOverlap(polygon1, polygon2))
            {
                // AABB가 겹치지 않으므로 다각형도 겹치지 않음
                return false;
            }

            // 모든 변에 대해 교차 검사
            for (var i = 0; i < polygon1.Length; i++)
            {
                var p1 = polygon1[i];
                var p2 = polygon1[(i + 1) % polygon1.Length];

                for (var j = 0; j < polygon2.Length; j++)
                {
                    var q1 = polygon2[j];
                    var q2 = polygon2[(j + 1) % polygon2.Length];

                    if (LineSegmentsIntersect(p1, p2, q1, q2))
                    {
                        // 변이 교차하므로 다각형이 겹침
                        return true;
                    }
                }
            }

            // 포함 관계 검사
            if (IsPointInPolygon(polygon1[0], polygon2) || IsPointInPolygon(polygon2[0], polygon1))
            {
                // 한 다각형의 꼭짓점이 다른 다각형 내부에 있으므로 겹침
                return true;
            }

            // 겹치지 않음
            return false;
        }
    
        private static bool IsAABBOverlap(Vector2[] polygon1, Vector2[] polygon2)
        {
            var bounds1 = GetPolygonBounds(polygon1);
            var bounds2 = GetPolygonBounds(polygon2);

            return bounds1.Overlaps(bounds2);
        }
        
        private static Rect GetPolygonBounds(Vector2[] polygon)
        {
            var minX = polygon[0].x;
            var maxX = polygon[0].x;
            var minY = polygon[0].y;
            var maxY = polygon[0].y;

            foreach (var point in polygon)
            {
                if (point.x < minX)
                    minX = point.x;
                else if (point.x > maxX)
                    maxX = point.x;

                if (point.y < minY)
                    minY = point.y;
                else if (point.y > maxY)
                    maxY = point.y;
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
                if (Mathf.Approximately(qpxr, 0))
                {
                    // 선분이 일직선상에 있음
                    // 겹치는지 확인
                    var t0 = Vector2.Dot(qp, r) / Vector2.Dot(r, r);
                    var t1 = t0 + Vector2.Dot(s, r) / Vector2.Dot(r, r);

                    if ((t0 >= 0 && t0 <= 1) || (t1 >= 0 && t1 <= 1) || (t0 <= 0 && t1 >= 1))
                    {
                        // 선분이 겹침
                        return true;
                    }

                    // 선분이 겹치지 않음
                    return false;
                }

                // 평행하지만 교차하지 않음
                return false;
            }

            var t = Cross(qp, s) / rxs;
            var u = Cross(qp, r) / rxs;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                // 선분이 교차함
                return true;
            }

            // 선분이 교차하지 않음
            return false;
        }
        
        private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            var inside = false;
            var n = polygon.Length;

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