using System.Collections.Generic;
using System.Linq;
using Game.Characters.CharacterActions;
using Game.World;
using Game.World.Region;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class DesolationCharacter : Character
    {
        public Region CurrentRegion;
        public TraitLoader.Trait CharacterClass, CharacterTrait;
        private CharacterConditions _conditions;

        public DesolationCharacter(string name, TraitLoader.Trait characterClass, TraitLoader.Trait characterTrait, GameObject gameObject) : base(name, gameObject)
        {
            CharacterInventory = new DesolationInventory(name);
            CharacterClass = characterClass;
            CharacterTrait = characterTrait;
            Attributes = new DesolationCharacterAttributes(this);
            
            SetCharacterUi();
            
            ActionStates.AddState(new CollectResources(this));
            ActionStates.AddState(new CharacterActions.Combat(this));
            ActionStates.AddState(new Sleep(this));
            ActionStates.AddState(new Idle(this));
            ActionStates.AddState(new PrepareTravel(this));
            ActionStates.AddState(new Travel(this));
            ActionStates.AddState(new Return(this));
            ActionStates.AddState(new LightFire(this));
            ActionStates.SetDefaultState("Idle");
            
            UpdateActionUi();
            
            _conditions = new CharacterConditions(this);
            CharacterInventory.MaxWeight = 50;
        }

        public Condition GetCondition(ConditionType type)
        {
            return _conditions.Conditions[type];
        }

        public void SetActionListActive(bool active)
        {
            CharacterUiDetailed.ActionMenuList.gameObject.SetActive(active);
            CharacterUiDetailed.DetailedCurrentActionText.gameObject.SetActive(!active);
        }
        
        public override void TakeDamage(int amount)
        {
            Attributes.Strength.Val -= amount;
            if (Attributes.Strength.ReachedMin())
            {
                //TODO kill character
            }
        }

        public override void Kill()
        {
            WorldState.HomeInventory.RemoveItem(this);
        }
        
        private void UpdateActionUi()
        {
            List<BaseCharacterAction> availableActions = StatesAsList(false).Cast<BaseCharacterAction>().ToList();
            CharacterUiDetailed.ActionMenuList.SetItems(new List<MyGameObject>(availableActions));

            List<InventoryUi> actionUiList = CharacterUiDetailed.ActionMenuList.GetItems();
            for (int i = 0; i < actionUiList.Count; ++i)
            {
                InventoryUi actionUi = actionUiList[i];
                
                Helper.SetNavigation(actionUi.GetNavigationButton(), CharacterUiDetailed.WeaponGearUi.GearButton.gameObject, Direction.Left);
                if (i == availableActions.Count - 1)
                {
                    Helper.SetReciprocalNavigation(actionUi.GetNavigationButton(), CharacterUiDetailed.CollapseCharacterButton.gameObject);
                }

                else if (i == 0)
                {
                    Helper.SetNavigation(CharacterUiDetailed.WeaponGearUi.GearButton.gameObject, actionUi.GetNavigationButton(),
                       Direction.Right);
                    Helper.SetNavigation(CharacterUiDetailed.ArmourGearUi.GearButton.gameObject, actionUi.GetNavigationButton(),
                        Direction.Right);
                    Helper.SetNavigation(CharacterUiDetailed.AccessoryGearUi.GearButton.gameObject, actionUi.GetNavigationButton(),
                        Direction.Right);
                }
            }
        }

        protected override void SetCharacterUi()
        {
            base.SetCharacterUi();

            CharacterUiDetailed.NameText.text = Name;
            CharacterUiDetailed.ClassTraitText.text = CharacterTrait.Name + " " + CharacterClass.Name;
            CharacterUiDetailed.DetailedClassText.text = CharacterClass.GetTraitDetails();
            CharacterUiDetailed.DetailedTraitText.text = CharacterTrait.GetTraitDetails();
            CharacterUiDetailed.WeightText.text = "Weight: " + Attributes.Weight + " (requires " + ((int) Attributes.Weight + 5) + " fuel)";

            WorldState.Instance().MinuteEvent += delegate
            {
                string currentActionString = ActionStates.GetCurrentState().Name + " " + ((BaseCharacterAction) ActionStates.GetCurrentState()).GetCostAsString();
                CharacterUiDetailed.CurrentActionText.text = currentActionString;
                CharacterUiDetailed.DetailedCurrentActionText.text = currentActionString;
            };
            Attributes.BindUi();
        }
        
        protected override bool IsOverburdened()
        {
            return CharacterInventory.Weight > Attributes.Strength.Val;
        }
        
        private void Tire(int amount)
        {
            Attributes.Endurance.Val -= IsOverburdened() ? amount * 2 : amount;
            CheckEnduranceZero();
        }

        private void CheckEnduranceZero()
        {
            if (!Attributes.Endurance.ReachedMin()) return;
            BaseCharacterAction action = ActionStates.GetCurrentState() as BaseCharacterAction;
            action.Interrupt();
            Sleep sleepAction = ActionStates.NavigateToState("Sleep") as Sleep;
            sleepAction.SetDuration((int)(Attributes.Endurance.Max / 5f));
            sleepAction.SetStateTransitionTarget(action.Name);
            sleepAction.AddOnExit(() =>
            {
                action.Resume();
            });
            sleepAction.Start();
        }

        public void Rest(int amount)
        {
            Attributes.Endurance.Val += amount;
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
            int inventoryWeight = (int)(CharacterInventory.Weight / 10);
            return characterWeight + inventoryWeight;
        }
        
        public int CalculateEnduranceCostForDistance(int distance)
        {
            return distance * CalculateTotalWeight();
        }
    }
}