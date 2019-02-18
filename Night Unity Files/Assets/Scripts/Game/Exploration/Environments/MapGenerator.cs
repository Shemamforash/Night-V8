using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Misc;
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

        private static readonly Dictionary<RegionType, List<string>> _genericNames = new Dictionary<RegionType, List<string>>();
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
            LoadRegionNames();
            _regions.Clear();
            XmlNode regionsNode = doc.SelectSingleNode("Regions");
            foreach (XmlNode regionNode in regionsNode.SelectNodes("Region"))
            {
                Region region = Region.Load(regionNode);
                if (region.GetRegionType() == RegionType.Gate) initialNode = region;
                _regions.Add(region);
            }

            _regions.ForEach(r => r.ConnectNeighbors());
            string regionTypeString = regionsNode.StringFromNode("RegionTypes");
            if (regionTypeString == "") return;
            string[] regionTypesRemaining = regionTypeString.Split(',');
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
            _regions.ForEach(r => r.Discover());
#endif
        }

        private const int BaseRegionCount = 10;

        private static void GenerateRegions()
        {
            LoadRegionNames();
            _regions.Clear();

            int regionCount = (2 + (int) EnvironmentManager.CurrentEnvironmentType()) * BaseRegionCount;
            int numberOfRegions = regionCount + 1;
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

            List<Edge> existingEdges = CreateMinimumSpanningTree();
            AddRandomLinks(existingEdges);
        }

        private static void AddRandomLinks(List<Edge> existingEdges)
        {
            Dictionary<Region, HashSet<Region>> possibleLinks = new Dictionary<Region, HashSet<Region>>();
            for (int i = 0; i < _regions.Count; ++i)
            {
                Region rA = _regions[i];
                for (int j = i + 1; j < _regions.Count; ++j)
                {
                    Region rB = _regions[j];
                    Region from = rA.RegionID < rB.RegionID ? rA : rB;
                    Region to = from == rA ? rB : rA;
                    if (from.Neighbors().Contains(to)) continue;
                    if (!possibleLinks.ContainsKey(from)) possibleLinks.Add(from, new HashSet<Region>());
                    possibleLinks[from].Add(to);
                }
            }

            List<Tuple<Region, Region, float>> possibleLinkList = new List<Tuple<Region, Region, float>>();
            foreach (Region from in possibleLinks.Keys)
            {
                foreach (Region to in possibleLinks[from])
                {
                    possibleLinkList.Add(Tuple.Create(from, to, from.Position.Distance(to.Position)));
                }
            }

            possibleLinkList.Sort((a, b) => a.Item3.CompareTo(b.Item3));
            int totalLinks = ((int) EnvironmentManager.CurrentEnvironment.EnvironmentType + 1) * 4;
            if (totalLinks > possibleLinkList.Count) totalLinks = possibleLinkList.Count;
            for (int i = 0; i < totalLinks; ++i)
            {
                (Region from, Region to, _) = possibleLinkList[i];
                bool valid = true;
                foreach (Edge edge in existingEdges)
                {
                    Vector2? intersection = AdvancedMaths.LineIntersection(from.Position, to.Position, edge.A.Position, edge.B.Position);
                    if (intersection == null) continue;
                    valid = false;
                    break;
                }

                if (!valid) continue;
                from.AddNeighbor(to);
            }
        }

        public static string GenerateName(RegionType type)
        {
            LoadRegionNames();
            switch (type)
            {
                case RegionType.Tutorial:
                    return "";
                case RegionType.Tomb:
                    switch (EnvironmentManager.CurrentEnvironmentType())
                    {
                        case EnvironmentType.Desert:
                            return "The Tomb of Eo";
                        case EnvironmentType.Mountains:
                            return "The Tomb of Hythinea";
                        case EnvironmentType.Ruins:
                            return "The Tomb of Rha";
                        case EnvironmentType.Sea:
                            return "The Tomb of Ahna";
                        case EnvironmentType.Wasteland:
                            return "The Tomb of Corypthos";
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

            string regionName = EnvironmentManager.CurrentEnvironment.GetRegionName(type);
            if (type == RegionType.Temple)
            {
                Debug.Log(regionName + " " + type);
                Debug.Log(regionName == null);
            }

            regionName = regionName ?? _genericNames[type].RemoveRandom();
            return regionName;
        }

        private static readonly List<RegionType> _regionTypes = new List<RegionType>();

        private static string StripBlanks(string text)
        {
            return Regex.Replace(text, @"\s+", "");
        }

        private static void LoadRegionNames()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Regions", "Names");
            RegionType[] regionTypes = {RegionType.Danger, RegionType.Animal, RegionType.Temple, RegionType.Shelter, RegionType.Shrine, RegionType.Monument, RegionType.Fountain, RegionType.Cache};
            regionTypes.ForEach(r =>
            {
                XmlNode regionNode = root.GetNode(r.ToString());
                string nameString = regionNode.StringFromNode("Generic");
                List<string> names = nameString.Split(',').ToList();
                _genericNames.Add(r, names);
            });
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
            _regionTypeBag.Add(RegionType.Animal);
            for (int i = 0; i < 5; ++i) _regionTypeBag.Add(RegionType.Danger);

            bool isDesert = EnvironmentManager.CurrentEnvironmentType() == EnvironmentType.Desert;
            bool includeBonusRegions = isDesert && includeTemple || !isDesert;
            if (includeBonusRegions)
            {
                _regionTypeBag.Add(isDesert ? RegionType.Danger : RegionType.Monument);
                _regionTypeBag.Add(RegionType.Fountain);
                _regionTypeBag.Add(RegionType.Cache);
            }
            else
            {
                _regionTypeBag.Add(RegionType.Danger);
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
            ++_regionsDiscovered;
            if (_regionsDiscovered == 0) return RegionType.Gate;
            UpdateAvailableRegionTypes();
            bool isDesert = EnvironmentManager.CurrentEnvironmentType() != EnvironmentType.Desert;
            bool containsCache = _regionTypeBag.Contains(RegionType.Cache);
            bool selectRandom = !isDesert || !containsCache;
            if (selectRandom) return _regionTypeBag.RemoveRandom();
            _regionTypeBag.Remove(RegionType.Cache);
            return RegionType.Cache;
        }

        private static void SetWaterQuantities()
        {
            int waterSources = EnvironmentManager.CurrentEnvironment.WaterSources;
            SetItemQuantities(waterSources, r => r.WaterSourceCount, r => ++r.WaterSourceCount);
        }

        private static void SetJournalQuantities()
        {
            int journalCount = 10 + (int) EnvironmentManager.CurrentEnvironment.EnvironmentType * 4;
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

        private static List<Edge> CreateMinimumSpanningTree()
        {
            Graph map = new Graph();
            _regions.ForEach(n => map.AddNode(n));
            map.SetRootNode(initialNode);
            map.ComputeMinimumSpanningTree();
            map.Edges().ForEach(edge => { edge.A.AddNeighbor(edge.B); });
            return map.Edges();
        }
    }
}