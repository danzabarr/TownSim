using System.Collections.Generic;
using UnityEngine;

public static class Geom2D
{
    public static readonly float TAU = Mathf.PI * 2;
    public static float Wrap(float radians) => (radians % TAU + TAU) % TAU;

    public enum PolygonBoundsIntersectionType
    {
        None,
        BoundsContainsPolygonPoint,
        PolygonContainsBoundsCorner,
        LineSegmentsIntersect
    }

    public static bool CircleContainsPoint(Vector2 center, float radius, Vector2 testPoint)
    {
        return (testPoint - center).sqrMagnitude <= radius * radius;
    }

    public static bool PolygonContainsPoint(Vector2[] polygon, Vector2 testPoint)
    {
        bool result = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
            {
                if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        return result;
    }

    public static bool CircleIntersectsBounds(Vector2 center, float radius, Vector2 boundsMin, Vector2 boundsMax)
    {
        Rect rect1 = new Rect(boundsMin.x, boundsMin.y - radius, boundsMax.x - boundsMin.x, boundsMax.y - boundsMin.y + radius * 2);
        Rect rect2 = new Rect(boundsMin.x - radius, boundsMin.y, boundsMax.x - boundsMin.x + radius * 2, boundsMax.y - boundsMin.y);

        if (rect1.Contains(center))
            return true;

        if (rect2.Contains(center))
            return true;

        if (CircleContainsPoint(boundsMin, radius, center))
            return true;

        if (CircleContainsPoint(boundsMax, radius, center))
            return true;

        if (CircleContainsPoint(new Vector2(boundsMin.x, boundsMax.y), radius, center))
            return true;

        if (CircleContainsPoint(new Vector2(boundsMax.x, boundsMin.y), radius, center))
            return true;

        return false;
    }

    public static bool PolygonIntersectsBounds(Vector2[] polygon, Vector2 boundsMin, Vector2 boundsMax, out PolygonBoundsIntersectionType intersectionType)
    {
        Vector2 c00 = new Vector3(boundsMin.x, boundsMin.y);
        Vector2 c01 = new Vector3(boundsMin.x, boundsMax.y);
        Vector2 c11 = new Vector3(boundsMax.x, boundsMax.y);
        Vector2 c10 = new Vector3(boundsMax.x, boundsMin.y);

        intersectionType = PolygonBoundsIntersectionType.BoundsContainsPolygonPoint;

        foreach (Vector2 p in polygon)
        {
            if (p.x > boundsMin.x && p.x <= boundsMax.x &&
                p.y > boundsMin.y && p.y <= boundsMax.y)
            {
                return true;
            }
        }

        intersectionType = PolygonBoundsIntersectionType.PolygonContainsBoundsCorner;

        if (PolygonContainsPoint(polygon, c00))
            return true;

        if (PolygonContainsPoint(polygon, c01))
            return true;

        if (PolygonContainsPoint(polygon, c11))
            return true;

        if (PolygonContainsPoint(polygon, c10))
            return true;

        intersectionType = PolygonBoundsIntersectionType.LineSegmentsIntersect;

        if (polygon.Length > 1)
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 p0 = polygon[i];
                Vector2 p1 = polygon[(i + 1) % polygon.Length];

                if (LineSegmentIntersection(p0, p1, c00, c01, out _))
                    return true;

                if (LineSegmentIntersection(p0, p1, c01, c11, out _))
                    return true;

                if (LineSegmentIntersection(p0, p1, c11, c10, out _))
                    return true;

                if (LineSegmentIntersection(p0, p1, c10, c00, out _))
                    return true;
            }

        intersectionType = PolygonBoundsIntersectionType.None;
        return false;
    }

