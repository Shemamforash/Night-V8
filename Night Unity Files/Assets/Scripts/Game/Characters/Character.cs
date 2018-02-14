using System;
using System.Xml;
using Facilitating.Audio;
using Facilitating.Persistence;
using Game.Characters.Player;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Skills;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine.Assertions;

namespace Game.Characters
{
    public abstract class Character : MyGameObject, IPersistenceTemplate
    {
        protected readonly DesolationInventory CharacterInventory;
        public Weapon Weapon;
        public Accessory Accessory;
        public Armour Armour;

        public virtual XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("Name", doc, Name);
            Weapon?.Save(doc, saveType);
            Armour?.Save(doc, saveType);
            Accessory?.Save(doc, saveType);
            Inventory().Save(doc, saveType);
            return doc;
        }

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            CharacterInventory = new DesolationInventory(name);
        }

        public virtual void EquipWeapon(Weapon weapon)
        {
            Weapon?.Unequip();
            weapon.Equip(CharacterInventory);
            Weapon = weapon;
            Weapon.Reload(Inventory());
        }

        public virtual void EquipArmour(Armour armour)
        {
            Armour?.Unequip();
            armour.Equip(CharacterInventory);
            Armour = armour;
        }
        
        public virtual void EquipAccessory(Accessory accessory)
        {
            Accessory?.Unequip();
            accessory.Equip(CharacterInventory);
            Accessory = accessory;
        }

        public abstract void Kill();

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            Name = doc.SelectSingleNode("Name").InnerText;
        }

        public DesolationInventory Inventory()
        {
            return CharacterInventory;
        }
    }
}