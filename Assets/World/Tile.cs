using TownSim.Navigation;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Tile : MonoBehaviour
{
    public int X { get; private set;}
    public int Y { get; private set;}

    public int RegionID, ContigRegionID;
    public int state;
    public bool inFloodFillSet;
    public bool[] edgesVisited = new bool[6];

    private Mesh[] skirt;
    private MeshFilter terrainFilter;
    private MeshCollider terrainCollider;
    private MeshRenderer terrainRenderer;
    public Camera pathMaskCamera;
    public float pathMaskResolution;
    public MeshRenderer grassRenderer;
    public MeshFilter grassFilter;

    public GameObject screen;

    public float frameDuration;
    public int screenFrequency;
    private float clock;
    private int screenClock;

    public bool renderPathMaskCamera;

    public Tile[] Neighbours
    {
        get
        {
            return new Tile[]
            {
                Map.GetTile(new Vector2Int(X + 1, Y + 0)),
                Map.GetTile(new Vector2Int(X + 0, Y + 1)),
                Map.GetTile(new Vector2Int(X - 1, Y + 1)),
                Map.GetTile(new Vector2Int(X - 1, Y + 0)),
                Map.GetTile(new Vector2Int(X + 0, Y - 1)),
                Map.GetTile(new Vector2Int(X + 1, Y - 1)),
            };
        }
    }

    private void Start()
    {
        pathMaskCamera.enabled = false;
    }

    private void Update()
    {


        //Graphics.DrawMeshInstanced(meshFilter.mesh, 0, grassMaterial, new Matrix4x4[] { Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one) }, 1, grassProperties, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        //Graphics.DrawMesh(meshFilter.mesh, transform.position, Quaternion.identity, grassMaterial, LayerMask.NameToLayer("Grass"), Camera.main, 0, grassProperties, false, false);

        clock += Time.deltaTime;

        if (clock > frameDuration)
        {
            clock -= frameDuration;
            screenClock++;
            screen.transform.localScale = new Vector3(Map.Size * 2, Map.Size * HexUtils.SQRT_3, 1);
            if (screenClock >= screenFrequency)
            {
                screen.SetActive(true);
                screenClock -= screenFrequency;
            }
            pathMaskCamera.Render();
            screen.SetActive(false);
        }
    }

    public void SetupPathMask()
    {
        if (pathMaskCamera == null)
            pathMaskCamera = GetComponentInChildren<Camera>();

        if (terrainRenderer == null)
            terrainRenderer = GetComponent<MeshRenderer>();

        if (pathMaskResolution < 0)
            return;

        int textureWidth = (int)(2 / HexUtils.SQRT_3 * pathMaskResolution);
        int textureHeight = (int)pathMaskResolution;

        if (pathMaskCamera.targetTexture == null || textureWidth != pathMaskCamera.pixelWidth || textureHeight != pathMaskCamera.pixelHeight)
        {
            if (pathMaskCamera.targetTexture != null)
            {
                RenderTexture old = pathMaskCamera.targetTexture;
                pathMaskCamera.targetTexture = null;
                old.Release();
            }

            pathMaskCamera.targetTexture = new RenderTexture(textureWidth, textureHeight, 0);

            grassRenderer.material.SetVector("_PathMaskSize", new Vector4(transform.position.x, transform.position.z, Map.Size, Map.Size * HexUtils.SQRT_3 / 2));
            grassRenderer.material.SetTexture("_PathMaskTex", pathMaskCamera.targetTexture);

            terrainRenderer.material.SetVector("_PathMaskSize", new Vector4(transform.position.x, transform.position.z, Map.Size, Map.Size * HexUtils.SQRT_3 / 2));
            terrainRenderer.material.SetTexture("_PathMaskTex", pathMaskCamera.targetTexture);

        }
        pathMaskCamera.orthographicSize = Map.Size * HexUtils.SQRT_3 / 2;
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
        name = $"Tile [{x},{y}]";
        transform.position = HexUtils.HexToCart(x, y, Map.Size).x0y();
    }

    public void GenerateMesh()
    {
        float size = Map.Size;
        int res = Map.MeshRes;

        if (terrainFilter == null)
            terrainFilter = GetComponent<MeshFilter>();
        if (terrainCollider == null)
            terrainCollider = GetComponent<MeshCollider>();


        if (terrainFilter.sharedMesh != null)
            Destroy(terrainFilter.sharedMesh);

        terrainFilter.sharedMesh = GenerateMesh(X, Y, size, res, true);
        terrainCollider.sharedMesh = terrainFilter.sharedMesh;
        grassFilter.sharedMesh = terrainFilter.sharedMesh;

        if (skirt == null)
            skirt = new Mesh[6];

        for (int e = 0; e < 6; e++)
        {
            if (skirt[e] != null)
                Destroy(skirt[e]);
            skirt[e] = GenerateSkirt(X, Y, 0, size, res);
        }
    }

    public void AddNodes()
    {
        float size = Map.Size;
        int res = Map.NodeRes;

        Vector2Int vert = default;
        Vector2Int[] neighbours = new Vector2Int[6];
        Vector3 position;
        Node n, node;

        for (int r = 0; r < res * 2 + 1; r++)
        {
            int cLen = 2 * res + 1 - Mathf.Abs(res - r);
            for (int c = 0; c < cLen; c++)
            {

                vert.x = (X - Y) * res + c - Mathf.Min(r, res);
                vert.y = (X + Y * 2) * res + r - res;

                if (!Map.nodes.ContainsKey(vert))
                {
                    
                    position = Map.Instance.OnMesh(HexUtils.VertToCart(vert.x, vert.y, size, res));

                    if (position.y < 0)
                        continue;

                    node = new Node(vert, position);
                    Map.nodes.Add(vert, node);

                    for (int i = 0; i < 6; i++)
                        neighbours[i] = vert;

                    neighbours[0].y += 1;

                    neighbours[1].x += 1;

                    neighbours[2].x += 1;
                    neighbours[2].y -= 1;

                    neighbours[3].y -= 1;

                    neighbours[4].x -= 1;

                    neighbours[5].x -= 1;
                    neighbours[5].y += 1;

                    for (int i = 0; i < 6; i++)
                    {
                        if (Map.nodes.TryGetValue(neighbours[i], out n))
                            Node.Connect(node, n);
                    }

                    //if (Map.nodes.TryGetValue(vert + new Vector2Int(+0, +1), out n))
                    //    Node.Connect(node, n);
                    //
                    //if (Map.nodes.TryGetValue(vert + new Vector2Int(+1, +0), out n))
                    //    Node.Connect(node, n);
                    //
                    //if (Map.nodes.TryGetValue(vert + new Vector2Int(+1, -1), out n))
                    //    Node.Connect(node, n);
                    //
                    //if (Map.nodes.TryGetValue(vert + new Vector2Int(+0, -1), out n))
                    //    Node.Connect(node, n);
                    //
                    //if (Map.nodes.TryGetValue(vert + new Vector2Int(-1, +0), out n))
                    //    Node.Connect(node, n);
                    //
                    //if (Map.nodes.TryGetValue(vert + new Vector2Int(-1, +1), out n))
                    //    Node.Connect(node, n);

                    //if (Random.value > .7f)
                    //    node.Obstructions++;


                    //
                    //float neDist = Map.nodes.TryGetValue(new Vector2Int(vert.x + 0, vert.y + 1), out Map.Node node) ? node.swDist
                    //: Vector2.Distance(new Vector2(0, position.y), new Vector2(size, Map.Instance.MeshHeight(HexUtils.VertToCart(vert.x + 0, vert.y + 1, size, res))));
                    //
                    //float  eDist = Map.nodes.TryGetValue(new Vector2Int(vert.x + 1, vert.y + 0), out node) ? node.wDist
                    //: Vector2.Distance(position, Map.Instance.OnMesh(HexUtils.VertToCart(vert.x + 1, vert.y + 0, size, res)));
                    //    
                    //float seDist = Map.nodes.TryGetValue(new Vector2Int(vert.x + 1, vert.y - 1), out node) ? node.nwDist
                    //: Vector2.Distance(position, Map.Instance.OnMesh(HexUtils.VertToCart(vert.x + 1, vert.y - 1, size, res)));
                    //
                    //float swDist = Map.nodes.TryGetValue(new Vector2Int(vert.x + 0, vert.y - 1), out node) ? node.neDist
                    //: Vector2.Distance(position, Map.Instance.OnMesh(HexUtils.VertToCart(vert.x + 0, vert.y - 1, size, res)));
                    //
                    //float  wDist = Map.nodes.TryGetValue(new Vector2Int(vert.x - 1, vert.y + 0), out node) ? node.eDist
                    //: Vector2.Distance(position, Map.Instance.OnMesh(HexUtils.VertToCart(vert.x - 1, vert.y + 0, size, res)));
                    //
                    //float nwDist = Map.nodes.TryGetValue(new Vector2Int(vert.x - 1, vert.y + 1), out node) ? node.seDist
                    //: Vector2.Distance(position, Map.Instance.OnMesh(HexUtils.VertToCart(vert.x - 1, vert.y + 1, size, res)));
                    //
                    //
                    //
                    //Map.nodes.Add(
                    //    vert,
                    //    new Node()
                    //    {
                    //        vertex = vert,
                    //        position = position,
                    //        active = true,
                    //        cost = 10,
                    //
                    //        neDist = neDist,
                    //         eDist =  eDist,
                    //        seDist = seDist,
                    //        swDist = swDist,
                    //         wDist =  wDist,
                    //        nwDist = nwDist,
                    //    }
                    //);
                }
            }
        }
    }

    public void AddResources()
    {
        float size = Map.Size;
        int res = Map.NodeRes;

        for (int r = 0; r < res * 2 + 1; r++)
        {
            int cLen = 2 * res + 1 - Mathf.Abs(res - r);
            for (int c = 0; c < cLen; c++)
            {

                Vector2Int vert = new Vector2Int
                (
                    (X - Y) * res + c - Mathf.Min(r, res),
                    (X + Y * 2) * res + r - res
                );

                Vector3 position = Map.Instance.OnMesh(HexUtils.VertToCart(vert.x, vert.y, size, res));

                foreach(GenerationSettings.TreeLayer treeLayer in Map.Instance.settings.treeLayers)
                {
                    if (position.y < treeLayer.minAltitude)
                        continue;
                    if (position.y > treeLayer.maxAltitude)
                        continue;

                    float sample = Perlin.Noise(position.x + Map.Instance.SeedOffsetX, position.z + Map.Instance.SeedOffsetY, treeLayer.noise);

                    if (sample < treeLayer.threshold)
                        continue;

                    float random = Random.value * (1 - treeLayer.threshold);

                    if (sample - treeLayer.threshold < random)
                        continue;

                    if (Map.Instance.ResourceInRange(vert, 3, out _))
                        continue;

                    Tree tree = Instantiate(treeLayer.prefab);

                    tree.transform.position = position;
                    tree.transform.rotation = Quaternion.AngleAxis(Random.value * 360, Vector3.up);
                    tree.SetColor(treeLayer.colors.Evaluate(Random.value));
                    tree.SetSize(treeLayer.sizes.Evaluate((sample - treeLayer.threshold) / (1 - treeLayer.threshold)));

                    Map.resources.Add(vert, tree);

                    break;

                }
            }
        }


        for (int r = 0; r < res * 2 + 1; r++)
        {
            int cLen = 2 * res + 1 - Mathf.Abs(res - r);
            for (int c = 0; c < cLen; c++)
            {

                Vector2Int vert = new Vector2Int
                (
                    (X - Y) * res + c - Mathf.Min(r, res),
                    (X + Y * 2) * res + r - res
                );

                Vector3 position = Map.Instance.OnMesh(HexUtils.VertToCart(vert.x, vert.y, size, res));

                foreach (GenerationSettings.RockLayer rockLayer in Map.Instance.settings.rockLayers)
                {
                    if (position.y < rockLayer.minAltitude)
                        continue;
                    if (position.y > rockLayer.maxAltitude)
                        continue;

                    float sample = Perlin.Noise(position.x + Map.Instance.SeedOffsetX, position.z + Map.Instance.SeedOffsetY, rockLayer.noise);

                    if (sample < rockLayer.threshold)
                        continue;

                    float random = Random.value * (1 - rockLayer.threshold);

                    if (sample - rockLayer.threshold < random)
                        continue;

                    if (Map.Instance.ResourceInRange(vert, 3, out _))
                        continue;

                    Rocks rocks = Instantiate(rockLayer.prefab);

                    rocks.transform.position = position;
                    rocks.transform.rotation = Quaternion.AngleAxis(Random.value * 360, Vector3.up);

                    Map.resources.Add(vert, rocks);

                    break;

                }
            }
        }


    }

    public static Vector2Int[] HexDirections = 
    {
        new Vector2Int(+ 0, + 1),
        new Vector2Int(+ 1, + 0),
        new Vector2Int(+ 1, - 1),
        new Vector2Int(+ 0, - 1),
        new Vector2Int(- 1, + 0),
        new Vector2Int(- 1, + 1),
    };

    public static void DrawTile(int tileHexX, int tileHexY, float size, int res)
    {
        Gizmos.color = Color.white;
        HexUtils.DrawHexagon(tileHexX, tileHexY, size);

        //Gizmos.color = new Color(.5f, .5f, .5f, .5f);
        //
        //for (int r = 0; r < res * 2 + 1; r++)
        //{
        //    int cLen = 2 * res + 1 - Mathf.Abs(res - r);
        //    for (int c = 0;  c < cLen; c++)
        //    {
        //        int x = (tileHexX - tileHexY) * res + c - Mathf.Min(r, res);
        //        int z = (tileHexX + tileHexY * 2) * res + r - res;
        //        HexUtils.DrawHexagonVert(x, z, size, res);
        //        Vector2 cart = HexUtils.VertToCart(x, z, size, res);
        //        //Gizmos.DrawSphere(cart.x0y(), .1f);
        //        Handles.Label(cart.x0y(), $"[{x},{z}]");
        //    }
        //}

        //for (int x = -10; x < 10; x++)
        //    for (int y = -10; y < 10; y++)
        //        HexUtils.DrawHexagon(x, y, 1);
    }

    

    public void DrawSkirt(int e, Material material, int layer)
    {
        Graphics.DrawMesh(skirt[e], transform.position, Quaternion.identity, material, layer);
    }

    public static Mesh GenerateSkirt(int x, int y, float baseY, float size, int res)
    {
        Vector2 centerCartesian = HexUtils.HexToCart(x, y, size);

        int verticesCount = 2 * 6 * res;
        int trianglesCount = 6 * res * 6;
        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[trianglesCount];

        for (int e = 0; e < 6; e++)
        {
            Vector2 a0 = new Vector2(HexUtils.sinAngles[e], HexUtils.cosAngles[e]) * size;
            Vector2 a1 = new Vector2(HexUtils.sinAngles[(e + 1) % 6], HexUtils.cosAngles[(e + 1) % 6]) * size;
            for (int r = 0; r < res; r++)
            {
                Vector2 v = Vector2.Lerp(a0, a1, (float)r / res);

                vertices[0 * res * 6 + e * res + r] = v.xny(baseY);
                vertices[1 * res * 6 + e * res + r] = v.xny(Map.Instance.Height(centerCartesian.x + v.x, centerCartesian.y + v.y));

                //Gizmos.DrawSphere(vertices[0 * res * 6 + e * res + r], .25f);
                //Gizmos.DrawSphere(vertices[1 * res * 6 + e * res + r], .25f);

                triangles[(e * res + r) * 6 + 0] = 0 * res * 6 + e * res + r;
                triangles[(e * res + r) * 6 + 1] = 1 * res * 6 + (e * res + r + 1) % (6 * res);
                triangles[(e * res + r) * 6 + 2] = 1 * res * 6 + e * res + r;
                triangles[(e * res + r) * 6 + 3] = 0 * res * 6 + e * res + r;
                triangles[(e * res + r) * 6 + 4] = 0 * res * 6 + (e * res + r + 1) % (6 * res);
                triangles[(e * res + r) * 6 + 5] = 1 * res * 6 + (e * res + r + 1) % (6 * res);
            }
        }

        Mesh mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Mesh GenerateSkirt(int x, int y, float baseY, int edge, float size, int res)
    {
        Vector2 centerCartesian = HexUtils.HexToCart(x, y, size);

        int verticesCount = 2 * (res + 1);
        int trianglesCount = res * 6;
        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[trianglesCount];

        Vector2 a0 = new Vector2(HexUtils.sinAngles[edge], HexUtils.cosAngles[edge]) * size;
        Vector2 a1 = new Vector2(HexUtils.sinAngles[(edge + 1) % 6], HexUtils.cosAngles[(edge + 1) % 6]) * size;
        for (int r = 0; r < res + 1; r++)
        {
            Vector2 v = Vector2.Lerp(a0, a1, (float)r / res);

            vertices[0 * (res + 1) + r] = v.xny(baseY);
            vertices[1 * (res + 1) + r] = v.xny(Map.Instance.Height(centerCartesian.x + v.x, centerCartesian.y + v.y));

            if (r < res)
            {
                triangles[r * 6 + 0] = 0 * (res + 1) + r;
                triangles[r * 6 + 1] = 1 * (res + 1) + r + 1;
                triangles[r * 6 + 2] = 1 * (res + 1) + r;
                triangles[r * 6 + 3] = 0 * (res + 1) + r;
                triangles[r * 6 + 4] = 0 * (res + 1) + r + 1;
                triangles[r * 6 + 5] = 1 * (res + 1) + r + 1;
            }
        }

        Mesh mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }


    public static Mesh GenerateMesh(int x, int y, float size, int res, bool fixNormalsAtSeams)
    {
        Vector2 centerCartesian = HexUtils.HexToCart(x, y, size);

        Vector2 centerVert = new Vector2Int
        (
            (x - y) * res,
            (x + y * 2) * res
        );

        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(HexUtils.VertToCart(centerVert.x, centerVert.y, size, res).x0y(), .1f);

        int CenteredHexagonalNum(int n) => 3 * n * (n - 1) + 1;

        //Setting up the mesh arrays
        //uv is just global XZ coordinates
        //uv2 are uvs which can be used for texturing the tiles in a radial pattern, i.e. around and from the center of the tile to the edges. Custom shader required to make use of these uvs
        int verticesCount = 3 * (res + 1) * res + 1;
        int trianglesCount = 6 * res * res * 3;
        Vector3[] vertices = new Vector3[verticesCount];
        Vector2[] uv = new Vector2[verticesCount];
        Vector2[] uv2 = new Vector2[verticesCount];
        int[] triangles = new int[trianglesCount];

        int i = 0;
        int j = 0;
        for (int r = 0; r < res * 2 + 1; r++)
        {
            int cLen = 2 * res + 1 - Mathf.Abs(res - r);
            for (int c = 0; c < cLen; c++)
            {

                Vector2Int vert = new Vector2Int
                (
                    c - Mathf.Min(r, res),
                    r - res
                );


                Vector3 globalVertex = Map.Instance.OnTerrain(HexUtils.VertToCart(centerVert.x + vert.x, centerVert.y + vert.y, size, res));
                Vector3 localVertex = globalVertex - centerCartesian.x0y();

                //Vector2 c1 = HexUtils.VertToCart(centerVert.x + vert.x - 1, centerVert.y + vert.y + 1, size, res);
                //Vector2 c2 = HexUtils.VertToCart(centerVert.x + vert.x + 0, centerVert.y + vert.y + 1, size, res);
                //Vector2 c3 = HexUtils.VertToCart(centerVert.x + vert.x + 1, centerVert.y + vert.y + 0, size, res);

                int i1 = i + cLen;
                if (r >= res) i1--;
                int i2 = i1 + 1;
                int i3 = i + 1;
                //Handles.Label(onTerrain, $"{i} ({i1},{i2},{i3}) ({vert.x},{vert.y})");

                //Gizmos.color = new Color(.5f, .5f, .5f);
                //ExtraGizmos.DrawArrow(cart, c1);
                //ExtraGizmos.DrawArrow(cart, c2);
                //ExtraGizmos.DrawArrow(cart, c3);


                if (!(vert.y >= res || vert.x + vert.y >= res || vert.x <= -res))
                {
                    //Gizmos.color = Color.green;
                    //Gizmos.DrawLine(cart.x0y(), c1.x0y());
                    //Gizmos.DrawLine(cart.x0y(), c2.x0y());
                    //Gizmos.DrawLine(c1.x0y(), c2.x0y());
                    triangles[j + 0] = i;
                    triangles[j + 1] = i1;
                    triangles[j + 2] = i2;
                    j += 3;
                }

                if (!(vert.y >= res || vert.x + vert.y >= res || vert.x >= res))
                {
                    //Gizmos.color = Color.red;
                    //Gizmos.DrawLine(cart.x0y(), c2.x0y());
                    //Gizmos.DrawLine(cart.x0y(), c3.x0y());
                    //Gizmos.DrawLine(c2.x0y(), c3.x0y());
                    triangles[j + 0] = i;
                    triangles[j + 1] = i2;
                    triangles[j + 2] = i3;
                    j += 3;
                }

                vertices[i] = localVertex;
                uv[i] = globalVertex.xz();
                i++;
            }
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uv,
            uv2 = uv2,
            triangles = triangles
        };

        mesh.RecalculateNormals();

        if (fixNormalsAtSeams)
        {
            //Gizmos.color = Color.red;
            Vector3 CalculateVectorNormal(Vector3 centerPoint, float radius)
            {
                /*
                * Calculates a vector normal for a vertex at a given position, used for 'smooth shading'.
                * Samples six points equally spaced around the vertex at a given radius and calculates the surface normal of the six equilateral triangles that are formed.
                * Takes the average of the six surface normals to return as the vector normal.
                * This function is used to correct the normals at the edges of the tile mesh, which when automatically calculated using mesh.RecalculateNormals(), does not take into account the geometry of adjacent meshes, and produces 'creases' at the tile edges.
                * Toggle fixNormalsAtSeams to see this in effect.
                */
                Vector3[] radialPoints = new Vector3[6];

                for (int p = 0; p < 6; p++)
                {
                    float pX = centerPoint.x + HexUtils.sinAngles[p] * radius;
                    float pZ = centerPoint.z + HexUtils.cosAngles[p] * radius;
                    radialPoints[p] = Map.Instance.OnTerrain(pX, pZ);
                }

                Vector3 surfaceNormalSum = Vector3.zero;

                for (int p = 0; p < 6; p++)
                    surfaceNormalSum += CalculateSurfaceNormal(centerPoint, radialPoints[p], radialPoints[(p + 1) % 6]);

                //Can avoid a sqrt here as CalculateSurfaceNormal returns a unit vector and dividing by six works fine
                //return surfaceNormalSum.normalized;
                return surfaceNormalSum / 6;
            }

            Vector3[] normals = mesh.normals;

            for (int r = 0; r < res; r++)
            {
                i = r;
                normals[i] = CalculateVectorNormal(centerCartesian.x0y() + vertices[i], size / res);
            }

            for (int r = 0; r < res; r++)
            {
                i = verticesCount - r - 1;
                normals[i] = CalculateVectorNormal(centerCartesian.x0y() + vertices[i], size / res);
            }

            i = 0;
            for (int r = 0; r < res; r++)
            {
                i += res + r + 1;

                normals[i] = CalculateVectorNormal(centerCartesian.x0y() + vertices[i], size / res);
            }

                
            for (int r = 0; r < res; r++)
            {
                i += res * 2 - r + 1;

                normals[i] = CalculateVectorNormal(centerCartesian.x0y() + vertices[i], size / res);
            }

            i = -1;
            for (int r = 0; r < res; r++)
            {
                i += res + r + 1;

                normals[i] = CalculateVectorNormal(centerCartesian.x0y() + vertices[i], size / res);
            }

            for (int r = 0; r < res; r++)
            {
                i += res * 2 - r + 1;

                normals[i] = CalculateVectorNormal(centerCartesian.x0y() + vertices[i], size / res);
            }

            mesh.normals = normals;
        }

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;

    }

    public static Mesh GenerateMeshOld(int x, int y, float size, int res, bool fixNormalsAtSeams)
    {

        //Set position
        Vector2 centerCartesian = HexUtils.HexToCart(x, y, size);

        //Setting up the mesh arrays
        //uv is just global XZ coordinates
        //uv2 are uvs which can be used for texturing the tiles in a radial pattern, i.e. around and from the center of the tile to the edges. Custom shader required to make use of these uvs
        int verticesCount = 3 * (res + 1) * res + 1;
        int trianglesCount = 6 * res * res * 3;
        Vector3[] vertices = new Vector3[verticesCount];
        Vector2[] uv = new Vector2[verticesCount];
        Vector2[] uv2 = new Vector2[verticesCount];
        int[] triangles = new int[trianglesCount];

        //Calculate the center point in the mesh
        float centerHeight = Map.Instance.Height(centerCartesian.x, centerCartesian.y);
        vertices[0] = new Vector3(0, centerHeight, 0);
        uv[0] = centerCartesian;
        uv2[0] = Vector2.zero;

        //See https://en.wikipedia.org/wiki/Centered_hexagonal_number
        int CenteredHexagonalNum(int n) => 3 * n * (n - 1) + 1;

        //i is the index of the vertex array
        int i = 1;
        //j is a number used for generating the triangle indices
        int j = 0;

        //For each hexagon ring, starting from the inside working out
        for (int r = 0; r < res; r++)
        {
            float radius = size * (1 + r) / res;
            int trianglesPerEdge = (r * 2) + 1;
            int k = 1;

            //For each of the six edges
            for (int e = 0; e < 6; e++)
            {
                //The XZ start and end point of each edge
                Vector2 e0 = new Vector2(HexUtils.sinAngles[e], HexUtils.cosAngles[e]) * radius;
                Vector2 e1 = new Vector2(HexUtils.sinAngles[(e + 1) % 6], HexUtils.cosAngles[(e + 1) % 6]) * radius;

                //For each subdivision of the ring edge
                for (int v = 0; v < r + 1; v++)
                {
                    //Interpolate between the start and the end of the edge to find the XZ point for this subdivision
                    Vector2 localCartesian = Vector2.Lerp(e0, e1, (float)v / (r + 1));
                    //Convert to global by adding the center position
                    Vector2 globalCartesian = centerCartesian + localCartesian;
                    //Sample the height for this vertex
                    float height = Map.Instance.Height(globalCartesian.x, globalCartesian.y);
                    //Get the angle and construct the radial coordinates used for the uv2 of this vertex
                    float angle = Mathf.Atan2(localCartesian.x, localCartesian.y);
                    float radialX = (angle + Mathf.PI) / (Mathf.PI * 2);
                    float radialY = radius;

                    //Fill the arrays for this vertex and increment i
                    vertices[i] = new Vector3(localCartesian.x, height, localCartesian.y);
                    uv[i] = centerCartesian + localCartesian;
                    uv2[i] = new Vector2(radialX, radialY);
                    i++;
                }

                //For each triangle of the ring edge
                for (int t = 0; t < trianglesPerEdge; t++)
                {
                    //The inside of this loop is pure number fuckery to get the correct indices for the vertices that form the triangle for this ring edge.
                    //I just wrote down the pattern for a few rings and figured out some equations by trial and error that would procedurally generate the same pattern.
                    //i0, i1, i2 are the three indices. It is a clockwise winding order.
                    int i0 = CenteredHexagonalNum(r + 1) + e * (trianglesPerEdge / 2 + 1) + (t + 1) / 2;
                    int i1;
                    int i2 = 0;

                    if (r > 0)
                        i2 = CenteredHexagonalNum(r) + (e * (trianglesPerEdge / 2) + (t) / 2) % (CenteredHexagonalNum(r + 1) - CenteredHexagonalNum(r));
                    if (r == 0)
                        i1 = (e + 1) % 6 + 1;
                    else if (t % 2 == 0)
                        i1 = CenteredHexagonalNum(r + 1) + (e * (trianglesPerEdge / 2 + 1) + (t + 1) / 2 + 1) % (CenteredHexagonalNum(r + 2) - CenteredHexagonalNum(r + 1));
                    else
                    {
                        i1 = CenteredHexagonalNum(r) + k % (CenteredHexagonalNum(r + 1) - CenteredHexagonalNum(r));
                        k++;
                    }

                    //Fill the triangles array with the correct indices and increment j
                    triangles[j * 3 + 0] = i0;
                    triangles[j * 3 + 1] = i1;
                    triangles[j * 3 + 2] = i2;
                    j++;
                }
            }
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uv,
            uv2 = uv2,
            triangles = triangles
        };

        mesh.RecalculateNormals();

        if (fixNormalsAtSeams)
        {
            Vector3 CalculateVectorNormal(Vector3 centerPoint, float radius)
            {
                /*
                * Calculates a vector normal for a vertex at a given position, used for 'smooth shading'.
                * Samples six points equally spaced around the vertex at a given radius and calculates the surface normal of the six equilateral triangles that are formed.
                * Takes the average of the six surface normals to return as the vector normal.
                * This function is used to correct the normals at the edges of the tile mesh, which when automatically calculated using mesh.RecalculateNormals(), does not take into account the geometry of adjacent meshes, and produces 'creases' at the tile edges.
                * Toggle fixNormalsAtSeams to see this in effect.
                */
                Vector3[] radialPoints = new Vector3[6];

                for (int p = 0; p < 6; p++)
                {
                    float pX = centerPoint.x + HexUtils.sinAngles[p] * radius;
                    float pZ = centerPoint.z + HexUtils.cosAngles[p] * radius;
                    radialPoints[p] = Map.Instance.OnTerrain(pX, pZ);
                }

                Vector3 surfaceNormalSum = Vector3.zero;

                for (int p = 0; p < 6; p++)
                    surfaceNormalSum += CalculateSurfaceNormal(centerPoint, radialPoints[p], radialPoints[(p + 1) % 6]);

                //Can avoid a sqrt here as CalculateSurfaceNormal returns a unit vector and dividing by six works fine
                //return surfaceNormalSum.normalized;
                return surfaceNormalSum / 6;
            }

            Vector3[] normals = mesh.normals;

            //Loops through all the normals in the array that need correcting.
            //Starting with the index which is the centered hexagonal number one smaller than the tile (which is the resolution)
            //And looping through all the points on all six edges 
            i = CenteredHexagonalNum(res);
            for (int e = 0; e < res * 6; e++)
            {
                //Conveniently use the local vertex position stored in the array, and add the transform.position for global position
                //The radius to sample at is 1 / resolution, which replicates the positions of the six surrounding mesh vertices, whether they exist in this mesh or not
                normals[i] = CalculateVectorNormal(new Vector3(centerCartesian.x, 0, centerCartesian.y) + vertices[i], size / res);
                i++;
            }

            mesh.normals = normals;
        }

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }
    static Vector3 CalculateSurfaceNormal(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 v1 = Vector3.zero;             // Vector 1 (x,y,z) & Vector 2 (x,y,z)
        Vector3 v2 = Vector3.zero;
        Vector3 normal = Vector3.zero;

        // Finds The Vector Between 2 Points By Subtracting
        // The x,y,z Coordinates From One Point To Another.

        // Calculate The Vector From Point 2 To Point 1
        v1.x = p1.x - p2.x;
        v1.y = p1.y - p2.y;
        v1.z = p1.z - p2.z;
        // Calculate The vector From Point 3 To Point 2
        v2.x = p2.x - p3.x;
        v2.y = p2.y - p3.y;
        v2.z = p2.z - p3.z;

        // Compute The Cross Product To Give Us A Surface Normal
        normal.x = v1.y * v2.z - v1.z * v2.y;   // Cross Product For Y - Z
        normal.y = v1.z * v2.x - v1.x * v2.z;   // Cross Product For X - Z
        normal.z = v1.x * v2.y - v1.y * v2.x;   // Cross Product For X - Y

        normal.Normalize();

        return normal;
    }
}
