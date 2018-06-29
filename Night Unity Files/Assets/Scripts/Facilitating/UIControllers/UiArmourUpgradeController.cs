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
    public class UiArmourUpgradeController : UiGearMenuTemplate
    {
        private static ArmourPlateUi _plateOneUi;
        private static ArmourPlateUi _plateTwoUi;

        private ArmourPlate _currentSelectedPlate;
        private ArmourPlateUi _selectedPlateUi;
        private bool _upgradingAllowed;

        public void Awake()
        {
            _plateOneUi = new ArmourPlateUi(Helper.FindChildWithName(gameObject, "Plate 1"));
            _plateOneUi.Button.AddOnSelectEvent(() => SelectPlateUi(_plateOneUi));
            _plateOneUi.Button.AddOnClick(() =>
            {
                if (GearIsAvailable()) UiGearMenuController.Instance().EnableInput();
            });
            _plateTwoUi = new ArmourPlateUi(Helper.FindChildWithName(gameObject, "Plate 2"));
            _plateTwoUi.Button.AddOnSelectEvent(() => SelectPlateUi(_plateTwoUi));
            _plateTwoUi.Button.AddOnClick(() =>
            {
                if (GearIsAvailable()) UiGearMenuController.Instance().EnableInput();
            });
        }

        private void SelectPlateUi(ArmourPlateUi plateUi)
        {
            _selectedPlateUi = plateUi;
            bool isPlateOne = plateUi == _plateOneUi;
            _plateOneUi.SetSelected(isPlateOne);
            _plateTwoUi.SetSelected(!isPlateOne);
            UpdatePlates();
        }

        private void UpdatePlates()
        {
            _plateOneUi.SetPlate(CurrentPlayer.ArmourController.GetPlateOne());
            _plateTwoUi.SetPlate(CurrentPlayer.ArmourController.GetPlateTwo());
        }

        public override bool GearIsAvailable()
        {
            return CharacterManager.Armour.Count != 0;
        }

        public override void SelectGearItem(InventoryItem item, UiGearMenuController.GearUi gearUi)
        {
            ArmourPlate plate = item as ArmourPlate;
            gearUi.SetTypeText(plate.Protection + " Armour");
            gearUi.SetNameText(plate.Name);
            gearUi.SetDpsText("");
        }

        public override void Show(Player player)
        {
            base.Show(player);
            SelectPlateUi(_plateOneUi);
            UpdatePlates();
        }

        public override void CompareTo(InventoryItem comparisonItem)
        {
        }

        public override void StopComparing()
        {
        }

        public override List<InventoryItem> GetAvailableGear()
        {
            return new List<InventoryItem>(CharacterManager.Armour);
        }

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            ArmourPlate plate = CharacterManager.Armour[selectedGear];
            if (_selectedPlateUi == _plateOneUi)
                CurrentPlayer.EquipArmourSlotOne(plate);
            else
                CurrentPlayer.EquipArmourSlotTwo(plate);

            Show(CurrentPlayer);
        }

        public override Button GetGearButton()
        {
            return _selectedPlateUi.Button.Button();
        }

        private class ArmourPlateUi
        {
            private readonly EnhancedText _currentArmourText, _armourDetailText;
            private readonly EnhancedText _nameText;
            private readonly Image _selectedImage;
            public readonly EnhancedButton Button;

            public ArmourPlateUi(GameObject gameObject)
            {
                _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
                _currentArmourText = Helper.FindChildWithName<EnhancedText>(gameObject, "Current Armour");
                _armourDetailText = Helper.FindChildWithName<EnhancedText>(gameObject, "Armour Detail");
                Button = Helper.FindChildWithName<EnhancedButton>(gameObject, "Info");
            }

            public void SetPlate(ArmourPlate plate)
            {
                Button.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
                if (plate == null)
                {
                    _nameText.Text("");
                    _currentArmourText.Text("No Plate");
                    _armourDetailText.Text("");
                    return;
                }

                _nameText.Text(plate.Name);
                _currentArmourText.Text(plate.Protection + " Armour");
                _armourDetailText.Text("TODO");
            }

            public void SetSelected(bool selected)
            {
                if (selected) Button.Select();
            }
        }
    }
}