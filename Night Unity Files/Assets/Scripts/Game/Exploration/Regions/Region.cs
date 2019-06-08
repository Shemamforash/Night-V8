using System.Collections.Generic;
using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Ui;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Regions
{
	public class Region : Node
	{
		private static GameObject _nodePrefab;
		private static int        _currentId;

		private readonly List<int> _neighborIds = new List<int>();

		private MapNodeController _mapNode;
		private GameObject        _nodeObject;
		private RegionType        _regionType;
		private int               _size;
		private bool              _discovered;
		private bool              _seen;
		private bool              _generated;
		private bool              _templeCleansed;
		private bool              _isDynamicRegion;
		private bool              _justDiscovered;
		private bool              _cleared;

		public readonly List<EnemyCampfire>       Fires      = new List<EnemyCampfire>();
		public readonly List<ContainerController> Containers = new List<ContainerController>();

		public List<Barrier> Barriers = new List<Barrier>();
		public Player        CharacterHere;
		public string        Name;
		public int           RegionID;
		public int           WaterSourceCount;
		public int           FoodSourceCount;
		public int           ResourceSourceCount;
		public bool          MonumentUsed;
		public bool          FountainVisited;
		public bool          IsWeaponHere = true;
		public bool          RitesRemain  = true;
		public bool          JournalIsHere;

		public Region() : base(Vector2.zero)
		{
			RegionID = _currentId;
			++_currentId;
		}

		public bool RingChallengeComplete;

		public void SetTempleCleansed()
		{
			_templeCleansed = true;
			Name            = "Cleansed Temple";
		}

		public void CheckForRegionExplored()
		{
			if (!_justDiscovered) return;
			if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Tutorial) return;
			PlayerCombat.Instance.Player.BrandManager.IncreaseRegionsExplored();
		}

		public static Region Load(XmlNode doc)
		{
			Region region = new Region();
			region.Name     = doc.ParseString("Name");
			region.RegionID = doc.ParseInt("RegionId");
			if (region.RegionID > _currentId) _currentId = region.RegionID + 1;
			region.Position = doc.ParseString("Position").ToVector2();
			foreach (XmlNode n in doc.SelectSingleNode("Neighbors").SelectNodes("ID"))
				region._neighborIds.Add(n.ParseInt("ID"));
			region._regionType = (RegionType) doc.ParseInt("Type");
			region._discovered = doc.ParseBool("Discovered");
			region._seen       = doc.ParseBool("Seen");
			region._cleared    = doc.ParseBool("Cleared");

			region._size               = doc.ParseInt("Size");
			region._justDiscovered     = doc.ParseBool("JustDiscovered");
			region._templeCleansed     = doc.ParseBool("TempleCleansed");
			region.WaterSourceCount    = doc.ParseInt("WaterSourceCount");
			region.FoodSourceCount     = doc.ParseInt("FoodSourceCount");
			region.ResourceSourceCount = doc.ParseInt("ResourceSourceCount");
			region.JournalIsHere       = doc.ParseBool("JournalIsHere");
			region.MonumentUsed        = doc.ParseBool("MonumentUsed");
			region.RitesRemain         = doc.ParseBool("RitesRemaining");
			region.FountainVisited     = doc.ParseBool("FountainVisited");
			region.IsWeaponHere        = doc.ParseBool("IsWeaponHere");
			TryLoadRingChallenge(region, doc);
			int characterClassHere = doc.ParseInt("CharacterHere");
			region.CharacterHere = characterClassHere == -1 ? null : CharacterManager.GenerateCharacter((CharacterClass) characterClassHere);
			region.CheckIsDynamic();
			return region;
		}

		private static void TryLoadRingChallenge(Region region, XmlNode doc)
		{
			XmlNode node = doc.SelectSingleNode(nameof(RingChallengeComplete));
			if (node == null) return;
			region.RingChallengeComplete = doc.ParseBool(nameof(RingChallengeComplete));
		}

		public void Save(XmlNode doc)
		{
			XmlNode regionNode = doc.CreateChild("Region");
			regionNode.CreateChild("Name",     Name);
			regionNode.CreateChild("RegionId", RegionID);
			regionNode.CreateChild("Position", Position.ToNiceString());
			XmlNode neighborNode = regionNode.CreateChild("Neighbors");
			foreach (Node n in Neighbors())
				neighborNode.CreateChild("ID", ((Region) n).RegionID);
			regionNode.CreateChild("Type",                        (int) _regionType);
			regionNode.CreateChild("Discovered",                  _discovered);
			regionNode.CreateChild("JustDiscovered",              _justDiscovered);
			regionNode.CreateChild("Seen",                        _seen);
			regionNode.CreateChild("Cleared",                     _cleared);
			regionNode.CreateChild("Size",                        _size);
			regionNode.CreateChild("TempleCleansed",              _templeCleansed);
			regionNode.CreateChild("WaterSourceCount",            WaterSourceCount);
			regionNode.CreateChild("FoodSourceCount",             FoodSourceCount);
			regionNode.CreateChild("ResourceSourceCount",         ResourceSourceCount);
			regionNode.CreateChild("JournalIsHere",               JournalIsHere);
			regionNode.CreateChild("MonumentUsed",                MonumentUsed);
			regionNode.CreateChild("RitesRemaining",              RitesRemain);
			regionNode.CreateChild("FountainVisited",             FountainVisited);
			regionNode.CreateChild("IsWeaponHere",                IsWeaponHere);
			regionNode.CreateChild(nameof(RingChallengeComplete), RingChallengeComplete);
			int characterClassHere = CharacterHere == null ? -1 : (int) CharacterHere.CharacterTemplate.CharacterClass;
			regionNode.CreateChild("CharacterHere", characterClassHere);
		}

		public void HideNode()
		{
			if (!_seen) return;
			if (_mapNode == null) return;
			_mapNode.Hide();
		}

		public void ShowNode(Player player)
		{
			if (!_seen) return;
			if (_nodeObject == null) CreateNodeObject();
			_mapNode.Show(player);
		}

		private void CreateNodeObject()
		{
			if (_nodePrefab == null) _nodePrefab = Resources.Load<GameObject>("Prefabs/Map/Map Node");
			_nodeObject                      = Object.Instantiate(_nodePrefab, MapMenuController.Instance().MapTransform, true);
			_nodeObject.name                 = Name;
			_nodeObject.transform.position   = new Vector3(Position.x, Position.y, 0);
			_nodeObject.transform.localScale = Vector3.one;
			_mapNode                         = _nodeObject.transform.GetComponentInChildren<MapNodeController>(true);
			_mapNode.SetRegion(this);
		}

		private List<EnemyType> GenerateEncounter(List<EnemyType> allowedTypes)
		{
			List<EnemyType> templates = new List<EnemyType>();
			if (!IsDynamic()) return templates;
			if (!_cleared && _size == 0) _size = WorldState.Difficulty() + 10;
			templates = EnemyTemplate.RandomiseEnemiesToSize(allowedTypes, _size);
			_size     = 0;
			return templates;
		}

		private List<EnemyType> GenerateAnimalEncounter()
		{
			List<EnemyType> animalTypes = new List<EnemyType>
			{
				EnemyType.Grazer,
				EnemyType.Watcher,
				EnemyType.Curio
			};
			if (!_cleared && _size == 0) _size = 5;
			return EnemyTemplate.RandomiseEnemiesToSize(animalTypes, _size);
		}

		private void CheckIsDynamic()
		{
			_isDynamicRegion = _regionType != RegionType.Gate
			                && _regionType != RegionType.Rite
			                && _regionType != RegionType.Tomb
			                && _regionType != RegionType.Temple
			                && _regionType != RegionType.Tutorial;
		}

		public void SetRegionType(RegionType regionType)
		{
			_regionType = regionType;
			CheckIsDynamic();
			Name = MapGenerator.GenerateName(_regionType);
			if (_regionType                         != RegionType.Shelter) return;
			if (CharacterManager.AlternateCharacter != null) return;
			CharacterHere = CharacterManager.GenerateRandomCharacter();
		}

		public bool IsDynamic() => _isDynamicRegion;

		public void ConnectNeighbors()
		{
			_neighborIds.ForEach(i => AddNeighbor(MapGenerator.GetRegionById(i)));
		}

		public void Discover()
		{
			if (_discovered)
			{
				_justDiscovered = false;
				return;
			}

			_justDiscovered = true;
			SetRegionType(MapGenerator.GetNewRegionType());
			_discovered = true;
			_seen       = true;
			foreach (Node neighbor in Neighbors())
				((Region) neighbor)._seen = true;
		}

		public bool Discovered() => _discovered;

		public List<EnemyType> GetEnemies()
		{
			List<EnemyType> enemyTypes = new List<EnemyType>();
			switch (_regionType)
			{
				case RegionType.Danger:
					enemyTypes = GenerateEncounter(WorldState.GetAllowedHumanEnemyTypes());
					break;
				case RegionType.Fountain:
					enemyTypes = GenerateEncounter(WorldState.GetAllowedHumanEnemyTypes());
					break;
				case RegionType.Monument:
					enemyTypes = GenerateEncounter(WorldState.GetAllowedHumanEnemyTypes());
					break;
				case RegionType.Shrine:
					enemyTypes = GenerateEncounter(WorldState.GetAllowedHumanEnemyTypes());
					break;
				case RegionType.Animal:
					enemyTypes = GenerateAnimalEncounter();
					break;
			}

			return enemyTypes;
		}

		public bool Seen() => _seen;

		public void SetPosition(Vector2 position) => Position = position;

		public MapNodeController MapNode() => _mapNode;

		public bool Generated() => _generated;

		public void MarkGenerated() => _generated = true;

		public RegionType GetRegionType() => _regionType;

		public void RestoreSize(EnemyType enemyType)
		{
			int size = EnemyTemplate.GetEnemyValue(enemyType);
			_size += size;
		}

		public void RestoreSize(CanTakeDamage takeDamage)
		{
			EnemyBehaviour enemyBehaviour = takeDamage as EnemyBehaviour;
			if (enemyBehaviour       == null) return;
			if (enemyBehaviour.Enemy == null) return;
			_size += enemyBehaviour.Enemy.Template.Value;
		}

		public bool IsTempleCleansed() => _templeCleansed;

		public void SetCleared()
		{
			_cleared = true;
		}

		public bool IsCleared() => _cleared;

		public bool ShouldDrawInnerRing() => _isDynamicRegion || _regionType == RegionType.Temple;
	}
}