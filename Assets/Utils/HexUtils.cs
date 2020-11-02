using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HexUtils
{
    public static readonly float SQRT_3 = Mathf.Sqrt(3);

    public static readonly int[] neighbourX = { 1, 0, -1, -1, 0, 1, };
    public static readonly int[] neighbourY = { 0, 1, 1, 0, -1, -1, };

    public static readonly float[] angles =
    {
        Mathf.PI / 2f + Mathf.PI * 2f / 6f * 0,
        Mathf.PI / 2f + Mathf.PI * 2f / 6f * 1,
        Mathf.PI / 2f + Mathf.PI * 2f / 6f * 2,
        Mathf.PI / 2f + Mathf.PI * 2f / 6f * 3,
        Mathf.PI / 2f + Mathf.PI * 2f / 6f * 4,
        Mathf.PI / 2f + Mathf.PI * 2f / 6f * 5,
    };

    public static readonly float[] sinAngles =
    {
        Mathf.Sin(angles[0]),
        Mathf.Sin(angles[1]),
        Mathf.Sin(angles[2]),
        Mathf.Sin(angles[3]),
        Mathf.Sin(angles[4]),
        Mathf.Sin(angles[5]),
    };

    public static readonly float[] cosAngles =
    {
        Mathf.Cos(angles[0]),
        Mathf.Cos(angles[1]),
        Mathf.Cos(angles[2]),
        Mathf.Cos(angles[3]),
        Mathf.Cos(angles[4]),
        Mathf.Cos(angles[5]),
    };

    public static Vector2 HexToCart(float x, float y, float size) => new Vector2(3f / 2f * x, SQRT_3 / 2f * x + SQRT_3 * y) * size;
    public static Vector2 HexToCart(Vector2 hex, float size) => HexToCart(hex.x, hex.y, size);
    public static Vector2 CartToHex(float x, float z, float size) => new Vector2(2f / 3f * x, -1f / 3f * x + SQRT_3 / 3f * z) / size;
    public static Vector2 CubeToHex(Vector3 cube) => new Vector2(cube.x, cube.y);
    public static Vector2Int CubeToHex(Vector3Int cube) => new Vector2Int(cube.x, cube.y);
    public static Vector3 HexToCube(Vector2 hex) => HexToCube(hex.x, hex.y);
    public static Vector3Int HexToCube(Vector2Int hex) => HexToCube(hex.x, hex.y);
    public static Vector3 HexToCube(float x, float y) => new Vector3(x, y, -x - y);
    public static Vector3Int HexToCube(int x, int y) => new Vector3Int(x, y, -x - y);
    public static Vector2 CartToVert(float x, float z, float size, int resolution) => new Vector2(x - z * SQRT_3 / 3f, 2f / 3f * z * SQRT_3) / (size / resolution);//new Vector2(-x + z / SQRT_3, z * 2f / SQRT_3) / (size / resolution );////new Vector2(-x + z / SQRT_3, z / SQRT_3 * 2f) / (size / resolution);
    public static Vector2Int NearestVert(Vector3 position, float size, int resolution) => NearestVert(position.x, position.z, size, resolution);
    public static Vector2Int NearestVert(float x, float z, float size, int resolution) => HexRound(CartToVert(x, z, size, resolution));
    public static Vector2 VertToCart(float x, float z, float size, int resolution)
    {
        float cZ = z * (size / resolution) / (2f / 3f * SQRT_3);
        float cX = x * (size / resolution) + cZ * SQRT_3 / 3f;

        return new Vector2(cX, cZ);
    }
    public static Vector2 SnapToVert(float x, float z, float size, int resolution)
    {
        Vector2Int vert = HexRound(CartToVert(x, z, size, resolution));
        Vector2 cart = VertToCart(vert, size, resolution);

        return cart;
    }

    public static Vector2 VertToCart(Vector2 vert, float size, int resolution) => VertToCart(vert.x, vert.y, size, resolution);
    public static int CubeDistance(Vector3Int a, Vector3Int b) => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    public static int HexDistance(Vector2Int a, Vector2Int b) => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs((a.x - a.y) - (b.x - b.y))) / 2;
    public static float HexDistance(Vector2 a, Vector2 b) => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs((a.x - a.y) - (b.x - b.y))) / 2f;
    public static int RowSize(int r, int N)
    {
        return 2 * N + 1 - Mathf.Abs(N - r);
    }

    public static int StorePosY(int q, int r, int N)
    {
        return q - Mathf.Max(0, N - r);
    }

    public static float[] CosSixths =
    {
        Mathf.Cos(0 * Mathf.PI / 3f),
        Mathf.Cos(1 * Mathf.PI / 3f),
        Mathf.Cos(2 * Mathf.PI / 3f),
        Mathf.Cos(3 * Mathf.PI / 3f),
        Mathf.Cos(4 * Mathf.PI / 3f),
        Mathf.Cos(5 * Mathf.PI / 3f),
    };

    public static float[] SinSixths =
    {
        Mathf.Sin(0 * Mathf.PI / 3f),
        Mathf.Sin(1 * Mathf.PI / 3f),
        Mathf.Sin(2 * Mathf.PI / 3f),
        Mathf.Sin(3 * Mathf.PI / 3f),
        Mathf.Sin(4 * Mathf.PI / 3f),
        Mathf.Sin(5 * Mathf.PI / 3f),
    };

    public static Vector2Int[] NearestThreeVertices(Vector3 position, float size, int res)
    {
        Vector2Int[] nearest = new Vector2Int[3];

        nearest[0] = NearestVert(position.x, position.z, size, res);

        Vector2 v0 = VertToCart(nearest[0].x, nearest[0].y, size, res);

        int angle = Mathf.RoundToInt((Mathf.PI - Mathf.Atan2(v0.x - position.x, v0.y - position.z)) / (Mathf.PI * 2) * 6);

        Vector2 v1 = new Vector2(v0.x + CosSixths[(angle + 1) % 6] * (size / res), v0.y + SinSixths[(angle + 1) % 6] * (size / res));
        Vector2 v2 = new Vector2(v0.x + CosSixths[(angle + 2) % 6] * (size / res), v0.y + SinSixths[(angle + 2) % 6] * (size / res));

        nearest[1] = NearestVert(v1.x, v1.y, size, res);
        nearest[2] = NearestVert(v2.x, v2.y, size, res);

        return nearest;
    }

    public static Vector3Int Triangle(float x, float y, float size, int res)
    {
        float scale = size / res;

        float cx = x / scale - y * SQRT_3 / 3 / scale;
        float cy = y / SQRT_3 * 2 / scale;
        int ix = Mathf.FloorToInt(cx);
        int iy = Mathf.FloorToInt(cy);

        cx -= ix;
        cy -= iy;

        bool top = (cx + cy) > 1;
        int z = top ? 1 : 0;

        Vector2 t1 = top ? VertToCart(ix + 1, iy + 1, size, res) : VertToCart(ix, iy, size, res);
        Vector2 t2 = VertToCart(ix + 1, iy, size, res);
        Vector2 t3 = VertToCart(ix, iy + 1, size, res);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(t1.x0y(), .05f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(t2.x0y(), .05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(t3.x0y(), .05f);


        float d = (t2.y - t3.y) * (t1.x - t3.x) + (t3.x - t2.x) * (t1.y - t3.y);

        float bx = ((t2.y - t3.y) * (x - t3.x) + (t3.x - t2.x) * (y - t3.y)) / d;
        float by = ((t3.y - t1.y) * (x - t3.x) + (t1.x - t3.x) * (y - t3.y)) / d;
        float bz = 1 - bx - by;

        //Handles.Label(new Vector3(x, 0, y), $"{bx},{by},{bz}");

        Gizmos.color = new Color(bx, by, bz);
        Gizmos.DrawSphere(new Vector3(x, 0, y), .05f);
        return new Vector3Int(ix, iy, z);
    }

    public static Vector3Int CubeRound(Vector3 cube)
    {
        int rx = Mathf.RoundToInt(cube.x);
        int ry = Mathf.RoundToInt(cube.y);
        int rz = Mathf.RoundToInt(cube.z);

        float x_diff = Mathf.Abs(rx - cube.x);
        float y_diff = Mathf.Abs(ry - cube.y);
        float z_diff = Mathf.Abs(rz - cube.z);

        if (x_diff > y_diff && x_diff > z_diff)
            rx = -ry - rz;
        else if (y_diff > z_diff)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        return new Vector3Int(rx, ry, rz);
    }

    public static Vector2Int HexRound(Vector2 hex)
    {
        Vector3Int cube = CubeRound(HexToCube(hex));
        return new Vector2Int(cube.x, cube.y);
    }

    public static void DrawHexagon(float x, float y, float size)
    {
        Vector2 cartesian = HexToCart(x, y, size);

        for (int i = 0; i < 6; i++)
        {
            float angle0 = Mathf.PI / 2f + Mathf.PI * 2f / 6f * i;
            float angle1 = Mathf.PI / 2f + Mathf.PI * 2f / 6f * (i + 1);

            float sinAngle0 = Mathf.Sin(angle0) * size;
            float cosAngle0 = Mathf.Cos(angle0) * size;
            float sinAngle1 = Mathf.Sin(angle1) * size;
            float cosAngle1 = Mathf.Cos(angle1) * size;

            Vector3 i0 = new Vector3(cartesian.x + sinAngle0, 0, cartesian.y + cosAngle0);
            Vector3 i1 = new Vector3(cartesian.x + sinAngle1, 0, cartesian.y + cosAngle1);

            Gizmos.DrawLine(i0, i1);
        }
    }

    public static void DrawHexagonVert(float x, float z, float size, int resolution)
    {
        DrawHexagonVert(x, 0, z, size, resolution);
    }

    public static void DrawHexagonVert(float x, float y, float z, float size, int resolution)
    {
        Vector2 cartesian = VertToCart(x, z, size, resolution);

        for (int i = 0; i < 6; i++)
        {
            float angle0 = Mathf.PI * 2f / 6f * i;
            float angle1 = Mathf.PI * 2f / 6f * (i + 1);

            float sinAngle0 = Mathf.Sin(angle0) * size / resolution / SQRT_3;
            float cosAngle0 = Mathf.Cos(angle0) * size / resolution / SQRT_3;
            float sinAngle1 = Mathf.Sin(angle1) * size / resolution / SQRT_3;
            float cosAngle1 = Mathf.Cos(angle1) * size / resolution / SQRT_3;

            Vector3 i0 = new Vector3(cartesian.x + sinAngle0, y, cartesian.y + cosAngle0);
            Vector3 i1 = new Vector3(cartesian.x + sinAngle1, y, cartesian.y + cosAngle1);

            Gizmos.DrawLine(i0, i1);
        }
    }

    public static Mesh Mesh { get; } = CreateHexagonMesh();

    public static Mesh CreateHexagonMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[7];
        Vector2[] uv = new Vector2[7];
        int[] triangles = new int[18];

        vertices[0] = Vector3.zero;
        uv[0] = Vector3.zero;

        for (int i = 0; i < 6; i++)
        {
            vertices[1 + i] = new Vector3(sinAngles[i], 0, cosAngles[i]);
            uv[1 + i] = new Vector2((angles[i] + Mathf.PI) / (Mathf.PI * 2), 1);
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = 1 + i;
            triangles[i * 3 + 2] = 1 + (i + 1) % 6;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    public delegate bool TraversalCallback(Vector2Int hex, Vector2 intersection, float t);

    public static void Traversal(Vector2 p0, Vector2 p1, float scale, TraversalCallback callback, int iterations = 1000)
    {
        Vector2 HexToCart(Vector2 hex)
        {
            float cY = hex.y * scale / (2f / 3f * SQRT_3);
            float cX = hex.x * scale + cY * SQRT_3 / 3f;

            return new Vector2(cX, cY);
        }

        Vector2 CartToHex(Vector2 cart)
        {
            float vX = (cart.x - cart.y * SQRT_3 / 3f) / scale;
            float vY = (2f / 3f * cart.y * SQRT_3) / scale;

            return new Vector2(vX, vY);
        }

        Vector2Int HexRound(Vector2 hex)
        {
            Vector3 cube = new Vector3(hex.x, hex.y, -hex.x - hex.y);

            int rx = Mathf.RoundToInt(cube.x);
            int ry = Mathf.RoundToInt(cube.y);
            int rz = Mathf.RoundToInt(cube.z);

            float x_diff = Mathf.Abs(rx - cube.x);
            float y_diff = Mathf.Abs(ry - cube.y);
            float z_diff = Mathf.Abs(rz - cube.z);

            if (x_diff > y_diff && x_diff > z_diff)
                rx = -ry - rz;
            else if (y_diff > z_diff)
                ry = -rx - rz;

            return new Vector2Int(rx, ry);
        }

        //Delta of the line p0, p1
        Vector2 delta = (p1 - p0);

        //Unit vectors of three hexagonal directions
        Vector2 n0 = new Vector2(1, 0);
        Vector2 n1 = new Vector2(+.5f, SQRT_3 * .5f);
        Vector2 n2 = new Vector2(-.5f, SQRT_3 * .5f);

        //The sign of each of the three directions
        int s0 = (int)Mathf.Sign(Vector2.Dot(n0, delta));
        int s1 = (int)Mathf.Sign(Vector2.Dot(n1, delta));
        int s2 = (int)Mathf.Sign(Vector2.Dot(n2, delta));

        //Orient the directions so they are the three normals nearest to the line
        n0 *= s0;
        n1 *= s1;
        n2 *= s2;

        //Scale the directions to the size of the grid
        n0 /= scale;
        n1 /= scale;
        n2 /= scale;

        //The steps in integer hex coordinates for each of the three directions
        Vector2Int step0 = new Vector2Int(1, 0) * s0;
        Vector2Int step1 = new Vector2Int(0, 1) * s1;
        Vector2Int step2 = new Vector2Int(-1, 1) * s2;

        //Calculate the current hex that the ray origin is contained within
        Vector2Int current_hex = HexRound(CartToHex(p0));

        for (int i = 0; i < iterations; i++)
        {
            //Get the difference between the center of the current hex and the start of the ray
            Vector2 rdelta = p0 - HexToCart(current_hex);

            //Get the distances to each edge
            float d0 = (.5f - Vector2.Dot(n0, rdelta)) / Vector2.Dot(n0, delta);
            float d1 = (.5f - Vector2.Dot(n1, rdelta)) / Vector2.Dot(n1, delta);
            float d2 = (.5f - Vector2.Dot(n2, rdelta)) / Vector2.Dot(n2, delta);

            //Find the nearest edge
            float t = d0;
            Vector2Int step = step0;

            if (d1 < t)
            {
                t = d1;
                step = step1;
            }

            if (d2 < t)
            {
                t = d2;
                step = step2;
            }

            //Break at the end of the line
            if (t > 1)
                break;

            //Calculate where the line intersects with the edge
            Vector2 intersect = p0 + delta * t;

            //Increment the current hex tile across the nearest edge
            current_hex += step;

            //Do callback and break on return true
            if (callback(current_hex, intersect, t))
                break;
        }
    }

    public static void TraverseOutline(Vector2[] poly, float scale, TraversalCallback callback, int iterations = 1000)
    {
        for (int i = 0; i < poly.Length; i++)
        {
            Vector2 p0 = poly[i];
            Vector2 p1 = poly[(i + 1) % poly.Length];

            Traversal(p0, p1, scale, callback, iterations);
        }
    }

    public static List<Vector2Int> TraverseOutline(Vector2[] poly, float scale, bool removeDuplicates, int iterations = 1000)
    {
        List<Vector2Int> list = new List<Vector2Int>();

        bool PopulateList(Vector2Int hex, Vector2 intersection, float t)
        {
            if (!removeDuplicates || !list.Contains(hex))
                list.Add(hex);
            return false;
        }

        TraverseOutline(poly, scale, PopulateList, iterations);
        return list;
    }

    public static List<float> ScanLineIntersections(Vector2[] points, float y, bool sort)
    {
        List<float> intersections = new List<float>();
        if (points == null)
            return intersections;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 p0 = points[i];
            Vector2 p1 = points[(i + 1) % points.Length];

            if (p0.y > y && p1.y > y)
                continue;

            if (p0.y <= y && p1.y <= y)
                continue;

            float t2 = (y - p0.y) / (p1.y - p0.y);
            if ((t2 >= 0.0 && t2 <= 1.0))
            {
                float x = (p1.x - p0.x) * (y - p0.y) / (p1.y - p0.y) + p0.x;
                intersections.Add(x);
            }
        }

        if (sort)
            intersections.Sort((a, b) => a.CompareTo(b));

        return intersections;
    }

    public delegate bool ScanLineFillCallback(Vector2Int hex);

    public static void ScanLineFill(Vector2[] points, float size, int res, ScanLineFillCallback callback)
    {
        if (points.Length <= 1)
            return;

        Vector2 min = points[0];
        Vector2 max = points[0];

        for (int i = 1; i < points.Length; i++)
        {
            min = Vector2.Min(min, points[i]);
            max = Vector2.Max(max, points[i]);
        }

        Vector2Int hexMin = HexRound(CartToVert(min.x, min.y, size, res));
        Vector2Int hexMax = HexRound(CartToVert(max.x, max.y, size, res));

        for (int hexY = hexMin.y - 1; hexY < hexMax.y + 1; hexY++)
        {
            Vector2 lineStart = VertToCart(hexMin.x, hexY, size, res);
            Vector2Int tileStart = hexMin;
            bool open = false;
            foreach (float p in ScanLineIntersections(points, lineStart.y, true))
            {
                //Gizmos.DrawSphere(new Vector3(p, 0, lineStart.y), .1f);

                Vector2 vert = CartToVert(p, lineStart.y, size, res);

                Vector2Int hex = HexRound(vert);

                if (!open)
                {
                    hex.x = (int)(Mathf.Ceil(vert.x * (size / res) / (size / res)));
                    open = true;
                    tileStart = hex;
                }
                else
                {
                    hex.x = (int)(Mathf.Floor(vert.x * (size / res) / (size / res)));
                    open = false;
                    for (int x = tileStart.x; x < hex.x + 1; x++)
                    {
                        if (callback(new Vector2Int(x, hex.y)))
                            return;
                    }
                }
            }
        }
    }
    public static List<Vector2Int> ScanLineFill(Vector2[] points, float size, int res)
    {
        List<Vector2Int> list = new List<Vector2Int>();

        bool PopulateList(Vector2Int hex)
        {
            list.Add(hex);
            return false;
        }

        ScanLineFill(points, size, res, PopulateList);

        return list;
    }

    public static IEnumerable<Vector2Int> Spiral(Vector2Int center, int radius)
    {
        if (radius > 0)
        {
            radius--;
            yield return center;

            Vector2Int current = center;
            for (int r = 1; r < radius + 1; r++)
            {
                current.x += neighbourX[4];
                current.y += neighbourY[4];
                for (int i = 0; i < 6; i++)
                    for (int j = 0; j < r; j++)
                    {
                        current.x += neighbourX[i];
                        current.y += neighbourY[i];
                        yield return current;
                    }

            }
        }
    }

    public static IEnumerable<Vector2Int> Ring(Vector2Int center, int radius)
    {
        Vector2Int current = center + new Vector2Int(neighbourX[4], neighbourY[4]) * radius;
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < radius; j++)
            {
                current.x += neighbourX[i];
                current.y += neighbourY[i];
                yield return current;
            }
    }
}
