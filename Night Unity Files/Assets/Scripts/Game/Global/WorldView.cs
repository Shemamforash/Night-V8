using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Global
{
    public class WorldView : Menu
    {
        private static string _environmentString, _temperatureString, _weatherString, _timeString;
        private static TextMeshProUGUI _environmentText;
        private readonly Dictionary<string, TextMeshProUGUI> _resourceText = new Dictionary<string, TextMeshProUGUI>();
        private Selectable _lastSelectedButton;

        public static void SetEnvironmentText(string text)
        {
            _environmentString = text;
        }

        public static void SetTemperatureText(string text)
        {
            _temperatureString = text;
        }

        public static void SetWeatherText(string text)
        {
            _weatherString = text;
        }

        public static void Update(int hours)
        {
            _timeString = TimeToName(hours);
            UpdateDescription();
        }

        private static void UpdateDescription()
        {
            int remainingTemples = WorldState.GetRemainingTemples();
            string templeString;
            switch (remainingTemples)
            {
                case 0:
                    templeString = "No temples remain uncleansed, the gate is open.";
                    break;
                case 1:
                    templeString = "Only one temple remains uncleansed.";
                    break;
                default:
                    templeString = remainingTemples + " temples remain uncleansed";
                    break;
            }

            _environmentText.text = _timeString + " in the " + _environmentString + ". It is " + _temperatureString + " and " + _weatherString + ". " + templeString;
        }

        private static readonly string[] resources = {"Meat", "Water", "Essence", "Ice", "Salt", "Scrap", "Fuel", "Charcoal", "Fruit", "Skin", "Leather", "Metal", "Meteor", "Alloy"};

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
            if (_lastSelectedButton != null) _lastSelectedButton.Select();
        }

        public override void Exit()
        {
            base.Exit();
            _lastSelectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
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


        public void Update()
        {
            foreach (string resourceType in resources)
            {
                int quantity;
                switch (resourceType)
                {
                    case "Meat":
                        quantity = Inventory.Consumables().Count(c => c.Template.ResourceType == "Meat");
                        _resourceText[resourceType].text = "Meat\n" + quantity;
                        continue;
                    case "Water":
                        quantity = Inventory.Consumables().Count(c => c.Template.ResourceType == "Water");
                        _resourceText[resourceType].text = "Water\n" + quantity;
                        continue;
                }

                quantity = Mathf.FloorToInt(Inventory.GetResourceQuantity(resourceType));
                if (quantity == 0 && resourceType != "Food" && resourceType != "Water")
                {
                    _resourceText[resourceType].gameObject.SetActive(false);
                    continue;
                }

                _resourceText[resourceType].gameObject.SetActive(true);
                _resourceText[resourceType].text = resourceType + "\n" + quantity;
            }
        }
    }
}