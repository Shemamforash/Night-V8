using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Gear.Armour;
using Game.Global.Tutorial;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiArmourUpgradeController : UiInventoryMenuController, IInputListener
    {
        private bool _upgradingAllowed;
        private static bool _unlocked;
        private List<TutorialOverlay> _overlays;
        private static UiArmourUpgradeController _instance;
        private GameObject _armourObject, _upgradeObject;
        private EnhancedText _nameText, _bonusText, _nextLevelText, _upgradeText, _upgradeMessage;
        private ArmourController _armourController;
        private EnhancedButton _upgradeButton, _acceptButton;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
        }

        public static void Load(XmlNode root)
        {
            _unlocked = root.BoolFromNode("Armour");
        }

        public static void Save(XmlNode root)
        {
            root.CreateChild("Armour", _unlocked);
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public static UiArmourUpgradeController Instance() => _instance;

        public static void Unlock() => _unlocked = true;

        public override bool Unlocked() => _unlocked;

        protected override void CacheElements()
        {
            _armourObject = gameObject.FindChildWithName("Details");
            _upgradeObject = gameObject.FindChildWithName("Upgrade Success");
            _nameText = _armourObject.gameObject.FindChildWithName<EnhancedText>("Name");
            _bonusText = _armourObject.gameObject.FindChildWithName<EnhancedText>("Description");
            _nextLevelText = _armourObject.gameObject.FindChildWithName<EnhancedText>("Next Level");
            _upgradeText = _armourObject.gameObject.FindChildWithName<EnhancedText>("Upgrade Text");
            _upgradeMessage = _upgradeObject.gameObject.FindChildWithName<EnhancedText>("Message");
            _upgradeButton = _armourObject.gameObject.FindChildWithName<EnhancedButton>("Upgrade");
            _upgradeButton.AddOnClick(Upgrade);
            _acceptButton = _upgradeObject.gameObject.FindChildWithName<EnhancedButton>("Accept");
            _acceptButton.AddOnClick(OnShow);
            _overlays = new List<TutorialOverlay>
            {
                new TutorialOverlay(_armourObject.GetComponent<RectTransform>()),
                new TutorialOverlay(_upgradeButton.GetComponent<RectTransform>())
            };
        }

        protected override void Initialise()
        {
        }

        private void UpdatePlates()
        {
            UpdateArmourDescriptions();
        }

        protected override void OnShow()
        {
            _armourController = CharacterManager.SelectedCharacter.ArmourController;
            UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
            UpdatePlates();
            InputHandler.RegisterInputListener(this);
            _armourObject.SetActive(true);
            _upgradeObject.SetActive(false);
            _upgradeButton.Select();
            _upgradeButton.Button().enabled = _armourController.CanUpgrade();
            TutorialManager.TryOpenTutorial(12, _overlays);
        }

        private void Upgrade()
        {
            _armourController.Upgrade();
            _upgradeMessage.SetText("Upgraded to " + _armourController.GetName());
            _armourObject.SetActive(false);
            _upgradeObject.SetActive(true);
            _acceptButton.Select();
            CharacterManager.SelectedCharacter.CharacterView()?.ArmourController.UpdateArmour();
        }

        private void UpdateArmourDescriptions()
        {
            _nameText.SetText(_armourController.GetName());
            _bonusText.SetText(_armourController.GetBonus());
            _nextLevelText.SetText(_armourController.GetNextLevelBonus());
            _upgradeText.SetText(_armourController.GetUpgradeRequirements());
        }

        protected override void OnHide()
        {
            InputHandler.UnregisterInputListener(this);
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
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