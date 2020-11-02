using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace TownSim.Navigation
{

    public static class PathFinding
    {
        public static readonly int PathFindingThreads = 32;
        public static readonly int MaxTries = 10000;

        private static Thread[] threads;
        private static BlockingCollection<Request>[] queues;
        private static Dictionary<Node, Data>[] nodeDataMaps;
        private static List<Node>[] openLists;
        private static List<Node>[] visitedLists;

        public struct Data
        {
            public int index;
            public int parent;
            public float hCost;
            public float gCost;
            public bool open;
        }

        public delegate bool Match(Node node);

        public readonly struct Point
        {
            public Point(Node node, Data data)
            {
                Node = node;
                Data = data;
            }
            public Node Node { get; }
            public Data Data { get; }
        }

        public class Request
        {
            public delegate void Callback();
            public string identifier;
            public Vector3 start, end;
            public Node startNode, endNode;
            public Node[] addStartNeighbours;
            public Node[] endNodes;
            public Match match;
            public float maxCost;
            public float takeExistingPaths;
            public bool matchTowardsEnd;
            public bool allowInaccessibleEnd;
            public Callback callback;
            public bool Queued { get; private set; }
            public bool Started { get; private set; }
            public bool Cancelled { get; private set; }
            public bool Completed { get; private set; }
            public List<Point> Path { get; private set; }
            public Result Result { get; private set; }
            public Node First => Path[0].Node;
            public Node Last => Path[Path.Count - 1].Node;

            public static void StartThreads()
            {
                if (queues == null)
                    StartThreads(PathFindingThreads);
            }

            public static void JoinThreads()
            {

                if (queues != null)
                    foreach(BlockingCollection<Request> queue in queues)
                        foreach(Request request in queue)
                            request.Cancel();

                    if (threads != null)
                    foreach (Thread thread in threads)
                        thread.Join();

                threads = null;
                queues = null;
                nodeDataMaps = null;
                openLists = null;
                visitedLists = null;
            }

            private static void StartThreads(int amount)
            {
                threads = new Thread[amount];
                queues = new BlockingCollection<Request>[amount];
                nodeDataMaps = new Dictionary<Node, Data>[amount];
                openLists = new List<Node>[amount];
                visitedLists = new List<Node>[amount];

                for (int i = 0; i < amount; i++)
                {
                    queues[i] = new BlockingCollection<Request>();
                    nodeDataMaps[i] = new Dictionary<Node, Data>();
                    openLists[i] = new List<Node>();
                    visitedLists[i] = new List<Node>();
                    threads[i] = new Thread(ProcessQueue);
                    threads[i].Start(i);
                }
            }
            private static void ProcessQueue(object queue)
            {
                int i = (int)queue;
                while (true)
                {
                    foreach (Request request in queues[i].GetConsumingEnumerable())
                    {
                        if (request.Cancelled)
                            continue;
                        request.Execute(i);
                    }
                }
            }

            public void Queue()
            {
                if (Queued)
                    return;

                if (queues == null)
                    StartThreads(PathFindingThreads);

                int Shortest()
                {
                    int shortestLength = int.MaxValue;
                    int shortestIndex = 0;
                    for (int i = 0; i < threads.Length; i++)
                    {
                        int length = queues[i].Count;
                        if (length == 0)
                            return i;
                        if (length < shortestLength)
                        {
                            shortestLength = length;
                            shortestIndex = i;
                        }
                    }
                    return shortestIndex;
                }
                queues[Shortest()].Add(this);
                Queued = true;
            }

            public void Cancel()
            {
                if (!Started && !Cancelled)
                {
                    Cancelled = true;
                    callback?.Invoke();
                }
            }

            private void Execute(int thread)
            {
                Started = true;
                Result = PathFind(start, end, startNode, endNode, addStartNeighbours, endNodes, match, matchTowardsEnd, allowInaccessibleEnd, maxCost, takeExistingPaths, out List<Point> path, thread);
                Path = path;
                Completed = true;
                callback?.Invoke();
            }
        }

        public enum Result
        {
            Pending,
            FailureNoPath,
            Success,
            AtDestination,
            FailureTooManyTries,
            FailureTooFar,
            FailureStartObstructed,
            FailureEndObstructed,
            FailureZeroEndNodes,
        }

        public static void DrawPath(List<Point> path, bool drawSpheres = false, bool labelNodes = false, bool labelEdges = false)
        {
            if (path == null)
                return;
#if UNITY_EDITOR

            for (int i = 0; i < path.Count - 1; i++)
                Gizmos.DrawLine(path[i].Node.Position, path[i + 1].Node.Position);

            if (labelEdges)
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector3 midPoint = (path[i].Node.Position + path[i + 1].Node.Position) / 2f;
                    float length = Node.Distance(path[i].Node, path[i + 1].Node);
                    Handles.Label(midPoint, length + "");
                }

            if (drawSpheres)
                foreach (Point pp in path)
                    Gizmos.DrawSphere(pp.Node.Position, 0.1f);

            if (labelNodes)
                foreach (Point pp in path)
                    Handles.Label(pp.Node.Position, pp.Node.Position + "");
#endif
        }

        public static Result PathFind(Vector3 start, Vector3 end, Node startNode, Node endNode, Node[] addStartNeighbours, Node[] endNodes, Match match, bool matchTowardsEnd, bool allowInaccessibleEnd, float maxCost, float takeExistingPaths, out List<Point> path, int thread = -1)
        {
            if (endNodes != null)
                return PathFind(start, startNode, endNodes, addStartNeighbours, allowInaccessibleEnd, maxCost, takeExistingPaths, out path, thread);

            path = null;
            Node s = startNode;
            Node e = endNode;
            Vector2Int[] endNeighbours = new Vector2Int[0];

            if (endNode != null && !endNode.Accessible && !allowInaccessibleEnd)
                return Result.FailureEndObstructed;


            if (startNode == null)
            {
                s = new Node(start);
                Vector2Int[] startNeighbours = Map.Instance.NearestNodes(start.x, start.z);

                foreach (Vector2Int v in startNeighbours)
                {
                    if (!Map.nodes.TryGetValue(v, out Node neighbour))
                        continue;

                    if (!neighbour.Accessible)
                        continue;

                    if (!s.Neighbours.ContainsKey(neighbour))
                        s.Neighbours.TryAdd(neighbour, Node.Distance(s, neighbour));
                }
            }

            if (addStartNeighbours != null)
                foreach (Node add in addStartNeighbours)
                    if (!s.Neighbours.ContainsKey(add))
                        s.Neighbours.TryAdd(add, Node.Distance(s, add));
            if (s.Neighbours.Count <= 0)
                return Result.FailureStartObstructed;

            int enCount = 0;
            if (endNode == null)
            {
                e = new Node(end);
                endNeighbours = Map.Instance.NearestNodes(end.x, end.z);
                foreach (Vector2Int v in endNeighbours)
                {
                    if (!Map.nodes.TryGetValue(v, out Node neighbour))
                        continue;

                    if (!neighbour.Accessible)
                        continue;

                    if (!neighbour.Neighbours.ContainsKey(e))
                    {
                        if (neighbour.Neighbours.TryAdd(e, Node.Distance(neighbour, e)))
                            enCount++;
                    }
                }
            }

            enCount += e.Neighbours.Count;
            if (enCount <= 0)
                return Result.FailureEndObstructed;

            Result result;

            if (match != null)
                result = PathToMatch(s, match, end, matchTowardsEnd, allowInaccessibleEnd, maxCost, takeExistingPaths, out path, thread);
            else
                result = PathToNode(s, e, allowInaccessibleEnd, maxCost, takeExistingPaths, out path, thread);

            if (endNode == null)
            {
                foreach (Vector2Int v in endNeighbours)
                {
                    if (!Map.nodes.TryGetValue(v, out Node neighbour))
                        continue;
                    neighbour.Neighbours.TryRemove(e, out _);
                }
            }

            if (result == Result.Success)
                RaycastModifier(path, takeExistingPaths);

            return result;
        }

        public static Result PathFind(Vector3 start, Node startNode, Node[] endNodes, Node[] addStartNeighbours, bool allowInaccessibleEnd, float maxCost, float takeExistingPaths, out List<Point> path, int thread = -1)
        {
            path = default;
            if (endNodes == null || endNodes.Length <= 0)
                return Result.FailureZeroEndNodes;

            Node s = startNode;
            Vector2Int[] startNeighbours = new Vector2Int[0];

            if (startNode == null)
            {
                s = new Node(start);
                startNeighbours = Map.Instance.NearestNodes(start.x, start.z);

                foreach (Vector2Int v in startNeighbours)
                {
                    if (!Map.nodes.TryGetValue(v, out Node neighbour))
                        continue;

                    if (!neighbour.Accessible)
                        continue;

                    if (!s.Neighbours.ContainsKey(neighbour))
                        s.Neighbours.TryAdd(neighbour, Node.Distance(s, neighbour));
                }

            }

            foreach (Node add in addStartNeighbours)
                if (!s.Neighbours.ContainsKey(add))
                    s.Neighbours.TryAdd(add, Node.Distance(s, add));

            if (s.Neighbours.Count <= 0)
                return Result.FailureStartObstructed;


            List<Node> sortedEndNodes = new List<Node>(endNodes);
            sortedEndNodes.Sort((Node a, Node b) =>
            {
                float da = Node.Distance(a, s);
                float db = Node.Distance(b, s);
                return da.CompareTo(db);
            });

            float bestPathCost = maxCost;

            foreach (Node e in sortedEndNodes)
            {
                if (e == null)
                    continue;
                if (!e.Accessible && !allowInaccessibleEnd)
                    continue;

                //float euclideanDistance = Node.Distance(e, s);
                //if (euclideanDistance * Node.MinDesirePathCost > bestPathCost)
                //    continue;

                Result result = PathToNode(s, e, allowInaccessibleEnd, bestPathCost, takeExistingPaths, out List<Point> p, thread);

                if (result == Result.AtDestination)
                {
                    path = p;
                    return Result.AtDestination;
                }

                if (result != Result.Success)
                    continue;

                RaycastModifier(p, takeExistingPaths);

                float pathCost = p[p.Count - 1].Data.gCost;

                if (pathCost < bestPathCost)
                {
                    bestPathCost = pathCost;
                    path = p;
                }
            }

            if (path == null)
                return Result.FailureNoPath;

            return Result.Success;
        }

        private static Result PathToNode(Node start, Node end, bool allowInaccessibleEnd, float maxCost, float takeExistingPaths, out List<Point> path, int thread = -1)
        {
            path = null;
            if (start == null || end == null)
                return Result.FailureNoPath;

            if (!allowInaccessibleEnd && !end.Accessible)
                return Result.FailureEndObstructed;

            if (start.Equals(end))
                return Result.AtDestination;

            Dictionary<Node, Data> nodeData;
            List<Node> visited;
            List<Node> open;

            if (thread < 0 || thread >= threads.Length)
            {
                nodeData = new Dictionary<Node, Data>();
                visited = new List<Node>();
                open = new List<Node>();
            }
            else
            {
                nodeData = nodeDataMaps[thread];
                visited = visitedLists[thread];
                open = openLists[thread];
            }

            void Clear()
            {
                nodeData.Clear();
                visited.Clear();
                open.Clear();
            }

            nodeData.Add(start, new Data
            {
                index = 0,
                parent = -1,
                gCost = 0,
                hCost = Node.Distance(start, end),
                open = true
            });

            open.Add(start);
            visited.Add(start);

            int tries = 0;
            while (true)
            {
                tries++;
                if (tries > MaxTries)
                {
                    Clear();
                    return Result.FailureTooManyTries;
                }

                if (open.Count == 0)
                {
                    Clear();
                    return Result.FailureNoPath;
                }

                Node currentNode = null;
                float currentCost = float.MaxValue;
                int currentIndex = 0;

                for (int i = 0; i < open.Count; i++)
                {
                    Node node = open[i];
                    Data data = nodeData[node];

                    float cost = data.gCost + data.hCost;
                    if (cost < currentCost)
                    {
                        currentIndex = i;
                        currentNode = node;
                        currentCost = cost;
                    }
                }

                if (currentNode.Equals(end))
                {
                    break;
                }

                Data currentData = nodeData[currentNode];
                if (currentData.gCost > maxCost)
                {
                    Clear();
                    return Result.FailureTooFar;
                }

                open.RemoveAt(currentIndex);
                currentData.open = false;
                nodeData[currentNode] = currentData;

                foreach (Node neighbour in currentNode.Neighbours.Keys)
                {
                    if (neighbour == null)
                        continue;

                    if (!neighbour.Accessible && (!allowInaccessibleEnd || !neighbour.Equals(end)))
                        continue;

                    if (!currentNode.Neighbours.TryGetValue(neighbour, out float nDistance))
                        continue;

                    //if (!currentNode.NeighbourAccessible(i))
                    //    continue;

                    float tentativeGCost = currentData.gCost + nDistance * Mathf.Lerp(1, (currentNode.MovementCost + neighbour.MovementCost) / 2, takeExistingPaths);
                    float tentativeHCost = Node.Distance(neighbour, end);
                    float tentativeCost = tentativeGCost + tentativeHCost;

                    bool neighbourExists = nodeData.TryGetValue(neighbour, out Data neighbourData);

                    if (!neighbourExists || tentativeCost < neighbourData.gCost + neighbourData.hCost)
                    {
                        if (!neighbourExists)
                            neighbourData = new Data();

                        neighbourData.parent = currentData.index;
                        neighbourData.gCost = tentativeGCost;
                        neighbourData.hCost = tentativeHCost;

                        if (!neighbourData.open)
                        {
                            neighbourData.open = true;
                            open.Add(neighbour);
                        }

                        if (neighbourData.index == -1 || !neighbourExists)
                        {
                            neighbourData.index = visited.Count;
                            visited.Add(neighbour);
                        }
                        if (!neighbourExists)
                            nodeData.Add(neighbour, neighbourData);
                        else
                            nodeData[neighbour] = neighbourData;
                    }
                }
            }

            path = new List<Point>();
            Node n = end;
            Data d = nodeData[end];
            while (d.parent != -1)
            {
                path.Insert(0, new Point(n, d));
                n = visited[d.parent];
                d = nodeData[n];
            }
            path.Insert(0, new Point(start, nodeData[start]));

            Clear();
            return Result.Success;
        }

        private static Result PathToMatch(Node start, Match match, Vector3 end, bool matchTowardsEnd, bool allowInaccessibleEnd, float maxCost, float takeExistingPaths, out List<Point> path, int thread = -1)
        {
            path = null;
            if (start == null)
                return Result.FailureNoPath;

            if (match(start))
                return Result.AtDestination;

            Dictionary<Node, Data> nodeData;
            List<Node> visited;
            List<Node> open;

            if (thread < 0 || thread >= threads.Length)
            {
                nodeData = new Dictionary<Node, Data>();
                visited = new List<Node>();
                open = new List<Node>();
            }
            else
            {
                nodeData = nodeDataMaps[thread];
                visited = visitedLists[thread];
                open = openLists[thread];
            }

            void Clear()
            {
                nodeData.Clear();
                visited.Clear();
                open.Clear();
            }

            nodeData.Add(start, new Data
            {
                index = 0,
                parent = -1,
                gCost = 0,
                hCost = matchTowardsEnd ? Vector3.Distance(start.Position, end) : 0,
                open = true
            });

            open.Add(start);
            visited.Add(start);
            Node currentNode;

            int tries = 0;
            while (true)
            {
                tries++;
                if (tries > MaxTries)
                {
                    Clear();
                    return Result.FailureTooManyTries;
                }

                if (open.Count == 0)
                {
                    Clear();
                    return Result.FailureNoPath;
                }

                currentNode = null;
                float currentCost = float.MaxValue;
                int currentIndex = 0;

                for (int i = 0; i < open.Count; i++)
                {
                    Node node = open[i];
                    Data data = nodeData[node];

                    float cost = data.gCost + data.hCost;
                    if (cost < currentCost)
                    {
                        currentIndex = i;
                        currentNode = node;
                        currentCost = cost;
                    }
                }

                if (match(currentNode))
                {
                    break;
                }

                Data currentData = nodeData[currentNode];
                if (currentData.gCost > maxCost)
                {
                    Clear();
                    return Result.FailureTooFar;
                }

                open.RemoveAt(currentIndex);
                currentData.open = false;
                nodeData[currentNode] = currentData;

                if (currentNode == null)
                    Debug.Log("currentNode");
                else if (currentNode.Neighbours == null)
                {
                    Debug.Log("Neighbours " + currentNode);
                    continue;
                }
                else if (currentNode.Neighbours.Keys == null)
                    Debug.Log("Keys");
                List<Node> neighbours = new List<Node>(currentNode.Neighbours.Keys);
                //foreach(KeyValuePair<Node, float> pair in currentNode.Neighbours)
                //{




                foreach (Node neighbour in neighbours)
                {
                    //Node neighbour = pair.Key;
                    if (neighbour == null)
                        continue;

                    if (!neighbour.Accessible && (!allowInaccessibleEnd || !match(neighbour)))
                        continue;
                    float distance = currentNode.Neighbours[neighbour];

                    //if (!currentNode.NeighbourAccessible(i))
                    //    continue;

                    float tentativeGCost = currentData.gCost + distance * Mathf.Lerp(1, (currentNode.MovementCost + neighbour.MovementCost) / 2, takeExistingPaths);
                    float tentativeHCost = matchTowardsEnd ? Vector3.Distance(neighbour.Position, end) : 0;
                    float tentativeCost = tentativeGCost + tentativeHCost;

                    bool neighbourExists = nodeData.TryGetValue(neighbour, out Data neighbourData);

                    if (!neighbourExists || tentativeCost < neighbourData.gCost + neighbourData.hCost)
                    {
                        if (!neighbourExists)
                            neighbourData = new Data();

                        neighbourData.parent = currentData.index;
                        neighbourData.gCost = tentativeGCost;
                        neighbourData.hCost = tentativeHCost;

                        if (!neighbourData.open)
                        {
                            neighbourData.open = true;
                            open.Add(neighbour);
                        }

                        if (neighbourData.index == -1 || !neighbourExists)
                        {
                            neighbourData.index = visited.Count;
                            visited.Add(neighbour);
                        }
                        if (!neighbourExists)
                            nodeData.Add(neighbour, neighbourData);
                        else
                            nodeData[neighbour] = neighbourData;
                    }
                }
            }


            path = new List<Point>();
            Node n = currentNode;
            Data d = nodeData[n];
            while (d.parent != -1)
            {
                path.Insert(0, new Point(n, d));
                n = visited[d.parent];
                d = nodeData[n];
            }
            path.Insert(0, new Point(start, nodeData[start]));

            //Data lastData = path[path.Count - 1].Data;
            //Debug.Log($"cost: {lastData.gCost + lastData.hCost} tries:{tries}");

            Clear();
            return Result.Success;
        }

        private static void RaycastModifier(List<Point> path, float takeExistingPaths)
        {
            if (path == null)
                return;

            if (path.Count <= 2)
                return;


            int startIndex = 0;

            List<Vector3> points = new List<Vector3>();

            float totalCost = path[path.Count - 1].Data.gCost;

            float scale = Map.Size / Map.NodeRes;
            float SQRT_3 = HexUtils.SQRT_3;

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


            while (path.Count > startIndex)
            {
                for (int i = path.Count - 1; i > startIndex; i--)
                {
                    //float pathDistance = path[i].Data.PathDistance - path[startIndex].Data.PathDistance;
                    float pathCost = path[i].Data.gCost - path[startIndex].Data.gCost;// path[i].Data.gCost - path[startIndex].Data.gCost;



                    bool valid = true;
                    points.Clear();
                    //float shortCutDistance = 0;
                    float shortCutCost = 0;

                    Vector2 p0 = path[startIndex].Node.Position.xz();
                    Vector2 p1 = path[i].Node.Position.xz();

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

                    Vector3 lastIntersection = path[startIndex].Node.Position;

                    for (int j = 0; j < 1000; j++)
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



                        //Calculate where the line intersects with the edge
                        Vector3 intersection = Map.Instance.OnMesh(p0 + delta * t);

                        //Increment the current hex tile across the nearest edge
                        current_hex += step;


                        if (!Map.nodes.TryGetValue(current_hex, out Node node))
                        {
                            valid = false;
                            break;
                        }

                        if (!node.Accessible)
                        {
                            valid = false;
                            break;
                        }

                        //Break at the end of the line
                        if (t > 1)
                        {
                            float finalDistance = Vector3.Distance(lastIntersection, path[i].Node.Position);
                            //shortCutDistance += finalDistance;
                            float finalCost = finalDistance * Mathf.Lerp(1, node.MovementCost, takeExistingPaths);
                            shortCutCost += finalCost;

                            //Handles.Label(Vector3.Lerp(lastIntersection, path[i].Position, .5f), node.Hex + " " + finalDistance + " " + finalCost);

                            if (shortCutCost > pathCost)
                            {
                                valid = false;
                                break;
                            }
                            break;
                        }

                        float distance = Vector3.Distance(lastIntersection, intersection);
                        //shortCutDistance += distance;

                        float cost = distance * Mathf.Lerp(1, node.MovementCost, takeExistingPaths);
                        shortCutCost += cost;

                        //Handles.Label(Vector3.Lerp(lastIntersection, intersection, .5f), node.Hex + " " + distance + " " + cost);

                        if (shortCutCost > pathCost)
                        {
                            valid = false;
                            break;
                        }

                        points.Add(intersection);

                        lastIntersection = intersection;
                    }



                    if (valid)
                    {
                        totalCost += shortCutCost - pathCost;
                        //Debug.Log("(" + startIndex + "-" + i + ") Path distance: " + pathDistance + " Shortcut distance: " + shortCutDistance);
                        path.RemoveRange(startIndex + 1, i - startIndex - 1);
                        foreach (Vector3 point in points)
                        {
                            startIndex++;
                            path.Insert(startIndex, new Point(new Node(point), default));
                        }
                        break;
                    }
                }
                startIndex++;
            }

            Node lastNode = path[path.Count - 1].Node;
            Data lastData = path[path.Count - 1].Data;
            lastData.gCost = totalCost;
            path[path.Count - 1] = new Point(lastNode, lastData);
        }
        /*
        private static void RaycastModifier(List<Point> path, float takeExistingPaths)
        {
            if (path == null)
                return;

            if (path.Count <= 2)
                return;

            Map map = Map.Instance;

            int startIndex = 0;

            List<Vector3> points = new List<Vector3>();

            float totalCost = path[path.Count - 1].Data.gCost;

            while (path.Count > startIndex)
            {
                for (int i = path.Count - 1; i > startIndex; i--)
                {
                    //float pathDistance = path[i].Data.PathDistance - path[startIndex].Data.PathDistance;
                    float pathCost = path[i].Data.gCost - path[startIndex].Data.gCost;// path[i].Data.gCost - path[startIndex].Data.gCost;

                    bool valid = true;
                    points.Clear();
                    //float shortCutDistance = 0;
                    float shortCutCost = 0;

                    Vector2 p0 = path[startIndex].Node.Position.xz();
                    Vector2 p1 = path[i].Node.Position.xz();
                    float voxelSize = World.Map.TileSize / NavigationGraph.Resolution;
                    Vector2 voxelOffset = Vector2.one * .5f;// .5f;

                    float Step(float x, float y) => y >= x ? 1 : 0;
                    Vector2 Vector2Abs(Vector2 a) => new Vector2(Mathf.Abs(a.x), Mathf.Abs(a.y));

                    p0 /= voxelSize;
                    p1 /= voxelSize;

                    p0 -= voxelOffset;
                    p1 -= voxelOffset;

                    Vector2 rd = p1 - p0;
                    Vector2 p = new Vector2(Mathf.Floor(p0.x), Mathf.Floor(p0.y));
                    Vector2 rdinv = Vector2.one / rd;
                    Vector2 stp = new Vector2(Mathf.Sign(rd.x), Mathf.Sign(rd.y));
                    Vector2 delta = Vector2.Min(rdinv * stp, Vector2.one);
                    Vector2 t_max = Vector2Abs((p + Vector2.Max(stp, Vector2.zero) - p0) * rdinv);

                    Vector3 lastIntersection = path[startIndex].Node.Position;

                    int steps = 0;
                    while (steps < 1000)
                    {
                        steps++;
                        Vector2Int square = Vector2Int.RoundToInt(p) + Vector2Int.one;

                        if (!NavigationGraph.TryGetNode(square, out Node node))
                        {
                            valid = false;
                            break;
                        }

                        if (!node.Accessible)
                        {
                            valid = false;
                            break;
                        }

                        //Gizmos.DrawCube(node.Position, new Vector3(voxelSize, .05f, voxelSize));

                        float next_t = Mathf.Min(t_max.x, t_max.y);
                        if (next_t > 1.0)
                        {
                            float finalDistance = Vector3.Distance(lastIntersection, path[i].Node.Position);
                            //shortCutDistance += finalDistance;
                            float finalCost = finalDistance * Mathf.Lerp(1, node.MovementCost, takeExistingPaths);
                            shortCutCost += finalCost;

                            //Handles.Label(Vector3.Lerp(lastIntersection, path[i].Position, .5f), node.Hex + " " + finalDistance + " " + finalCost);

                            if (shortCutCost > pathCost)
                            {
                                valid = false;
                                break;
                            }
                            break;
                        }

                        Vector3 intersection = map.OnTerrain((p0 + next_t * rd + voxelOffset) * voxelSize);
                        float distance = Vector3.Distance(lastIntersection, intersection);
                        //shortCutDistance += distance;

                        float cost = distance * Mathf.Lerp(1, node.MovementCost, takeExistingPaths);
                        shortCutCost += cost;

                        //Handles.Label(Vector3.Lerp(lastIntersection, intersection, .5f), node.Hex + " " + distance + " " + cost);

                        if (shortCutCost > pathCost)
                        {
                            valid = false;
                            break;
                        }

                        points.Add(intersection);

                        lastIntersection = intersection;

                        Vector2 cmp = new Vector2(Step(t_max.x, t_max.y), Step(t_max.y, t_max.x));
                        t_max += delta * cmp;
                        p += stp * cmp;
                    }

                    if (valid)
                    {
                        totalCost += shortCutCost - pathCost;
                        //Debug.Log("(" + startIndex + "-" + i + ") Path distance: " + pathDistance + " Shortcut distance: " + shortCutDistance);
                        path.RemoveRange(startIndex + 1, i - startIndex - 1);
                        foreach (Vector3 point in points)
                        {
                            startIndex++;
                            path.Insert(startIndex, new Point(new Node(point), default));
                        }
                        break;
                    }
                }
                startIndex++;
            }

            Node lastNode = path[path.Count - 1].Node;
            Data lastData = path[path.Count - 1].Data;
            lastData.gCost = totalCost;
            path[path.Count - 1] = new Point(lastNode, lastData);
        }
        */
    }
}
