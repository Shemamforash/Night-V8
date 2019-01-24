using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Regions;
using SamsHelper.Libraries;
using Sirenix.Utilities;
using TriangleNet.Voronoi.Legacy;
using UnityEngine;
using UnityEngine.Assertions;
using Edge = SamsHelper.Libraries.Edge;
using Random = UnityEngine.Random;

namespace Game.Exploration.Environment
{
    public static class MapGenerator
    {
        public const int MinRadius = 3;

        private static readonly Dictionary<RegionType, string[]> prefixes = new Dictionary<RegionType, string[]>();
        private static readonly Dictionary<RegionType, string[]> suffixes = new Dictionary<RegionType, string[]>();
        private static readonly Dictionary<RegionType, List<string>> _regionNames = new Dictionary<RegionType, List<string>>();
        private static readonly List<Region> _regions = new List<Region>();
        private static Region initialNode;

        private static readonly List<RegionType> _regionTypeBag = new List<RegionType>();
        private static int _regionsDiscovered;

        private static bool _loaded;

        public static void Save(XmlNode root)
        {
            XmlNode regionNode = root.CreateChild("Regions");
            foreach (Region region in _regions) region.Save(regionNode);
            string regionTypesRemaining = "";
            _regionTypeBag.Print();
            for (int i = 0; i < _regionTypeBag.Count; i++)
            {
                regionTypesRemaining += (int) _regionTypeBag[i];
                if (i != _regionTypeBag.Count - 1) regionTypesRemaining += ",";
            }

            regionNode.CreateChild("RegionTypes", regionTypesRemaining);
        }

        public static Region GetRegionById(int id)
        {
            return _regions.First(r => r.RegionID == id);
        }

        public static List<Region> SeenRegions()
        {
            return _regions.FindAll(n => n.Seen());
        }

        public static List<Region> DiscoveredRegions()
        {
            return _regions.FindAll(n => n.Discovered());
        }

        public static void Load(XmlNode doc)
        {
            GenerateNames();
            _regions.Clear();
            XmlNode regionsNode = doc.SelectSingleNode("Regions");
            foreach (XmlNode regionNode in regionsNode.SelectNodes("Region"))
            {
                Region region = Region.Load(regionNode);
                if (region.GetRegionType() == RegionType.Gate) initialNode = region;
                _regions.Add(region);
            }

            _regions.ForEach(r => r.ConnectNeighbors());
            string[] regionTypesRemaining = regionsNode.StringFromNode("RegionTypes").Split(',');
            regionTypesRemaining.ForEach(r => _regionTypeBag.Add((RegionType) int.Parse(r)));
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
//            _regions.ForEach(r => r.Discover());
#endif
        }

        private const int BaseRegionCount = 1 + 12;

        private static void GenerateRegions()
        {
            GenerateNames();
            _regions.Clear();

            int additionalRegionCount = (1 + (int) EnvironmentManager.CurrentEnvironmentType()) * 12;
            int numberOfRegions = BaseRegionCount + additionalRegionCount;
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
            switch (type)
            {
                case RegionType.Tutorial:
                    return "";
                case RegionType.Tomb:
                    switch (EnvironmentManager.CurrentEnvironmentType())
                    {
                        case EnvironmentType.Desert:
                            return "The Grave of Eo";
                        case EnvironmentType.Mountains:
                            return "The Mausoleum of Hythinea";
                        case EnvironmentType.Ruins:
                            return "The Barrow of Rha";
                        case EnvironmentType.Sea:
                            return "The Tomb of Ahna";
                        case EnvironmentType.Wasteland:
                            return "The Throne of Corypthos";
                    }

                    break;
                case RegionType.Rite:
                    return "Chamber of Rites";
                case RegionType.Gate:
                    switch (EnvironmentManager.CurrentEnvironmentType())
                    {
                        case EnvironmentType.Desert:
                            return "Eo's Gate";
                        case EnvironmentType.Mountains:
                            return "Hythinea's Gate";
                        case EnvironmentType.Ruins:
                            return "Rhallos' Gate";
                        case EnvironmentType.Sea:
                            return "Ahna's Gate";
                        case EnvironmentType.Wasteland:
                            return "Corypthos' Gate";
                    }

                    break;
            }

            return _regionNames[type].RemoveRandom();
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
                if (type == RegionType.None || type == RegionType.Gate || type == RegionType.Rite || type == RegionType.Tomb || type == RegionType.Tutorial) continue;
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
            _regionTypeBag.Clear();
            _addedShelter = false;
            AddBaseRegionTypes(false);
            SetJournalQuantities();
            SetWaterQuantities();
            SetFoodQuantities();
            SetResourceQuantities();
        }

