using Facilitating.UI.Inventory;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Characters
{
    public abstract class GearItem : InventoryItem
    {
        private bool _equipped;
        private readonly GearSubtype _gearType;

        protected GearItem(string name, float weight, GearSubtype gearSubtype) : base(name, GameObjectType.Gear, weight)
        {
            _gearType = gearSubtype;
        }

        public void Equip()
        {
            //if in inventory, auto equip and replace
            //if not in inventory open equip window
//            if (!Inventory.InventoryHasSpace(Weight()) && !Inventory.ContainsItem(this)) return;
//            c.AddItemToInventory(this);
//            _equipped = true;
//            c.ReplaceGearInSlot(_gearslot, this);
        }

        public void Unequip()
        {
            _equipped = false;
        }

        public bool IsEquipped()
        {
            return _equipped;
        }

        public abstract string GetSummary();

        public GearSubtype GetGearType()
        {
            return _gearType;
        }

        public override BaseInventoryUi CreateUi(Transform parent)
        {
            return new GearInventoryUi(this, parent);
        }
    }
}