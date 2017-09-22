using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Characters;
using Facilitating.Persistence;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Gear;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Characters
{
    public abstract class Character : MyGameObject, IPersistenceTemplate
    {
        public CharacterUI CharacterUi;
        public WeaponUi WeaponUi;
        
        public DesolationCharacterAttributes Attributes;
        
        public readonly StateMachine ActionStates = new StateMachine();
        public readonly CombatStateMachine CombatStates;
        
        private readonly Dictionary<GearSlot, EquippableItem> _equippedGear = new Dictionary<GearSlot, EquippableItem>();
        public Inventory CharacterInventory;
        private Weapon _weapon;
        
        protected Character(string name, GameObject gameObject = null) : base(name, GameObjectType.Character, gameObject)
        {
            CombatStates = new CombatStateMachine(this);
            foreach (GearSlot gearSlot in Enum.GetValues(typeof(GearSlot)))
            {
                _equippedGear[gearSlot] = null;
            }
        }

        public void AddItemToInventory(BasicInventoryItem item)
        {
            CharacterInventory.AddItem(item);
        }

        public abstract void TakeDamage(int amount);
        protected abstract bool IsOverburdened();
        public abstract void Kill();

        public void SetWeapon(Weapon weapon)
        {
            _weapon = weapon;
            WeaponUi.Update(weapon);
        }

        public Weapon GetWeapon()
        {
            return _weapon;
        }

        protected virtual void SetCharacterUi(GameObject g)
        {
            CharacterUi = new CharacterUI(g);
            WeaponUi = new WeaponUi(CharacterUi);
        }

        protected List<State> StatesAsList(bool includeInactiveStates)
        {
            return (from BaseCharacterAction s in ActionStates.StatesAsList() where s.IsStateVisible() || includeInactiveStates select s).Cast<State>().ToList();
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            Name = doc.SelectSingleNode("Name").InnerText;
            XmlNode attributesNode = doc.SelectSingleNode("Attributes");
            Attributes.Load(attributesNode, saveType);
        }

        public void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("Name", doc, Name);
            XmlNode attributesNode = SaveController.CreateNodeAndAppend("Attributes", doc);
            Attributes.Save(attributesNode, saveType);
        }

        public void ReplaceGearInSlot(GearSlot gearSlot, EquippableItem equippableItem)
        {
            EquippableItem equipped = _equippedGear[gearSlot];
            if (equipped != null)
            {
                equipped.Unequip();
            }
            _equippedGear[gearSlot] = equippableItem;
        }
    }
}