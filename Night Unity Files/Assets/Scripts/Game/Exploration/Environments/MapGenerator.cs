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
using Edge = SamsHelper.Libraries.Edge;
using Random = UnityEngine.Random;

namespace Game.Exploration.Environment
{
    public static class MapGenerator
    {
        public const int MinRadius = 4;
        private const int WaterSourcesPerEnvironment = 30;
        private const int FoodSourcesPerEnvironment = 20;
        private const int ResourcesPerEnvironment = 30;

        private static readonly Dictionary<RegionType, string[]> prefixes = new Dictionary<RegionType, string[]>();
        private static readonly Dictionary<RegionType, string[]> suffixes = new Dictionary<RegionType, string[]>();
        private static readonly Dictionary<RegionType, List<string>> _regionNames = new Dictionary<RegionType, List<string>>();
        private static readonly List<Region> _regions = new List<Region>();
        private static Region initialNode;

        private static readonly List<RegionType> _regionTypeBag = new List<RegionType>();
        private static int _regionsDiscovered;
        private static int _templesAdded;

        private const int ShrineQuantityModifier = 2;
        private const int AnimalQuantityModifier = 2;
        private const int DangerQuantityModifier = 5;

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
//#if UNITY_EDITOR
//            _regions.ForEach(r => r.Discover());
//#endif
        }

        private static void GenerateRegions()
        {
            GenerateNames();
            _regions.Clear();
            int templeCount = EnvironmentManager.CurrentEnvironment.Temples;
            int baseRegionCount = 1 + 1 + ShrineQuantityModifier + AnimalQuantityModifier + DangerQuantityModifier; //monument  + fountain + shrine + animal + danger
            int numberOfRegions = 1; //initial gate
            numberOfRegions += baseRegionCount * (templeCount + 1);
            numberOfRegions += templeCount;
            if (EnvironmentManager.CurrentEnvironment.EnvironmentType != EnvironmentType.Desert) ++numberOfRegions; //shelters
            while (numberOfRegions > 0)
            {
                _regions.Add(new Region());
                --numberOfRegions;
            }

            initialNode = _regions[0];
        }

        private static void ConnectRegions()
        {
            _regions.ForEach(s => { s.Reset(); });
            int regionCount = _regions.Count - 1;
            int ringNo = 0;
            int regionNo = 1;
            _regions[0].SetPosition(Vector2.zero);

            while (regionCount > 0)
            {
                int maxRingSize = 3 * (ringNo + 1);
                int regionsOnRing = Random.Range(ringNo + 1, maxRingSize);
                if (regionsOnRing > regionCount) regionsOnRing = regionCount;
                regionCount -= regionsOnRing;

                float radius = (ringNo + 1) * MinRadius;
                int[] slots = new int[maxRingSize];
                for (int i = 0; i < maxRingSize; ++i)
                {
                    if (i < regionsOnRing) slots[i] = 1;
                    else slots[i] = 0;
                }

                slots.Shuffle();

                float angleInterval = 360f / maxRingSize;

                for (int i = 0; i < maxRingSize; ++i)
                {
                    if (slots[i] == 0) continue;
                    float angleFrom = i * angleInterval;
                    float angleTo = (i + 1) * angleInterval;
                    angleFrom += angleInterval / 5f;
                    angleTo -= angleInterval / 5f;
                    float angle = Random.Range(angleFrom, angleTo);
                    Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, radius, Vector2.zero);
                    _regions[regionNo].SetPosition(position);
                    ++regionNo;
                }

                ++ringNo;
            }

            Triangulate();
        }

        private class RegionNode : Node
        {
            public readonly Region RegionHere;

            public RegionNode(Region region) : base(region.Position)
            {
                RegionHere = region;
            }
        }

        private static void Triangulate()
        {
            List<Edge> edges = new List<Edge>();
            for (int i = 0; i < _regions.Count; ++i)
            {
                RegionNode a = new RegionNode(_regions[i]);
                for (int j = i; j < _regions.Count; ++j)
                {
                    RegionNode b = new RegionNode(_regions[j]);
                    Edge edge = new Edge(a, b);
                    edges.Add(edge);
                }
            }

            edges.RemoveAll(e => e.Length > MinRadius * 1.5f);
            edges.Sort((a, b) => a.Length.CompareTo(b.Length));
            int targetEdges = (int) (_regions.Count / 2f) + 10;
            if (targetEdges > edges.Count) targetEdges = edges.Count;
            for (int i = 0; i < targetEdges; ++i)
            {
                RegionNode from = (RegionNode) edges[i].A;
                RegionNode to = (RegionNode) edges[i].B;
                from.RegionHere.AddNeighbor(to.RegionHere);
            }

            CreateMinimumSpanningTree();
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

        private static void SetRegionTypes()
        {
            _regionsDiscovered = -1;
            _templesAdded = 0;
            _regionTypeBag.Clear();
            AddBaseRegionTypes();
            SetWaterQuantities();
            SetFoodQuantities();
            SetResourceQuantities();
        }

        private static void AddBaseRegionTypes()
        {
            Environment currentEnvironment = EnvironmentManager.CurrentEnvironment;
            for (int i = 0; i < 1; ++i) _regionTypeBag.Add(RegionType.Monument);
            for (int i = 0; i < ShrineQuantityModifier; ++i) _regionTypeBag.Add(RegionType.Shrine);
            for (int i = 0; i < 1; ++i) _regionTypeBag.Add(RegionType.Fountain);
            for (int i = 0; i < AnimalQuantityModifier; ++i) _regionTypeBag.Add(RegionType.Animal);
            for (int i = 0; i < DangerQuantityModifier; ++i) _regionTypeBag.Add(RegionType.Danger);
            if (currentEnvironment.EnvironmentType != EnvironmentType.Desert && !_regionTypeBag.Contains(RegionType.Shelter)) _regionTypeBag.Add(RegionType.Shelter);
        }

        private static void UpdateAvailableRegionTypes()
        {
            Environment currentEnvironment = EnvironmentManager.CurrentEnvironment;
            if (_templesAdded == currentEnvironment.Temples) return;
            if (_regionsDiscovered % 8 != 0 || _regionsDiscovered <= 0) return;
            AddBaseRegionTypes();
            _regionTypeBag.Add(RegionType.Temple);
            ++_templesAdded;
        }

        public static RegionType GetNewRegionType()
        {
            if (_regionsDiscovered == -1)
            {
                ++_regionsDiscovered;
                return RegionType.Gate;
            }

            UpdateAvailableRegionTypes();
            ++_regionsDiscovered;
            return _regionTypeBag.RemoveRandom();
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

        private static void CreateMinimumSpanningTree()
        {
            Graph map = new Graph();
            _regions.ForEach(n => map.AddNode(n));
            map.SetRootNode(initialNode);
            map.ComputeMinimumSpanningTree();
            map.Edges().ForEach(edge => { edge.A.AddNeighbor(edge.B); });
        }
    }
}