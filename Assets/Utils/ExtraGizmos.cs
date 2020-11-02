using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraGizmos : MonoBehaviour
{
    public static void DrawArrow(Vector2 start, Vector2 end)
    {
        Gizmos.DrawLine(start.x0y(), end.x0y());
        Vector2 dir = end - start;
        float rad = Mathf.Atan2(dir.y, dir.x);
        float len = Mathf.Clamp(dir.magnitude * .25f, .25f, 2);
        float flare = 30 * Mathf.Deg2Rad;
        Vector2 tipA = end - new Vector2(Mathf.Cos(rad + flare), Mathf.Sin(rad + flare)) * len;
        Vector2 tipB = end - new Vector2(Mathf.Cos(rad - flare), Mathf.Sin(rad - flare)) * len;
        Gizmos.DrawLine(end.x0y(), tipA.x0y());
        Gizmos.DrawLine(end.x0y(), tipB.x0y());
    }
}
