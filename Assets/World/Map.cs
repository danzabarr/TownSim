using TownSim.Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEditor;
using TownSim.Units;
using TownSim.IO;
using UnityEngine.Profiling;

public class Map : MonoBehaviour, IEnumerable<Tile>
{
    private static Map instance;
    public static Map Instance
    {
        get
        {
            //if (instance == null)
            //    instance = FindObjectOfType<Map>();
            return instance;
        }
    }

    public bool generateOnStart;
    private void Start()
    {
        if (generateOnStart)
        {
            ClearAll();
            Reseed(seed);
            PurchaseTile(0, 0);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (ScreenCast.MouseTerrain.Cast(out Tile mouse))
            {
                PurchaseTile(mouse.X, mouse.Y);
            }
        }
    }

    private void PurchaseTile(int x, int y)
    {
        AddTile(x + 0, y + 0, true);
        AddTile(x + 1, y + 0, true);
        AddTile(x + 0, y + 1, true);
        AddTile(x - 1, y + 0, true);
        AddTile(x + 0, y - 1, true);
        AddTile(x + 1, y - 1, true);
        AddTile(x - 1, y + 1, true);
        RegionManager.SetRegion(1, x, y);
        RegionManager.UpdateTileVisibility();
    }

    private void Awake()
    {
        instance = this;
    }

