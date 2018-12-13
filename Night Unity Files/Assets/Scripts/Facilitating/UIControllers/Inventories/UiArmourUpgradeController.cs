﻿using System;
using System.Collections.Generic;
using System.Xml;
using DefaultNamespace;
using Facilitating.Persistence;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Gear.Armour;
using Game.Global;
using Game.Global.Tutorial;
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
        private static bool _unlocked;
        private List<TutorialOverlay> _overlays;

        public static void Load(XmlNode root)
        {
            _unlocked = root.BoolFromNode("Armour");
        }

        public static void Save(XmlNode root)
        {
            root.CreateChild("Armour", _unlocked);
        }

        public override bool Unlocked()
        {
            if (!_unlocked) _unlocked = Inventory.GetAvailableArmour().Count != 0;
            return _unlocked;
        }

        public override void Awake()
        {
            base.Awake();
            _overlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(GetComponent<RectTransform>()),
                new TutorialOverlay(),
                new TutorialOverlay(),
                new TutorialOverlay()
            };
        }

        protected override void CacheElements()
        {
            _plateOneUi = new ArmourPlateUi(gameObject.FindChildWithName("Left Plate"), Armour.ArmourType.Chest);
            _plateTwoUi = new ArmourPlateUi(gameObject.FindChildWithName("Right Plate"), Armour.ArmourType.Head);
        }

        private void UpdatePlates()
        {
            _plateOneUi.SetPlate();
            _plateTwoUi.SetPlate();
            UpdateArmourDescriptions();
        }

        protected override void OnShow()
        {
            UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
            UpdatePlates();
            InputHandler.RegisterInputListener(this);
            SetPlateListActive(_plateOneUi);
            TutorialManager.TryOpenTutorial(12, _overlays);
        }

        private void UpdateArmourDescriptions()
        {
            ArmourController armourController = CharacterManager.SelectedCharacter.ArmourController;
            _plateOneUi.UpdateDescription(armourController.GetChestArmour());
            _plateTwoUi.UpdateDescription(armourController.GetHeadArmour());
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
            List<object> armourList = Inventory.GetAvailableArmour().ToObjectList();
            return armourList.FindAll(a => ((Armour) a).GetArmourType() == armourType);
        }

        private void EquipPlateOne(object armourObject)
        {
            Armour armour = (Armour) armourObject;
            CharacterManager.SelectedCharacter.EquipChestArmour(armour);
            UiGearMenuController.PlayAudio(AudioClips.EquipArmour);
            UpdatePlates();
            SetPlateListActive(_plateOneUi);
        }

        private void EquipPlateTwo(object armourObject)
        {
            Armour armour = (Armour) armourObject;
            CharacterManager.SelectedCharacter.EquipHeadArmour(armour);
            UiGearMenuController.PlayAudio(AudioClips.EquipArmour);
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

            protected override void Update(object o, bool isCentreItem)
            {
                Armour armour = (Armour) o;
                _armourText.SetText(GetArmourProtection(armour));
            }
        }

        private static string GetArmourProtection(Armour armour)
        {
            int max = armour.GetMaxProtection();
            int current = armour.GetCurrentProtection();
            string armourString = armour.Name + " - " + current + "/" + max + " Armour";
            return armourString;
        }
        
        private class ArmourPlateUi
        {
            private readonly EnhancedText _description;
            private readonly ListController PlateList;
            private readonly Armour.ArmourType _armourType;

            public ArmourPlateUi(GameObject gameObject, Armour.ArmourType armourType)
            {
                PlateList = gameObject.FindChildWithName<ListController>("List");
                _description = gameObject.FindChildWithName<EnhancedText>("Description");
                _armourType = armourType;
            }

            public void UpdateDescription(Armour armour)
            {
                string descriptionString = "No Armour Equipped";
                if (armour != null) descriptionString = GetArmourProtection(armour);
                _description.SetText(descriptionString);
            }
            
            public void SetPlate()
            {
                PlateList.Show();
            }

            public void Initialise(Action<object> equipPlateAction)
            {
                PlateList.Initialise(typeof(ArmourElement), equipPlateAction, () => { }, () => GetAvailableArmour(_armourType));
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
                case InputAxis.Menu:
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