    /// <summary>
    /// Returns true when a ray defined by rayOrigin and rayDirection intersects with a line segment defined by point1 and point2.
    /// Passes out the point of intersection and the distance (not the square distance) between the ray origin and the intersection.
    /// </summary>
    public static bool LineRayIntersection(Vector2 rayOrigin, Vector2 rayDirection, Vector2 point1, Vector2 point2, out Vector2 intersection, out float distance)
    {
        Vector2 v1 = rayOrigin - point1;
        Vector2 v2 = point2 - point1;
        Vector2 v3 = new Vector2(-rayDirection.y, rayDirection.x);

        float dot = v2.Dot(v3);
        if (Mathf.Abs(dot) < 0.0000001f)
        {
            intersection = Vector2.zero;
            distance = -1;
            return false;
        }

        float t1 = v2.Cross(v1) / dot;
        float t2 = v1.Dot(v3) / dot;

        if (t1 >= 0.0 && (t2 >= 0.0 && t2 <= 1.0))
        {
            intersection = rayOrigin + rayDirection * t1;
            distance = t1;
            return true;
        }

        intersection = Vector2.zero;
        distance = -1;
        return false;
    }

    public static bool LineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        // Get the segments' parameters.
        float dx12 = p2.x - p1.x;
        float dy12 = p2.y - p1.y;
        float dx34 = p4.x - p3.x;
        float dy34 = p4.y - p3.y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1 =
            ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                / denominator;
        if (float.IsInfinity(t1))
        {
            // The lines are parallel (or close enough to it).
            intersection = Vector2.zero;
            return false;
        }