    public static bool started;
    public void Begin()
    {
        started = true;
    }
    public void ReloadScene()
    {
        started = false;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public GenerationSettings settings;

    public int seed;
    public float SeedOffsetX { get; private set; }
    public float SeedOffsetY { get; private set; }

    public Tile tilePrefab;
    public bool drawNodes;

    public static Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
    public static Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    public static Dictionary<Vector2Int, ResourceNode> resources = new Dictionary<Vector2Int, ResourceNode>();

    public static float Size { get; private set; } = 48;
    public static int MeshRes { get; private set; } = 48;
    public static int NodeRes { get; private set; } = 48;
    public static int Seed => Instance.seed;

    public IEnumerator<Tile> GetEnumerator() => tiles.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static Tile GetTile(Vector2Int hex)
    {
        if (tiles.TryGetValue(hex, out Tile tile))
            return tile;
        return null;
    }

    public static int TileCount => tiles.Count;
    public void ClearAll()
    {
        foreach (KeyValuePair<Vector2Int, Tile> pair in tiles)
            Destroy(pair.Value);

        tiles = new Dictionary<Vector2Int, Tile>();
        nodes = new Dictionary<Vector2Int, Node>();
        resources = new Dictionary<Vector2Int, ResourceNode>();
    }

    public float Height(float x, float z)
    {
        return settings.Sample(x + SeedOffsetX, z + SeedOffsetY);
    }

    public float Height(Vector2 pos) => Height(pos.x, pos.y);

    public float MeshHeight(Vector2 pos) => MeshHeight(pos.x, pos.y);

    public float MeshHeight(float x, float z)
    {
        return Height(x, z);
        Profiler.BeginSample("Mesh Height");
        float size = Size;
        int res = MeshRes;

        float scale = size / res;

        float cx = x / scale - z * HexUtils.SQRT_3 / 3 / scale;
        float cy = z / HexUtils.SQRT_3 * 2 / scale;

        int ix = Mathf.FloorToInt(cx);
        int iy = Mathf.FloorToInt(cy);

        cx -= ix;
        cy -= iy;

        Vector3 VertPosition(int tx, int ty) => OnTerrain(HexUtils.VertToCart(tx, ty, size, res)); 

        Vector3 t1 = cx + cy > 1 ? VertPosition(ix + 1, iy + 1) : VertPosition(ix, iy);
        Vector3 t2 = VertPosition(ix + 1, iy);
        Vector3 t3 = VertPosition(ix, iy + 1);

        float d = (t2.z - t3.z) * (t1.x - t3.x) + (t3.x - t2.x) * (t1.z - t3.z);

        float bx = ((t2.z - t3.z) * (x - t3.x) + (t3.x - t2.x) * (z - t3.z)) / d;
        float by = ((t3.z - t1.z) * (x - t3.x) + (t1.x - t3.x) * (z - t3.z)) / d;
        float bz = 1 - bx - by;

        float result = t1.y * bx + t2.y * by + t3.y * bz;
        Profiler.EndSample();
        return result;
    }

    public Vector3 OnTerrain(float x, float z) => new Vector3(x, Height(x, z), z);
    public Vector3 OnTerrain(Vector2 pos) => OnTerrain(pos.x, pos.y);
    public Vector3 OnTerrain(Vector3 pos) => OnTerrain(pos.x, pos.z);

    public Vector3 NodePosition(Vector2Int pos)
    {
        return 
            nodes.TryGetValue(pos, out Node node) ? node.Position :
            OnTerrain(HexUtils.VertToCart(pos.x, pos.y, Size, NodeRes));
    }

    public float NeighbourDistance(Vector2Int n0, Vector2Int n1)
    {
        float h0 = nodes.TryGetValue(n0, out Node node0) ? node0.Position.y : MeshHeight(HexUtils.VertToCart(n0.x, n0.y, Size, NodeRes));
        float h1 = nodes.TryGetValue(n1, out Node node1) ? node1.Position.y : MeshHeight(HexUtils.VertToCart(n1.x, n1.y, Size, NodeRes));

        float size = Size / NodeRes;

        return Mathf.Sqrt(size * size + (h1 - h0) * (h1 - h0));
    }

    public float NeighbourDistance(float h0, Vector2Int nodePos)
    {
        float h1 = nodes.TryGetValue(nodePos, out Node node) ? node.Position.y : MeshHeight(HexUtils.VertToCart(nodePos.x, nodePos.y, Size, NodeRes));
        float size = Size / NodeRes;
        return Mathf.Sqrt(size * size + (h1 - h0) * (h1 - h0));
    }

    public Vector2Int[] NearestNodes(float x, float z)
    {
        float size = Size;
        int res = NodeRes;

        float scale = size / res;

        float cx = x / scale - z * HexUtils.SQRT_3 / 3 / scale;
        float cy = z / HexUtils.SQRT_3 * 2 / scale;

        int ix = Mathf.FloorToInt(cx);
        int iy = Mathf.FloorToInt(cy);

        cx -= ix;
        cy -= iy;

        return new Vector2Int[]
        {
            (cx + cy > 1 ? new Vector2Int(ix + 1, iy + 1) : new Vector2Int(ix, iy)),
            new Vector2Int(ix + 1, iy),
            new Vector2Int(ix, iy + 1)
        };
    }

    public Vector3 OnMesh(Vector2 pos) => new Vector3(pos.x, MeshHeight(pos.x, pos.y), pos.y);

    public void Reseed(int seed)
    {
        this.seed = seed;
        Random.InitState(seed);
        SeedOffsetX = Random.value * 10000 + 10000;
        SeedOffsetY = Random.value * 10000 + 10000;
    }

    public Tile AddTile(int x, int y, bool newTile = true)
    {

        Vector2Int hex = new Vector2Int(x, y);

        if (tiles.ContainsKey(hex))
            return null;


        Debug.Log($"Generating tile [{x},{y}] with seed {seed} and settings '{settings.name}'");

        Profiler.BeginSample($"Tile [{x},{y}]: Instantiating");
        Tile tile = Instantiate(tilePrefab);
        tile.transform.parent = transform;
        tile.SetPosition(x, y);
        Profiler.EndSample();

        Profiler.BeginSample($"Tile [{x},{y}]: Setting up path mask");
        tile.SetupPathMask();
        Profiler.EndSample();

        Profiler.BeginSample($"Tile [{x},{y}]: Adding nodes");
        tile.AddNodes();
        Profiler.EndSample();

        Profiler.BeginSample($"Tile [{x},{y}]: Generating mesh");
        tile.GenerateMesh();
        Profiler.EndSample();
        if (newTile)
        {
            Profiler.BeginSample($"Tile [{x},{y}]: Adding resources");
            tile.AddResources();
            Profiler.EndSample();
        }

        tiles.Add(hex, tile);

        return tile;
    }

    public bool TryGetTile(float x, float z, out Tile tile)
    {
        return tiles.TryGetValue(HexUtils.HexRound(HexUtils.CartToHex(x, z, Size)), out tile);
    }

    public bool TryGetTile(int x, int z, out Tile tile)
    {
        return tiles.TryGetValue(new Vector2Int(x, z), out tile);
    }

    public void OnDrawGizmos()
    {
        if (drawNodes)
        {
            foreach(KeyValuePair<Vector2Int, Node> pair in nodes)
            {
                Vector2Int key = pair.Key;
                Node value = pair.Value;
                if (value.Accessible)
                    Gizmos.color = Color.white;
                else
                    Gizmos.color = Color.red;

                HexUtils.DrawHexagonVert(key.x, value.Position.y, key.y, Size, NodeRes);
               // Gizmos.DrawCube(pair.Value.Position, Vector3.one * .1f);
            }
        }
    }

    public delegate bool Check(Vector3 position);

    public bool FindPosition(Vector3 center, float scale, int iterations, Check check, out Vector3 point)
    {
        point = center;

        for (int i = 0; i < iterations; i++)
        {
            point = OnMesh(center.xz() + Sunflower(i, scale));

            if (check(point))
            {
                return true;
            }
        }

        return false;
    }

    public static Vector2 Sunflower(int k, float scale, float ratio = 137.5f)
    {
        float r = scale * Mathf.Sqrt(k);
        float t = k * ratio;
        float x = Mathf.Cos(t * Mathf.Deg2Rad) * r;
        float y = Mathf.Sin(t * Mathf.Deg2Rad) * r;
        return new Vector2(x, y);
    }

    public bool ResourceInRange(Vector2Int node, int radius, out ResourceNode resource)
    {
        resource = null;
        foreach(Vector2Int p in HexUtils.Spiral(node, radius))
            if (resources.TryGetValue(p, out resource))
                return true;
        return false;
    }
}
