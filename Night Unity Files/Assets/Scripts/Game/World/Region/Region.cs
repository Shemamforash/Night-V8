using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class Region : DesolationInventory
    {
        public readonly int Distance;
        private readonly RegionTemplate _template;
        private bool _discovered;
        private readonly CombatScenario _combatScenario;
        public readonly int PerceptionRequirement;
        public readonly Region Origin;
        public readonly List<Region> Connections = new List<Region>();
        private Action<Player> _enterNodeAction;
        public readonly int RegionNumber;

        public override XmlNode Save(XmlNode doc, PersistenceType type)
        {
            XmlNode regionNode = base.Save(doc, type);
            SaveController.CreateNodeAndAppend("Distance", regionNode, Distance);
            SaveController.CreateNodeAndAppend("Discovered", regionNode, _discovered);
            SaveController.CreateNodeAndAppend("PerceptionReq", regionNode, PerceptionRequirement);
            SaveController.CreateNodeAndAppend("Origin", regionNode, Origin?.RegionNumber.ToString() ?? "None");
            SaveController.CreateNodeAndAppend("ID", regionNode, RegionNumber);
            SaveController.CreateNodeAndAppend("Type", regionNode, _template.Type);
            XmlNode connectionNode = SaveController.CreateNodeAndAppend("Connections",regionNode);
            foreach (Region region in Connections)
            {
                SaveController.CreateNodeAndAppend("RegionID", connectionNode, region.RegionNumber);
            }
            XmlNode combatNode = SaveController.CreateNodeAndAppend("Scenario", regionNode);
            _combatScenario?.Save(combatNode, type);
            return regionNode;
        }
        
        public Region(string name, Region origin, int regionNumber, RegionTemplate template) : base(name)
        {
            _template = template;
            Distance = Random.Range(1, 2);
            if (origin != null) Distance += origin.Distance;
            Origin = origin;
            RegionNumber = regionNumber;
            if (origin != null)
            {
                int randomVal = (int) (6 - Mathf.Sqrt(Random.Range(0f, 37f)));
                if (randomVal == 0 && origin.PerceptionRequirement == 0) randomVal = (int) (5 - Mathf.Sqrt(Random.Range(0f, 26f)));
                PerceptionRequirement = origin.PerceptionRequirement + randomVal;
            }
            else PerceptionRequirement = 0;
            SetInitialResourceValues(InventoryResourceType.Water, _template.WaterAvailable);
            SetInitialResourceValues(InventoryResourceType.Food, _template.FoodAvailable);
            SetInitialResourceValues(InventoryResourceType.Fuel, _template.FuelAvailable);
            SetInitialResourceValues(InventoryResourceType.Scrap, _template.ScrapAvailable);
            //TODO different combat scenarios for region tier and animal/human enemies
            if (template.Type == RegionType.Human)
            {
                _combatScenario = CombatScenario.Generate(Random.Range(1, 3));
            }
            _enterNodeAction = player => UIExploreMenuController.Instance().SetRegion(this, player);
        }

        public void AddConnection(Region region)
        {
            Connections.Add(region);
        }

        private void SetInitialResourceValues(InventoryResourceType resourceType, float resourceRating)
        {
            InventoryResource resource = GetResource(resourceType);
            float initialQuantity = resourceRating * 10;
            resource.SetMax(initialQuantity);
            IncrementResource(resourceType, initialQuantity);
        }

        public RegionType GetRegionType()
        {
            return _template.Type;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            InventoryUi ui = new InventoryUi(this, parent);
            ui.PrimaryButton.AddOnSelectEvent(() => RegionManager.UpdateRegionInfo(this));
            ui.SetCentralTextCallback(() => Name);
            ui.SetLeftTextCallback(() => GetRegionType().ToString());
            ui.SetLeftTextWidth(200);
            ui.SetRightTextCallback(() => Distance + " hrs");
            return ui;
        }

        public void Enter(Player player)
        {
            if (!_combatScenario.IsFinished())
            {
                CombatManager.EnterCombat(player, _combatScenario);
            }
            else
            {
                player.CollectResourcesInRegion(this);
            }
        }

        public void Discover(Player player)
        {
            if (_discovered) return;
            _discovered = true;
            RegionManager.DiscoverRegion(this);
            _enterNodeAction(player);
            if (Connections.Count != 0) return;
            Origin?.Connections.Remove(this);
        }

        public bool Discovered() => _discovered;

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
            for (int i = 0; i < amount; i += 10)
            {
                amountRemaining += "+";
            }
            return amountRemaining;
        }

        public CombatScenario GetCombatScenario()
        {
            return _combatScenario;
        }
    }
}