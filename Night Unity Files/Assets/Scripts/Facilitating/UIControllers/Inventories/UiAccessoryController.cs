using System.Collections.Generic;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;

namespace Facilitating.UIControllers
{
    public class UiAccessoryController : UiInventoryMenuController
    {
        private ListController _accessoryList;

        protected override void CacheElements()
        {
            _accessoryList = gameObject.FindChildWithName<ListController>("List");
        }

        protected override void Initialise()
        {
            _accessoryList.Initialise(typeof(AccessoryElement), Equip, UiGearMenuController.Close);
        }

        private void Equip(object accessoryObject)
        {
            Accessory accessory = (Accessory) accessoryObject;
            CharacterManager.SelectedCharacter.EquipAccessory(accessory);
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

            protected override void Update(object o)
            {
                Accessory accessory = (Accessory) o;
                CentreText.SetText(accessory.Name);
                bool equipped = CharacterManager.SelectedCharacter.EquippedAccessory == accessory;
                LeftText.SetText(equipped ? "Equipped" : "Not Equipped");
                RightText.SetText(accessory.GetSummary());
            }
        }

        protected override void OnShow()
        {
            _accessoryList.Show(GetAvailableAccessories);
        }

        private static List<object> GetAvailableAccessories()
        {
            return Inventory.GetAvailableAccessories().ToObjectList();
        }
    }
}