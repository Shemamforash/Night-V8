﻿using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;

namespace Game.Exploration.Region
{
    public class Region : DesolationInventory
    {
        private readonly CombatScenario _combatScenario;
        private readonly RegionTemplate _template;
        private bool _discovered;

        public Region(string name, RegionTemplate template) : base(name)
        {
            _template = template;
            SetInitialResourceValues(InventoryResourceType.Water, _template.WaterAvailable);
            SetInitialResourceValues(InventoryResourceType.Food, _template.FoodAvailable);
            SetInitialResourceValues(InventoryResourceType.Fuel, _template.FuelAvailable);
            SetInitialResourceValues(InventoryResourceType.Scrap, _template.ScrapAvailable);
            //TODO different combat scenarios for region tier and animal/human enemies
        }

        public override XmlNode Save(XmlNode doc, PersistenceType type)
        {
            XmlNode regionNode = base.Save(doc, type);
            SaveController.CreateNodeAndAppend("Discovered", regionNode, _discovered);
            XmlNode combatNode = SaveController.CreateNodeAndAppend("Scenario", regionNode);
            _combatScenario?.Save(combatNode, type);
            return regionNode;
        }

        private void SetInitialResourceValues(InventoryResourceType resourceType, float resourceRating)
        {
            InventoryResource resource = GetResource(resourceType);
            float initialQuantity = resourceRating * 10;
            resource.SetMax(initialQuantity);
            IncrementResource(resourceType, initialQuantity);
        }

        public void Enter(Player player)
        {
            if (_combatScenario != null && !_combatScenario.IsFinished())
                CombatManager.EnterCombat(player, _combatScenario);
            else
                player.CollectResourcesInRegion(this);
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

        public CombatScenario GetCombatScenario()
        {
            return _combatScenario;
        }
    }
}