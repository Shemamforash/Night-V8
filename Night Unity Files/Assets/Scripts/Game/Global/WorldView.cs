using System;
using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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

        private static readonly string[] resources = {"Food", "Water", "Essence", "Ice", "Salt", "Scrap", "Fuel", "Charcoal", "Meat", "Fruit", "Skin", "Leather", "Metal", "Meteor", "Alloy"};

        public override void Awake()
        {
            base.Awake();
            PauseOnOpen = false;
            _environmentText = GameObject.Find("Environment").GetComponent<TextMeshProUGUI>();

            GameObject resourcesObject= GameObject.Find("Resources");
            foreach (string resourceType in resources)
            {
                TextMeshProUGUI resourceText = Helper.FindChildWithName<TextMeshProUGUI>(resourcesObject, resourceType);
                _resourceText.Add(resourceType, resourceText);
            }
        }

        public override void Enter()
        {
            base.Enter();
            Player selectedCharacter = CharacterManager.SelectedCharacter;
            CharacterManager.SelectCharacter(selectedCharacter);
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

        private readonly Dictionary<string, TextMeshProUGUI> _resourceText = new Dictionary<string, TextMeshProUGUI>();
        
        public void Update()
        {
            foreach (string resourceType in resources)
            {
                int quantity = Mathf.FloorToInt(WorldState.HomeInventory().GetResource(resourceType).Quantity());
                if (quantity == 0 && resourceType != "Food" && resourceType != "Water")
                {
                    _resourceText[resourceType].gameObject.SetActive(false);
                    continue;
                }

                _resourceText[resourceType].gameObject.SetActive(true);
                _resourceText[resourceType].text = resourceType + " " + quantity;
            }
        }
    }
}