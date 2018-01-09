using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;

namespace Game.Characters
{
    public class EquipmentController : IPersistenceTemplate
    {
        private readonly Dictionary<GearSubtype, GearItem> _equippedGear = new Dictionary<GearSubtype, GearItem>();
        private DesolationInventory _inventory;
        
        public EquipmentController(Character character)
        {
            _inventory = character.Inventory();
            foreach (GearSubtype gearSlot in Enum.GetValues(typeof(GearSubtype)))
            {
                _equippedGear[gearSlot] = null;
            }
        }

        public GearItem GetGearItem(GearSubtype type)
        {
            return _equippedGear.ContainsKey(type) ? _equippedGear[type] : null;
        }
        
        public virtual void Equip(GearItem gearItem)
        {
            Inventory previousInventory = gearItem.ParentInventory;
            if (!_inventory.ContainsItem(gearItem))
            {
                gearItem.MoveTo(_inventory);
            }
            GearItem previousEquipped = _equippedGear[gearItem.GetGearType()];
            if (previousEquipped != null)
            {
                previousEquipped.Unequip();
                previousEquipped.MoveTo(previousInventory);
            }
            gearItem.Equip(_inventory);
            _equippedGear[gearItem.GetGearType()] = gearItem;
        }
        
        public Weapon Weapon() => GetGearItem(GearSubtype.Weapon) as Weapon;
        public Armour Armour() => GetGearItem(GearSubtype.Armour) as Armour;
        public Accessory Accessory() => GetGearItem(GearSubtype.Accessory) as Accessory;
        public void Load(XmlNode doc, PersistenceType saveType)
        {
//            throw new NotImplementedException();
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode weaponNode = SaveController.CreateNodeAndAppend("Weapon", doc);
            Weapon()?.Save(weaponNode, saveType);
            XmlNode armourNode = SaveController.CreateNodeAndAppend("Armour", doc);
            Armour()?.Save(weaponNode, saveType);
            XmlNode accessoryNode = SaveController.CreateNodeAndAppend("Accessory", doc);
            Accessory()?.Save(weaponNode, saveType);
            return doc;
        }
    }
}