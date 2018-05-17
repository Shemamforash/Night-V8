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
using SamsHelper.BaseGameFunctionality.InventorySystem;
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
        public EnemyCampfire Fire;
        public List<ContainerController> Containers = new List<ContainerController>();
        private readonly Dictionary<Region, Path> _paths = new Dictionary<Region, Path>();
        private readonly HashSet<Region> _neighbors = new HashSet<Region>();
        public Vector2 Position;
        private static GameObject _nodePrefab;

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

        public List<Region> Neighbors()
        {
            return _neighbors.ToList();
        }

        public void CreateObject()
        {
            if (!Discovered() && MapGenerator.DontShowHiddenNodes) return;
            if (_nodePrefab == null) _nodePrefab = Resources.Load<GameObject>("Prefabs/Map/Node");
            GameObject nodeObject = GameObject.Instantiate(_nodePrefab);
            nodeObject.transform.SetParent(MapGenerator.MapTransform);
            nodeObject.name = Name;
            nodeObject.transform.position = new Vector3(Position.x, Position.y, 0);
            nodeObject.transform.localScale = Vector3.one;
            UpdatePaths();
            MapNodeController mapNodeController = nodeObject.transform.GetComponentInChildren<MapNodeController>(true);
            mapNodeController.gameObject.SetActive(true);
            string name = Name + "\n" + _regionType + " d=" + CalculateDanger();
            mapNodeController.SetName(name);
        }

        private void UpdatePaths()
        {
            foreach (Region node in Neighbors())
            {
                if (node.Discovered() && !_paths.ContainsKey(node))
                {
                    UiPathDrawController.CreatePathBetweenNodes(this, node);
                }
            }
        }

        public void AddPathTo(Region node, Path path)
        {
            _paths[node] = path;
        }

        public Path GetPathTo(Region node)
        {
            return _paths[node];
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public float DistanceToPoint(Region node)
        {
            return DistanceToPoint(node.Position);
        }

        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(point, Position);
        }

        public Enemy AddEnemy(EnemyType enemyType, int difficulty)
        {
            Enemy newEnemy = new Enemy(enemyType);
            newEnemy.GenerateArmour(difficulty);
            newEnemy.GenerateWeapon(difficulty);
            _enemies.Add(newEnemy);
            return newEnemy;
        }

        private void GenerateSimpleEncounter()
        {
            AddEnemy(EnemyType.Sentinel, 10);
            AddEnemy(EnemyType.Witch, 10);
            AddEnemy(EnemyType.Brawler, 10);
        }

        private void GenerateNightmare()
        {
            AddEnemy(EnemyType.GhoulMother, 10);
            AddEnemy(EnemyType.Maelstrom, 10);
            AddEnemy(EnemyType.Ghast, 10);
            AddEnemy(EnemyType.Nightmare, 10);
        }

        private int CalculateDanger()
        {
            return WorldState.StormDistanceMax - WorldState.StormDistance + (int) (Vector2.Distance(Position, Vector2.zero) / 5f);
        }

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

        public List<Enemy> Enemies()
        {
            return _enemies;
        }

        public RegionType GetRegionType()
        {
            return _regionType;
        }
        
        public void SetRegionType(RegionType regionType)
        {
            //TODO different combat scenarios for region tier and animal/human enemies
            _regionType = regionType;
            switch (regionType)
            {
                case RegionType.Danger:
                    GenerateSimpleEncounter();
//                    AreaGenerator.GenerateForest(this);
            AreaGenerator.GenerateCanyon(this);
//                    AreaGenerator.GenerateSplitRock(this);
                    break;
                case RegionType.Nightmare:
                    GenerateNightmare();
                    break;
            }
        }

        public void Enter()
        {
            SceneChanger.ChangeScene("Combat");
        }

        public void Discover()
        {
            if (_discovered) return; 
            _discovered = true;
            RegionManager.GetRegionType(this);
        }

        public bool Discovered()
        {
            return _discovered;
        }

        public string Description()
        {
            string description = "";
            return description;
        }

        public void AddWater(int ratingPoints)
        {
//            IncrementResource(InventoryResourceType.Water, 10 * ratingPoints);
        }

        public void AddFood(int ratingPoints)
        {
//            IncrementResource(InventoryResourceType.Food, 10 * ratingPoints);
        }

        private static string GetAmountRemainingDescripter(float amount)
        {
            string amountRemaining = "";
            for (int i = 0; i < amount; i += 10) amountRemaining += "+";
            return amountRemaining;
        }

        public Region() : base(Vector2.zero)
        {
        }
    }
}