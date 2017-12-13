using System.Collections.Generic;
using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine.UI;
using TMPro;
using UIControllers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.World
{
    public class WorldView : Menu
    {
        private static Button _inventoryButton;
        private static TextMeshProUGUI _timeText;
        
        public void Awake()
        {
            _timeText = Helper.FindChildWithName(gameObject, "Time").GetComponent<TextMeshProUGUI>();
            _inventoryButton = Helper.FindChildWithName<Button>(gameObject, "Inventory");
            DefaultSelectable = _inventoryButton;
            PreserveLastSelected = true;
            PauseOnOpen = false;
            _inventoryButton.onClick.AddListener(() =>
            {
                UIInventoryController.ShowInventory(WorldState.HomeInventory(), g =>
                {
                    GearItem item = g as GearItem;
                    if (item != null)
                    {
                        ShowCharacterPopup(item.Name, item);
                    }
                });
            });
        }

        private void ShowCharacterPopup(string name, GearItem gearItem)
        {
            List<MyGameObject> characterGear = new List<MyGameObject>();
            CharacterManager.Characters().ForEach(c => characterGear.Add(new CharacterGearComparison(c, gearItem)));
//            UIInventoryController.ShowInventory(characterGear);
//            UIInventoryController.SetInventoryTitle("Equip " + name);
        }
        
        public static void SetTime(int days, int hours, int minutes)
        {
            string dayTime;
            if (minutes < 10)
            {
                dayTime = hours + ":0" + minutes;
            }
            else
            {
                dayTime = hours + ":" + minutes;
            }
            dayTime += "   Day " + days;
            _timeText.text = dayTime;
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

        public static Button GetInventoryButton() => _inventoryButton;
    }
}