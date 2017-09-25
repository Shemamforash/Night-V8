using Game.Characters;
using Game.Combat;
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

//        private List<Encounter> _enemyEncounters = new List<Encounter>();

        public Region(string name, RegionTemplate template) : base(name)
        {
            _template = template;
            _distance = Random.Range(1, 2);
            IncrementResource("Water", _template.WaterAvailable);
            IncrementResource("Food", _template.FoodAvailable);
            IncrementResource("Fuel", _template.FuelAvailable);
            IncrementResource("Scrap", _template.ScrapAvailable);
            IncrementResource("Ammo", _template.AmmoAvailable);
        }

        public string Type()
        {
            return _template.Type;
        }

        public void ExtractResources(DesolationCharacter c)
        {
            float maximumCarryingCapacity = c.Attributes.RemainingCarryCapacity();
        }

        public override BaseInventoryUi CreateUi(Transform parent)
        {
            return new RegionUi(this, parent);
        }
        
        public string Description()
        {
            string description = "";
            description += "Water: " + GetAmountRemainingDescripter(GetResource("Water").Quantity());
            description += "\nFood: " + GetAmountRemainingDescripter(GetResource("Food").Quantity());
            description += "\nFuel: " + GetAmountRemainingDescripter(GetResource("Fuel").Quantity());
            description += "\nScrap: " + GetAmountRemainingDescripter(GetResource("Scrap").Quantity());
            description += "\nAmmo: " + GetAmountRemainingDescripter(GetResource("Ammo").Quantity());
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