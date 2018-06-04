using System.Xml;
using Facilitating.Persistence;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
namespace Game.Characters
{
    public abstract class Character : MyGameObject
    {
        public readonly ArmourController ArmourController;
        protected readonly Inventory CharacterInventory;
        public Accessory Accessory;
        public Weapon Weapon;

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            CharacterInventory = new Inventory(name);
            ArmourController = new ArmourController(this);
        }

        public override XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            doc = base.Save(doc, saveType);
            SaveController.CreateNodeAndAppend("Name", doc, Name);
            Weapon?.Save(doc, saveType);
            ArmourController?.Save(doc, saveType);
            Accessory?.Save(doc, saveType);
            Inventory().Save(doc, saveType);
            return doc;
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            base.Load(doc, saveType);
            Name = doc.SelectSingleNode("Name").InnerText;
        }

        public virtual void EquipWeapon(Weapon weapon)
        {
            Weapon?.Unequip();
            weapon.Equip(this);
            Weapon = weapon;
            Weapon.Reload(Inventory());
        }

        public virtual void EquipAccessory(Accessory accessory)
        {
            Accessory?.Unequip();
            accessory.Equip(this);
            Accessory = accessory;
        }

        public Inventory Inventory()
        {
            return CharacterInventory;
        }
    }
}