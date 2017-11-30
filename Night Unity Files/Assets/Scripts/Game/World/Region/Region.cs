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
        private CombatScenario _combatScenario;

//        private List<Encounter> _enemyEncounters = new List<Encounter>();

        public Region(string name, RegionTemplate template) : base(name)
        {
            _template = template;
            _distance = Random.Range(1, 2);
            SetInitialResourceValues(InventoryResourceType.Water, _template.WaterAvailable);
            SetInitialResourceValues(InventoryResourceType.Food, _template.FoodAvailable);
            SetInitialResourceValues(InventoryResourceType.Fuel, _template.FuelAvailable);
            SetInitialResourceValues(InventoryResourceType.Scrap, _template.ScrapAvailable);
            SetInitialResourceValues(InventoryResourceType.Ammo, _template.AmmoAvailable);
        }

        private void SetInitialResourceValues(InventoryResourceType resourceType, float resourceRating)
        {
            InventoryResource resource = GetResource(resourceType);
            float initialQuantity = resourceRating * 10;
            resource.SetMax(initialQuantity);
            IncrementResource(resourceType, initialQuantity);
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
            ui.PrimaryButton.AddOnSelectEvent(() => RegionManager.UpdateRegionInfo(this));
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

        public int Distance()
        {
            return _distance;
        }

        public void GenerateCombatScenario()
        {
            _combatScenario = CombatScenario.Generate(Random.Range(1, 3));
        }

        public CombatScenario GetCombatScenario()
        {
            return _combatScenario;
        }
    }
}