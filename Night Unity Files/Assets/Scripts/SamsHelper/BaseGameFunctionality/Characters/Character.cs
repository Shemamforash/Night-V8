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
        public CharacterUiDetailed CharacterUiDetailed;

        public DesolationCharacterAttributes Attributes;

        public readonly StateMachine ActionStates = new StateMachine();
        public readonly CombatStateMachine CombatStates;

        public readonly Dictionary<GearSubtype, GearItem> EquippedGear = new Dictionary<GearSubtype, GearItem>();
        public Inventory CharacterInventory;
        private Weapon _weapon;

        protected Character(string name, GameObject gameObject = null) : base(name, GameObjectType.Character, gameObject)
        {
            CombatStates = new CombatStateMachine(this);
            foreach (GearSubtype gearSlot in Enum.GetValues(typeof(GearSubtype)))
            {
                EquippedGear[gearSlot] = null;
            }
        }

        public void AddItemToInventory(InventoryItem item)
        {
            CharacterInventory.AddItem(item);
        }

        public abstract void TakeDamage(int amount);
        protected abstract bool IsOverburdened();
        public abstract void Kill();

        public void SetWeapon(Weapon weapon)
        {
            _weapon = weapon;
        }

        public Weapon GetWeapon()
        {
            return _weapon;
        }

        protected virtual void SetCharacterUi()
        {
            CharacterUiDetailed = new CharacterUiDetailed(this);
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

        public void Equip(GearItem gearItem)
        {
            Inventory previousInventory = gearItem.Inventory;
            GearItem previousEquipped = EquippedGear[gearItem.GetGearType()];
            previousEquipped?.Modifier.Remove(Attributes);
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
            gearItem.Modifier.Apply(Attributes);
            switch (gearItem.GetGearType())
            {
                case GearSubtype.Weapon:
                    CharacterUiDetailed.WeaponGearUi.Update(gearItem);
                    break;
                case GearSubtype.Armour:
                    CharacterUiDetailed.ArmourGearUi.Update(gearItem);
                    break;
                case GearSubtype.Accessory:
                    CharacterUiDetailed.AccessoryGearUi.Update(gearItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}