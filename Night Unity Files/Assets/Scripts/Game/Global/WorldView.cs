﻿using System.Collections.Generic;
using Game.Characters;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

namespace Game.Global
{
    public class WorldView : Menu
    {
        private static TextMeshProUGUI _environmentText;
        private static string _environmentString, _temperatureString, _weatherString, _timeString;

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

        public static void SetTime(int hours)
        {
            _timeString = TimeToName(hours);
            UpdateDescription();
        }

        private static void UpdateDescription()
        {
            _environmentText.text = _timeString + ". It is " + _temperatureString + " and " + _weatherString + " in the " + _environmentString + ".";
        }

        private static readonly string[] resources = {"Water", "Essence", "Ice", "Salt", "Scrap", "Fuel", "Charcoal", "Fruit", "Skin", "Leather", "Metal", "Meteor", "Alloy"};

        public override void Awake()
        {
            base.Awake();
            PauseOnOpen = false;
            _environmentText = gameObject.FindChildWithName<TextMeshProUGUI>("Environment");

            GameObject resourcesObject = GameObject.Find("Resources");
            foreach (string resourceType in resources)
            {
                TextMeshProUGUI resourceText = resourcesObject.FindChildWithName<TextMeshProUGUI>(resourceType);
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
                int quantity;
                switch (resourceType)
                {
                    case "Food":
                        quantity = 0;
                        WorldState.HomeInventory().Consumables().ForEach(c => quantity += c.HungerModifier);
                        _resourceText[resourceType].text = "Food " + quantity;
                        continue;
                    case "Water":
                        quantity = 0;
                        WorldState.HomeInventory().Consumables().ForEach(c => quantity += c.ThirstModifier);
                        _resourceText[resourceType].text = "Water " + quantity;
                        continue;
                }

                quantity = Mathf.FloorToInt(WorldState.HomeInventory().GetResourceQuantity(resourceType));
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