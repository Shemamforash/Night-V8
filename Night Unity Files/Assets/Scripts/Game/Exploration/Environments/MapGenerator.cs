using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class MapGenerator : MonoBehaviour
    {
        public const int MapWidth = 120;
        public const int MinRadius = 6, MaxRadius = 9;
        private static readonly List<Region> storedNodes = new List<Region>();
        private static Region initialNode;
        private static List<Region> route;
        public static Transform MapTransform;
        public static bool DontShowHiddenNodes = true;
        private float nextRouteTime = 0.3f;
        private static readonly List<GameObject> _routeTrails = new List<GameObject>();

        public void Awake()
        {
            _routeTrails.Clear();
            MapTransform = transform;
            storedNodes.ForEach(n => n.CreateObject());
            CreateMapRings();
            UpdateNodeColor();
        }

        private void CreateMapRings()
        {
            GameObject ringPrefab = Resources.Load<GameObject>("Prefabs/Map/Map Ring");
            for (int i = 1; i <= 10; ++i)
            {
                int ringRadius = i * MapGenerator.MinRadius;
                GameObject ring = Instantiate(ringPrefab, transform.position, ringPrefab.transform.rotation);
                ring.transform.SetParent(transform);
                ring.name = "Ring: distance " + i + " hours";
                RingDrawer ringDrawer = ring.GetComponent<RingDrawer>();
                ringDrawer.DrawCircle(ringRadius);
                float alpha = 1f / 9f * i + 1f / 9f;
                alpha = 1 - alpha;
                ringDrawer.SetColor(new Color(1, 1, 1, alpha));
            }
        }

        public static Region GetInitialNode()
        {
            return initialNode;
        }

        public static int NodeDistanceToTime(float distance)
        {
            //minradius = 30 minutes = 6 ticks
            return Mathf.CeilToInt(distance / 6f * (WorldState.MinutesPerHour / 2f));
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

            initialNode.Discover();
//            regions.ForEach(r => r.Discover());
        }

        private static void CreateMinimumSpanningTree()
        {
            Graph map = new Graph();
            storedNodes.ForEach(n => map.AddNode(n));
            map.ComputeMinimumSpanningTree();
            map.Edges().ForEach(edge => { edge.A.AddNeighbor(edge.B); });
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

            Regions.ForEach(r =>
            {
                if (r.Neighbors().Count != 0) return;
                storedNodes.Sort((a, b) => Vector2.Distance(r.Position, a.Position).CompareTo(Vector2.Distance(r.Position, b.Position)));
                foreach (Region node in storedNodes)
                {
                    if (node == r) continue;
                    if (node.Neighbors().Count == 0) continue;
                    r.AddNeighbor(node);
                    break;
                }
            });
        }

        public static void SetRoute(Region from, Region to)
        {
            _routeTrails.ForEach(g =>
            {
                if (g == null) return;
                FadeAndDieTrailRenderer fad = g.GetComponent<FadeAndDieTrailRenderer>();
                fad.StartFade(1);
            });
            _routeTrails.Clear();
            route = RoutePlotter.RouteBetween(from, to);
            Camera.main.GetComponent<FitScreenToRoute>().FitRoute(route);
        }

        private void DrawRoute()
        {
            if (route.Count <= 1) return;
            nextRouteTime -= Time.deltaTime;
            if (nextRouteTime > 0) return;
            nextRouteTime = Random.Range(0.2f, 0.5f);
            GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/Borders/Path Trail"));
            _routeTrails.Add(g);
            g.transform.position = route[0].Position;
            Vector3[] rArr = new Vector3[route.Count];
            for (int j = 0; j < route.Count; ++j)
            {
                Vector3 pos = route[j].Position;
                if (j > 0 && j < route.Count - 1)
                {
                    pos = AdvancedMaths.RandomVectorWithinRange(pos, Random.Range(0.25f, 0.5f));
                }
                else
                {
                    pos = AdvancedMaths.RandomVectorWithinRange(pos, 0.1f);
                }

                rArr[j] = pos;
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(g.transform.DOPath(rArr, Random.Range(2, 5), PathType.CatmullRom, PathMode.TopDown2D));
            sequence.AppendCallback(() => _routeTrails.Remove(g));
        }

        public void Update()
        {
            if (route == null || route.Count == 1) return;
            DrawRoute();
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

        public static void UpdateNodeColor()
        {
//            foreach (Region n in storedNodes) n.UpdateColor();
        }
    }
}