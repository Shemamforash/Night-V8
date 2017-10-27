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
        private readonly int _distance;
        private readonly RegionTemplate _template;
        private bool _discovered;

//        private List<Encounter> _enemyEncounters = new List<Encounter>();

        public Region(string name, RegionTemplate template) : base(name)
        {
            _template = template;
            _distance = Random.Range(1, 2);
            IncrementResource(InventoryResourceType.Water, _template.WaterAvailable);
            IncrementResource(InventoryResourceType.Food, _template.FoodAvailable);
            IncrementResource(InventoryResourceType.Fuel, _template.FuelAvailable);
            IncrementResource(InventoryResourceType.Scrap, _template.ScrapAvailable);
            IncrementResource(InventoryResourceType.Ammo, _template.AmmoAvailable);
        }

        public string RegionType()
        {
            return _template.Type;
        }

        public void ExtractResources(Player playerCharacter)
        {
            float maximumCarryingCapacity = playerCharacter.BaseAttributes.RemainingCarryCapacity();
        }

        public override ViewParent CreateUi(Transform parent)
        {
            InventoryUi ui = new InventoryUi(this, parent);
            ui.OnEnter(() => RegionManager.UpdateRegionInfo(this));
            ui.SetCentralTextCallback(() => Name);
            ui.SetLeftTextCallback(RegionType);
            ui.SetLeftTextWidth(200);
            ui.SetRightTextCallback(() => Distance() + " hrs");
            return ui;
        }

        public bool Discover() => _discovered = true;
        public bool Discovered() => _discovered;
        
        public string Description()
        {
            string description = "";
            description += "Water: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Water).Quantity());
            description += "\nFood: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Food).Quantity());
            description += "\nFuel: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Fuel).Quantity());
            description += "\nScrap: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Scrap).Quantity());
            description += "\nAmmo: " + GetAmountRemainingDescripter(GetResource(InventoryResourceType.Ammo).Quantity());
            description += "\nEncounters: " + _template.Encounters;
            description += "\nPossible items: " + _template.Items;
            return description;
        }

        private static string GetAmountRemainingDescripter(float amount)
        {
            if (amount == 0)
            {
                return "Barren";
            }
            if (amount < 10)
            {
                return "Scarce";
            }
            if (amount < 25)
            {
                return "Some";
            }
            if (amount < 100)
            {
                return "Plentiful";
            }
            return "Bounteous";
        }

        public int Distance()
        {
            return _distance;
        }

        public CombatScenario GetCombatScenario()
        {
            //TODO combat scenarios
            return null;
        }
    }
}