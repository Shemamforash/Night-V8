using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Gear;
using Game.Gear.Armour;
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
        public readonly StateMachine States = new StateMachine();
        public readonly CombatStateMachine CombatStates;

        public readonly Dictionary<GearSubtype, GearItem> EquippedGear = new Dictionary<GearSubtype, GearItem>();
        protected Inventory CharacterInventory;

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            CombatStates = new CombatStateMachine(this);
            foreach (GearSubtype gearSlot in Enum.GetValues(typeof(GearSubtype)))
            {
                EquippedGear[gearSlot] = null;
            }
        }

        public abstract void TakeDamage(int amount);
        public abstract void Kill();
        public abstract void IncreaseDistance(float speedModifier = 0);
        public abstract void DecreaseDistance(float speedModifier = 0);

        public Inventory Inventory()
        {
            return CharacterInventory;
        }

        public List<State> StatesAsList(bool includeInactiveStates)
        {
            return (from BaseCharacterAction s in States.StatesAsList() where s.IsStateVisible() || includeInactiveStates select s).Cast<State>().ToList();
        }

        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
            Name = doc.SelectSingleNode("Name").InnerText;
        }

        public virtual void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("Name", doc, Name);
        }

        public abstract AttributeContainer GetAttributes();

        public virtual void Equip(GearItem gearItem)
        {
            Inventory previousInventory = gearItem.Inventory;
            GearItem previousEquipped = EquippedGear[gearItem.GetGearType()];
            previousEquipped?.Modifier.Remove(GetAttributes());
            if (!CharacterInventory.ContainsItem(gearItem))
            {
                gearItem.MoveTo(CharacterInventory);
            }
            if (previousEquipped != null)
            {
                previousEquipped.Equipped = false;
                previousEquipped.MoveTo(previousInventory);
            }
            gearItem.Equipped = true;
            EquippedGear[gearItem.GetGearType()] = gearItem;
            gearItem.Modifier.Apply(GetAttributes());
        }
    }
}