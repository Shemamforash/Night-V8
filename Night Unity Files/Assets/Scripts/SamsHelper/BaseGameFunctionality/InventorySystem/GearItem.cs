using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear;
using Game.Global;
using SamsHelper.Libraries;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class GearItem : InventoryItem
    {
        private ItemQuality _itemQuality;
        public Character EquippedCharacter;

        protected GearItem(string name, ItemQuality itemQuality) : base(name, GameObjectType.Gear)
        {
            SetQuality(itemQuality);
        }

        public override XmlNode Save(XmlNode root)
        {
            root = base.Save(root);
            root.CreateChild("Quality", (int) _itemQuality);
            return root;
        }

        public override void Load(XmlNode root)
        {
            base.Load(root);
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
    }
}