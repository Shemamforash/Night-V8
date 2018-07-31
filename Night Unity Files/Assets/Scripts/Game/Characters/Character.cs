using System.Xml;
using Facilitating.Persistence;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.Persistence;

namespace Game.Characters
{
    public abstract class Character : MyGameObject
    {
        public readonly ArmourController ArmourController;
        protected readonly Inventory CharacterInventory;
        public Accessory EquippedAccessory;
        public Weapon EquippedWeapon;

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            CharacterInventory = new Inventory(name);
            ArmourController = new ArmourController(this);
        }

        public override XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            doc = base.Save(doc, saveType);
            SaveController.CreateNodeAndAppend("Name", doc, Name);
            EquippedWeapon?.Save(doc, saveType);
            ArmourController?.Save(doc, saveType);
            EquippedAccessory?.Save(doc, saveType);
            Inventory().Save(doc, saveType);
            return doc;
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            base.Load(doc, saveType);
            Name = doc.GetNodeText("Name");
        }

        public virtual void EquipWeapon(Weapon weapon)
        {
            EquippedWeapon?.Unequip();
            weapon.Equip(this);
            EquippedWeapon = weapon;
        }

        public virtual void EquipAccessory(Accessory accessory)
        {
            EquippedAccessory?.Unequip();
            accessory.Equip(this);
            EquippedAccessory = accessory;
        }

        public Inventory Inventory() => CharacterInventory;
    }
}