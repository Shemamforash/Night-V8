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
        private ListController _accessoryList;
        private EnhancedText _name, _bonus, _description;
        private static bool _unlocked;
        private List<TutorialOverlay> _overlays;

        public void Start()
        {
            _overlays = new List<TutorialOverlay>
            {
                new TutorialOverlay()
            };
        }
        
        protected override void CacheElements()
        {
            _accessoryList = gameObject.FindChildWithName<ListController>("List");
            GameObject equipped = gameObject.FindChildWithName("Equipped");
            _name = equipped.FindChildWithName<EnhancedText>("Name");
            _bonus = equipped.FindChildWithName<EnhancedText>("Bonus");
            _description = equipped.FindChildWithName<EnhancedText>("Description");
        }

        private void UpdateEquipped()
        {
            Accessory equippedAccessory = CharacterManager.SelectedCharacter.EquippedAccessory;
            if (equippedAccessory == null)
            {
                _name.SetText("No Accessory Equipped");
                _bonus.SetText("-");
                _description.SetText("-");
            }
            else
            {
                _name.SetText(equippedAccessory.Name);
                _bonus.SetText(equippedAccessory.GetSummary());
                _description.SetText(equippedAccessory.Description());
            }
        }

        protected override void Initialise()
        {
            _accessoryList.Initialise(typeof(AccessoryElement), Equip, UiGearMenuController.Close, GetAvailableAccessories);
        }

        public static void Load(XmlNode root)
        {
            _unlocked = root.BoolFromNode(nameof(GetType));
        }

        public static void Save(XmlNode root)
        {
            root.CreateChild(nameof(GetType), _unlocked);
        }

        public override bool Unlocked()
        {
            if(!_unlocked) _unlocked = Inventory.GetAvailableAccessories().Count != 0;
            return _unlocked;
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

        protected override void OnShow()
        {
            UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
            _accessoryList.Show();
            TutorialManager.TryOpenTutorial(13, _overlays);
            UpdateEquipped();
        }

        private static List<object> GetAvailableAccessories()
        {
            return Inventory.GetAvailableAccessories().ToObjectList();
        }
    }
}