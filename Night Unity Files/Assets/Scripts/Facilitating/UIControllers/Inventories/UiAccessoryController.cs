using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Gear.Armour;
using Game.Global;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiAccessoryController : UiInventoryMenuController
    {
        private static UiAccessoryController _instance;
        private static bool _unlocked;

        private ListController _accessoryList;
        private EnhancedText _name, _bonus, _description;
        private bool _seenTutorial;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
        }

        private void OnDestroy() => _instance = null;

        public static UiAccessoryController Instance() => _instance;

        public static void Load(XmlNode root) => _unlocked = root.BoolFromNode("Accessories");

        public static void Save(XmlNode root) => root.CreateChild("Accessories", _unlocked);

        public static void Unlock() => _unlocked = true;

        public override bool Unlocked() => _unlocked;

        protected override void CacheElements()
        {
            _accessoryList = gameObject.FindChildWithName<ListController>("List");
            GameObject equipped = gameObject.FindChildWithName("Equipped");
            _name = equipped.FindChildWithName<EnhancedText>("Name");
            _bonus = equipped.FindChildWithName<EnhancedText>("Bonus");
            _description = equipped.FindChildWithName<EnhancedText>("Description");
        }

        protected override void Initialise()
        {
            _accessoryList.Initialise(typeof(AccessoryElement), Equip, null, GetAvailableAccessories);
        }

        protected override void OnShow()
        {
            UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
            _accessoryList.Show();
            UpdateEquipped();
            ShowAccessoryTutorial();
        }

        private void ShowAccessoryTutorial()
        {
            if (_seenTutorial || !TutorialManager.Active()) return;
            TutorialManager.Instance.TryOpenTutorial(14, new TutorialOverlay());
            _seenTutorial = true;
        }
        
        private void UpdateEquipped()
        {
            Accessory equippedAccessory = CharacterManager.SelectedCharacter.EquippedAccessory;
            if (equippedAccessory == null)
            {
                _name.SetText("No Accessory Equipped");
                _bonus.SetText("-");
                _description.SetText("");
            }
            else
            {
                _name.SetText(equippedAccessory.Name);
                _bonus.SetText(equippedAccessory.GetSummary());
                _description.SetText(equippedAccessory.Description());
            }
        }

        private void Equip(object accessoryObject)
        {
            Accessory accessory = (Accessory) accessoryObject;
            CharacterManager.SelectedCharacter.EquipAccessory(accessory);
            UiGearMenuController.PlayAudio(AudioClips.EquipAccessory);
            Show();
        }

        private class AccessoryElement : BasicListElement
        {
            protected override void UpdateCentreItemEmpty()
            {
                CentreText.SetText("No Accessories Found");
                LeftText.SetText("");
                RightText.SetText("");
            }

            protected override void Update(object o, bool isCentreItem)
            {
                Accessory accessory = (Accessory) o;
                CentreText.SetText(accessory.GetSummary());
                LeftText.SetText(accessory.Name);
                RightText.SetText(accessory.Description());
            }
        }

        private static List<object> GetAvailableAccessories() => Inventory.GetAvailableAccessories().ToObjectList();
    }
}