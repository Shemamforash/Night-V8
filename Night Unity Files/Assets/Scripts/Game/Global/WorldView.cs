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
        private static TextMeshProUGUI _environmentText;
        private static string _environmentString, _temperatureString, _weatherString, _timeString;
        private static int stormDistance;

        public static void SetEnvironmentText(string text)
        {
            _environmentString = text;
            UpdateDescription();
        }

        public static void SetTemperatureText(string text)
        {
            _temperatureString = text;
            UpdateDescription();
        }

        public static void SetWeatherText(string text)
        {
            _weatherString = text;
            UpdateDescription();
        }

        public static void SetStormDistance(int distance)
        {
            stormDistance = distance;
            UpdateDescription();
        }

        public static void SetTime(int days, int hours, int minutes)
        {
            _timeString = TimeToName(hours);
//            dayTime += "   Day " + days;
            UpdateDescription();
        }

        private static void UpdateDescription()
        {
            string stormString;
            if (stormDistance < 1) stormString = "here.";
            else if (stormDistance < 3)
                stormString = "looming.";
            else if (stormDistance < 6)
                stormString = "distant.";
            else
                stormString = "a whisper on the breeze.";
            _environmentText.text = _timeString + ". It is " + _temperatureString + " and there is " + _weatherString + " in the " + _environmentString + ". The storm is " + stormString + ".";
        }

        public override void Awake()
        {
            base.Awake();
            PauseOnOpen = false;
            _environmentText = GameObject.Find("Environment").GetComponent<TextMeshProUGUI>();
            _waterText = GameObject.Find(InventoryResourceType.Water.ToString()).GetComponent<TextMeshProUGUI>();
            _foodText = GameObject.Find(InventoryResourceType.Food.ToString()).GetComponent<TextMeshProUGUI>();
            _fuelText = GameObject.Find(InventoryResourceType.Fuel.ToString()).GetComponent<TextMeshProUGUI>();
            _scrapText = GameObject.Find(InventoryResourceType.Scrap.ToString()).GetComponent<TextMeshProUGUI>();
            PreserveLastSelected = true;
            PauseOnOpen = false;
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

        private TextMeshProUGUI _waterText, _foodText, _fuelText, _scrapText;

        public void Update()
        {
            _waterText.text = "<sprite name=\"Water\">" + Mathf.FloorToInt(WorldState.HomeInventory().GetResource(InventoryResourceType.Water).Quantity());
            _foodText.text = "<sprite name=\"Food\">" + Mathf.FloorToInt(WorldState.HomeInventory().GetResource(InventoryResourceType.Food).Quantity());
            _fuelText.text = "<sprite name=\"Fuel\">" + Mathf.FloorToInt(WorldState.HomeInventory().GetResource(InventoryResourceType.Fuel).Quantity());
            _scrapText.text = "<sprite name=\"Scrap\">" + Mathf.FloorToInt(WorldState.HomeInventory().GetResource(InventoryResourceType.Scrap).Quantity());
        }
    }
}