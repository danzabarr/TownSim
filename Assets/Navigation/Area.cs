using UnityEngine;

namespace TownSim.Navigation
{
    public class Area : MonoBehaviour
    {
        public bool showHandles;
        public static readonly Color outlineColor = new Color(1, 0, 0, .2f);
        public static readonly Color fillColor = new Color(1, 1, 1, .2f);

        private bool hasStarted;
        private bool hasObstructed;

        [SerializeField] private bool calculateOutline;
        [SerializeField] private bool calculateFill = true;
        [SerializeField] private Vector2[] points;

        private Vector2[] transformedPoints;
        private Bounds2 transformedBounds;

        public Bounds2 Bounds => transformedBounds;
        public Vector2Int[] FillPoints { get; private set; }
        public Vector2Int[] OutlinePoints { get; private set; }

        private void Recalculate()
        {
            transformedPoints = Polygon2.TransformedPoints(points, transform, true);
            transformedBounds = Polygon2.CalculateBounds(transformedPoints);
            if (calculateFill) FillPoints = HexUtils.ScanLineFill(transformedPoints, Map.Size, Map.NodeRes).ToArray();
            if (calculateOutline) OutlinePoints = HexUtils.TraverseOutline(transformedPoints, Map.Size / Map.NodeRes, true).ToArray();
        }

        public bool InsidePolygon(Vector3 point)
        {
            return Polygon2.PolygonContainsPoint(points, point.xz());
        }

        public bool InsideArea(Vector2Int point)
        {
            if (calculateFill)
                foreach (Vector2Int p in FillPoints)
                    if (point == p)
                        return true;
            if (calculateOutline)
                foreach (Vector2Int p in OutlinePoints)
                    if (point == p)
                        return true;
            return false;
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
                Recalculate();
        }

        private void Start()
        {
            if (!hasObstructed)
            {
                ObstructArea();
                hasObstructed = true;
            }
            hasStarted = true;
        }

        private void OnEnable()
        {
            if (hasStarted && !hasObstructed)
            {
                ObstructArea();
                hasObstructed = true;
            }
        }

        private void OnDisable()
        {
            if (hasObstructed)
            {
                RevertArea();
                hasObstructed = false;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Polygon2.DrawPolygon(Polygon2.TransformedPoints3(points, transform, true));
        }

        private void OnDrawGizmosSelected()
        {
            float size = Map.Size;
            int res = Map.NodeRes;

            Gizmos.color = Color.red;

            if (calculateFill && FillPoints != null)
                foreach (Vector2Int fill in FillPoints)
                    HexUtils.DrawHexagonVert(fill.x, fill.y, size, res);

            Gizmos.color = Color.yellow;

            if (calculateOutline && OutlinePoints != null)
                foreach (Vector2Int fill in OutlinePoints)
                    HexUtils.DrawHexagonVert(fill.x, fill.y, size, res);

            //Vector2 voxelSize = Vector2.one * World.Map.TileSize / NavigationGraph.Resolution;
            //Vector2 voxelOffset = Vector2.one * 0;
            //Map map = Map.Instance;
            //
            //void DrawCube(Vector2Int p)
            //{
            //    Vector3 pos = ((p + voxelOffset) * voxelSize).x0y();
            //    Vector3 size = voxelSize.x0y();
            //
            //    if (map)
            //        pos = map.OnTerrain(pos);
            //
            //    Gizmos.DrawCube(pos, size * .95f);
            //}
            //
            //if (calculateFill && fillPoints != null)
            //{
            //    Gizmos.color = fillColor;
            //    foreach (Vector2Int p in fillPoints)
            //        DrawCube(p);
            //}
            //
            //if (calculateOutline && outlinePoints != null)
            //{
            //    Gizmos.color = outlineColor;
            //    foreach (Vector2Int p in outlinePoints)
            //        DrawCube(p);
            //}
        }

        public void ObstructArea()
        {
            Recalculate();
            if (calculateFill && FillPoints != null)
            {
                foreach (Vector2Int p in FillPoints)
                    if (Map.nodes.TryGetValue(p, out Node node))
                        node.Obstructions++;
            }
            if (calculateOutline && OutlinePoints != null)
            {
                foreach (Vector2Int p in OutlinePoints)
                    if (Map.nodes.TryGetValue(p, out Node node))
                        node.Obstructions++;
            }
        }

        public void RevertArea()
        {
            if (calculateFill && FillPoints != null)
            {
                foreach (Vector2Int p in FillPoints)
                    if (Map.nodes.TryGetValue(p, out Node node))
                        node.Obstructions--;
            }
            if (calculateOutline && OutlinePoints != null)
            {
                foreach (Vector2Int p in OutlinePoints)
                    if (Map.nodes.TryGetValue(p, out Node node))
                        node.Obstructions--;
            }
        }
    }
}
