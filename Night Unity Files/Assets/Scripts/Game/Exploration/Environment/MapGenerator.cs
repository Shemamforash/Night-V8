using System.Collections.Generic;
using System.Linq;
using Game.Exploration.Region;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class MapGenerator : MonoBehaviour
    {
        public const int MapWidth = 120;
        public const int MinRadius = 6, MaxRadius = 9;
        public const int VisionRadius = 1;
        private const int DesiredSamples = 50;
        private static int _currentSamples;
        private static readonly List<MapNode> storedNodes = new List<MapNode>();
        private static MapNode initialNode;
        private static float _currentAlpha;
        private static float _flashSpeed = 1;
        private static List<MapNode> route;
        public static Transform MapTransform;

        public void Awake()
        {
            MapTransform = transform;
            storedNodes.ForEach(n => n.CreateObject());
//            CreateFogOfWar();
            UpdateNodeColor();
        }

        private void CreateFogOfWar()
        {
            GameObject fowprefab = Resources.Load<GameObject>("Prefabs/Map/FOW");
            for (float x = -20; x < 20; x += 0.5f)
            {
                for (float y = -20; y < 20; y += 0.5f)
                {
                    Vector3 fowPos = new Vector3(x, y, 0);
                    if (storedNodes.Any(n => n.Discovered() && Vector2.Distance(n.Position, fowPos) < 3)) continue;
                    GameObject g = Instantiate(fowprefab);
                    g.transform.position = new Vector3(x, y, 0);
                    g.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
                    g.transform.localScale = Vector2.one;
                }
            }
        }

        public static MapNode GetInitialNode()
        {
            return initialNode;
        }

        public static int NodeDistanceToTime(float distance)
        {
            //minradius = 30 minutes = 6 ticks
            float ticksPerUnit = (MinRadius * 2f) / WorldState.MinutesPerHour;
            int ticks = Mathf.CeilToInt(distance / ticksPerUnit);
            return ticks;
        }

        public static void Generate()
        {
            List<Region.Region> regions = RegionManager.GenerateRegions(DesiredSamples);

            List<Vector2> samples = AdvancedMaths.GetPoissonDiscDistribution(DesiredSamples, MinRadius, MaxRadius, MapWidth / 2f, true);
            for (int i = 0; i < samples.Count; ++i)
            {
                Vector2Int point = new Vector2Int((int) samples[i].x, (int) samples[i].y);
                if (i == 0)
                {
                    initialNode = MapNode.CreateNode(point, null);
                    storedNodes.Add(initialNode);
                }
                else
                {
                    CreateNewNode(point, regions[i]);
                }
            }

            ConnectNodes();
        }


        private static void CreateMinimumSpanningTree()
        {
            List<Node<MapNode>> nodes = new List<Node<MapNode>>();
            storedNodes.ForEach(n => nodes.Add(new Node<MapNode>(n, n.Position)));
            List<Edge<MapNode>> minTree = Pathfinding.MinimumSpanningTree(nodes);

            foreach (Edge<MapNode> edge in minTree)
            {
                edge.A.AddNeighbor(edge.B);
            }
        }

        private static void ConnectNodes()
        {
            CreateMinimumSpanningTree();
            List<MapNode> mapNodes = new List<MapNode>(storedNodes);
            foreach (MapNode current in mapNodes)
            {
                storedNodes.Sort((a, b) => Vector2.Distance(current.Position, a.Position).CompareTo(Vector2.Distance(current.Position, b.Position)));
                storedNodes.ForEach(n =>
                {
                    if (n == current) return;
                    if (current.Neighbors().Count >= 4) return;
                    if (Vector2.Distance(current.Position, n.Position) > MaxRadius) return;
                    current.AddNeighbor(n);
                });
            }
        }

        public static void SetRoute(MapNode from, MapNode to)
        {
            if (route != null)
                for (int i = 0; i < route.Count; ++i)
                    if (i < route.Count - 1)
                        route[i].GetPathTo(route[i + 1]).StopGlowing();

            route = RoutePlotter.RouteBetween(from, to);
            Camera.main.GetComponent<FitScreenToRoute>().FitRoute(route);
        }

        public void Update()
        {
            if (route == null || route.Count == 1) return;
            for (int i = 1; i < route.Count; ++i)
            {
                float scaledAlpha = _currentAlpha / 2f + 0.5f;
                route[i - 1].GetPathTo(route[i]).GlowSegments(scaledAlpha);
            }

            _currentAlpha += Time.deltaTime * _flashSpeed;
            if (_currentAlpha > 1)
            {
                _currentAlpha = 1;
                _flashSpeed = -_flashSpeed;
            }
            else if (_currentAlpha < 0)
            {
                _currentAlpha = 0;
                _flashSpeed = -_flashSpeed;
            }
        }

        public static List<MapNode> GetVisibleNodes(MapNode origin)
        {
            return storedNodes.FindAll(n =>
            {
                if (n.Discovered()) return false;
                return Vector2.Distance(n.Position, origin.Position) <= MaxRadius;
            });
        }

        public static List<MapNode> DiscoveredNodes()
        {
            return storedNodes.FindAll(n => n.Discovered());
        }

        public static List<MapNode> AllNodes()
        {
            return storedNodes;
        }

        private static void CreateNewNode(Vector2Int point, Region.Region region)
        {
            MapNode newMapNode = MapNode.CreateNode(point, region);
            storedNodes.Add(newMapNode);
            --_currentSamples;
        }

        public static void UpdateNodeColor()
        {
            foreach (MapNode n in storedNodes) n.UpdateColor();
        }

        public static MapNode GetNearestNode(Vector3 position, MapNode excludedNode)
        {
            float nearestDistance = 100;
            MapNode nearestNode = null;
            storedNodes.ForEach(n =>
            {
                if (n == excludedNode) return;
                float distance = n.DistanceToPoint(position);
                if (distance >= nearestDistance) return;
                nearestDistance = distance;
                nearestNode = n;
            });
            return nearestNode;
        }
    }
}