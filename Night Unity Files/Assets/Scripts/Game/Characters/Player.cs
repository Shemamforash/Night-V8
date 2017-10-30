using System;
using Game.Characters.Attributes;
using Game.Characters.CharacterActions;
using Game.World;
using Game.World.Region;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Characters
{
    public class Player : Character
    {
        public readonly TraitLoader.Trait CharacterClass, CharacterTrait;
        public Region CurrentRegion;
        public CharacterView CharacterView;
        public readonly SurvivalAttributes SurvivalAttributes;
        
        //Create Character in code only- no view section, no references to objects in the scene
        public Player(string name, TraitLoader.Trait characterClass, TraitLoader.Trait characterTrait) : base(name)
        {
            SurvivalAttributes = new SurvivalAttributes(this);
            CombatStates.EnableInput();
            CharacterInventory = new DesolationInventory(name);
            CharacterClass = characterClass;
            CharacterTrait = characterTrait;
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
            States.AddState(new CollectResources(this));
            States.AddState(new CharacterActions.Combat(this));
            States.AddState(new Sleep(this));
            States.AddState(new Idle(this));
            States.AddState(new PrepareTravel(this));
            States.AddState(new Travel(this));
            States.AddState(new Return(this));
            States.AddState(new LightFire(this));
            States.SetDefaultState("Idle");
            CharacterView.UpdateActionUi();
        }
        
        private bool IsOverburdened()
        {
            return CharacterInventory.Weight > BaseAttributes.Strength.GetCurrentValue();
        }
        
        private void Tire(int amount)
        {
            BaseAttributes.Endurance.SetCurrentValue(BaseAttributes.Endurance.GetCurrentValue() - (IsOverburdened() ? amount * 2 : amount));
            CheckEnduranceZero();
        }

        private void CheckEnduranceZero()
        {
            if (!BaseAttributes.Endurance.ReachedMin()) return;
            BaseCharacterAction action = States.GetCurrentState() as BaseCharacterAction;
            action.Interrupt();
            Sleep sleepAction = States.NavigateToState("Sleep") as Sleep;
            sleepAction.SetDuration((int) (BaseAttributes.Endurance.Max / 5f));
            sleepAction.SetStateTransitionTarget(action.Name);
            sleepAction.AddOnExit(() => { action.Resume(); });
            sleepAction.Start();
        }

        public void Rest(int amount)
        {
            BaseAttributes.Endurance.SetCurrentValue(BaseAttributes.Endurance.GetCurrentValue() + amount);
            if (!BaseAttributes.Endurance.ReachedMax()) return;
            if (CurrentRegion == null)
            {
                States.NavigateToState("Idle");
            }
        }

        public void Travel()
        {
            Tire(CalculateEnduranceCostForDistance(1));
        }

        public int CalculateTotalWeight()
        {
            int characterWeight = 5 + (int) SurvivalAttributes.Weight;
            int inventoryWeight = (int) (CharacterInventory.Weight / 10);
            return characterWeight + inventoryWeight;
        }

        public int CalculateEnduranceCostForDistance(int distance)
        {
            return distance * CalculateTotalWeight();
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
    }
}