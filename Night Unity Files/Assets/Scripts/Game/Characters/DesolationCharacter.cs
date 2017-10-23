using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.World;
using Game.World.Region;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Characters
{
    public class DesolationCharacter : Character
    {
        public Region CurrentRegion;
        public readonly TraitLoader.Trait CharacterClass, CharacterTrait;
        public readonly CharacterConditions Conditions;
        public CharacterView CharacterView;
        public readonly DesolationCharacterAttributes Attributes;

        
        //Create Character in code only- no view section, no references to objects in the scene
        public DesolationCharacter(string name, TraitLoader.Trait characterClass, TraitLoader.Trait characterTrait) : base(name)
        {
            CharacterInventory = new DesolationInventory(name);
            CharacterClass = characterClass;
            CharacterTrait = characterTrait;
            Attributes = new DesolationCharacterAttributes(this);
            Conditions = new CharacterConditions();
            CharacterInventory.MaxWeight = 50;
        }

        //Links character to object in scene
        public override void SetGameObject(GameObject gameObject)
        {
            base.SetGameObject(gameObject);
            SetCharacterUi();
            AddStates();
        }

        private void SetCharacterUi()
        {
            CharacterView = new CharacterView(this);
        }

        private void AddStates()
        {
            ActionStates.AddState(new CollectResources(this));
            ActionStates.AddState(new CharacterActions.Combat(this));
            ActionStates.AddState(new Sleep(this));
            ActionStates.AddState(new Idle(this));
            ActionStates.AddState(new PrepareTravel(this));
            ActionStates.AddState(new Travel(this));
            ActionStates.AddState(new Return(this));
            ActionStates.AddState(new LightFire(this));
            ActionStates.SetDefaultState("Idle");
            CharacterView.UpdateActionUi();
        }

        public override AttributeContainer GetAttributes()
        {
            return Attributes;
        }

        public Condition GetCondition(ConditionType type)
        {
            return Conditions.Conditions[type];
        }

        public override void TakeDamage(int amount)
        {
            Attributes.Strength.SetCurrentValue(Attributes.Strength.GetCurrentValue() - amount);
            if (Attributes.Strength.ReachedMin())
            {
                //TODO kill character
            }
        }

        public override void Equip(GearItem gearItem)
        {
            base.Equip(gearItem);
            switch (gearItem.GetGearType())
            {
                case GearSubtype.Weapon:
                    CharacterView?.WeaponGearUi.Update(gearItem);
                    break;
                case GearSubtype.Armour:
                    CharacterView?.ArmourGearUi.Update(gearItem);
                    break;
                case GearSubtype.Accessory:
                    CharacterView?.AccessoryGearUi.Update(gearItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Kill()
        {
            WorldState.HomeInventory().RemoveItem(this);
        }

        private bool IsOverburdened()
        {
            return CharacterInventory.Weight > Attributes.Strength.GetCurrentValue();
        }

        private void Tire(int amount)
        {
            Attributes.Endurance.SetCurrentValue(Attributes.Endurance.GetCurrentValue() - (IsOverburdened() ? amount * 2 : amount));
            CheckEnduranceZero();
        }

        private void CheckEnduranceZero()
        {
            if (!Attributes.Endurance.ReachedMin()) return;
            BaseCharacterAction action = ActionStates.GetCurrentState() as BaseCharacterAction;
            action.Interrupt();
            Sleep sleepAction = ActionStates.NavigateToState("Sleep") as Sleep;
            sleepAction.SetDuration((int) (Attributes.Endurance.Max / 5f));
            sleepAction.SetStateTransitionTarget(action.Name);
            sleepAction.AddOnExit(() => { action.Resume(); });
            sleepAction.Start();
        }

        public void Rest(int amount)
        {
            Attributes.Endurance.SetCurrentValue(Attributes.Endurance.GetCurrentValue() + amount);
            if (!Attributes.Endurance.ReachedMax()) return;
            if (CurrentRegion == null)
            {
                ActionStates.NavigateToState("Idle");
            }
        }

        public void Travel()
        {
            Tire(CalculateEnduranceCostForDistance(1));
        }

        public int CalculateTotalWeight()
        {
            int characterWeight = 5 + (int) Attributes.Weight;
            int inventoryWeight = (int) (CharacterInventory.Weight / 10);
            return characterWeight + inventoryWeight;
        }

        public int CalculateEnduranceCostForDistance(int distance)
        {
            return distance * CalculateTotalWeight();
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            base.Load(doc, saveType);
            XmlNode attributesNode = doc.SelectSingleNode("Attributes");
            Attributes.Load(attributesNode, saveType);
        }

        public override void Save(XmlNode doc, PersistenceType saveType)
        {
            base.Save(doc, saveType);
            XmlNode attributesNode = SaveController.CreateNodeAndAppend("Attributes", doc);
            Attributes.Save(attributesNode, saveType);
        }
    }
}