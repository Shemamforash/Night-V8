using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.Persistence;

namespace Game.Exploration.Region
{
    public class Region : DesolationInventory
    {
        private static List<EnemyTemplate> _enemyTypes = EnemyTemplate.GetEnemyTypes();
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private readonly RegionTemplate _template;
        private bool _discovered;

        public void AddEnemy(Enemy enemy)
        {
            _enemies.Add(enemy);
        }

        public void GenerateSimpleEncounter()
        {
            AddEnemy(new Enemy(EnemyType.Sentinel));
            AddEnemy(new Enemy(EnemyType.Witch));
        }

        private void AddEnemy(EnemyType enemyType, int difficulty)
        {
            Enemy newEnemy = new Enemy(enemyType);
            AddEnemy(newEnemy);
            newEnemy.GenerateArmour(difficulty);
            newEnemy.GenerateWeapon(difficulty);
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

        public Region(string name, RegionTemplate template) : base(name)
        {
            _template = template;
            SetInitialResourceValues(InventoryResourceType.Water, _template.WaterAvailable);
            SetInitialResourceValues(InventoryResourceType.Food, _template.FoodAvailable);
            SetInitialResourceValues(InventoryResourceType.Fuel, _template.FuelAvailable);
            SetInitialResourceValues(InventoryResourceType.Scrap, _template.ScrapAvailable);
            GenerateSimpleEncounter();
            //TODO different combat scenarios for region tier and animal/human enemies
        }

        public override XmlNode Save(XmlNode doc, PersistenceType type)
        {
            XmlNode regionNode = base.Save(doc, type);
            SaveController.CreateNodeAndAppend("Discovered", regionNode, _discovered);
            XmlNode combatNode = SaveController.CreateNodeAndAppend("Scenario", regionNode);
            return regionNode;
        }

        private void SetInitialResourceValues(InventoryResourceType resourceType, float resourceRating)
        {
            InventoryResource resource = GetResource(resourceType);
            float initialQuantity = resourceRating * 10;
            resource.SetMax(initialQuantity);
            IncrementResource(resourceType, initialQuantity);
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
            description += "Water: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Water).Quantity());
            description += "\nFood: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Food).Quantity());
            description += "\nFuel: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Fuel).Quantity());
            description += "\nScrap: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Scrap).Quantity());
//            description += "\nAmmo: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Ammo).Quantity());
            description += "\nEncounters: " + _template.Encounters;
            description += "\nPossible items: " + _template.Items;
            return description;
        }

        public void AddWater(int ratingPoints)
        {
            IncrementResource(InventoryResourceType.Water, 10 * ratingPoints);
        }

        public void AddFood(int ratingPoints)
        {
            IncrementResource(InventoryResourceType.Food, 10 * ratingPoints);
        }

        private static string GetAmountRemainingDescripter(float amount)
        {
            string amountRemaining = "";
            for (int i = 0; i < amount; i += 10) amountRemaining += "+";
            return amountRemaining;
        }
    }
}