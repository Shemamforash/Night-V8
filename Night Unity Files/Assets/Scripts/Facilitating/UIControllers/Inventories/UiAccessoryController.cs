using System.Collections.Generic;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiAccessoryController : UiInventoryMenuController
    {
        private ListController _accessoryList;
        private EnhancedText _equippedName, _equippedDescription;

        protected override void CacheElements()
        {
            _accessoryList = gameObject.FindChildWithName<ListController>("List");
            GameObject equipped = gameObject.FindChildWithName("Equipped");
            _equippedName = equipped.FindChildWithName<EnhancedText>("Name");
            _equippedDescription = equipped.FindChildWithName<EnhancedText>("Description");
        }

        private void UpdateEquipped()
        {
            Accessory equippedAccessory = CharacterManager.SelectedCharacter.EquippedAccessory;
            if (equippedAccessory == null)
            {
                _equippedName.SetText("No Accessory Equipped");
                _equippedDescription.SetText("-");
            }
            else
            {
                _equippedName.SetText(equippedAccessory.Name);
                _equippedDescription.SetText(equippedAccessory.GetSummary());
            }
        }

        protected override void Initialise()
        {
            _accessoryList.Initialise(typeof(AccessoryElement), Equip, UiGearMenuController.Close);
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
            _accessoryList.Show(GetAvailableAccessories);
            TutorialManager.TryOpenTutorial(10);
            UpdateEquipped();
        }

        private static List<object> GetAvailableAccessories()
        {
            return Inventory.GetAvailableAccessories().ToObjectList();
        }
    }
}