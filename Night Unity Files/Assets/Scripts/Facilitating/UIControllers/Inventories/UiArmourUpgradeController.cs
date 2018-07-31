using System.Collections.Generic;
using Game.Characters;
using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.Basic;
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
            _plateOneUi = new ArmourPlateUi(gameObject.FindChildWithName("Plate 1"));
            _plateOneUi.Button.AddOnSelectEvent(() => SelectPlateUi(_plateOneUi));
            _plateOneUi.Button.AddOnClick(() =>
            {
                if (GearIsAvailable()) UiGearMenuController.EnableInput();
            });
            _plateTwoUi = new ArmourPlateUi(gameObject.FindChildWithName("Plate 2"));
            _plateTwoUi.Button.AddOnSelectEvent(() => SelectPlateUi(_plateTwoUi));
            _plateTwoUi.Button.AddOnClick(() =>
            {
                if (GearIsAvailable()) UiGearMenuController.EnableInput();
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
            _plateOneUi.SetPlate(CharacterManager.SelectedCharacter.ArmourController.GetPlateOne());
            _plateTwoUi.SetPlate(CharacterManager.SelectedCharacter.ArmourController.GetPlateTwo());
        }

        public override bool GearIsAvailable()
        {
            return UiGearMenuController.Inventory().Armour.Count != 0;
        }

        public override void SelectGearItem(MyGameObject item, UiGearMenuController.GearUi gearUi)
        {
            ArmourPlate plate = (ArmourPlate)item;
            gearUi.SetTypeText(plate.Protection + " Armour");
            gearUi.SetNameText(plate.Name);
            gearUi.SetDpsText("");
        }

        public override void Show()
        {
            base.Show();
            SelectPlateUi(_plateOneUi);
            UpdatePlates();
        }

        public override void CompareTo(MyGameObject comparisonItem)
        {
        }

        public override void StopComparing()
        {
        }

        public override List<MyGameObject> GetAvailableGear()
        {
            return new List<MyGameObject>(UiGearMenuController.Inventory().Armour);
        }

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            ArmourPlate plate = UiGearMenuController.Inventory().Armour[selectedGear];
            if (_selectedPlateUi == _plateOneUi)
                CharacterManager.SelectedCharacter.EquipArmourSlotOne(plate);
            else
                CharacterManager.SelectedCharacter.EquipArmourSlotTwo(plate);

            Show();
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
                _nameText = gameObject.FindChildWithName<EnhancedText>("Name");
                _currentArmourText = gameObject.FindChildWithName<EnhancedText>("Current Armour");
                _armourDetailText = gameObject.FindChildWithName<EnhancedText>("Armour Detail");
                Button = gameObject.FindChildWithName<EnhancedButton>("Swap");
            }

            public void SetPlate(ArmourPlate plate)
            {
                Button.SetDownNavigation(UiGearMenuController.GetCloseButton());
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