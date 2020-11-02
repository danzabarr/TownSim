using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainPlane : MonoBehaviour
{
    public Vector2 pivot;
    public Vector2 size;
    public Vector2Int resolution;
    public int seed;
    public GenerationSettings settings;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
        

    public void OnDrawGizmos()
    {
        Random.InitState(seed);
        float seedX = Random.value * 10000 + 10000;
        float seedY = Random.value * 10000 + 10000;

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        int vertexCount = (resolution.x + 1) * (resolution.y + 1);
        int triangleCount = 6 * resolution.x * resolution.y;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int[] triangles = new int[triangleCount];
        int t = 0;

        for (int i = 0; i < vertexCount; i++)
        {
            int ix = i % (resolution.x + 1);
            int iy = i / (resolution.x + 1);

            float x = ((float) ix / resolution.x - pivot.x) * size.x;
            float z = ((float) iy / resolution.y - pivot.y) * size.y;
            float y = settings.Sample(x + seedX, z + seedY);

            vertices[i] = new Vector3(x, y, z);
            uv[i] = new Vector2(x, z);

            if (ix >= resolution.x)
                continue;

            if (iy >= resolution.y)
                continue;

            triangles[t + 0] = (ix + 0) + (iy + 0) * (resolution.x + 1);
            triangles[t + 1] = (ix + 1) + (iy + 1) * (resolution.x + 1);
            triangles[t + 2] = (ix + 1) + (iy + 0) * (resolution.x + 1);
            triangles[t + 3] = (ix + 0) + (iy + 0) * (resolution.x + 1);
            triangles[t + 4] = (ix + 0) + (iy + 1) * (resolution.x + 1);
            triangles[t + 5] = (ix + 1) + (iy + 1) * (resolution.x + 1);

            t += 6;
        }

        Mesh mesh = new Mesh()
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.sharedMesh = mesh;
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider)
            meshCollider.sharedMesh = mesh;

        for (int i = 0; i < vertexCount; i++)
        {
            int ix = i % (resolution.x + 1);
            int iy = i / (resolution.x + 1);

            float x = ((float)ix / resolution.x - pivot.x) * size.x;
            float z = ((float)iy / resolution.y - pivot.y) * size.y;
            float y = settings.Sample(x + seedX, z + seedY);

            if (ix >= resolution.x)
                continue;

            if (iy >= resolution.y)
                continue;

            bool broken = false;

            Gizmos.color = new Color(.1f, .8f, .3f);
            foreach (GenerationSettings.TreeLayer treeLayer in settings.treeLayers)
            {
                if (y < treeLayer.minAltitude)
                    continue;
                if (y > treeLayer.maxAltitude)
                    continue;

                float sample = Perlin.Noise(x + seedX, z + seedY, treeLayer.noise);
                if (sample < treeLayer.threshold)
                    continue;

                Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one);
                broken = true;
                break;
            }

            if (broken)
                continue;

            Gizmos.color = new Color(.8f, .3f, .1f);
            foreach (GenerationSettings.RockLayer rockLayer in settings.rockLayers)
            {
                if (y < rockLayer.minAltitude)
                    continue;
                if (y > rockLayer.maxAltitude)
                    continue;

                float sample = Perlin.Noise(x + seedX, z + seedY, rockLayer.noise);
                if (sample < rockLayer.threshold)
                    continue;

                Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one);
                broken = true;
                break;
            }
            if (broken)
                continue;
        }
    }
}