        private static bool _addedShelter;

        private static void AddBaseRegionTypes(bool includeTemple)
        {
            _regionTypeBag.Add(RegionType.Shrine);
            for (int i = 0; i < 2; ++i) _regionTypeBag.Add(RegionType.Animal);
            for (int i = 0; i < 8; ++i) _regionTypeBag.Add(RegionType.Danger);

            Environment currentEnvironment = EnvironmentManager.CurrentEnvironment;
            bool isDesert = currentEnvironment.EnvironmentType == EnvironmentType.Desert;
            bool includeBonusRegions = isDesert && includeTemple || !isDesert;
            if (includeBonusRegions)
            {
                _regionTypeBag.Add(RegionType.Monument);
                _regionTypeBag.Add(RegionType.Fountain);
            }
            else
            {
                _regionTypeBag.Add(RegionType.Danger);
                _regionTypeBag.Add(RegionType.Danger);
            }

            if (!includeTemple) return;
            _regionTypeBag.Remove(RegionType.Danger);
            _regionTypeBag.Add(RegionType.Temple);
            if (_addedShelter || isDesert) return;
            _addedShelter = true;
            _regionTypeBag.Remove(RegionType.Danger);
            _regionTypeBag.Add(RegionType.Shelter);
        }

        private static void UpdateAvailableRegionTypes()
        {
            if (_regionTypeBag.Count > 0) return;
            AddBaseRegionTypes(true);
        }

        public static RegionType GetNewRegionType()
        {
            if (_regionsDiscovered == -1)
            {
                ++_regionsDiscovered;
                return RegionType.Gate;
            }

            return RegionType.Temple;

            UpdateAvailableRegionTypes();
            _regionTypeBag.Print();
            ++_regionsDiscovered;
            if (!TutorialManager.Active() || !Region.InTutorialPeriod()) return _regionTypeBag.RemoveRandom();
            _regionTypeBag.Remove(RegionType.Danger);
            return RegionType.Danger;
        }

        private static void SetWaterQuantities()
        {
            int waterSources = EnvironmentManager.CurrentEnvironment.WaterSources;
            SetItemQuantities(waterSources, r => r.WaterSourceCount, r => ++r.WaterSourceCount);
        }

        private static void SetJournalQuantities()
        {
            int journalCount = 15 + (int) EnvironmentManager.CurrentEnvironment.EnvironmentType * 4;
            _regions.Shuffle();
            for (int i = 0; i < journalCount; ++i)
            {
                if (_regions[i] == initialNode) continue;
                _regions[i].ReadJournal = false;
            }
        }

        private static void SetItemQuantities(int total, Func<Region, float> getCount, Action<Region> increment)
        {
            while (total > 0)
            {
                _regions.Shuffle();
                bool added = false;
                foreach (Region r in _regions)
                {
                    if (r == initialNode) continue;
                    if (total == 0) break;
                    if (getCount(r) > 2) continue;
                    added = true;
                    increment(r);
                    --total;
                }

                Assert.IsTrue(added);
            }
        }

        private static void SetFoodQuantities()
        {
            int foodSources = EnvironmentManager.CurrentEnvironment.FoodSources;
            SetItemQuantities(foodSources, r => r.FoodSourceCount, r => ++r.FoodSourceCount);
        }

        private static void SetResourceQuantities()
        {
            int resourcesCount = EnvironmentManager.CurrentEnvironment.ResourceSources;
            SetItemQuantities(resourcesCount, r => r.ResourceSourceCount, r => ++r.ResourceSourceCount);
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