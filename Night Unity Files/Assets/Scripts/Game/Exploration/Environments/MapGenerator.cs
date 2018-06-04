using System.Collections.Generic;
using System.Linq;
using Game.Exploration.Regions;
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
        private static readonly List<Region> storedNodes = new List<Region>();
        private static Region initialNode;
        private static float _currentAlpha;
        private static float _flashSpeed = 1;
        private static List<Region> route;
        public static Transform MapTransform;
        public static bool DontShowHiddenNodes = true;

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

        public static Region GetInitialNode()
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
            List<Region> regions = RegionManager.GenerateRegions();

            List<Vector2> samples = AdvancedMaths.GetPoissonDiscDistribution(regions.Count, MinRadius, MaxRadius, MapWidth / 2f, true);
            for (int i = 0; i < samples.Count; ++i)
            {
                Vector2Int point = new Vector2Int((int) samples[i].x, (int) samples[i].y);
                regions[i].SetPosition(point);
                storedNodes.Add(regions[i]);
                if (i == 0) initialNode = regions[0];
            }

            ConnectNodes();

            DiscoverNode(initialNode);
        }

        private static void DiscoverNode(Region r)
        {
            r.Discover();
            r.Neighbors().ForEach(r2 =>
            {
//                if (r2.Discovered()) return;
//                DiscoverNode(r2);
            });
        }


        private static void CreateMinimumSpanningTree()
        {
            Graph map = new Graph();
            storedNodes.ForEach(n => map.AddNode(n));
            map.ComputeMinimumSpanningTree();
            map.Edges().ForEach(edge =>
            {
                edge.A.AddNeighbor(edge.B);
            });
        }

        private static void ConnectNodes()
        {
            CreateMinimumSpanningTree();
            List<Region> Regions = new List<Region>(storedNodes);
            foreach (Region current in Regions)
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

        public static void SetRoute(Region from, Region to)
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

        public static List<Region> GetVisibleNodes(Region origin)
        {
            return storedNodes.FindAll(n =>
            {
                if (n.Discovered()) return false;
                return Vector2.Distance(n.Position, origin.Position) <= MaxRadius;
            });
        }

        public static List<Region> DiscoveredNodes()
        {
            return storedNodes.FindAll(n => n.Discovered());
        }

        public static List<Region> AllNodes()
        {
            return storedNodes;
        }

        public static void UpdateNodeColor()
        {
//            foreach (Region n in storedNodes) n.UpdateColor();
        }

        public static Region GetNearestNode(Vector3 position, Region excludedNode)
        {
            float nearestDistance = 100;
            Region nearestNode = null;
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