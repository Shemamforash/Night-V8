using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear;
using Game.Global;
using SamsHelper.Persistence;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class GearItem : InventoryItem
    {
        private readonly GearSubtype _gearType;
        private ItemQuality _itemItemQuality;
        protected Character EquippedCharacter;

        protected GearItem(string name, GearSubtype gearSubtype, ItemQuality itemQuality) : base(name, GameObjectType.Gear)
        {
            SetQuality(itemQuality);
            _gearType = gearSubtype;
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            SaveController.CreateNodeAndAppend("GearType", root, _gearType);
            return root;
        }

        public ItemQuality Quality() => _itemItemQuality;

        public void SetQuality(ItemQuality quality)
        {
            _itemItemQuality = quality;
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
            ParentInventory = targetInventory;
        }
    }
}