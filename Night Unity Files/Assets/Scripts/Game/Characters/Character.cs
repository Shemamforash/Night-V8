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

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            XmlNode equipped = doc.CreateChild("EquippedItems");
            if (EquippedWeapon != null) equipped.CreateChild("Weapon", EquippedWeapon.Id);
            if(ArmourController?.GetPlateOne() != null) equipped.CreateChild("ArmourPlate1", ArmourController.GetPlateOne().Id);
            if (ArmourController?.GetPlateTwo() != null) equipped.CreateChild("ArmourPlate2", ArmourController.GetPlateTwo().Id);
            if (EquippedAccessory != null) equipped.CreateChild("Accessory", EquippedAccessory.Id);
            CharacterInventory?.Save(doc);
            return doc;
        }

        public override void Load(XmlNode doc)
        {
            base.Load(doc);
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