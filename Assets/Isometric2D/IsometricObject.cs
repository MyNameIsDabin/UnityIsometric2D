using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[ExecuteAlways]
public class IsometricBody : MonoBehaviour
{
    [SerializeField] private Vector2 extends;
    [SerializeField] private float virtual3dHeight;
    
    private void OnDrawGizmos()
    {
        DrawIsometricBody();
    }

    private void DrawIsometricBody()
    {
        var isometricWorld = IsometricWorld.Instance;

        if (isometricWorld == null)
            return;
        
        var previousColor = Gizmos.color;
        
        Gizmos.color = isometricWorld.TileColor;
        
        var floorCorners = isometricWorld.GetIsometricCorners(transform.position, extends);
        
        if (virtual3dHeight > 0)
        {
            var topCorners = isometricWorld.GetIsometricCorners(transform.position + Vector3.up * virtual3dHeight, extends);
            isometricWorld.DrawIsometricTile(floorCorners, Color.grey, Color.white, Color.white, Color.grey);
            isometricWorld.DrawIsometricTile(topCorners);
            
            Gizmos.DrawLine(floorCorners[1], topCorners[1]);
            Gizmos.DrawLine(floorCorners[2], topCorners[2]);
            Gizmos.DrawLine(floorCorners[3], topCorners[3]);
        }
        else
        {
            isometricWorld.DrawIsometricTile(floorCorners);
        }
        
        Gizmos.color = previousColor;
    }
}
