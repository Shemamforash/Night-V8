using System;
using System.Collections.Generic;
using DefaultNamespace;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Facilitating.UIControllers
{
    public class UiArmourUpgradeController : UiInventoryMenuController, IInputListener
    {
        private static ArmourPlateUi _plateOneUi;
        private static ArmourPlateUi _plateTwoUi;

        private ArmourPlate _currentSelectedPlate;
        private bool _upgradingAllowed;

        protected override void CacheElements()
        {
            _plateOneUi = new ArmourPlateUi(gameObject.FindChildWithName("Left Plate"));
            _plateTwoUi = new ArmourPlateUi(gameObject.FindChildWithName("Right Plate"));
        }

        private void UpdatePlates()
        {
            _plateOneUi.SetPlate(CharacterManager.SelectedCharacter.ArmourController.GetPlateOne());
            _plateTwoUi.SetPlate(CharacterManager.SelectedCharacter.ArmourController.GetPlateTwo());
        }

        protected override void OnShow()
        {
            UpdatePlates();
            InputHandler.RegisterInputListener(this);
            SetPlateListActive(_plateOneUi);
        }

        protected override void OnHide()
        {
            InputHandler.UnregisterInputListener(this);
        }

        protected override void Initialise()
        {
            _plateOneUi.Initialise(EquipPlateOne);
            _plateTwoUi.Initialise(EquipPlateTwo);
        }

        private static List<object> GetAvailableArmour()
        {
            Player player = CharacterManager.SelectedCharacter;
            Inventory inventory = player.TravelAction.AtHome() ? WorldState.HomeInventory() : player.Inventory();
            return inventory.Armour.ToObjectList();
        }

        private void EquipPlateOne(object armourObject)
        {
            ArmourPlate armour = (ArmourPlate) armourObject;
            CharacterManager.SelectedCharacter.EquipArmourSlotOne(armour);
            UpdatePlates();
            SetPlateListActive(_plateOneUi);
        }

        private void EquipPlateTwo(object armourObject)
        {
            ArmourPlate armour = (ArmourPlate) armourObject;
            CharacterManager.SelectedCharacter.EquipArmourSlotTwo(armour);
            UpdatePlates();
            SetPlateListActive(_plateTwoUi);
        }

        private class ArmourElement : ListElement
        {
            private EnhancedText _armourText;

            protected override void UpdateCentreItemEmpty()
            {
                _armourText.SetText("No Plates Available");
            }

            protected override void SetVisible(bool visible)
            {
                _armourText.gameObject.SetActive(visible);
            }

            protected override void CacheUiElements(Transform transform)
            {
                _armourText = transform.GetComponent<EnhancedText>();
            }

            public override void SetColour(Color c)
            {
                _armourText.SetColor(c);
            }

            protected override void Update(object o)
            {
                ArmourPlate armour = (ArmourPlate) o;
                int max = armour.GetMaxProtection();
                int current = armour.GetCurrentProtection();
                _armourText.SetText(armour.Name + " - " + current + "/" + max + " Armour");
            }
        }

        private class ArmourPlateUi
        {
            private readonly EnhancedText _nameText, _currentArmourText, _armourDetailText;
            public readonly ListController PlateList;

            public ArmourPlateUi(GameObject gameObject)
            {
                GameObject plate = gameObject.FindChildWithName("Plate");
                _nameText = plate.FindChildWithName<EnhancedText>("Name");
                _currentArmourText = plate.FindChildWithName<EnhancedText>("Current Armour");
                _armourDetailText = plate.FindChildWithName<EnhancedText>("Armour Detail");
                PlateList = gameObject.FindChildWithName<ListController>("List");
            }

            public void SetPlate(ArmourPlate plate)
            {
                PlateList.Show(GetAvailableArmour);

                if (plate == null)
                {
                    _nameText.SetText("");
                    _currentArmourText.SetText("No Plate");
                    _armourDetailText.SetText("");
                    return;
                }

                _nameText.SetText(plate.Name);
                _currentArmourText.SetText(plate.Protection + " Armour");
                _armourDetailText.SetText("TODO");
            }

            public void Initialise(Action<object> equipPlateAction)
            {
                PlateList.Initialise(typeof(ArmourElement), equipPlateAction, () => { });
            }
        }

        private static void SetPlateListActive(ArmourPlateUi plate)
        {
            plate.PlateList.Show(GetAvailableArmour);
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.Horizontal:
                    Debug.Log("fart");
                    SetPlateListActive(direction < 0 ? _plateOneUi : _plateTwoUi);
                    return;
                case InputAxis.Cover:
                    UiGearMenuController.Close();
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }
    }
}