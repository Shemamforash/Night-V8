﻿using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Characters;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Xml;
using Facilitating.Persistence;
using Assert = NUnit.Framework.Assert;
using Random = UnityEngine.Random;

namespace Game.Exploration.Environment
{
    public class MapGenerator : MonoBehaviour
    {
        private const int MapWidth = 120, MinRadius = 6, MaxRadius = 9;
        private const int WaterSourcesPerEnvironment = 30;
        private const int FoodSourcesPerEnvironment = 20;
        private const int ResourcesPerEnvironment = 30;

        private static readonly Dictionary<RegionType, List<string>> _regionNames = new Dictionary<RegionType, List<string>>();
        private static readonly List<GameObject> _routeTrails = new List<GameObject>();
        private static readonly List<Region> _regions = new List<Region>();
        private static List<Region> route;
        private static Region initialNode;
        public static Transform MapTransform;
        private static bool _loaded;

        private readonly List<Tuple<Region, Region>> _allRoutes = new List<Tuple<Region, Region>>();
        private readonly Queue<Tuple<Region, Region>> _undrawnRoutes = new Queue<Tuple<Region, Region>>();
        private float nextRouteTime = 0.3f;
        private float currentTime;


        public static void Save(XmlNode doc)
        {
            XmlNode regionNode = doc.CreateChild("Regions");
            foreach (Region region in _regions) region.Save(regionNode);
        }

        public static Region GetRegionById(int id)
        {
            return _regions.First(r => r.RegionID == id);
        }

        public static void Load(XmlNode doc)
        {
            Debug.Log(_regions.Count);
            _regions.Clear();
            XmlNode regionsNode = doc.SelectSingleNode("Regions");
            foreach (XmlNode regionNode in regionsNode.SelectNodes("Region"))
            {
                Region region = Region.Load(regionNode);
                if (region.GetRegionType() == RegionType.Gate) initialNode = region;
                _regions.Add(region);
            }

            _regions.ForEach(r => r.ConnectNeighbors());
        }

        public void Awake()
        {
            _routeTrails.Clear();
            MapTransform = transform;
            _regions.ForEach(n => n.CreateObject());
            CreateMapRings();
            UpdateNodeColor();
            CreateRouteLinks();
        }

        private void CreateRouteLinks()
        {
            List<Region> _discovered = DiscoveredRegions();
            foreach (Region from in _discovered)
            {
                foreach (Region to in _discovered)
                {
                    if (!from.Neighbors().Contains(to)) continue;
                    _allRoutes.Add(Tuple.Create(from, to));
                }
            }
        }

        private void DrawBasicRoutes()
        {
            if (_undrawnRoutes.Count == 0)
            {
                _allRoutes.Shuffle();
                _allRoutes.ForEach(a => _undrawnRoutes.Enqueue(a));
            }

            currentTime += Time.deltaTime;
            if (currentTime < 1f / _allRoutes.Count) return;
            Tuple<Region, Region> link = _undrawnRoutes.Dequeue();
            Region from = link.Item1;
            Region to = link.Item2;
            GameObject g = Instantiate(Resources.Load<GameObject>("Prefabs/Borders/Path Trail Faded"));
            Vector3[] rArr = new Vector3[Random.Range(2, 6)];
            for (int j = 0; j < rArr.Length; ++j)
            {
                if (j == 0)
                {
                    rArr[j] = from.Position;
                    continue;
                }

                if (j == rArr.Length - 1)
                {
                    rArr[j] = to.Position;
                    continue;
                }

                float normalisedDistance = (float) j / rArr.Length;
                Vector2 pos = AdvancedMaths.PointAlongLine(from.Position, to.Position, normalisedDistance);
                pos = AdvancedMaths.RandomVectorWithinRange(pos, Random.Range(0.1f, 0.1f));
                rArr[j] = pos;
            }

            g.transform.position = from.Position;
            Sequence s = DOTween.Sequence();
            s.Append(g.transform.DOPath(rArr, Random.Range(1f, 3f), PathType.CatmullRom, PathMode.TopDown2D));
            s.AppendCallback(() => g.GetComponent<FadeAndDieTrailRenderer>().StartFade(1f));
            currentTime = 0f;
        }

