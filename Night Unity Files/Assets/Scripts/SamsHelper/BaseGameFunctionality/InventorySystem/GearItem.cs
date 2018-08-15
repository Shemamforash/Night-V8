using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.Persistence;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class GearItem : InventoryItem
    {
        private readonly GearSubtype _gearType;
        private ItemQuality _itemQuality;
        protected Character EquippedCharacter;
        
        protected GearItem(string name, GearSubtype gearSubtype, ItemQuality itemQuality) : base(name, GameObjectType.Gear)
        {
            SetQuality(itemQuality);
            _gearType = gearSubtype;
        }

        public override XmlNode Save(XmlNode root)
        {
            root = base.Save(root);
            root.CreateChild("Quality", (int)_itemQuality);
            return root;
        }

        public override void Load(XmlNode root)
        {
            base.Load(root);
            _itemQuality = (ItemQuality) root.IntFromNode("Quality");
        }

        public ItemQuality Quality() => _itemQuality;

        public void SetQuality(ItemQuality quality)
        {
            _itemQuality = quality;
        }

        public virtual void Equip(Character character)
        {
            EquippedCharacter = character;
            MoveTo(character.Inventory());
        }

        public virtual void Unequip()
        {
            MoveTo(WorldState.HomeInventory());
            EquippedCharacter = null;
        }

        public abstract string GetSummary();

        public GearSubtype GetGearType() => _gearType;

        private void MoveTo(Inventory targetInventory)
        {
            targetInventory.Move(this, 1);
            SetParentInventory(targetInventory);
        }
    }
}