using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Isometric2D
{
    public class GizmoUtils
    {
        public static void DrawVector(Vector3 start, Vector3 end, Color color, string name = null, Vector2? nameOffset = null)
        {
#if UNITY_EDITOR
            var origin = end - start;
            var angle = Mathf.Atan2(origin.y, origin.x);

            var headAngleL = angle + (160.0f * Mathf.Deg2Rad);
            var headAngleR = angle + (200.0f * Mathf.Deg2Rad);

            var arrowLength = Mathf.Min(origin.magnitude * 0.1f, 0.2f);
            var headLeft = new Vector3(Mathf.Cos(headAngleL), Mathf.Sin(headAngleL)).normalized * arrowLength;
            var headRight = new Vector3(Mathf.Cos(headAngleR), Mathf.Sin(headAngleR)).normalized * arrowLength;

            Debug.DrawLine(start, end, color);
            Debug.DrawLine(end, end + headLeft, color);
            Debug.DrawLine(end, end + headRight, color);

            if (!string.IsNullOrEmpty(name))
            {
                var style = new GUIStyle
                {
                    alignment = TextAnchor.LowerLeft
                };
                style.normal.textColor = color;
                Handles.Label(new Vector2(end.x, end.y) + (nameOffset ?? new Vector2(0.1f, 0.3f)), name, style);
            }
        }
        #endif
    }
}