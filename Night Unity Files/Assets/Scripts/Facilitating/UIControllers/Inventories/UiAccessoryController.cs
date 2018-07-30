using System.Collections.Generic;
using Game.Characters;
using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
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
            if (CurrentPlayer.EquippedAccessory != null)
            {
                _nameText.Text(CurrentPlayer.EquippedAccessory.Name);
                _descriptionText.Text(CurrentPlayer.EquippedAccessory.GetSummary());
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

        public override void SelectGearItem(MyGameObject accessory, UiGearMenuController.GearUi gearUi)
        {
            gearUi.SetTypeText("");
            gearUi.SetNameText(accessory.Name);
            gearUi.SetDpsText("");
        }

        public override void Show(Player player)
        {
            base.Show(player);
            ShowAccessoryInfo();
        }

        public override void CompareTo(MyGameObject comparisonItem)
        {
            _compareText.Text(CurrentPlayer.EquippedAccessory != null ? ((Accessory)comparisonItem).GetSummary() : "");
        }

        public override void StopComparing()
        {
            _compareText.Text("");
        }

        public override List<MyGameObject> GetAvailableGear()
        {
            return new List<MyGameObject>(CharacterManager.Accessories);
        }

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            CurrentPlayer.EquipAccessory(CharacterManager.Accessories[selectedGear]);
            Show(CurrentPlayer);
        }
    }
}