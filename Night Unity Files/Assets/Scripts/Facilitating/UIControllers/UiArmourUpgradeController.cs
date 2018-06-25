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

        private void UpdatePlateUi(ArmourPlate plate, ArmourPlateUi plateUi)
        {
            if (plate == null)
                plateUi.SetNoPlateInserted();
            else
                plateUi.SetPlateInserted(plate);
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

        public override void SelectGearItem(GearItem item, UiGearMenuController.GearUi gearUi)
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

        public override void CompareTo(GearItem comparisonItem)
        {
        }

        public override void StopComparing()
        {
        }

        public override List<GearItem> GetAvailableGear()
        {
            return new List<GearItem>(CharacterManager.Armour);
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

        private static void UpdateNavigation()
        {
            bool repairOneActive = _plateOneUi._repairButton.Button().interactable;
            bool repairTwoActive = _plateTwoUi._repairButton.Button().interactable;
            bool inscribeOneActive = _plateOneUi._inscribeButton.Button().interactable;
            bool inscribeTwoActive = _plateTwoUi._inscribeButton.Button().interactable;

            if (repairOneActive)
                if (repairTwoActive)
                    _plateOneUi._repairButton.SetRightNavigation(_plateTwoUi._repairButton);
                else if (inscribeTwoActive)
                    _plateOneUi._repairButton.SetRightNavigation(_plateTwoUi._inscribeButton, false);
                else
                    _plateOneUi._repairButton.SetRightNavigation(_plateTwoUi.Button, false);

            if (inscribeOneActive)
                if (inscribeTwoActive)
                    _plateOneUi._inscribeButton.SetRightNavigation(_plateTwoUi._inscribeButton);
                else
                    _plateOneUi._inscribeButton.SetRightNavigation(_plateTwoUi.Button, false);

            if (repairTwoActive)
                if (repairOneActive)
                    _plateTwoUi._repairButton.SetLeftNavigation(_plateOneUi._repairButton);
                else if (inscribeOneActive)
                    _plateTwoUi._repairButton.SetLeftNavigation(_plateOneUi._inscribeButton, false);
                else
                    _plateTwoUi._repairButton.SetLeftNavigation(_plateOneUi.Button, false);

            if (inscribeTwoActive)
                if (inscribeOneActive)
                    _plateTwoUi._inscribeButton.SetLeftNavigation(_plateOneUi._inscribeButton);
                else
                    _plateTwoUi._inscribeButton.SetLeftNavigation(_plateOneUi.Button, false);
        }

        private class ArmourPlateUi
        {
            private readonly EnhancedText _currentArmourText, _armourDetailText;
            public readonly EnhancedButton _inscribeButton, _repairButton;
            private readonly EnhancedText _inscriptionText;
            private readonly EnhancedText _nameText;
            private readonly Image _selectedImage;
            public readonly EnhancedButton Button;

            public ArmourPlateUi(GameObject gameObject)
            {
                _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
                _inscriptionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Inscription");
                _currentArmourText = Helper.FindChildWithName<EnhancedText>(gameObject, "Current Armour");
                _armourDetailText = Helper.FindChildWithName<EnhancedText>(gameObject, "Armour Detail");
                _inscribeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Inscribe");
                _repairButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Repair");
                Button = Helper.FindChildWithName<EnhancedButton>(gameObject, "Info");
            }

            public void SetPlate(ArmourPlate plate)
            {
                if (plate == null)
                {
                    _nameText.Text("");
                    _currentArmourText.Text("No Plate");
                    _armourDetailText.Text("");
                    _inscriptionText.Text("");
                    SetNoPlateInserted();
                    return;
                }

                _nameText.Text(plate.Name);
                _currentArmourText.Text(plate.Protection + " Armour");
                _armourDetailText.Text("TODO");
                SetPlateInserted(plate);
            }

            public void SetSelected(bool selected)
            {
                if (selected) Button.Select();
            }

            public void SetNoPlateInserted()
            {
                _inscribeButton.Button().interactable = false;
                _repairButton.Button().interactable = false;

                UpdateNavigation();

                Button.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
            }

            public void SetPlateInserted(ArmourPlate plate)
            {
                if (plate.GetRepairCost() > 0) _repairButton.Button().interactable = true;

                _inscribeButton.Button().interactable = plate.Inscribable;

                UpdateNavigation();

                if (_inscribeButton.Button().interactable)
                {
                    Button.SetDownNavigation(_inscribeButton);
                    if (_repairButton.Button().interactable)
                    {
                        _inscribeButton.SetDownNavigation(_repairButton);
                        _repairButton.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
                    }
                    else
                    {
                        _inscribeButton.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
                    }
                }
                else if (_repairButton.Button().interactable)
                {
                    Button.SetDownNavigation(_repairButton);
                    _repairButton.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
                }
                else
                {
                    Button.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
                }
            }
        }
    }
}