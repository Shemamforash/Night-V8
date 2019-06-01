using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Characters
{
	public abstract class Character
	{
		public readonly ArmourController Armour = new ArmourController();
		public          Accessory        Accessory;
		public          Weapon           Weapon;
		public          string           Name;

		protected Character(string name)
		{
			Name           = name;
		}

		public virtual XmlNode Save(XmlNode root)
		{
			root = root.CreateChild("Character");
			root.CreateChild("Name", Name);
			XmlNode equipped = root.CreateChild("EquippedItems");
			Armour.Save(equipped);
			if (Weapon != null) equipped.CreateChild("Weapon", Weapon.ID());
			if (Accessory != null) equipped.CreateChild("Accessory", Accessory.ID());
			return root;
		}

		public virtual void Load(XmlNode root)
		{
			Name = root.ParseString("Name");
			root = root.SelectSingleNode("EquippedItems");
			XmlNode weaponNode    = root.SelectSingleNode("Weapon");
			XmlNode accessoryNode = root.SelectSingleNode("Accessory");
			int     weaponId      = weaponNode?.ParseInt("Weapon")       ?? -1;
			int     accessoryId   = accessoryNode?.ParseInt("Accessory") ?? -1;
			Armour.Load(root);
			EquipWeapon(Inventory.FindWeapon(weaponId));
			EquipAccessory(Inventory.FindAccessory(accessoryId));
		}

		public virtual void EquipAccessory(Accessory accessory)
		{
			Accessory?.UnEquip();
			Accessory = accessory;
			accessory?.Equip(this);
		}

		public virtual void EquipWeapon(Weapon weapon)
		{
			Weapon?.UnEquip();
			Weapon = weapon;
			weapon?.Equip(this);
		}
	}
}