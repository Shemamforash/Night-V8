using System.Xml;
using Facilitating.Persistence;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;

namespace Game.Characters
{
    public abstract class Character
    {
        public readonly ArmourController ArmourController;
        public Accessory EquippedAccessory;
        public Weapon EquippedWeapon;
        public string Name;

        protected Character(string name)
        {
            Name = name;
            ArmourController = new ArmourController(this);
        }

        public virtual XmlNode Save(XmlNode doc)
        {
            doc = doc.CreateChild("Character");
            doc.CreateChild("Name", Name);
            XmlNode equipped = doc.CreateChild("EquippedItems");
            if (EquippedWeapon != null) equipped.CreateChild("Weapon", EquippedWeapon.ID());
            ArmourController.Save(doc);
            if (EquippedAccessory != null) equipped.CreateChild("Accessory", EquippedAccessory.ID());
            return doc;
        }

        public virtual void Load(XmlNode root)
        {
            Name = root.StringFromNode("Name");
            root = root.SelectSingleNode("EquippedItems");
            XmlNode weaponNode = root.SelectSingleNode("Weapon");
            XmlNode accessoryNode = root.SelectSingleNode("Accesory");
            int weaponId = weaponNode?.IntFromNode("Weapon") ?? -1;
            int accessoryId = accessoryNode?.IntFromNode("Accessory") ?? -1;
            ArmourController.Load(root);
            EquipWeapon(Inventory.FindWeapon(weaponId));
            EquipAccessory(Inventory.FindAccessory(accessoryId));
        }

        public virtual void EquipWeapon(Weapon weapon)
        {
            EquippedWeapon?.UnEquip();
            EquippedWeapon = weapon;
            weapon?.Equip(this);
        }

        public virtual void EquipAccessory(Accessory accessory)
        {
            EquippedAccessory?.UnEquip();
            EquippedAccessory = accessory;
            accessory?.Equip(this);
        }
    }
}