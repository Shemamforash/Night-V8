using System;
using System.Collections.Generic;
using DefaultNamespace;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiArmourUpgradeController : UiInventoryMenuController, IInputListener
    {
        private static ArmourPlateUi _plateOneUi;
        private static ArmourPlateUi _plateTwoUi;

        private Armour _currentSelectedArmour;
        private bool _upgradingAllowed;

        protected override void CacheElements()
        {
            _plateOneUi = new ArmourPlateUi(gameObject.FindChildWithName("Left Plate"), Armour.ArmourType.Chest);
            _plateTwoUi = new ArmourPlateUi(gameObject.FindChildWithName("Right Plate"), Armour.ArmourType.Head);
        }

        private void UpdatePlates()
        {
            _plateOneUi.SetPlate();
            _plateTwoUi.SetPlate();
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

        private static List<object> GetAvailableArmour(Armour.ArmourType armourType)
        {
            Player player = CharacterManager.SelectedCharacter;
            Inventory inventory = player.TravelAction.AtHome() ? WorldState.HomeInventory() : player.Inventory();
            List<object> armourList = inventory.Armour.ToObjectList();
            armourList.RemoveAll(a =>
            {
                bool isNotCorrectType = ((Armour) a).GetArmourType() != armourType;
                return isNotCorrectType;
            });
            return armourList;
        }

        private void EquipPlateOne(object armourObject)
        {
            Armour armour = (Armour) armourObject;
            CharacterManager.SelectedCharacter.EquipArmourSlotOne(armour);
            UpdatePlates();
            SetPlateListActive(_plateOneUi);
        }

        private void EquipPlateTwo(object armourObject)
        {
            Armour armour = (Armour) armourObject;
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
                _armourText.SetColor(visible ? Color.white : UiAppearanceController.InvisibleColour);
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
                Armour armour = (Armour) o;
                int max = armour.GetMaxProtection();
                int current = armour.GetCurrentProtection();
                ArmourController armourController = CharacterManager.SelectedCharacter.ArmourController;
                bool equipped = armourController.GetChestArmour() == armour || armourController.GetHeadArmour() == armour;
                string armourString = armour.Name + " - " + current + "/" + max + " Armour";
                if (equipped) armourString = "(E) " + armourString;
                _armourText.SetText(armourString);
            }
        }

        private class ArmourPlateUi
        {
            private readonly ListController PlateList;
            private readonly Armour.ArmourType _armourType;

            public ArmourPlateUi(GameObject gameObject, Armour.ArmourType armourType)
            {
                PlateList = gameObject.FindChildWithName<ListController>("List");
                _armourType = armourType;
            }

            public void SetPlate()
            {
                PlateList.Show(() => GetAvailableArmour(_armourType));
            }

            public void Initialise(Action<object> equipPlateAction)
            {
                PlateList.Initialise(typeof(ArmourElement), equipPlateAction, () => { });
            }
        }

        private static void SetPlateListActive(ArmourPlateUi plate)
        {
            plate.SetPlate();
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.Horizontal:
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