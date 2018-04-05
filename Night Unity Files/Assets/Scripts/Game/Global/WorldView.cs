using System.Collections.Generic;
using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UIControllers;
using UnityEngine;

namespace Game.World
{
    public class WorldView : Menu
    {
        private static EnhancedButton _inventoryButton;
        private static TextMeshProUGUI _timeText;
        private static TextMeshProUGUI _stormDistanceText;
        
        public void Awake()
        {
            PauseOnOpen = false;
            _timeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Time");
            _stormDistanceText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Storm Distance");
            _inventoryButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Inventory");
            DefaultSelectable = _inventoryButton.Button();
            PreserveLastSelected = true;
            PauseOnOpen = false;
            _inventoryButton.AddOnClick(() =>
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
            CharacterManager.Characters.ForEach(c => characterGear.Add(new CharacterGearComparison(c, gearItem)));
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
            dayTime = TimeToName(hours);
            dayTime += "   Day " + days;
            _timeText.text = dayTime;
        }

        private static string TimeToName(int hours)
        {
            if (hours >= 5 && hours <= 7) return "Dawn";
            if (hours > 7 && hours <= 11)return "Morning";
            if (hours > 11 && hours <= 13) return "Noon";
            if (hours > 13 && hours <= 17) return "Afternoon";
            if (hours > 17 && hours < 20) return "Evening";
            return "Night";
        }

        public static void SetStormDistance(int distance)
        {
            _stormDistanceText.text = "Storm is " + distance + " days away";
        }
        
        public void Start()
        {
            SetResourceSuffix(InventoryResourceType.Water, "sips");
            SetResourceSuffix(InventoryResourceType.Food, "meals");
            SetResourceSuffix(InventoryResourceType.Fuel, "dregs");
            WorldState.HomeInventory().GetResource(InventoryResourceType.PistolMag).AddOnUpdate(f => UIAmmoDisplayController.Instance().SetPistolText(((int)f.CurrentValue()).ToString()));
            WorldState.HomeInventory().GetResource(InventoryResourceType.RifleMag).AddOnUpdate(f => UIAmmoDisplayController.Instance().SetRifleText(((int)f.CurrentValue()).ToString()));
            WorldState.HomeInventory().GetResource(InventoryResourceType.ShotgunMag).AddOnUpdate(f => UIAmmoDisplayController.Instance().SetShotgunText(((int)f.CurrentValue()).ToString()));
            WorldState.HomeInventory().GetResource(InventoryResourceType.SmgMag).AddOnUpdate(f => UIAmmoDisplayController.Instance().SetSmgText(((int)f.CurrentValue()).ToString()));
            WorldState.HomeInventory().GetResource(InventoryResourceType.LmgMag).AddOnUpdate(f => UIAmmoDisplayController.Instance().SetLmgText(((int)f.CurrentValue()).ToString()));
            SetResourceSuffix(InventoryResourceType.Scrap, "bits");
        }

        private static void SetResourceSuffix(InventoryResourceType name, string convention)
        {
            TextMeshProUGUI resourceText = GameObject.Find(name.ToString()).GetComponent<TextMeshProUGUI>();
            WorldState.HomeInventory().GetResource(name).AddOnUpdate(f => { resourceText.text = "<sprite name=\"" + name + "\">" + Mathf.Round(f.CurrentValue()) + " " + convention; });
        }

        public static EnhancedButton GetInventoryButton() => _inventoryButton;
    }
}