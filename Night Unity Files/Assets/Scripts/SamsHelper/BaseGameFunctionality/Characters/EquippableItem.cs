using Game.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear
{
    public class EquippableItem : BasicInventoryItem
    {
        private bool _equipped;
        private readonly GearSlot _gearslot;

        protected EquippableItem(string name, float weight, GearSlot gearSlot) : base(name, weight)
        {
            _gearslot = gearSlot;
        }

        private void Equip(Character c)
        {
            if (!c.CharacterInventory.InventoryHasSpace(Weight()) && !c.CharacterInventory.ContainsItem(this)) return;
            c.AddItemToInventory(this);
            _equipped = true;
            c.ReplaceGearInSlot(_gearslot, this);
        }

        public void Unequip()
        {
            _equipped = false;
        }

        public bool IsEquipped()
        {
            return _equipped;
        }
    }
}