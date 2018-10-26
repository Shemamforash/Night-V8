using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Exploration.Environment
{
    public static class MapGenerator
    {
        public const int MinRadius = 4;
        private const int MaxRadius = 8;
        private const int WaterSourcesPerEnvironment = 30;
        private const int FoodSourcesPerEnvironment = 20;
        private const int ResourcesPerEnvironment = 30;

        private static readonly Dictionary<RegionType, string[]> prefixes = new Dictionary<RegionType, string[]>();
        private static readonly Dictionary<RegionType, string[]> suffixes = new Dictionary<RegionType, string[]>();
        private static readonly Dictionary<RegionType, List<string>> _regionNames = new Dictionary<RegionType, List<string>>();
        private static readonly List<Region> _regions = new List<Region>();
        private static Region initialNode;

        private static bool _loaded;

        public static void Save(XmlNode doc)
        {
            XmlNode regionNode = doc.CreateChild("Regions");
            foreach (Region region in _regions) region.Save(regionNode);
        }

        public static Region GetRegionById(int id)
        {
            return _regions.First(r => r.RegionID == id);
        }

        public static List<Region> DiscoveredRegions()
        {
            return _regions.FindAll(n => n.Seen());
        }

        public static void Load(XmlNode doc)
        {
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

        public static List<Region> Regions()
        {
            return _regions;
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
            _regions.ForEach(r => r.Discover());
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
                int regionCount = _regions.Count - 1;
                int ringNo = 0;
                int regionNo = 1;
                _regions[0].SetPosition(Vector2.zero);
                
                while (regionCount > 0)
                {
                    int maxRingSize = 3 * (ringNo + 1);
                    int regionsOnRing = Random.Range(ringNo, maxRingSize);
                    if (regionsOnRing > regionCount) regionsOnRing = regionCount;
                    regionCount -= regionsOnRing;

                    float radius = (ringNo + 1) * MinRadius;
                    float angleInterval = 360f / maxRingSize;
                    int[] slots = new int[maxRingSize];
                    for (int i = 0; i < maxRingSize; ++i)
                    {
                        if (i < regionsOnRing) slots[i] = 1;
                        else slots[i] = 0;
                    }

                    slots.Shuffle();
                    for (int i = 0; i < maxRingSize; ++i)
                    {
                        if (slots[i] == 0) continue;
                        float angle = Random.Range(i * angleInterval, (i + 1) * angleInterval);
                        Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, radius, Vector2.zero);
                        _regions[regionNo].SetPosition(position);
                        ++regionNo;
                    }

                    ++ringNo;
                }

                succeeded = ConnectNodes();
            }
        }

        private static void SetRegionTypes()
        {
            Environment currentEnvironment = EnvironmentManager.CurrentEnvironment;
            DistributeNodeTypes(RegionType.Temple, currentEnvironment.Temples, 4);
            DistributeNodeTypes(RegionType.Monument, currentEnvironment.Monuments, 4);
            DistributeNodeTypes(RegionType.Shelter, currentEnvironment.Shelters, 3);
            DistributeNodeTypes(RegionType.Fountain, currentEnvironment.Fountains, 2);
            DistributeNodeTypes(RegionType.Shrine, currentEnvironment.Shrines, 2);
            DistributeNodeTypes(RegionType.Danger, currentEnvironment.Dangers, -1, false);
            DistributeNodeTypes(RegionType.Animal, currentEnvironment.Animals, -1, false);
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

        private static void DistributeNodeTypes(RegionType type, int quantity, int minDepth = -1, bool mustNotTouch = true)
        {
            if (quantity == 0) return;
            _regions.Shuffle();
            foreach (Region region in _regions)
            {
                if (region.GetRegionType() != RegionType.None) continue;
                if (region.GetRegionType() == RegionType.Temple) Debug.Log(minDepth + " " + region.Depth);
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
                --quantity;
                if (quantity == 0) break;
            }

            if (quantity <= 0) return;
            DistributeNodeTypes(type, quantity, minDepth - 1, false);
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
                SetMaxNodeDepth(4 + WorldState.CurrentLevel() * 2, map);
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
    }
}