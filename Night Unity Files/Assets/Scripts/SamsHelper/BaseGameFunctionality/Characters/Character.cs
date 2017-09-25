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
        public CharacterUiDetailed CharacterUiDetailed;
        public WeaponUiSimple WeaponUiSimple;
        
        public DesolationCharacterAttributes Attributes;
        
        public readonly StateMachine ActionStates = new StateMachine();
        public readonly CombatStateMachine CombatStates;
        
        private readonly Dictionary<GearSubtype, GearItem> _equippedGear = new Dictionary<GearSubtype, GearItem>();
        public Inventory CharacterInventory;
        private Weapon _weapon;
        
        protected Character(string name, GameObject gameObject = null) : base(name, GameObjectType.Character, gameObject)
        {
            CombatStates = new CombatStateMachine(this);
            foreach (GearSubtype gearSlot in Enum.GetValues(typeof(GearSubtype)))
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
            WeaponUiSimple.Update(weapon);
        }

        public Weapon GetWeapon()
        {
            return _weapon;
        }

        protected virtual void SetCharacterUi(GameObject g)
        {
            CharacterUiDetailed = new CharacterUiDetailed(g);
            WeaponUiSimple = new WeaponUiSimple(CharacterUiDetailed);
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

        public void ReplaceGearInSlot(GearSubtype gearSubtype, GearItem gearItem)
        {
            GearItem equipped = _equippedGear[gearSubtype];
            if (equipped != null)
            {
                equipped.Unequip();
            }
            _equippedGear[gearSubtype] = gearItem;
        }
    }
}