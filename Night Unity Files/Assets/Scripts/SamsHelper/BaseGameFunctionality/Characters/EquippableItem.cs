using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace SamsHelper.BaseGameFunctionality.Characters
{
    public abstract class EquippableItem : BasicInventoryItem
    {
        private bool _equipped;
        private readonly GearSlot _gearslot;

        protected EquippableItem(string name, float weight, GearSlot gearSlot, ItemType itemType) : base(name, weight, itemType)
        {
            _gearslot = gearSlot;
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
    }
}