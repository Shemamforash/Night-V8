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
    public abstract class Character : MyGameObject
    {
        public readonly ArmourController ArmourController;
        public Accessory EquippedAccessory;
        public Weapon EquippedWeapon;

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            ArmourController = new ArmourController(this);
        }

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            XmlNode equipped = doc.CreateChild("EquippedItems");
            if (EquippedWeapon != null) equipped.CreateChild("Weapon", EquippedWeapon.ID());
            if (ArmourController?.GetChestArmour() != null) equipped.CreateChild("ArmourPlate1", ArmourController.GetChestArmour().ID());
            if (ArmourController?.GetHeadArmour() != null) equipped.CreateChild("ArmourPlate2", ArmourController.GetHeadArmour().ID());
            if (EquippedAccessory != null) equipped.CreateChild("Accessory", EquippedAccessory.ID());
            return doc;
        }

        public override void Load(XmlNode root)
        {
            base.Load(root);
            XmlNode equipmentNode = root.SelectSingleNode("EquippedItems");
            XmlNode weaponNode = equipmentNode.SelectSingleNode("Weapon");
            XmlNode accessoryNode = equipmentNode.SelectSingleNode("Accesory");
            XmlNode armourNode1 = equipmentNode.SelectSingleNode("ArmourPlate1");
            XmlNode armourNode2 = equipmentNode.SelectSingleNode("ArmourPlate2");
            int weaponId = weaponNode?.IntFromNode("Weapon") ?? -1;
            int accessoryId = accessoryNode?.IntFromNode("Accessory") ?? -1;
            int armourPlate1Id = armourNode1?.IntFromNode("ArmourPlate1") ?? -1;
            int armourPlate2Id = armourNode2?.IntFromNode("ArmourPlate2") ?? -1;
            EquipWeapon((Weapon) Inventory.FindItem(weaponId));
            EquipAccessory((Accessory) Inventory.FindItem(accessoryId));
            ArmourController.SetArmour((Armour) Inventory.FindItem(armourPlate1Id));
            ArmourController.SetArmour((Armour) Inventory.FindItem(armourPlate2Id));
        }

        public virtual void EquipWeapon(Weapon weapon)
        {
            EquippedWeapon?.Unequip();
            EquippedWeapon = weapon;
            weapon?.Equip(this);
        }

        public virtual void EquipAccessory(Accessory accessory)
        {
            EquippedAccessory?.Unequip();
            EquippedAccessory = accessory;
            accessory?.Equip(this);
        }
    }
}