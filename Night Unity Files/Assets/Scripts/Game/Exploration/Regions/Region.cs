using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Exploration.Environment;
using Game.Exploration.Ui;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Exploration.Regions
{
    public class Region : Node, IPersistenceTemplate
    {
        public string Name;
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private RegionType _regionType;
        private bool _discovered;
        private bool _seen;
        public List<Barrier> Barriers = new List<Barrier>();
        public readonly List<EnemyCampfire> Fires = new List<EnemyCampfire>();
        public readonly List<ContainerController> Containers = new List<ContainerController>();
        private static GameObject _nodePrefab;
        public readonly int RegionID;
        private int _lastVisitDay = -1;
        public Vector2 ShrinePosition;
        public readonly List<Vector2> EchoPositions = new List<Vector2>();
        public int WaterSourceCount, FoodSourceCount, ResourceSourceCount;
        public Vector2? HealShrinePosition = null;
        public Vector2? EssenceShrinePosition = null;
        public Player _characterHere;
        public Vector2 CharacterPosition;

        public bool Visited()
        {
            return _lastVisitDay != -1;
        }

        public string TimeSinceLastVisit()
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
                        GenerateEncounter(WorldState.GetAllowedNightmareEnemyTypes(), 5, 10);
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
                        GenerateEncounter(WorldState.GetAllowedHumanEnemyTypes(), 1, 5);
                        break;
                }
            }
            _lastVisitDay = WorldState.Days;
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType type)
        {
            if (type != PersistenceType.Game) return doc;
            XmlNode regionNode = SaveController.CreateNodeAndAppend("Region", doc);
            SaveController.CreateNodeAndAppend("Name", regionNode, Name);
            SaveController.CreateNodeAndAppend("Discovered", regionNode, _discovered);
            XmlNode enemyNode = SaveController.CreateNodeAndAppend("Enemies", regionNode);
            _enemies.ForEach(e => e.Save(enemyNode, type));
//            XmlNode barrierNode = SaveController.CreateNodeAndAppend("Barriers", regionNode);
//            Barriers.ForEach(b => b.Save(barrierNode, type));
//            XmlNode fireNode = SaveController.CreateNodeAndAppend("Fires", regionNode);
//            Fires.ForEach(f => f.Save(fireNode, type));
            return regionNode;
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

        private void GenerateEncounter(List<EnemyTemplate> allowedTypes, int minSize, int maxSize)
        {
            int daysSpent = WorldState.GetDaysSpentHere();
            int size = Random.Range(minSize + daysSpent, maxSize + daysSpent);
            int difficulty = WorldState.Difficulty();

            if (difficulty >= 20)
            {
                difficulty -= 20;
                size += Mathf.FloorToInt(difficulty / 5f);
            }

            while (size > 0)
            {
                EnemyTemplate template = Helper.RandomInList(allowedTypes);
                AddEnemy(template);
                size -= template.Value;
            }
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
        }

        private void SetSeen()
        {
            if (_seen) return;
            _seen = true;
        }

        public void Discover()
        {
            if (_discovered) return;
            CharacterManager.SelectedCharacter.BrandManager.IncreaseRegionsExplored();
            _discovered = true;
            SetSeen();
            foreach (Node neighbor in Neighbors())
            {
                Region region = neighbor as Region;
                if (!region._seen)
                    region.SetSeen();
            }
        }

        public bool Discovered() => _discovered;

        private string AmountToDescriptor(int amount)
        {
            if (amount == 0) return "No";
            if (amount == 1) return "Some";
            if (amount < 3) return "Lots of";
            return "Plentiful";
        }

        private string DangerToDescriptor()
        {
            int enemies = Enemies().Count;
            if (enemies == 0) return "No";
            if (enemies == 1) return "Slight";
            if (enemies < 5) return "Some";
            if (enemies < 10) return "Considerable";
            return "Extreme";
        }

        public string Description()
        {
            if (!Visited()) return TimeSinceLastVisit();
            string description = "";
            description += AmountToDescriptor(GetFoodQuantity()) + " Food\n";
            description += AmountToDescriptor(GetWaterQuantity()) + " Water\n";
            description += DangerToDescriptor() + " Danger\n";
            description += TimeSinceLastVisit();
            return description;
        }

        public int GetWaterQuantity()
        {
            int water = 0;
            Containers.ForEach(c =>
            {
                WaterSource waterSource = c as WaterSource;
                if (waterSource == null) return;
                waterSource.Inventory().Consumables().ForEach(i =>
                {
                    if (i.IsWater) ++water;
                });
            });
            return water;
        }

        public int GetFoodQuantity()
        {
            int food = 0;
            Containers.ForEach(c =>
            {
                FoodSource foodSource = c as FoodSource;
                if (foodSource == null) return;
                foodSource.Inventory().Consumables().ForEach(i =>
                {
                    if (i.IsFood) ++food;
                });
            });
            return food;
        }

        public void ChangeWater(int polarity)
        {
            Containers.ForEach(c =>
            {
                WaterSource waterSource = c as WaterSource;
                if (waterSource == null) return;
                waterSource.Change(polarity);
            });
        }

        public void ChangeFood(int polarity)
        {
            Containers.ForEach(c =>
            {
                FoodSource foodSource = c as FoodSource;
                if (foodSource == null) return;
                foodSource.Change(polarity);
            });
        }

        private static int _currentId;

        public Region() : base(Vector2.zero)
        {
            RegionID = _currentId;
            ++_currentId;
        }

        public bool Seen()
        {
            return _seen;
        }
    }
}