        float t2 =
            ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12)
                / -denominator;

        // Find the point of intersection.
        intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        return
            ((t1 >= 0) && (t1 <= 1) &&
                (t2 >= 0) && (t2 <= 1));
    }

    public static bool LineSegmentCircleIntersection(Vector2 line1, Vector2 line2, Vector2 circleCenter, float circleRadius)
    {
        return CircleContainsPoint(circleCenter, circleRadius, line1)
            || CircleContainsPoint(circleCenter, circleRadius, line2)
            || (ClosestPointOnLine(circleCenter, line1, line2) - circleCenter).sqrMagnitude <= circleRadius * circleRadius;
    }

    /// <summary>
    /// Returns the shortest square distance between the point and any point on the line between the points lineA and lineB.
    /// </summary>
    public static Vector2 ClosestPointOnLine(Vector2 point, Vector2 lineA, Vector2 lineB)
    {
        float dot = Vector2.Dot(point - lineA, lineB - lineA);
        float len_sq = (lineB - lineA).sqrMagnitude;

        float param = -1;
        if (len_sq != 0) //in case of 0 length line
            param = dot / len_sq;

        if (param < 0)
            return lineA;

        else if (param > 1)
            return lineB;

        else
            return Vector2.Lerp(lineA, lineB, param);
    }


    /// <summary>
    ///Assumes a CLOCKWISE winding order for calculating normals.
    ///If the order of the triangle vertices is backwards, flip the normal to get it to point outward.
    /// </summary>
    public static bool RayTriangleIntersection(Vector2 rayOrigin, Vector2 rayDirection, Vector2 t1, Vector2 t2, Vector2 t3, float maxDistance, out Vector2 intersection, out Vector2 normal, out float distance)
    {
        bool hit = false;
        intersection = default;
        normal = default;
        distance = maxDistance;

        {
            if (LineRayIntersection(rayOrigin, rayDirection, t1, t2, out Vector2 i, out float d) && d < distance)
            {
                intersection = i;
                distance = d;
                normal = (t1 - t2).normalized;
                normal = new Vector2(normal.y, -normal.x);
                hit = true;
            }
        }
        {
            if (LineRayIntersection(rayOrigin, rayDirection, t2, t3, out Vector2 i, out float d) && d < distance)
            {
                intersection = i;
                distance = d;
                normal = (t2 - t3).normalized;
                normal = new Vector2(normal.y, -normal.x);
                hit = true;
            }
        }
        {
            if (LineRayIntersection(rayOrigin, rayDirection, t3, t1, out Vector2 i, out float d) && d < distance)
            {
                intersection = i;
                distance = d;
                normal = (t3 - t1).normalized;
                normal = new Vector2(normal.y, -normal.x);
                hit = true;
            }
        }

        return hit;
    }




    public delegate bool TraverseCallback(Vector2Int block, Vector2 intersection, Vector2 normal, float distance);

    public static void VoxelTraverse2D(Ray ray, float maxDistance, Vector2 voxelSize, Vector2 voxelOffset, TraverseCallback callback)
    {
        VoxelTraverse2D(ray.origin, ray.direction, maxDistance, voxelSize, voxelOffset, callback);
    }

    public static void VoxelTraverse2D(Vector2 rayOrigin, Vector2 rayDirection, float maxDistance, Vector2 voxelSize, Vector2 voxelOffset, TraverseCallback callback)
    {
        Vector2 p0 = rayOrigin;
        Vector2 p1 = rayOrigin + rayDirection * maxDistance;

        Vector2 Vector2Abs(Vector2 a) => new Vector2(Mathf.Abs(a.x), Mathf.Abs(a.y));

        p0.x /= voxelSize.x;
        p0.y /= voxelSize.y;

        p1.x /= voxelSize.x;
        p1.y /= voxelSize.y;

        p0 -= voxelOffset;
        p1 -= voxelOffset;

        Vector2 rd = p1 - p0;
        float length = rd.magnitude;
        Vector2 p = new Vector2(Mathf.Floor(p0.x), Mathf.Floor(p0.y));
        Vector2 rdinv = new Vector2(1f / rd.x, 1f / rd.y);
        Vector2 stp = new Vector2(Mathf.Sign(rd.x), Mathf.Sign(rd.y));
        Vector2 delta = Vector2.Min(Vector2.Scale(rdinv, stp), Vector2.one);
        Vector2 t_max = Vector2Abs(Vector2.Scale((p + Vector2.Max(stp, Vector2.zero) - p0), rdinv));

        Vector2Int square;
        Vector2 intersection;
        Vector2 normalX = Vector2.right * Mathf.Sign(delta.x);
        Vector2 normalY = Vector2.up * Mathf.Sign(delta.y);
        Vector2 normal = t_max.x < t_max.y ? normalX : normalY;
        float next_t = Mathf.Min(t_max.x, t_max.y);


        int i = 0;
        while (i < 1000)
        {
            i++;

            square = Vector2Int.RoundToInt(p);
            intersection = p0 + rd * next_t;

            if (callback(square, intersection, normal, next_t * maxDistance))
                break;

            if (t_max.x < t_max.y)
            {
                next_t = t_max.x;
                t_max.x += delta.x;
                p.x += stp.x;
                normal = normalX;
            }
            else
            {
                next_t = t_max.y;
                t_max.y += delta.y;
                p.y += stp.y;
                normal = normalY;
            }
            if (next_t > 1f)
                break;
        }
    }

    public static List<Vector2Int> VoxelTraverse2D(Vector2 p0, Vector2 p1, Vector2 voxelSize, Vector2 voxelOffset)
    {
        List<Vector2Int> line = new List<Vector2Int>();
        Vector2 Vector2Abs(Vector2 a) => new Vector2(Mathf.Abs(a.x), Mathf.Abs(a.y));

        // voxelOffset -= new Vector3(.5f, .5f, .5f);

        p0.x /= voxelSize.x;
        p0.y /= voxelSize.y;

        p1.x /= voxelSize.x;
        p1.y /= voxelSize.y;

        p0 -= voxelOffset;
        p1 -= voxelOffset;

        Vector2 rd = p1 - p0;
        Vector2 p = new Vector2(Mathf.Floor(p0.x), Mathf.Floor(p0.y));
        Vector2 rdinv = new Vector2(1f / rd.x, 1f / rd.y);
        Vector2 stp = new Vector2(Mathf.Sign(rd.x), Mathf.Sign(rd.y));
        Vector2 delta = Vector2.Min(Vector2.Scale(rdinv, stp), Vector2.one);
        Vector2 t_max = Vector2Abs(Vector2.Scale((p + Vector2.Max(stp, Vector2.zero) - p0), rdinv));
        int i = 0;
        while (i < 1000)
        {
            i++;
            Vector2Int square = Vector2Int.RoundToInt(p);
            line.Add(square);

            float next_t = Mathf.Min(t_max.x, t_max.y);
            if (next_t > 1.0) break;
            //Vector2 intersection = p0 + next_t * rd;  

            if (t_max.x < t_max.y)
            {
                t_max.x += delta.x;
                p.x += stp.x;
            }
            else
            {
                t_max.y += delta.y;
                p.y += stp.y;
            } 
        }

        return line;
    }
}
