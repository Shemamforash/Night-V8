using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Regions;
using Game.Global;
using Extensions;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Exploration.Environment
{
	public static class MapGenerator
	{
		public const int MinRadius = 3;

		private const           int                                  BaseRegionCount = 8;
		private static readonly List<Tuple<RegionType, bool>>        _regionOrder    = new List<Tuple<RegionType, bool>>();
		private static readonly Dictionary<RegionType, List<string>> _genericNames   = new Dictionary<RegionType, List<string>>();
		private static readonly List<Region>                         _regions        = new List<Region>();
		private static          Region                               initialNode;

		private static int _regionsDiscovered;

		private static bool _loaded;

		private static bool _addedShelter;

		public static void Save(XmlNode root)
		{
			XmlNode regionNode = root.CreateChild("Regions");
			foreach (Region region in _regions) region.Save(regionNode);
			string regionOrderString = "";
			for (int i = 0; i < _regionOrder.Count; i++)
			{
				Tuple<RegionType, bool> tup = _regionOrder[i];
				regionOrderString += (int) tup.Item1 + "," + tup.Item2;
				if (i != _regionOrder.Count                - 1) regionOrderString += ",";
			}

			regionNode.CreateChild("RegionTypes", regionOrderString);
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

		private static void RemoveExistingName(Region region)
		{
			EnvironmentManager.CurrentEnvironment.RemoveExistingName(region.GetRegionType(), region.Name);
			if (!_genericNames.ContainsKey(region.GetRegionType())) return;
			_genericNames[region.GetRegionType()].Remove(region.Name);
		}

		public static void Load(XmlNode doc)
		{
			LoadRegionNames();
			_regions.Clear();
			_regionOrder.Clear();
			_regionsDiscovered = -1;
			XmlNode regionsNode = doc.SelectSingleNode("Regions");
			foreach (XmlNode regionNode in regionsNode.SelectNodes("Region"))
			{
				Region region = Region.Load(regionNode);
				if (region.Discovered()) ++_regionsDiscovered;
				RemoveExistingName(region);
				if (region.GetRegionType() == RegionType.Gate) initialNode = region;
				_regions.Add(region);
			}

			_regions.ForEach(r => r.ConnectNeighbors());
			string regionTypeString = regionsNode.ParseString("RegionTypes");
			if (regionTypeString == "") return;
			string[] regionOrder = regionTypeString.Split(',');
			for (int i = 0; i < regionOrder.Length; i += 2)
			{
				RegionType regionType = (RegionType) int.Parse(regionOrder[i]);
				bool       storyHere  = bool.Parse(regionOrder[i + 1]);
				_regionOrder.Add(Tuple.Create(regionType, storyHere));
			}
		}

		public static List<Region> Regions() => _regions;

		public static Region GetInitialNode() => initialNode;

		public static void Generate()
		{
			Debug.Log("Generated");
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
			LoadRegionNames();
			_regions.Clear();

			int regionCount     = (2 + (int) EnvironmentManager.CurrentEnvironmentType) * BaseRegionCount;
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
			int ringNo      = 1;
			int regionNo    = 1;
			_regions[0].SetPosition(Vector2.zero);

			while (regionCount > 0)
			{
				int regionsOnRing                              = ringNo + 1;
				if (regionsOnRing > regionCount) regionsOnRing = regionCount;
				regionCount -= regionsOnRing;

				float radius = ringNo * MinRadius;
				int[] slots  = new int[regionsOnRing];
				for (int i = 0; i < regionsOnRing; ++i)
				{
					if (i < regionsOnRing)
					{
						slots[i] = 1;
					}
					else
					{
						slots[i] = 0;
					}
				}

				slots.Shuffle();

				float angleInterval = 360f / regionsOnRing;

				for (int i = 0; i < regionsOnRing; ++i)
				{
					if (slots[i] == 0) continue;
					float angleFrom = i       * angleInterval;
					float angleTo   = (i + 1) * angleInterval;
					angleFrom += angleInterval / 5f;
					angleTo   -= angleInterval / 5f;
					float   angle    = Random.Range(angleFrom, angleTo);
					Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, radius, Vector2.zero);
					_regions[regionNo].SetPosition(position);
					++regionNo;
				}

				++ringNo;
			}

			Triangulate();
		}

		private static void Triangulate()
		{
			List<Edge> edges = new List<Edge>();
			for (int i = 0; i < _regions.Count; ++i)
			{
				RegionNode a = new RegionNode(_regions[i]);
				for (int j = i; j < _regions.Count; ++j)
				{
					RegionNode b    = new RegionNode(_regions[j]);
					Edge       edge = new Edge(a, b);
					edges.Add(edge);
				}
			}

			edges.RemoveAll(e => e.Length > MinRadius * 1.5f);
			edges.Sort((a, b) => a.Length.CompareTo(b.Length));
			int targetEdges                            = (int) (_regions.Count / 2f) + 10;
			if (targetEdges > edges.Count) targetEdges = edges.Count;
			for (int i = 0; i < targetEdges; ++i)
			{
				RegionNode from = (RegionNode) edges[i].A;
				RegionNode to   = (RegionNode) edges[i].B;
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
					Region rB   = _regions[j];
					Region from = rA.RegionID < rB.RegionID ? rA : rB;
					Region to   = from        == rA ? rB : rA;
					if (from.Neighbors().Contains(to)) continue;
					if (!possibleLinks.ContainsKey(from)) possibleLinks.Add(from, new HashSet<Region>());
					possibleLinks[from].Add(to);
				}
			}

			List<Tuple<Region, Region, float>> possibleLinkList = new List<Tuple<Region, Region, float>>();
			foreach (Region from in possibleLinks.Keys)
			{
				foreach (Region to in possibleLinks[from]) possibleLinkList.Add(Tuple.Create(from, to, from.Position.Distance(to.Position)));
			}

			possibleLinkList.Sort((a, b) => a.Item3.CompareTo(b.Item3));
			int totalLinks                                      = ((int) EnvironmentManager.CurrentEnvironment.EnvironmentType + 1) * 4;
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
					switch (EnvironmentManager.CurrentEnvironmentType)
					{
						case EnvironmentType.Desert:
							return "The Tomb of Eo";
						case EnvironmentType.Mountains:
							return "The Tomb of Hythinea";
						case EnvironmentType.Ruins:
							return "The Tomb of Rhallos";
						case EnvironmentType.Sea:
							return "The Tomb of Ahna";
						case EnvironmentType.Wasteland:
							return "The Tomb of Corypthos";
					}

					break;
				case RegionType.Rite:
					return "Chamber of Rites";
				case RegionType.Gate:
					switch (EnvironmentManager.CurrentEnvironmentType)
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
			regionName = regionName ?? _genericNames[type].RemoveRandom();
			return regionName;
		}

		private static void LoadRegionNames()
		{
			if (_loaded) return;
			XmlNode      root        = Helper.OpenRootNode("Regions", "Names");
			RegionType[] regionTypes = {RegionType.Danger, RegionType.Animal, RegionType.Temple, RegionType.Shelter, RegionType.Shrine, RegionType.Monument, RegionType.Fountain, RegionType.Cache};
			Array.ForEach(regionTypes, r =>
			{
				XmlNode      regionNode = root.GetChild(r.ToString());
				string       nameString = regionNode.ParseString("Generic");
				List<string> names      = nameString.Split(',').ToList();
				_genericNames.Add(r, names);
			});
			_loaded = true;
		}

		private static void SetRegionTypes()
		{
			_regionsDiscovered = -1;
			_regionOrder.Clear();
			_addedShelter = false;
			SetJournalQuantities();
			SetWaterQuantities();
			SetFoodQuantities();
			SetResourceQuantities();
			SetRegionOrder();
		}

		private static void SetRegionOrder()
		{
			int environmentNumber = (int) EnvironmentManager.CurrentEnvironmentType;
			AddRegionTypes(false);
			for (int i = 0; i < environmentNumber + 1; ++i) AddRegionTypes(true);

			int lastTemple = _regionOrder.FindLastIndex(t => t.Item1 == RegionType.Temple);
			int storyCount = JournalEntry.GetStoryCount();
			if (lastTemple < storyCount)
			{
				_regionOrder.Swap(lastTemple, storyCount);
				lastTemple = storyCount;
			}

			List<int> validIndexes = new List<int>();
			for (int i = 0; i < lastTemple; ++i) validIndexes.Add(i);
			Debug.Log(storyCount + " " + lastTemple);
			while (storyCount > 0)
			{
				int                     regionIndex = validIndexes.RemoveRandom();
				Tuple<RegionType, bool> tup         = _regionOrder[regionIndex];
				_regionOrder[regionIndex] =  Tuple.Create(tup.Item1, true);
				storyCount                -= 1;
			}

			CheckToMoveCache();
		}

		private static void CheckToMoveCache()
		{
			if (EnvironmentManager.CurrentEnvironmentType != EnvironmentType.Desert) return;
			int                     cacheIndex = _regionOrder.FindIndex(t => t.Item1 == RegionType.Cache);
			Tuple<RegionType, bool> cache      = _regionOrder[cacheIndex];
			Tuple<RegionType, bool> other      = _regionOrder[BaseRegionCount];
			_regionOrder[BaseRegionCount] = Tuple.Create(cache.Item1, other.Item2);
			_regionOrder[cacheIndex]      = Tuple.Create(other.Item1, cache.Item2);
		}

		private static void AddRegionTypes(bool includeTemple)
		{
			List<RegionType> validTypes = GetValidRegionTypes(includeTemple);
			validTypes.Shuffle();
			validTypes.ForEach(r => _regionOrder.Add(Tuple.Create(r, false)));
		}

		private static List<RegionType> GetValidRegionTypes(bool includeTemple)
		{
			List<RegionType> validTypes = new List<RegionType>();
			validTypes.Add(RegionType.Shrine);
			validTypes.Add(RegionType.Animal);
			for (int i = 0; i < 3; ++i) validTypes.Add(RegionType.Danger);

			bool isDesert            = EnvironmentManager.CurrentEnvironmentType == EnvironmentType.Desert;
			bool includeBonusRegions = isDesert && includeTemple || !isDesert;
			if (includeBonusRegions)
			{
				validTypes.Add(isDesert ? RegionType.Danger : RegionType.Monument);
				validTypes.Add(RegionType.Fountain);
				validTypes.Add(RegionType.Cache);
			}
			else
			{
				validTypes.Add(RegionType.Danger);
				validTypes.Add(RegionType.Danger);
				validTypes.Add(RegionType.Danger);
			}

			if (!includeTemple) return validTypes;
			validTypes.Remove(RegionType.Danger);
			validTypes.Add(RegionType.Temple);
			if (_addedShelter || isDesert) return validTypes;
			_addedShelter = true;
			validTypes.Remove(RegionType.Danger);
			validTypes.Add(RegionType.Shelter);
			return validTypes;
		}

		public static RegionType GetNewRegionType()
		{
			++_regionsDiscovered;
			if (_regionsDiscovered == 0) return RegionType.Gate;
			Tuple<RegionType, bool> regionTup     = _regionOrder[_regionsDiscovered - 1];
			RegionType              newRegionType = regionTup.Item1;
			CombatStoryController.ShouldShow = regionTup.Item2;
			return newRegionType;
		}

		private static void SetWaterQuantities()
		{
			int waterSources = EnvironmentManager.CurrentEnvironment.WaterSources;
			SetItemQuantities(waterSources, r => r.WaterSourceCount, r => ++r.WaterSourceCount);
		}

		private static void SetJournalQuantities()
		{
			int journalCount = 2 + ((int) EnvironmentManager.CurrentEnvironment.EnvironmentType + 1) * 2;
			_regions.Shuffle();
			for (int i = 0; i < journalCount; ++i)
			{
				if (_regions[i] == initialNode) continue;
				_regions[i].JournalIsHere = true;
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
					if (r           == initialNode) continue;
					if (total       == 0) break;
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

		private class RegionNode : Node
		{
			public readonly Region RegionHere;

			public RegionNode(Region region) : base(region.Position) => RegionHere = region;
		}
	}
}