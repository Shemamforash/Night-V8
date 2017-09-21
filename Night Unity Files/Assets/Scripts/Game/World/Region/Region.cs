using System.Security;
using Characters;
using Game.Characters;
using Game.Combat;
using SamsHelper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Character = SamsHelper.BaseGameFunctionality.Characters.Character;

namespace Game.World.Region
{
    public class Region : DesolationInventory

    {
        private string _regionDescription;
        private int _distance;
        private RegionTemplate _template;
        private string _name;

        private GameObject _regionObject;
//        private List<Encounter> _enemyEncounters = new List<Encounter>();

        public Region(string name, RegionTemplate template, GameObject regionObject) : base(name)
        {
            _name = name;
            _template = template;
            _regionObject = regionObject;
            _distance = Random.Range(1, 2);
            IncrementResource("Water", _template.WaterAvailable);
            IncrementResource("Food", _template.FoodAvailable);
            IncrementResource("Fuel", _template.FuelAvailable);
            IncrementResource("Scrap", _template.ScrapAvailable);
            IncrementResource("Ammo", _template.AmmoAvailable);
            Helper.FindChildWithName<TextMeshProUGUI>(regionObject, "Text").text = "New Region";
        }

        public void DestroyGameObject()
        {
            GameObject.Destroy(_regionObject);
        }
        
        public string Name()
        {
            return _name;
        }

        public string Type()
        {
            return _template.Type;
        }

        public void ExtractResources(DesolationCharacter c)
        {
            float maximumCarryingCapacity = c.Attributes.RemainingCarryCapacity();
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

        public CombatScenario GetCombatScenario()
        {
            //TODO combat scenarios
            return null;
        }
    }
}