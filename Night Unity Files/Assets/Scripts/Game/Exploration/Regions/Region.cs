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

namespace Game.Exploration.Regions
{
    public class Region : Node, IPersistenceTemplate
    {
        public string Name;
        private static List<EnemyTemplate> _enemyTypes = EnemyTemplate.GetEnemyTypes();
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private RegionType _regionType;
        private bool _discovered;
        public List<Barrier> Barriers = new List<Barrier>();
        public List<EnemyCampfire> Fires = new List<EnemyCampfire>();
        public List<ContainerController> Containers = new List<ContainerController>();
        private readonly HashSet<Region> _neighbors = new HashSet<Region>();
        public Vector2 Position;
        private static GameObject _nodePrefab;
        public readonly int RegionID;
        private int _lastVisitDay = -1;

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

        public void AddNeighbor(Region neighbor)
        {
            _neighbors.Add(neighbor);
            neighbor._neighbors.Add(this);
        }

        public List<Region> Neighbors() => _neighbors.ToList();

        public void CreateObject()
        {
            if (!Discovered() && MapGenerator.DontShowHiddenNodes) return;
            if (_nodePrefab == null) _nodePrefab = Resources.Load<GameObject>("Prefabs/Map/Node");
            GameObject nodeObject = GameObject.Instantiate(_nodePrefab);
            nodeObject.transform.SetParent(MapGenerator.MapTransform);
            nodeObject.name = Name;
            nodeObject.transform.position = new Vector3(Position.x, Position.y, 0);
            nodeObject.transform.localScale = Vector3.one;
            MapNodeController mapNodeController = nodeObject.transform.GetComponentInChildren<MapNodeController>(true);
            mapNodeController.gameObject.SetActive(true);
            if (_regionType == RegionType.Temple)
            {
                GameObject g = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Map/Map Shadow"));
                g.transform.position = nodeObject.transform.position;
            }
            string name = Name + "\n" + _regionType + " d=" + CalculateDanger();
            mapNodeController.SetName(name);
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public float DistanceToPoint(Region node) => DistanceToPoint(node.Position);

        public float DistanceToPoint(Vector3 point) => Vector3.Distance(point, Position);

        public Enemy AddEnemy(EnemyType enemyType, int difficulty)
        {
            Enemy newEnemy = new Enemy(enemyType);
            newEnemy.GenerateArmour(difficulty);
            newEnemy.GenerateWeapon(difficulty);
            _enemies.Add(newEnemy);
            return newEnemy;
        }

        private void GenerateHumanEncounter()
        {
            AddEnemy(EnemyType.Sentinel, 10);
            AddEnemy(EnemyType.Witch, 10);
            AddEnemy(EnemyType.Brawler, 10);
        }

        private void GenerateNightmareEncounter()
        {
//            AddEnemy(EnemyType.GhoulMother, 10);
//            AddEnemy(EnemyType.Maelstrom, 10);
//            AddEnemy(EnemyType.Ghast, 10);
//            AddEnemy(EnemyType.Nightmare, 10);
//            AddEnemy(EnemyType.Revenant, 10);
//            AddEnemy(EnemyType.Shadow, 10);
        }

        private int CalculateDanger() => WorldState.Difficulty() + (int) (Vector2.Distance(Position, Vector2.zero) / 5f);

        public void Generate(int size)
        {
            int difficulty = CalculateDanger();
            AddEnemy(EnemyType.Medic, difficulty);
            AddEnemy(EnemyType.Sentinel, difficulty);
            AddEnemy(EnemyType.Sniper, difficulty);

//            if (size > MaxEncounterSize) size = MaxEncounterSize;
//            for (int i = 0; i < size; ++i)
//            {
//                Helper.Shuffle(ref _enemyTypes);
//                foreach (EnemyTemplate t in _enemyTypes)
//                {
//                    if (size < t.Value) continue;
//                    AddEnemy(t.EnemyType, scenario, difficulty);
//                    break;
//                }
//            }
        }

        public void Generate()
        {
            int difficulty = CalculateDanger();
            while (difficulty > 0)
            {
                Helper.Shuffle(ref _enemyTypes);
                foreach (EnemyTemplate t in _enemyTypes)
                {
                    if (difficulty < t.Value) continue;
                    AddEnemy(t.EnemyType, difficulty);
                    difficulty -= t.Value;
                }
            }
        }

        public List<Enemy> Enemies() => _enemies;

        public RegionType GetRegionType() => _regionType;

        public void SetRegionType(RegionType regionType)
        {
            _regionType = regionType;
            switch (regionType)
            {
                case RegionType.Danger:
                    GenerateHumanEncounter();
                    break;
                case RegionType.Nightmare:
                    GenerateNightmareEncounter();
                    break;
                case RegionType.Shelter:
                    GenerateShelter();
                    break;
                case RegionType.Animal:
                    GenerateAnimalEncounter();
                    break;
            }
        }

        private void GenerateAnimalEncounter()
        {
            for (int i = 0; i < 10; ++i)
            {
                AddEnemy(EnemyType.Grazer, 10);
            }

            for (int i = 0; i < Random.Range(0, 3); ++i)
            {
                AddEnemy(EnemyType.Watcher, 10);
                AddEnemy(EnemyType.Curio, 10);
                AddEnemy(EnemyType.Flit, 10);
            }
        }

        private void GenerateShelter()
        {
            if (CharacterManager.Characters.Count < 4)
            {
                //todo add character;
                return;
            }

            //todo add resources
        }

        public void Enter()
        {
            SceneChanger.ChangeScene("Combat");
        }

        public void Discover()
        {
            if (_discovered) return;
            RegionManager.GetRegionType(this);
            _discovered = true;
            foreach (Region neighbor in _neighbors)
            {
                if (neighbor._discovered) continue;
                RegionManager.GetRegionType(neighbor);
                neighbor._discovered = true;
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
                water += waterSource.Inventory.GetResourceQuantity("Water");
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
                food += foodSource.Inventory.GetResourceQuantity("Food");
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

        private static List<int> _availableIds;
        private const int _totalIds = 1000;

        private static int GetId()
        {
            if (_availableIds == null)
            {
                _availableIds = new List<int>();
                int anchor = Random.Range(0, 10000);
                for (int i = 0; i < _totalIds; ++i) _availableIds.Add(anchor + i);
            }

            int randomIndex = Random.Range(0, _availableIds.Count);
            int id = _availableIds[randomIndex];
            _availableIds.RemoveAt(randomIndex);
            return id;
        }

        public Region() : base(Vector2.zero)
        {
            RegionID = GetId();
        }
    }
}