using Characters;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.Region
{
    public class Region : DesolationInventory

    {
        private string _regionDescription;
        private int _distance;
        private RegionTemplate _template;

        private GameObject _regionObject;
//        private List<Encounter> _enemyEncounters = new List<Encounter>();

        public Region(RegionTemplate template, GameObject regionObject)
        {
            _template = template;
            _regionObject = regionObject;
            _distance = Random.Range(1, 2);
            IncrementResource("Water", _template.WaterAvailable);
            IncrementResource("Food", _template.FoodAvailable);
            IncrementResource("Fuel", _template.FuelAvailable);
            IncrementResource("Scrap", _template.ScrapAvailable);
            IncrementResource("Ammo", _template.AmmoAvailable);
            Helper.FindChildWithName<Text>(regionObject, "Text").text = "New Region";
        }

        public void DestroyGameObject()
        {
            GameObject.Destroy(_regionObject);
        }
        
        public string Name()
        {
            return _template.DisplayName == "" ? _template.InternalName : _template.DisplayName;
        }

        public string Type()
        {
            return _template.Type;
        }

        public void ExtractResources(Character c)
        {
            float maximumCarryingCapacity = c.RemainingCarryCapacity();
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

        public GameObject GetObject()
        {
            return _regionObject;
        }

        public int Distance()
        {
            return _distance;
        }
    }
}