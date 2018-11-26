using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Ui;
using Game.Exploration.WorldEvents;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Exploration.Regions
{
    public class Region : Node
    {
        public string Name;
        private RegionType _regionType;
        private bool _discovered, _seen, _generated;
        public List<Barrier> Barriers = new List<Barrier>();
        public readonly List<EnemyCampfire> Fires = new List<EnemyCampfire>();
        public readonly List<ContainerController> Containers = new List<ContainerController>();
        private static GameObject _nodePrefab;
        public int RegionID;
        private int _lastVisitDay = -1;
        public int WaterSourceCount, FoodSourceCount, ResourceSourceCount;
        public Player CharacterHere;
        public Vector2 CharacterPosition;
        private readonly List<int> _neighborIds = new List<int>();
        public int ClaimRemaining;
        private int _claimQuantity;
        private string _claimBenefit = "";
        public int RitesRemaining = 3;
        public bool FountainVisited;
        private static int _currentId;
        private bool _canHaveEnemies;
        private GameObject _nodeObject;

        public Region() : base(Vector2.zero)
        {
            RegionID = _currentId;
            ++_currentId;
        }

        public string ClaimBenefitString()
        {
            if (_claimBenefit == "") return _claimBenefit;
            int minutesPerDay = 24 * WorldState.MinutesPerHour;
            int timeRemaining = ClaimRemaining;
            if (timeRemaining > minutesPerDay) timeRemaining -= minutesPerDay;
            int timeRemainingInHours = Mathf.CeilToInt(timeRemaining / 24f);
            string hourString = timeRemainingInHours == 1 ? "hr" : "hrs";
            return " +" + _claimQuantity + " " + _claimBenefit + " in " + timeRemainingInHours + hourString;
        }

        public void Claim()
        {
            ClaimRemaining = 2 * 24 * WorldState.MinutesPerHour;
            switch (_regionType)
            {
                case RegionType.Shelter:
                    _claimBenefit = ResourceTemplate.GetResource().Name;
                    _claimQuantity = 1;
                    break;
                case RegionType.Temple:
                    _claimBenefit = "Essence";
                    _claimQuantity = 5;
                    break;
                case RegionType.Animal:
                    _claimBenefit = "Meat";
                    _claimQuantity = 2;
                    break;
                case RegionType.Danger:
                    if (Random.Range(0, 2) == 0)
                    {
                        _claimBenefit = "Water";
                        _claimQuantity = 1;
                        break;
                    }

                    _claimBenefit = "Meat";
                    _claimQuantity = 1;
                    break;
                case RegionType.Fountain:
                    _claimBenefit = "Water";
                    _claimQuantity = 2;
                    break;
                case RegionType.Monument:
                    _claimBenefit = ResourceTemplate.GetResource().Name;
                    _claimQuantity = 1;
                    break;
                case RegionType.Shrine:
                    _claimBenefit = ResourceTemplate.GetResource().Name;
                    _claimQuantity = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int willGain = Mathf.FloorToInt(PlayerCombat.Instance.Player.Attributes.ClaimRegionWillGainModifier);
            PlayerCombat.Instance.Player.Attributes.Get(AttributeType.Will).Increment(willGain);
        }

        public void Update()
        {
            if (ClaimRemaining == 0) return;
            --ClaimRemaining;
            if (ClaimRemaining == 0)
            {
                _claimBenefit = "";
                WorldEventManager.GenerateEvent(new WorldEvent(Name + " has been lost to the darkness"));
            }

            if (ClaimRemaining % (24 * WorldState.MinutesPerHour) != 0) return;
            Inventory.IncrementResource(_claimBenefit, 1);
        }

        public bool Generated()
        {
            return _generated;
        }

        public void MarkGenerated()
        {
            _generated = true;
        }

        private bool Visited()
        {
            return _lastVisitDay != -1;
        }

        private string TimeSinceLastVisit()
        {
            if (!Visited()) return "Unexplored";
            return "Visited " + (WorldState.Days - _lastVisitDay) + " days ago.";
        }

        public void Visit()
        {
            if (!Visited())
            {
                switch (_regionType)
                {
                    case RegionType.Nightmare:
                        GenerateEncounter(WorldState.GetAllowedNightmareEnemyTypes());
                        break;
                    case RegionType.Shelter:
                        GenerateShelter();
                        break;
                    case RegionType.Animal:
                        GenerateAnimalEncounter();
                        break;
                    case RegionType.Rite:
                        break;
                    case RegionType.Temple:
                        break;
                    default:
                        GenerateEncounter(WorldState.GetAllowedHumanEnemyTypes());
                        break;
                }
            }

            _lastVisitDay = WorldState.Days;
        }

        public static Region Load(XmlNode doc)
        {
            Region region = new Region();
            region.Name = doc.StringFromNode("Name");
            region.RegionID = doc.IntFromNode("RegionId");
            if (region.RegionID > _currentId) _currentId = region.RegionID + 1;
            region.Position = doc.StringFromNode("Position").ToVector2();
            foreach (XmlNode n in doc.SelectSingleNode("Neighbors").SelectNodes("ID"))
            {
                region._neighborIds.Add(n.IntFromNode("ID"));
            }

            region._regionType = (RegionType) doc.IntFromNode("Type");
            region._discovered = doc.BoolFromNode("Discovered");
            region._seen = doc.BoolFromNode("Seen");
            region._lastVisitDay = doc.IntFromNode("LastVisited");
            region.WaterSourceCount = doc.IntFromNode("WaterSourceCount");
            region.FoodSourceCount = doc.IntFromNode("FoodSourceCount");
            region.ResourceSourceCount = doc.IntFromNode("ResourceSourceCount");
            region.ClaimRemaining = doc.IntFromNode("ClaimRemaining");
            region._claimQuantity = doc.IntFromNode("ClaimQuantity");
            region._claimBenefit = doc.StringFromNode("ClaimBenefit");
            region.RitesRemaining = doc.IntFromNode("RitesRemaining");
            region.FountainVisited = doc.BoolFromNode("FountainVisited");
            return region;
        }

        public void Save(XmlNode doc)
        {
            XmlNode regionNode = doc.CreateChild("Region");
            regionNode.CreateChild("Name", Name);
            regionNode.CreateChild("RegionId", RegionID);
            regionNode.CreateChild("Position", Position.ToString());
            XmlNode neighborNode = regionNode.CreateChild("Neighbors");
            foreach (Node n in Neighbors())
            {
                neighborNode.CreateChild("ID", ((Region) n).RegionID);
            }

            regionNode.CreateChild("Type", (int) _regionType);
            regionNode.CreateChild("Discovered", _discovered);
            regionNode.CreateChild("Seen", _seen);
            regionNode.CreateChild("LastVisited", _lastVisitDay);
            regionNode.CreateChild("WaterSourceCount", WaterSourceCount);
            regionNode.CreateChild("FoodSourceCount", FoodSourceCount);
            regionNode.CreateChild("ResourceSourceCount", ResourceSourceCount);
            regionNode.CreateChild("ClaimRemaining", ClaimRemaining);
            regionNode.CreateChild("ClaimQuantity", _claimQuantity);
            regionNode.CreateChild("ClaimBenefit", _claimBenefit);
            regionNode.CreateChild("RitesRemaining", RitesRemaining);
            regionNode.CreateChild("FountainVisited", FountainVisited);

            XmlNode enemyNode = regionNode.CreateChild("Enemies");
        }

        private MapNodeController _mapNode;

        public MapNodeController MapNode()
        {
            return _mapNode;
        }

        public void HideNode()
        {
            if (!_seen) return;
            _mapNode.Hide();
        }

        public void ShowNode()
        {
            if (!_seen) return;
            if (_nodeObject == null) CreateNodeObject();
            _mapNode.Show();
        }

        private void CreateNodeObject()
        {
            if (_nodePrefab == null) _nodePrefab = Resources.Load<GameObject>("Prefabs/Map/Map Node");
            _nodeObject = GameObject.Instantiate(_nodePrefab);
            _nodeObject.transform.SetParent(MapMenuController.MapTransform);
            _nodeObject.name = Name;
            _nodeObject.transform.position = new Vector3(Position.x, Position.y, 0);
            _nodeObject.transform.localScale = Vector3.one;
            _mapNode = _nodeObject.transform.GetComponentInChildren<MapNodeController>(true);
            if (_regionType == RegionType.Temple)
            {
                GameObject g = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Map/Map Shadow"));
                g.transform.SetParent(_nodeObject.transform, false);
                g.transform.position = _nodeObject.transform.position;
            }

            _mapNode.SetRegion(this);
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public float DistanceToPoint(Region node) => DistanceToPoint(node.Position);

        private float DistanceToPoint(Vector3 point) => Vector3.Distance(point, Position);

        private List<EnemyTemplate> GenerateEncounter(List<EnemyTemplate> allowedTypes)
        {
            List<EnemyTemplate> templates = new List<EnemyTemplate>();
            if (!_canHaveEnemies) return templates;
            int size = WorldState.Difficulty() + 4;
            templates.AddRange(CombatManager.GenerateEnemies(size, allowedTypes));
            return templates;
        }

        public RegionType GetRegionType() => _regionType;

        private List<EnemyTemplate> GenerateAnimalEncounter()
        {
            List<EnemyTemplate> templates = new List<EnemyTemplate>();
            switch (Random.Range(0, 3))
            {
                case 0:
                    for (int i = 0; i < 10; ++i) templates.Add(EnemyTemplate.GetEnemyTemplate(EnemyType.Grazer));
                    for (int i = 0; i < 3; ++i) templates.Add(EnemyTemplate.GetEnemyTemplate(EnemyType.Watcher));
                    break;
                case 1:
                    for (int i = 0; i < Random.Range(1, 4); ++i) templates.Add(EnemyTemplate.GetEnemyTemplate(EnemyType.Curio));
                    break;
                case 2:
                    for (int i = 0; i < 20; ++i) templates.Add(EnemyTemplate.GetEnemyTemplate(EnemyType.Flit));
                    break;
            }

            return templates;
        }

        private void GenerateShelter()
        {
            if (CharacterManager.Characters.Count == 3) return;
            CharacterHere = CharacterManager.GenerateRandomCharacter();
        }

        public void SetRegionType(RegionType regionType)
        {
            _regionType = regionType;
            Name = MapGenerator.GenerateName(_regionType);
            _canHaveEnemies = _regionType != RegionType.Gate && _regionType != RegionType.Nightmare && _regionType != RegionType.Rite && _regionType != RegionType.Tomb;
        }

        private void SetSeen()
        {
            if (_seen) return;
            _seen = true;
        }

        public bool Discover()
        {
            if (_discovered) return false;
            SetRegionType(MapGenerator.GetNewRegionType());
            _discovered = true;
            SetSeen();
            foreach (Node neighbor in Neighbors())
            {
                Region region = neighbor as Region;
                if (!region._seen)
                    region.SetSeen();
            }

            return true;
        }

        public bool Seen()
        {
            return _seen;
        }

        public void ConnectNeighbors()
        {
            _neighborIds.ForEach(i => AddNeighbor(MapGenerator.GetRegionById(i)));
        }

        public List<Enemy> GetEnemies()
        {
            List<Enemy> enemies = new List<Enemy>();
            if (ClaimRemaining != 0) return enemies;
            List<EnemyTemplate> templates = _regionType == RegionType.Animal ? GenerateAnimalEncounter() : GenerateEncounter(WorldState.GetAllowedHumanEnemyTypes());
            templates.ForEach(t => enemies.Add(t.Create()));
            return enemies;
        }
    }
}