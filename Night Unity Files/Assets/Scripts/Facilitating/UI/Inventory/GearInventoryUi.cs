using Facilitating.MenuNavigation;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Characters;
using UnityEngine;

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
            ButtonText.text = "Actions";
            Weapon weapon = _item as Weapon;
            TypeText.text = weapon != null ? weapon.GetItemType() : _item.Slot().ToString();
            ActionButton.AddOnClick(ShowEquipPopup);
            ActionButton.gameObject.SetActive(true);
        }

        private void ShowEquipPopup()
        {
            Popup popup = new Popup(_item.Name);
            popup.AddOption("Equip", () => { });
            popup.AddOption("Move", () => { });
            popup.AddOption();
        }

        protected override void CacheUiElements()
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}