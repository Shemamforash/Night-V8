using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UiAccessoryController : UiGearMenuTemplate
    {
        private EnhancedButton _swapButton;
        private EnhancedText _nameText, _descriptionText;
        private bool _upgradingAllowed;

        public void Awake()
        {
            GameObject info = gameObject.FindChildWithName("Info");
            _nameText = info.FindChildWithName<EnhancedText>("Name");
            _descriptionText = info.FindChildWithName<EnhancedText>("Description");

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
                _nameText.SetText(CharacterManager.SelectedCharacter.EquippedAccessory.Name);
                _descriptionText.SetText(CharacterManager.SelectedCharacter.EquippedAccessory.GetSummary());
            }
            else
            {
                _nameText.SetText("");
                _descriptionText.SetText("No Accessory Equipped");
                _swapButton.SetDownNavigation(UiGearMenuController.GetCloseButton());
            }
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
        }

        public override void StopComparing()
        {
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