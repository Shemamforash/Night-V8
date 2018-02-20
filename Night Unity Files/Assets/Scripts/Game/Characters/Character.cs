using System.Xml;
using Facilitating.Persistence;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;

namespace Game.Characters
{
    public abstract class Character : MyGameObject, IPersistenceTemplate
    {
        protected readonly DesolationInventory CharacterInventory;
        public readonly ArmourController ArmourController;
        public Weapon Weapon;
        public Accessory Accessory;


        public virtual XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("Name", doc, Name);
            Weapon?.Save(doc, saveType);
            ArmourController?.Save(doc, saveType);
            Accessory?.Save(doc, saveType);
            Inventory().Save(doc, saveType);
            return doc;
        }

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            CharacterInventory = new DesolationInventory(name);
            ArmourController = new ArmourController(this);
        }

        public virtual void EquipWeapon(Weapon weapon)
        {
            Weapon?.Unequip();
            weapon.Equip(CharacterInventory);
            Weapon = weapon;
            Weapon.Reload(Inventory());
        }

        public virtual void EquipAccessory(Accessory accessory)
        {
            Accessory?.Unequip();
            accessory.Equip(CharacterInventory);
            Accessory = accessory;
        }

        public abstract void Kill();

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            base.Load(doc, saveType);
            Name = doc.SelectSingleNode("Name").InnerText;
        }

        public DesolationInventory Inventory()
        {
            return CharacterInventory;
        }
    }
}