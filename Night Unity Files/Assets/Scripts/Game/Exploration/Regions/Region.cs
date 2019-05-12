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
		private const  int        TimeToGenerateResource = 8 * WorldState.MinutesPerHour;
		private static GameObject _nodePrefab;
		private static int        _currentId;

		private readonly List<int>                 _neighborIds = new List<int>();
		public readonly  List<ContainerController> Containers   = new List<ContainerController>();

		public readonly List<EnemyCampfire> Fires         = new List<EnemyCampfire>();
		private         string              _claimBenefit = "";
		private         bool                _discovered, _seen, _generated, _templeCleansed, _isDynamicRegion, _justDiscovered, _cleared;
		private         MapNodeController   _mapNode;

		private GameObject    _nodeObject;
		private RegionType    _regionType;
		private int           _size, _claimQuantity, _remainingTimeToGenerateResource;
		public  List<Barrier> Barriers = new List<Barrier>();
		public  Player        CharacterHere;
		public  bool          MonumentUsed, FountainVisited, IsWeaponHere = true, RitesRemain = true, JournalIsHere;
		public  string        Name;
		public  Vector2?      RadianceStonePosition;
		public  int           RegionID, WaterSourceCount, FoodSourceCount, ResourceSourceCount;

		public Region() : base(Vector2.zero)
		{
			RegionID = _currentId;
			++_currentId;
		}

		public void SetTempleCleansed()
		{
			_templeCleansed = true;
			Name            = "Cleansed Temple";
		}

		public string ClaimBenefitString()
		{
			if (!Claimed()) return "";
			int    timeRemainingInHours = Mathf.CeilToInt(_remainingTimeToGenerateResource / (float) WorldState.MinutesPerHour);
			string hourString           = timeRemainingInHours == 1 ? "hr" : "hrs";
			return " +" + _claimQuantity + " " + _claimBenefit + " in " + timeRemainingInHours + hourString;
		}

		public void CheckForRegionExplored()
		{
			if (!_justDiscovered) return;
			if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Tutorial) return;
			PlayerCombat.Instance.Player.BrandManager.IncreaseRegionsExplored();
		}

		private void SetClaimResource()
		{
			switch (_regionType)
			{
				case RegionType.Shelter:
					_claimBenefit  = ResourceTemplate.GetResource().Name;
					_claimQuantity = 1;
					break;
				case RegionType.Temple:
					_claimBenefit  = "Essence";
					_claimQuantity = 2;
					break;
				case RegionType.Animal:
					_claimBenefit  = "Meat";
					_claimQuantity = 1;
					break;
				case RegionType.Danger:
					_claimBenefit  = Random.Range(0, 2) == 0 ? "Water" : "Meat";
					_claimQuantity = 1;
					break;
				case RegionType.Fountain:
					_claimBenefit  = "Water";
					_claimQuantity = 1;
					break;
				case RegionType.Monument:
					_claimBenefit  = ResourceTemplate.GetResource().Name;
					_claimQuantity = 1;
					break;
				case RegionType.Shrine:
					_claimBenefit  = ResourceTemplate.GetResource().Name;
					_claimQuantity = 1;
					break;
				case RegionType.Cache:
					_claimBenefit  = ResourceTemplate.GetResource().Name;
					_claimQuantity = 1;
					break;
			}
		}

		public void Claim(Vector2 position)
		{
			RadianceStonePosition            = position;
			_remainingTimeToGenerateResource = TimeToGenerateResource;
			int willGain = Mathf.FloorToInt(PlayerCombat.Instance.Player.Attributes.ClaimRegionWillGainModifier) > 0 ? 2 : 0;
			PlayerCombat.Instance.Player.Attributes.Get(AttributeType.Will).Increment(willGain);
		}

		public bool Claimed() => RadianceStonePosition != null;

		public void Update()
		{
			if (!Claimed()) return;
			--_remainingTimeToGenerateResource;
			if (_remainingTimeToGenerateResource != 0) return;
			_remainingTimeToGenerateResource = TimeToGenerateResource;
			Inventory.IncrementResource(_claimBenefit, _claimQuantity);
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

			region._size           = doc.ParseInt("Size");
			region._justDiscovered = doc.ParseBool("JustDiscovered");
			region._templeCleansed = doc.ParseBool("TempleCleansed");
			string radianceStoneString                                  = doc.ParseString("RadianceStonePosition");
			if (radianceStoneString != "") region.RadianceStonePosition = radianceStoneString.ToVector2();
			region.WaterSourceCount                 = doc.ParseInt("WaterSourceCount");
			region.FoodSourceCount                  = doc.ParseInt("FoodSourceCount");
			region.ResourceSourceCount              = doc.ParseInt("ResourceSourceCount");
			region.JournalIsHere                    = doc.ParseBool("JournalIsHere");
			region.MonumentUsed                     = doc.ParseBool("MonumentUsed");
			region._remainingTimeToGenerateResource = doc.ParseInt("ClaimRemaining");
			region._claimQuantity                   = doc.ParseInt("ClaimQuantity");
			region._claimBenefit                    = doc.ParseString("ClaimBenefit");
			region.RitesRemain                      = doc.ParseBool("RitesRemaining");
			region.FountainVisited                  = doc.ParseBool("FountainVisited");
			region.IsWeaponHere                     = doc.ParseBool("IsWeaponHere");
			int characterClassHere = doc.ParseInt("CharacterHere");
			region.CharacterHere = characterClassHere == -1 ? null : CharacterManager.GenerateCharacter((CharacterClass) characterClassHere);
			region.CheckIsDynamic();
			return region;
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
			regionNode.CreateChild("Type",                  (int) _regionType);
			regionNode.CreateChild("Discovered",            _discovered);
			regionNode.CreateChild("JustDiscovered",        _justDiscovered);
			regionNode.CreateChild("Seen",                  _seen);
			regionNode.CreateChild("Cleared",               _cleared);
			regionNode.CreateChild("Size",                  _size);
			regionNode.CreateChild("TempleCleansed",        _templeCleansed);
			regionNode.CreateChild("RadianceStonePosition", RadianceStonePosition == null ? "" : RadianceStonePosition.Value.ToNiceString());
			regionNode.CreateChild("WaterSourceCount",      WaterSourceCount);
			regionNode.CreateChild("FoodSourceCount",       FoodSourceCount);
			regionNode.CreateChild("ResourceSourceCount",   ResourceSourceCount);
			regionNode.CreateChild("JournalIsHere",         JournalIsHere);
			regionNode.CreateChild("MonumentUsed",          MonumentUsed);
			regionNode.CreateChild("ClaimRemaining",        _remainingTimeToGenerateResource);
			regionNode.CreateChild("ClaimQuantity",         _claimQuantity);
			regionNode.CreateChild("ClaimBenefit",          _claimBenefit);
			regionNode.CreateChild("RitesRemaining",        RitesRemain);
			regionNode.CreateChild("FountainVisited",       FountainVisited);
			regionNode.CreateChild("IsWeaponHere",          IsWeaponHere);
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
			if (Claimed() || !IsDynamic()) return templates;
			if (!_cleared && _size == 0)
			{
				_size =  Mathf.CeilToInt(WorldState.Difficulty() / 2.5f);
				_size += 6;
			}

			templates = EnemyTemplate.RandomiseEnemiesToSize(allowedTypes, _size);
			_size     = 0;
			return templates;
		}

		private List<EnemyType> GenerateAnimalEncounter()
		{
			if (Claimed()) return new List<EnemyType>();
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
			SetClaimResource();
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