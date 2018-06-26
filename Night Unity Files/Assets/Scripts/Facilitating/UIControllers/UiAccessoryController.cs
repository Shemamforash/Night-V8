using System.Collections.Generic;
using Game.Characters;
using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UiAccessoryController : UiGearMenuTemplate
    {
        private EnhancedButton _accessoryButton;
        private EnhancedText _nameText, _descriptionText, _compareText;
        private bool _upgradingAllowed;

        public void Awake()
        {
            _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
            _descriptionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Description");
            _compareText = Helper.FindChildWithName<EnhancedText>(gameObject, "Compare");

            _accessoryButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Info");
            _accessoryButton.AddOnClick(() =>
            {
                if (GearIsAvailable()) UiGearMenuController.Instance().EnableInput();
            });
        }

        public override Button GetGearButton()
        {
            return _accessoryButton.Button();
        }

        private void ShowAccessoryInfo()
        {
            if (CurrentPlayer.Accessory != null)
            {
                _nameText.Text(CurrentPlayer.Accessory.Name);
                _descriptionText.Text(CurrentPlayer.Accessory.GetSummary());
            }
            else
            {
                _nameText.Text("");
                _descriptionText.Text("No Accessory Equipped");
                _accessoryButton.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
            }

            _compareText.Text("");
        }

        public override bool GearIsAvailable()
        {
            return CharacterManager.Accessories.Count != 0;
        }

        public override void SelectGearItem(GearItem item, UiGearMenuController.GearUi gearUi)
        {
            Accessory accessory = item as Accessory;
            gearUi.SetTypeText("");
            gearUi.SetNameText(accessory.Name);
            gearUi.SetDpsText("");
        }

        public override void Show(Player player)
        {
            base.Show(player);
            ShowAccessoryInfo();
        }

        public override void CompareTo(GearItem comparisonItem)
        {
            _compareText.Text(CurrentPlayer.Accessory != null ? comparisonItem.GetSummary() : "");
        }

        public override void StopComparing()
        {
            _compareText.Text("");
        }

        public override List<GearItem> GetAvailableGear()
        {
            return new List<GearItem>(CharacterManager.Accessories);
        }

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            CurrentPlayer.EquipAccessory(CharacterManager.Accessories[selectedGear]);
            Show(CurrentPlayer);
        }
    }
}