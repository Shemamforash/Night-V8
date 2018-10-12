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
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private RegionType _regionType;
        private bool _discovered, _seen, _generated;
        public List<Barrier> Barriers = new List<Barrier>();
        public readonly List<EnemyCampfire> Fires = new List<EnemyCampfire>();
        public readonly List<ContainerController> Containers = new List<ContainerController>();
        private static GameObject _nodePrefab;
        public int RegionID;
        private int _lastVisitDay = -1;
        public Vector2 ShrinePosition;
        public int WaterSourceCount, FoodSourceCount, ResourceSourceCount;
        public Vector2? HealShrinePosition = null;
        public Player _characterHere;
        public Vector2 CharacterPosition;
        private readonly List<int> _neighborIds = new List<int>();
        public int ClaimRemaining;
        private int _claimQuantity;
        private string _claimBenefit = "";
        public int RitesRemaining = 3;
        public bool FountainVisited;

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

            int willpowerGain = Mathf.FloorToInt(PlayerCombat.Instance.Player.Attributes.ClaimRegionWillpowerGainModifier);
            PlayerCombat.Instance.Player.Attributes.Get(AttributeType.Willpower).Increment(willpowerGain);
        }

        public void Update()
        {
            if (ClaimRemaining == 0) return;
            --ClaimRemaining;
            if (ClaimRemaining == 0)
            {
                _claimBenefit = "";
                WorldEventManager.GenerateEvent(new WorldEvent(Name + " has been lost to the darkness"));
                if (_regionType == RegionType.Animal) GenerateAnimalEncounter();
                else GenerateEncounter(WorldState.GetAllowedHumanEnemyTypes());
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

            foreach (XmlNode enemyNode in doc.SelectSingleNode("Enemies").SelectNodes("Enemy"))
            {
                string enemyTypeString = enemyNode.StringFromNode("EnemyType");
                EnemyType enemyType = EnemyTemplate.StringToType(enemyTypeString);
                Enemy enemy = new Enemy(EnemyTemplate.GetEnemyTemplate(enemyType));
                region._enemies.Add(enemy);
            }

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
            _enemies.ForEach(e => e.Save(enemyNode));
        }

        private MapNodeController _mapNode;

        public MapNodeController MapNode()
        {
            return _mapNode;
        }

        public void CreateObject()
        {
            if (!_seen) return;
            if (_nodePrefab == null) _nodePrefab = Resources.Load<GameObject>("Prefabs/Map/Map Node");
            GameObject nodeObject = GameObject.Instantiate(_nodePrefab);
            nodeObject.transform.SetParent(MapGenerator.MapTransform);
            nodeObject.name = Name;
            nodeObject.transform.position = new Vector3(Position.x, Position.y, 0);
            nodeObject.transform.localScale = Vector3.one;
            _mapNode = nodeObject.transform.GetComponentInChildren<MapNodeController>(true);
            if (_regionType == RegionType.Temple)
            {
                GameObject g = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Map/Map Shadow"));
                g.transform.SetParent(nodeObject.transform, false);
                g.transform.position = nodeObject.transform.position;
            }

            _mapNode.SetRegion(this);
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public float DistanceToPoint(Region node) => DistanceToPoint(node.Position);

        private float DistanceToPoint(Vector3 point) => Vector3.Distance(point, Position);

        public Enemy AddEnemy(EnemyTemplate template)
        {
            Enemy newEnemy = new Enemy(template);
            _enemies.Add(newEnemy);
            return newEnemy;
        }

        private void GenerateEncounter(List<EnemyTemplate> allowedTypes)
        {
            if (!_canHaveEnemies) return;
            List<EnemyTemplate> templates = CombatManager.GenerateEnemies(WorldState.Difficulty(), allowedTypes);
            Debug.Log(templates.Count);
            templates.ForEach(template => AddEnemy(template));
        }

        public List<Enemy> Enemies() => _enemies;

        public RegionType GetRegionType() => _regionType;

        private void GenerateAnimalEncounter()
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    for (int i = 0; i < 10; ++i)
                    {
                        AddEnemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Grazer));
                    }

                    for (int i = 0; i < 3; ++i)
                    {
                        AddEnemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Watcher));
                    }

                    break;
                case 1:
                    for (int i = 0; i < Random.Range(1, 4); ++i)
                    {
                        AddEnemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Curio));
                    }

                    break;
                case 2:
                    for (int i = 0; i < 20; ++i)
                    {
                        AddEnemy(EnemyTemplate.GetEnemyTemplate(EnemyType.Flit));
                    }

                    break;
            }
        }

        private void GenerateShelter()
        {
            if (CharacterManager.Characters.Count < 4)
            {
                _characterHere = CharacterManager.GenerateRandomCharacter();
            }
        }

        public void SetRegionType(RegionType type)
        {
            _regionType = type;
            Name = MapGenerator.GenerateName(_regionType);
            _canHaveEnemies = type != RegionType.Gate && type != RegionType.Nightmare && type != RegionType.Rite && type != RegionType.Tomb && type != RegionType.Gate;
        }

        private void SetSeen()
        {
            if (_seen) return;
            _seen = true;
        }

        public bool Discover()
        {
            if (_discovered) return false;
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

        public bool Discovered() => _discovered;

        private static int _currentId;
        private bool _canHaveEnemies;

        public bool Seen()
        {
            return _seen;
        }

        public void ConnectNeighbors()
        {
            _neighborIds.ForEach(i => AddNeighbor(MapGenerator.GetRegionById(i)));
        }
    }
}