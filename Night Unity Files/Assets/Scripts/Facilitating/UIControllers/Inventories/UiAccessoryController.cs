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
        private EnhancedButton _swapButton;
        private EnhancedText _nameText, _descriptionText, _compareText;
        private bool _upgradingAllowed;

        public void Awake()
        {
            _nameText = gameObject.FindChildWithName<EnhancedText>("Name");
            _descriptionText = gameObject.FindChildWithName<EnhancedText>("Description");
            _compareText = gameObject.FindChildWithName<EnhancedText>("Compare");

            _swapButton = gameObject.FindChildWithName<EnhancedButton>("Swap");
            _swapButton.AddOnClick(() =>
            {
                if (GearIsAvailable()) UiGearMenuController.EnableInput();
            });
        }

        public override Button GetGearButton()
        {
            return _swapButton.Button();
        }

        private void ShowAccessoryInfo()
        {
            if (CharacterManager.SelectedCharacter.EquippedAccessory != null)
            {
                _nameText.Text(CharacterManager.SelectedCharacter.EquippedAccessory.Name);
                _descriptionText.Text(CharacterManager.SelectedCharacter.EquippedAccessory.GetSummary());
            }
            else
            {
                _nameText.Text("");
                _descriptionText.Text("No Accessory Equipped");
                _swapButton.SetDownNavigation(UiGearMenuController.GetCloseButton());
            }

            _compareText.Text("");
        }

        public override bool GearIsAvailable()
        {
            return UiGearMenuController.Inventory().Accessories.Count != 0;
        }

        public override void SelectGearItem(MyGameObject accessory, UiGearMenuController.GearUi gearUi)
        {
            gearUi.SetTypeText("");
            gearUi.SetNameText(accessory.Name);
            gearUi.SetDpsText("");
        }

        public override void Show()
        {
            base.Show();
            ShowAccessoryInfo();
            _swapButton.Select();
        }

        public override void CompareTo(MyGameObject comparisonItem)
        {
            _compareText.Text(CharacterManager.SelectedCharacter.EquippedAccessory != null ? ((Accessory)comparisonItem).GetSummary() : "");
        }

        public override void StopComparing()
        {
            _compareText.Text("");
        }

        public override List<MyGameObject> GetAvailableGear()
        {
            return new List<MyGameObject>(UiGearMenuController.Inventory().Accessories);
        }

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            CharacterManager.SelectedCharacter.EquipAccessory(UiGearMenuController.Inventory().Accessories[selectedGear]);
            Show();
        }
    }
}