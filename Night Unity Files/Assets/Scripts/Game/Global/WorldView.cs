using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

namespace Game.Global
{
    public class WorldView : Menu
    {
        private static EnhancedButton _inventoryButton;
        private static TextMeshProUGUI _timeText;
        private static TextMeshProUGUI _stormDistanceText;
        private static TextMeshProUGUI _weatherText;
        private static TextMeshProUGUI _environmentText, _temperatureText;

        public static void SetEnvironmentText(string text)
        {
            _environmentText.text = text;
        }
        
        public static void SetTemperatureText(string text)
        {
            _temperatureText.text = text;
        }
        
        public static void SetWeatherText(string text)
        {
            _weatherText.text = text;
        }
        
        public override void Awake()
        {
            base.Awake();
            PauseOnOpen = false;
            _weatherText = GameObject.Find("Weather").GetComponent<TextMeshProUGUI>();
            _environmentText = GameObject.Find("Environment").GetComponent<TextMeshProUGUI>();
            _temperatureText = GameObject.Find("Temperature").GetComponent<TextMeshProUGUI>();
            _timeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Time");
            _stormDistanceText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Storm Distance");
            _inventoryButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Inventory");
            _waterText = GameObject.Find(InventoryResourceType.Water.ToString()).GetComponent<TextMeshProUGUI>();
            _foodText = GameObject.Find(InventoryResourceType.Food.ToString()).GetComponent<TextMeshProUGUI>();
            _fuelText = GameObject.Find(InventoryResourceType.Fuel.ToString()).GetComponent<TextMeshProUGUI>();
            _scrapText = GameObject.Find(InventoryResourceType.Scrap.ToString()).GetComponent<TextMeshProUGUI>();
            DefaultSelectable = _inventoryButton.Button();
            PreserveLastSelected = true;
            PauseOnOpen = false;
            _inventoryButton.AddOnClick(() =>
            {
                UIInventoryController.ShowInventory(WorldState.HomeInventory(), g =>
                {
                    GearItem item = g as GearItem;
                    if (item != null) ShowCharacterPopup(item.Name, item);
                });
            });
        }

        private void ShowCharacterPopup(string name, GearItem gearItem)
        {
            List<MyGameObject> characterGear = new List<MyGameObject>();
            CharacterManager.Characters.ForEach(c => characterGear.Add(new CharacterGearComparison(c, gearItem)));
//            UIInventoryController.ShowInventory(characterGear);
//            UIInventoryController.SetInventoryTitle("Equip " + name);
        }

        public static void SetTime(int days, int hours, int minutes)
        {
            string dayTime;
            if (minutes < 10)
                dayTime = hours + ":0" + minutes;
            else
                dayTime = hours + ":" + minutes;
            dayTime = TimeToName(hours);
            dayTime += "   Day " + days;
            _timeText.text = dayTime;
        }

        private static string TimeToName(int hours)
        {
            if (hours >= 5 && hours <= 7) return "Dawn";
            if (hours > 7 && hours <= 11) return "Morning";
            if (hours > 11 && hours <= 13) return "Noon";
            if (hours > 13 && hours <= 17) return "Afternoon";
            if (hours > 17 && hours < 20) return "Evening";
            return "Night";
        }

        public static void SetStormDistance(int distance)
        {
            _stormDistanceText.text = "Storm is " + distance + " days away";
        }

        private TextMeshProUGUI _waterText, _foodText, _fuelText, _scrapText;

        public void Update()
        {
            _waterText.text = "<sprite name=\"Water\">" + Mathf.FloorToInt(WorldState.HomeInventory().GetResource(InventoryResourceType.Water).Quantity()) + " sips";
            _foodText.text = "<sprite name=\"Food\">" + Mathf.FloorToInt(WorldState.HomeInventory().GetResource(InventoryResourceType.Food).Quantity()) +"meals";
            _fuelText.text = "<sprite name=\"Fuel\">" + Mathf.FloorToInt(WorldState.HomeInventory().GetResource(InventoryResourceType.Fuel).Quantity()) +"dregs";
            _scrapText.text = "<sprite name=\"Scrap\">" + Mathf.FloorToInt(WorldState.HomeInventory().GetResource(InventoryResourceType.Scrap).Quantity()) +"bits";
        }
        
        public static EnhancedButton GetInventoryButton()
        {
            return _inventoryButton;
        }
    }
}