        private void CreateMapRings()
        {
            GameObject ringPrefab = Resources.Load<GameObject>("Prefabs/Map/Map Ring");
            for (int i = 1; i <= 10; ++i)
            {
                int ringRadius = i * MinRadius;
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

        public static void Generate()
        {
            GenerateRegions();
            ConnectRegions();
            SetRegionTypes();
            initialNode.Discover();
#if UNITY_EDITOR
//            _regions.ForEach(r => r.Discover());
#endif
        }

        private static void GenerateRegions()
        {
            GenerateNames();
            _regions.Clear();
            int numberOfRegions = EnvironmentManager.CurrentEnvironment.RegionCount + 1;
            while (numberOfRegions > 0)
            {
                _regions.Add(new Region());
                --numberOfRegions;
            }

            initialNode = _regions[0];
            initialNode.SetRegionType(RegionType.Gate);
        }

        private static void ConnectRegions()
        {
            bool succeeded = false;
            while (!succeeded)
            {
                _regions.ForEach(s => { s.Reset(); });
                List<Vector2> samples = AdvancedMaths.GetPoissonDiscDistribution(_regions.Count, MinRadius, MaxRadius, MapWidth / 2f, true);
                for (int i = 0; i < samples.Count; ++i)
                {
                    Vector2Int point = new Vector2Int((int) samples[i].x, (int) samples[i].y);
                    _regions[i].SetPosition(point);
                }

                succeeded = ConnectNodes();
            }
        }

        private static void SetRegionTypes()
        {
            Environment currentEnvironment = EnvironmentManager.CurrentEnvironment;
            DistributeNodeTypes(RegionType.Temple, currentEnvironment.Temples, 4);
            DistributeNodeTypes(RegionType.Monument, currentEnvironment.Monuments, 4);
            DistributeNodeTypes(RegionType.Shrine, currentEnvironment.Shrines, 1);
            DistributeNodeTypes(RegionType.Fountain, currentEnvironment.Fountains, 1);
            DistributeNodeTypes(RegionType.Shelter, currentEnvironment.Shelters, 3);
            DistributeNodeTypes(RegionType.Animal, currentEnvironment.Animals, -1, false);
            DistributeNodeTypes(RegionType.Danger, currentEnvironment.Dangers, -1, false);
            SetWaterQuantities();
            SetFoodQuantities();
            SetResourceQuantities();
        }

        public static string GenerateName(RegionType type)
        {
            if (type == RegionType.Rite || type == RegionType.Tomb) return "";
            return type == RegionType.Gate ? "Gate" : _regionNames[type].RemoveRandom();
        }

        private static readonly List<RegionType> _regionTypes = new List<RegionType>();

        private static void GenerateNames()
        {
            LoadRegionNames();
            _regionNames.Clear();
            foreach (RegionType type in _regionTypes)
            {
                List<string> combinations = new List<string>();
                foreach (string prefix in prefixes[type])
                {
                    foreach (string suffix in suffixes[type])
                    {
                        if (prefix == suffix) continue;
                        switch (type)
                        {
                            case RegionType.Monument:
                                combinations.Add(prefix + " of " + suffix);
                                break;
                            case RegionType.Fountain:
                                combinations.Add(prefix + " " + suffix);
                                break;
                            default:
                                combinations.Add(prefix + "'s " + suffix);
                                break;
                        }
                    }

                    if (type == RegionType.Monument) continue;
                    foreach (string environmentSuffix in EnvironmentManager.CurrentEnvironment.Suffixes())
                    {
                        switch (type)
                        {
                            case RegionType.Fountain:
                                combinations.Add(prefix + " " + environmentSuffix);
                                break;
                            case RegionType.Shrine:
                                combinations.Add(prefix + " " + environmentSuffix);
                                break;
                            default:
                                combinations.Add(prefix + "'s " + environmentSuffix);
                                break;
                        }
                    }
                }

                combinations.Shuffle();
                _regionNames.Add(type, combinations);
            }
        }

        private static string StripBlanks(string text)
        {
            return Regex.Replace(text, @"\s+", "");
        }

        private static readonly Dictionary<RegionType, string[]> prefixes = new Dictionary<RegionType, string[]>();
        private static readonly Dictionary<RegionType, string[]> suffixes = new Dictionary<RegionType, string[]>();

        private static void LoadRegionNames()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Regions", "RegionType");
            foreach (RegionType type in Enum.GetValues(typeof(RegionType)))
            {
                if (type == RegionType.None || type == RegionType.Gate || type == RegionType.Nightmare || type == RegionType.Rite || type == RegionType.Tomb) continue;
                _regionTypes.Add(type);
            }

            foreach (XmlNode regionTypeNode in root.ChildNodes)
            {
                RegionType regionType = _regionTypes.Find(r => r.ToString() == regionTypeNode.Name);
                prefixes[regionType] = StripBlanks(regionTypeNode.StringFromNode("Prefixes")).Split(',');
                suffixes[regionType] = StripBlanks(regionTypeNode.StringFromNode("Suffixes")).Split(',');
            }

            _loaded = true;
        }

        private static int assigned;

        private static void DistributeNodeTypes(RegionType type, int quantity, int minDepth = -1, bool mustNotTouch = true)
        {
            if (quantity == 0) return;
            _regions.Shuffle();
            foreach (Region region in _regions)
            {
                if (region.GetRegionType() != RegionType.None) continue;
                if (minDepth != -1 && region.Depth < minDepth) continue;
                bool valid = true;
                if (mustNotTouch)
                {
                    foreach (Node neighbor in region.Neighbors())
                    {
                        if (((Region) neighbor).GetRegionType() != type) continue;
                        valid = false;
                        break;
                    }
                }

                if (!valid) continue;
                region.SetRegionType(type);
                ++assigned;
                --quantity;
                if (quantity == 0) break;
            }

            if (quantity > 0)
            {
                _regions.Shuffle();
                foreach (Region region in _regions)
                {
                    if (region.GetRegionType() != RegionType.None) continue;
                    region.SetRegionType(type);
                    ++assigned;
                    --quantity;
                    if (quantity == 0) break;
                }
            }

            Assert.IsTrue(quantity == 0);
        }

        private static void SetWaterQuantities()
        {
            int waterSources = WaterSourcesPerEnvironment;
            while (waterSources > 0)
            {
                _regions.Shuffle();
                bool added = false;
                for (int index = 0; index < _regions.Count * 0.6f; index++)
                {
                    Region r = _regions[index];
                    if (waterSources == 0) break;
                    if (r.WaterSourceCount > 2) continue;
                    added = true;
                    ++r.WaterSourceCount;
                    --waterSources;
                }

                if (!added) Debug.Log("Decrease water sources per environment");
            }
        }

        private static void SetFoodQuantities()
        {
            int foodSource = FoodSourcesPerEnvironment;
            while (foodSource > 0)
            {
                _regions.Shuffle();
                bool added = false;
                foreach (Region r in _regions)
                {
                    if (foodSource == 0) break;
                    if (r.FoodSourceCount > 2) continue;
                    added = true;
                    ++r.FoodSourceCount;
                    --foodSource;
                }

                if (!added) Debug.Log("Decrease food sources per environment");
            }
        }

        private static void SetResourceQuantities()
        {
            int resourceCount = ResourcesPerEnvironment;
            while (resourceCount > 0)
            {
                _regions.Shuffle();
                bool added = false;
                foreach (Region r in _regions)
                {
                    if (resourceCount == 0) break;
                    if (r.ResourceSourceCount > 2) continue;
                    added = true;
                    ++r.ResourceSourceCount;
                    --resourceCount;
                }

                if (!added) Debug.Log("Decrease resource sources per environment");
            }
        }

        private static Graph CreateMinimumSpanningTree()
        {
            Graph map = new Graph();
            _regions.ForEach(n => map.AddNode(n));
            map.SetRootNode(initialNode);
            map.ComputeMinimumSpanningTree();
            map.Edges().ForEach(edge => { edge.A.AddNeighbor(edge.B); });
            return map;
        }

        private static bool ConnectNodes()
        {
            Graph map = CreateMinimumSpanningTree();
            try
            {
                SetMaxNodeDepth(8 + WorldState.Difficulty() * 3, map);
            }
            catch (Exception)
            {
                return false;
            }

            AddRandomLinks();
            return true;
        }

        private static void SetMaxNodeDepth(int maxDepth, Graph map)
        {
            List<Tuple<Region, Region>> edges = new List<Tuple<Region, Region>>();
            for (int i = 0; i < _regions.Count; ++i)
            {
                for (int j = i + 1; j < _regions.Count; ++j)
                {
                    Region from = _regions[i];
                    Region to = _regions[j];
                    if (from.Neighbors().Contains(to)) continue;
                    float distance = Vector2.Distance(from.Position, to.Position);
                    if (distance > MaxRadius) continue;
                    edges.Add(Tuple.Create(from, to));
                }
            }

            map.CalculateNodeDepths();
            float currentMaxNodeDepth = map.MaxDepth();
            while (currentMaxNodeDepth > maxDepth && edges.Count > 0)
            {
                edges.Sort((a, b) =>
                {
                    float deltaDepthA = Mathf.Abs(a.Item1.Depth - a.Item2.Depth);
                    float deltaDepthB = Mathf.Abs(b.Item1.Depth - b.Item2.Depth);
                    return -deltaDepthA.CompareTo(deltaDepthB);
                });
                Tuple<Region, Region> edge = edges[0];
                edges.RemoveAt(0);
                edge.Item1.AddNeighbor(edge.Item2);
                map.CalculateNodeDepths();
                currentMaxNodeDepth = map.MaxDepth();
            }

            if (currentMaxNodeDepth > maxDepth) throw new Exception();
        }

        private static void AddRandomLinks()
        {
            List<Region> Regions = new List<Region>(_regions);
            foreach (Region current in Regions)
            {
                _regions.Sort((a, b) => Vector2.Distance(current.Position, a.Position).CompareTo(Vector2.Distance(current.Position, b.Position)));
                _regions.ForEach(n =>
                {
                    if (n == current) return;
                    if (current.Neighbors().Count >= 2) return;
                    if (Vector2.Distance(current.Position, n.Position) > MaxRadius) return;
                    current.AddNeighbor(n);
                });
            }
        }

        public static void SetRoute(Region to)
        {
            _routeTrails.ForEach(g =>
            {
                if (g == null) return;
                FadeAndDieTrailRenderer fad = g.GetComponent<FadeAndDieTrailRenderer>();
                fad.StartFade(1);
            });
            _routeTrails.Clear();
            route = RoutePlotter.RouteBetween(CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode(), to);
//            Camera.main.GetComponent<FitScreenToRoute>().FitRoute(route);
        }

        private void DrawTargetRoute()
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
            sequence.Append(g.transform.DOPath(rArr, Random.Range(1f, 3f), PathType.CatmullRom, PathMode.TopDown2D));
            sequence.AppendCallback(() => _routeTrails.Remove(g));
        }

        public void Update()
        {
            DrawBasicRoutes();
            if (route == null || route.Count == 1) return;
            DrawTargetRoute();
        }

        public static List<Region> DiscoveredRegions()
        {
            return _regions.FindAll(n => n.Seen());
        }

        private static void UpdateNodeColor()
        {
//            foreach (Region n in storedNodes) n.UpdateColor();
        }
    }
}