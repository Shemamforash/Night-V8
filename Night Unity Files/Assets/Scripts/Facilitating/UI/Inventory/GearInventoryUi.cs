using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI.Inventory
{
    public class GearInventoryUi : InventoryItemUi
    {
        private readonly GearItem _item;

        public GearInventoryUi(GearItem item, Transform parent, Direction direction = Direction.None) : base(item, parent, direction)
        {
            _item = item;
            SummaryText.text = item.GetSummary();
            TypeText.text = _item.GetGearType().ToString();
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
            popup.AddButton("Move", ShowCharacterPopup);
            popup.AddBackButton();
        }

        private void ShowCharacterPopup()
        {
            Popup popupWithList = new Popup("Equip " + _item.Name);
            popupWithList.AddList(new List<MyGameObject>(DesolationCharacterManager.Characters()), EquipItem);
            popupWithList.AddBackButton();
        }

        private void EquipItem(MyGameObject item)
        {
            ShowEquipPopup();
        }

        public override GameObject GetNavigationButton() => Direction == Direction.None ? RightActionButton.gameObject : base.GetNavigationButton();
    }
}