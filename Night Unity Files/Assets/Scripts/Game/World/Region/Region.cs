using System;
using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Characters;
using Game.Combat;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class Region : DesolationInventory
    {
        private string _regionDescription;
        public readonly int Distance;
        private readonly RegionTemplate _template;
        private bool _discovered;
        private readonly CombatScenario _combatScenario;
        public readonly int PerceptionRequirement;
        public readonly Region Origin;
        public readonly List<Region> Connections = new List<Region>();
        private Action<Player> _enterNodeAction;
        public readonly int RegionNumber;
        
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
            AddNodeEnterAction();
        }

        private void AddNodeEnterAction()
        {
            _enterNodeAction = player =>
            {
                Popup p = new Popup("Reached Node " + _template.Type);
                foreach (Region e in Connections)
                {
                    if (e.PerceptionRequirement <= player.BaseAttributes.Perception.CurrentValue())
                    {
                        p.AddButton("Explore " + e._template.Type,
                            () => player.StartExploration(() => e.Discover(player), 1), true, true);
                    }
                    else
                    {
                        p.AddButton(e._template.Type + " (requires perception " + e.PerceptionRequirement + ")");
                    }
                }
                string eventHere = "Look Around";
                if (_combatScenario != null) eventHere = "Fight";
                p.AddButton(eventHere + Name, () => Enter(player), true, true);
            };
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