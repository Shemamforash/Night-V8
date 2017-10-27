using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World;
using Game.World.Region;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Characters
{
    public class Character : MyGameObject, IPersistenceTemplate
    {
        public readonly StateMachine States = new StateMachine();
        public readonly CombatStateMachine CombatStates;
        public readonly CharacterConditions Conditions;

        protected readonly Dictionary<GearSubtype, GearItem> EquippedGear = new Dictionary<GearSubtype, GearItem>();
        protected Inventory CharacterInventory;
        
        public readonly BaseAttributes BaseAttributes;

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            Conditions = new CharacterConditions();
            BaseAttributes = new BaseAttributes(this);
            CombatStates = new CombatStateMachine(this);
            foreach (GearSubtype gearSlot in Enum.GetValues(typeof(GearSubtype)))
            {
                EquippedGear[gearSlot] = null;
            }
        }

        public Condition GetCondition(ConditionType type)
        {
            return Conditions.Conditions[type];
        }

        public Weapon Weapon() => (Weapon) EquippedGear[GearSubtype.Weapon];
        public Armour Armour() => (Armour) EquippedGear[GearSubtype.Armour];
        public Accessory Accessory() => (Accessory) EquippedGear[GearSubtype.Accessory];

        public virtual void TakeDamage(int amount)
        {
            BaseAttributes.Strength.SetCurrentValue(BaseAttributes.Strength.GetCurrentValue() - amount);
            if (BaseAttributes.Strength.ReachedMin())
            {
                //TODO kill character
            }
        }

        public virtual void Kill()
        {
            WorldState.HomeInventory().RemoveItem(this);
        }

        private float GetSpeedModifier()
        {
            return 1f + BaseAttributes.Endurance.GetCalculatedValue() / 100f * Time.deltaTime;
        }

        public void IncreaseDistance()
        {
            CombatManager.IncreaseDistance(this, GetSpeedModifier());
        }

        public void DecreaseDistance()
        {
            CombatManager.DecreaseDistance(this, GetSpeedModifier());
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            Name = doc.SelectSingleNode("Name").InnerText;
            XmlNode attributesNode = doc.SelectSingleNode("Attributes");
            BaseAttributes.Load(attributesNode, saveType);
        }

        public void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("Name", doc, Name);
            XmlNode attributesNode = SaveController.CreateNodeAndAppend("Attributes", doc);
            BaseAttributes.Save(attributesNode, saveType);
        }

        public GearItem GetGearItem(GearSubtype type)
        {
            return EquippedGear[type];
        }

        public Inventory Inventory()
        {
            return CharacterInventory;
        }

        public List<State> StatesAsList(bool includeInactiveStates)
        {
            return (from BaseCharacterAction s in States.StatesAsList() where s.IsStateVisible() || includeInactiveStates select s).Cast<State>().ToList();
        }

        public virtual void Equip(GearItem gearItem)
        {
            Inventory previousInventory = gearItem.Inventory;
            GearItem previousEquipped = EquippedGear[gearItem.GetGearType()];
            previousEquipped?.Modifier.Remove(BaseAttributes);
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
            gearItem.Modifier.Apply(BaseAttributes);
        }

        public void TakeCover()
        {
            throw new NotImplementedException();
        }
    }
}