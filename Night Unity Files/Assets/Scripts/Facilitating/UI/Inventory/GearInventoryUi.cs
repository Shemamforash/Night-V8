using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Characters;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI.Inventory
{
    public class GearInventoryUi : InventoryItemUi
    {
        private readonly EquippableItem _item;

        public GearInventoryUi(EquippableItem item, Transform parent, bool equippable, Direction direction = Direction.None) : base(item, parent, direction)
        {
            _item = item;
            SummaryText.text = item.GetSummary();
            if (!equippable) return;
            Weapon weapon = _item as Weapon;
            TypeText.text = weapon != null ? weapon.GetItemType() : _item.Slot().ToString();
            RightActionButton.AddOnClick(ShowEquipPopup);
            RightActionButton.gameObject.SetActive(true);
            RightButtonText.text = "Equip";
            SummaryText.GetComponent<LayoutElement>().minWidth = 200;
            Bookends.SetActive(false);
        }

        private void ShowEquipPopup()
        {
            Popup popup = new Popup(_item.Name);
            popup.AddButton("Equip", ShowCharacterPopup);
            popup.AddButton("Move", () => { });
            popup.AddCancelButton();
        }

        private void ShowCharacterPopup()
        {
            Popup popupWithList = new Popup("Equip " + _item.Name);
            popupWithList.AddList(new List<MyGameObject>(DesolationCharacterManager.Characters()), EquipItem);
            popupWithList.AddCancelButton();
        }

        private void EquipItem(MyGameObject item)
        {
            
        }

        public override GameObject GetNavigationButton() => Direction == Direction.None ? RightActionButton.gameObject : base.GetNavigationButton();
    }
}