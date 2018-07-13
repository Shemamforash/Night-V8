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
        private readonly HashSet<Region> _neighbors = new HashSet<Region>();
        private static GameObject _nodePrefab;
        public readonly int RegionID;
        private int _lastVisitDay = -1;
        private static Sprite _animalSprite, _dangerSprite, _gateSprite, _fountainSprite, _monumentSprite, _shelterSprite, _shrineSprite, _templeSprite;
        public Vector2 ShrinePosition;
        public readonly List<Vector2> EchoPositions = new List<Vector2>();
        public int WaterSourceCount, FoodSourceCount, ResourceSourceCount;

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

        private void AssignSprite(GameObject nodeObject)
        {
            if (_animalSprite == null) _animalSprite = Resources.Load<Sprite>("Images/Regions/Animal");
            if (_dangerSprite == null) _dangerSprite = Resources.Load<Sprite>("Images/Regions/Danger");
            if (_gateSprite == null) _gateSprite = Resources.Load<Sprite>("Images/Regions/Gate");
            if (_fountainSprite == null) _fountainSprite = Resources.Load<Sprite>("Images/Regions/Fountain");
            if (_monumentSprite == null) _monumentSprite = Resources.Load<Sprite>("Images/Regions/Monument");
            if (_shelterSprite == null) _shelterSprite = Resources.Load<Sprite>("Images/Regions/Shelter");
            if (_shrineSprite == null) _shrineSprite = Resources.Load<Sprite>("Images/Regions/Shrine");
            if (_templeSprite == null) _templeSprite = Resources.Load<Sprite>("Images/Regions/Temple");
            SpriteRenderer image = Helper.FindChildWithName<SpriteRenderer>(nodeObject, "Icon");
            switch (_regionType)
            {
                case RegionType.Shelter:
                    image.sprite = _shelterSprite;
                    break;
                case RegionType.Gate:
                    image.sprite = _gateSprite;
                    break;
                case RegionType.Temple:
                    image.sprite = _templeSprite;
                    break;
                case RegionType.Animal:
                    image.sprite = _animalSprite;
                    break;
                case RegionType.Danger:
                    image.sprite = _dangerSprite;
                    break;
                case RegionType.Nightmare:
                    break;
                case RegionType.Fountain:
                    image.sprite = _fountainSprite;
                    break;
                case RegionType.Monument:
                    image.sprite = _monumentSprite;
                    break;
                case RegionType.Shrine:
                    image.sprite = _shrineSprite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CreateObject()
        {
            if (!_seen && MapGenerator.DontShowHiddenNodes) return;
            if (_nodePrefab == null) _nodePrefab = Resources.Load<GameObject>("Prefabs/Map/Node");
            GameObject nodeObject = GameObject.Instantiate(_nodePrefab);
            AssignSprite(nodeObject);
            nodeObject.transform.SetParent(MapGenerator.MapTransform);
            nodeObject.name = Name;
            nodeObject.transform.position = new Vector3(Position.x, Position.y, 0);
            nodeObject.transform.localScale = Vector3.one;
            MapNodeController mapNodeController = nodeObject.transform.GetComponentInChildren<MapNodeController>(true);
            if (_regionType == RegionType.Temple)
            {
                GameObject g = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Map/Map Shadow"));
                g.transform.position = nodeObject.transform.position;
            }

            mapNodeController.SetName(Name);
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public float DistanceToPoint(Region node) => DistanceToPoint(node.Position);

        public float DistanceToPoint(Vector3 point) => Vector3.Distance(point, Position);

        public Enemy AddEnemy(EnemyType enemyType)
        {
            Enemy newEnemy = new Enemy(enemyType);
            _enemies.Add(newEnemy);
            return newEnemy;
        }

        private void GenerateHumanEncounter()
        {
            int daysSpent = WorldState.GetDaysSpentHere();
            int size = Random.Range(1 + daysSpent, 5 + daysSpent);
            int difficulty = WorldState.Difficulty();
            List<EnemyType> allowedTypes = new List<EnemyType>();
            allowedTypes.Add(EnemyType.Sentinel);
            allowedTypes.Add(EnemyType.Brawler);

            if (difficulty >= 5)
            {
                allowedTypes.Add(EnemyType.Sniper);
                allowedTypes.Add(EnemyType.Martyr);
            }

            if (difficulty >= 10)
            {
                allowedTypes.Add(EnemyType.Witch);
                allowedTypes.Add(EnemyType.Medic);
            }

            if (difficulty >= 15)
            {
                allowedTypes.Add(EnemyType.Warlord);
                allowedTypes.Add(EnemyType.Mountain);
            }

            if (difficulty >= 20)
            {
                difficulty -= 20;
                size += Mathf.FloorToInt(difficulty / 5f);
            }

            while (size > 0)
            {
                EnemyType type = Helper.RandomInList(allowedTypes);
                AddEnemy(type);
                if (type == EnemyType.Sentinel || type == EnemyType.Brawler) --size;
                if (type == EnemyType.Sniper || type == EnemyType.Martyr) size -= 2;
                if (type == EnemyType.Witch || type == EnemyType.Medic) size -= 3;
                if (type == EnemyType.Warlord || type == EnemyType.Mountain) size -= 4;
            }
        }

        private void GenerateNightmareEncounter()
        {
            int daysSpent = WorldState.GetDaysSpentHere();
            int size = Random.Range(5 + daysSpent, 10 + daysSpent);
            int difficulty = WorldState.Difficulty();
            List<EnemyType> allowedTypes = new List<EnemyType>();
            allowedTypes.Add(EnemyType.Ghoul);
            allowedTypes.Add(EnemyType.Ghast);
            if (difficulty >= 5)
            {
                allowedTypes.Add(EnemyType.GhoulMother);
                allowedTypes.Add(EnemyType.Shadow);
            }

            if (difficulty >= 10)
            {
                allowedTypes.Add(EnemyType.Revenant);
                allowedTypes.Add(EnemyType.Maelstrom);
            }

            if (difficulty >= 15)
            {
                allowedTypes.Add(EnemyType.Nightmare);
            }

            if (difficulty >= 20)
            {
                difficulty -= 20;
                size += Mathf.FloorToInt(difficulty / 5f);
            }

            while (size > 0)
            {
                EnemyType type = Helper.RandomInList(allowedTypes);
                AddEnemy(type);
                if (type == EnemyType.Ghoul || type == EnemyType.Ghast) --size;
                if (type == EnemyType.GhoulMother || type == EnemyType.Shadow) size -= 2;
                if (type == EnemyType.Revenant || type == EnemyType.Maelstrom) size -= 3;
                if (type == EnemyType.Nightmare) size -= 4;
            }
        }

        public List<Enemy> Enemies() => _enemies;

        public RegionType GetRegionType() => _regionType;

        public void SetRegionType(RegionType regionType)
        {
            _regionType = regionType;
            switch (regionType)
            {
                case RegionType.Nightmare:
                    GenerateNightmareEncounter();
                    break;
                case RegionType.Shelter:
                    GenerateShelter();
                    break;
                case RegionType.Animal:
                    GenerateAnimalEncounter();
                    break;
                default:
                    GenerateHumanEncounter();
                    break;
            }
        }

        private void GenerateAnimalEncounter()
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    for (int i = 0; i < 10; ++i)
                    {
                        AddEnemy(EnemyType.Grazer);
                    }

                    for (int i = 0; i < 3; ++i)
                    {
                        AddEnemy(EnemyType.Watcher);
                    }

                    break;
                case 1:
                    for (int i = 0; i < Random.Range(1, 4); ++i)
                    {
                        AddEnemy(EnemyType.Curio);
                    }

                    break;
                case 2:
                    for (int i = 0; i < 20; ++i)
                    {
                        AddEnemy(EnemyType.Flit);
                    }

                    break;
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
            CharacterManager.SelectedCharacter.BrandManager.IncreaseRegionsExplored();
            _discovered = true;
            _seen = true;
            foreach (Region neighbor in _neighbors)
            {
                if (neighbor._seen) continue;
                RegionManager.GetRegionType(neighbor);
                neighbor._seen = true;
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

        public bool Seen()
        {
            return _seen;
        }
    }
}