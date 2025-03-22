using UnityEngine;

namespace Isometric2D
{
    public static class IsometricMath
    {
        public static bool IsInFrontOf(this IsometricObject obj1, IsometricObject obj2)
        {
            var obj1ToObj2 = obj1.FloorTopCorner.y - obj2.FloorBottomCorner.y;
            var obj2ToObj1 = obj2.FloorTopCorner.y - obj1.FloorBottomCorner.y;
            
            var range = obj1ToObj2 > obj2ToObj1 ? obj1ToObj2 : obj2ToObj1;
            
            var downVector = Vector2.down * (range);
            
            // 수직선상 교차 체크는 범위 안쪽으로 객체가 들어왔을 때만 
            if (obj1.FloorLeftCorner.x > obj2.FloorLeftCorner.x
                && obj1.FloorRightCorner.x < obj2.FloorRightCorner.x)
            {
                var leftCorner = new Vector2(obj1.FloorLeftCorner.x, obj1.FloorLeftCorner.y);
                var bottomCorner = new Vector2(obj1.FloorBottomCorner.x, obj1.FloorBottomCorner.y);
                var rightCorner = new Vector2(obj1.FloorRightCorner.x, obj1.FloorRightCorner.y);

                var rightToTop = obj2.FloorTopCorner - obj2.FloorRightCorner;
                var leftToTop = obj2.FloorTopCorner - obj2.FloorLeftCorner;
                
                // 비교 대상의 우측 상단 빗변과 하단 꼭짓점의 수직선상에 교차하는지 체크
                if (LineSegmentsIntersect(leftCorner, leftCorner + downVector, obj2.FloorRightCorner, obj2.FloorRightCorner + rightToTop)
                    || LineSegmentsIntersect(leftCorner, leftCorner + downVector, obj2.FloorLeftCorner, obj2.FloorLeftCorner + leftToTop))
                {
                    return false;
                }
            
                if (LineSegmentsIntersect(bottomCorner, bottomCorner + downVector, obj2.FloorRightCorner, obj2.FloorRightCorner + rightToTop)
                    || LineSegmentsIntersect(bottomCorner, bottomCorner + downVector, obj2.FloorLeftCorner, obj2.FloorLeftCorner + leftToTop))
                {
                    return false;
                }
            
                if (LineSegmentsIntersect(rightCorner, rightCorner + downVector, obj2.FloorRightCorner, obj2.FloorRightCorner + rightToTop)
                    || LineSegmentsIntersect(rightCorner, rightCorner + downVector, obj2.FloorLeftCorner, obj2.FloorLeftCorner + leftToTop))
                {
                    return false;
                }
            }
            else
            {
                // 이 경우는 꼭짓점이 안쪽에서 벗어난 경우, 면 하단에 체크 면적을 만들어서 비교 대상의 꼭짓점이 들어왔는지 확인
                var rightFaceCorners = new Vector2[]
                {
                    obj1.FloorBottomCorner,
                    obj1.FloorRightCorner,
                    obj1.FloorRightCorner + Vector3.down * range,
                    obj1.FloorBottomCorner + Vector3.down * range,
                };
            
                if (IsPointInPolygon(obj2.FloorLeftCorner, rightFaceCorners)
                    || IsPointInPolygon(obj2.FloorRightCorner, rightFaceCorners)
                    || IsPointInPolygon(obj2.FloorTopCorner, rightFaceCorners)
                    || IsPointInPolygon(obj2.FloorBottomCorner, rightFaceCorners))
                    return false;
            
                var leftFaceCorners = new Vector2[]
                {
                    obj1.FloorLeftCorner,
                    obj1.FloorBottomCorner,
                    obj1.FloorBottomCorner + Vector3.down * range,
                    obj1.FloorLeftCorner + Vector3.down * range,
                };
            
                if (IsPointInPolygon(obj2.FloorLeftCorner, leftFaceCorners)
                    || IsPointInPolygon(obj2.FloorRightCorner, leftFaceCorners)
                    || IsPointInPolygon(obj2.FloorBottomCorner, leftFaceCorners)
                    || IsPointInPolygon(obj2.FloorTopCorner, leftFaceCorners))
                    return false;   
            }

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
        
        public static Vector3? GetLineIntersection(Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2)
        {
            // 평행 여부 확인
            var denim = Vector3.Cross(d1, d2).magnitude;

            // 두 방향 벡터가 평행하면 교차점이 없다.
            if (denim == 0)
                return null;

            // 교차점을 찾기 위한 매개변수 계산
            var diff = p2 - p1;
            var t1 = Vector3.Cross(d2, diff).magnitude / denim;

            // 교차점을 반환
            var intersectionPoint = p1 + t1 * d1;
            return intersectionPoint;
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