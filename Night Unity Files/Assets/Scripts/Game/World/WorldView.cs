using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine.UI;
using TMPro;
using UIControllers;
using UnityEngine;

namespace Game.World
{
    public class WorldView : Menu
    {
        private static Button _inventoryButton;
        private static TextMeshProUGUI _timeText, _dayText;

        public void Awake()
        {
            _timeText = Helper.FindChildWithName(gameObject, "Time").GetComponent<TextMeshProUGUI>();
            _dayText = Helper.FindChildWithName(gameObject, "Day").GetComponent<TextMeshProUGUI>();
            _inventoryButton = Helper.FindChildWithName<Button>(gameObject, "Inventory");
            DefaultSelectable = _inventoryButton;
            PreserveLastSelected = true;
            PauseOnOpen = false;
            _inventoryButton.onClick.AddListener(() =>
            {
                UIInventoryController.ShowInventory("Vehicle Inventory", WorldState.HomeInventory().SortByType(), g =>
                {
                    GearItem item = g as GearItem;
                    if (item != null)
                    {
                        ShowCharacterPopup(item.Name, item);
                    }
                });
            });
        }

        public static void SetTime(int hours, int minutes)
        {
            if (minutes < 10)
            {
                _timeText.text = hours + ":0" + minutes;
                return;
            }
            _timeText.text = hours + ":" + minutes;
        }

        public static void SetDay(int days)
        {
            _dayText.text = "Day " + days;
        }

        public void Start()
        {
            SetResourceSuffix(InventoryResourceType.Water, "sips");
            SetResourceSuffix(InventoryResourceType.Food, "meals");
            SetResourceSuffix(InventoryResourceType.Fuel, "dregs");
            SetResourceSuffix(InventoryResourceType.Ammo, "rounds");
            SetResourceSuffix(InventoryResourceType.Scrap, "bits");
        }

        private static void SetResourceSuffix(InventoryResourceType name, string convention)
        {
            TextMeshProUGUI resourceText = GameObject.Find(name.ToString()).GetComponent<TextMeshProUGUI>();
            WorldState.HomeInventory().GetResource(name).AddOnUpdate(f => { resourceText.text = "<sprite name=\"" + name + "\">" + Mathf.Round(f.CurrentValue()) + " " + convention; });
        }

        private void ShowCharacterPopup(string name, GearItem gearItem)
        {
            List<MyGameObject> characterGear = new List<MyGameObject>();
            CharacterManager.Characters().ForEach(c => characterGear.Add(new CharacterGearComparison(c, gearItem)));
            UIInventoryController.ShowInventory("Equip " + name, characterGear);
        }

        public static Button GetInventoryButton() => _inventoryButton;
    }
}