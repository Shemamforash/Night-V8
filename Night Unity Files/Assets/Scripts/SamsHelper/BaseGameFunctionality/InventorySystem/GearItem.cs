using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear;
using SamsHelper.Libraries;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class GearItem : NamedItem
    {
        private ItemQuality _itemQuality;
        public Character EquippedCharacter;
        private static int _idCounter;
        private int _id;

        protected GearItem(string name, ItemQuality itemQuality) : base(name)
        {
            _id = _idCounter;
            ++_idCounter;
            SetQuality(itemQuality);
        }

        public virtual XmlNode Save(XmlNode root)
        {
            root.CreateChild("Name", Name);
            root.CreateChild("Id", _id);
            root.CreateChild("Quality", (int) _itemQuality);
            return root;
        }

        protected virtual void Load(XmlNode root)
        {
            Name = root.StringFromNode("Name");
            _id = root.IntFromNode("Id");
            if (_id > _idCounter) _idCounter = _id + 1;
            _itemQuality = (ItemQuality) root.IntFromNode("Quality");
        }

        public ItemQuality Quality() => _itemQuality;

        private void SetQuality(ItemQuality quality)
        {
            _itemQuality = quality;
        }

        public virtual void Equip(Character character)
        {
            EquippedCharacter = character;
        }

        public virtual void UnEquip()
        {
            EquippedCharacter = null;
        }

        public abstract string GetSummary();

        public int ID()
        {
            return _id;
        }
    }
}