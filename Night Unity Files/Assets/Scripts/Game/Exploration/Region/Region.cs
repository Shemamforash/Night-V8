using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.Persistence;

namespace Game.Exploration.Region
{
    public class Region : IPersistenceTemplate
    {
        public readonly string Name;
        private static List<EnemyTemplate> _enemyTypes = EnemyTemplate.GetEnemyTypes();
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private readonly RegionTemplate _template;
        private bool _discovered;
        public List<Barrier> Barriers = new List<Barrier>();
        public List<EnemyCampfire> Fires = new List<EnemyCampfire>();
        public List<ContainerController> Containers = new List<ContainerController>();

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
        
        public Enemy AddEnemy(EnemyType enemyType, int difficulty)
        {
            Enemy newEnemy = new Enemy(enemyType);
            newEnemy.GenerateArmour(difficulty);
            newEnemy.GenerateWeapon(difficulty);
            _enemies.Add(newEnemy);
            return newEnemy;
        }

        public void GenerateSimpleEncounter()
        {
            AddEnemy(EnemyType.Sentinel, 10);
            AddEnemy(EnemyType.Witch, 10);
        }

        public void Generate(int difficulty, int size)
        {
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

        public void Generate(int difficulty)
        {
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

        public Region(string name, RegionTemplate template)
        {
            Name = name;
            _template = template;
            SetInitialResourceValues(InventoryResourceType.Water, _template.WaterAvailable);
            SetInitialResourceValues(InventoryResourceType.Food, _template.FoodAvailable);
            SetInitialResourceValues(InventoryResourceType.Fuel, _template.FuelAvailable);
            SetInitialResourceValues(InventoryResourceType.Scrap, _template.ScrapAvailable);
            GenerateSimpleEncounter();
            AreaGenerator.GenerateArea(this);
            //TODO different combat scenarios for region tier and animal/human enemies
        }

        private void SetInitialResourceValues(InventoryResourceType resourceType, float resourceRating)
        {
//            InventoryResource resource = GetResource(resourceType);
//            float initialQuantity = resourceRating * 10;
//            resource.SetMax(initialQuantity);
//            IncrementResource(resourceType, initialQuantity);
        }

        public void Enter()
        {
            SceneChanger.ChangeScene("Combat");
        }

        public void Discover()
        {
            if (!_discovered) _discovered = true;
        }

        public bool Discovered()
        {
            return _discovered;
        }

        public string Description()
        {
            string description = "";
//            description += "Water: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Water).Quantity());
//            description += "\nFood: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Food).Quantity());
//            description += "\nFuel: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Fuel).Quantity());
//            description += "\nScrap: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Scrap).Quantity());
//            description += "\nAmmo: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Ammo).Quantity());
            description += "\nEncounters: " + _template.Encounters;
            description += "\nPossible items: " + _template.Items;
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
    }
}