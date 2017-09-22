using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters.CharacterActions;
using Game.World;
using Game.World.Region;
using Game.World.Time;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class DesolationCharacter : Character
    {
        public Region CurrentRegion;
        public TraitLoader.Trait CharacterClass, CharacterTrait;

        public DesolationCharacter(string name, TraitLoader.Trait characterClass, TraitLoader.Trait characterTrait, GameObject gameObject) : base(name, gameObject)
        {
            CharacterInventory = new DesolationInventory(name);
            CharacterClass = characterClass;
            CharacterTrait = characterTrait;
            Attributes = new DesolationCharacterAttributes(this);
            SetCharacterUi(gameObject);
            UpdateActionUi();
            
            ActionStates.AddState(new CollectResources(this));
            ActionStates.AddState(new CharacterActions.Combat(this));
            ActionStates.AddState(new Sleep(this));
            ActionStates.AddState(new Idle(this));
            ActionStates.AddState(new PrepareTravel(this));
            ActionStates.AddState(new Travel(this));
            ActionStates.AddState(new Return(this));
            ActionStates.SetDefaultState("Idle");
            
            CharacterInventory.MaxWeight = 50;
        }

        public void SetActionListActive(bool active)
        {
            CharacterUi.ActionScrollContent.SetActive(active);
            CharacterUi.DetailedCurrentActionText.gameObject.SetActive(!active);
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
            DesolationCharacterManager.RemoveCharacter(this, Name == "Driver");
        }

        
        
        private void UpdateActionUi()
        {
            List<BaseCharacterAction> _availableActions = StatesAsList(false).Cast<BaseCharacterAction>().ToList();
            for (int i = 0; i < _availableActions.Count; ++i)
            {
                BaseCharacterAction a = _availableActions[i];
                GameObject newActionButton = Helper.InstantiateUiObject("Prefabs/Action Button", CharacterUi.ActionScrollContent.transform);
                a.ActionButtonGameObject = newActionButton;
                newActionButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = a.Name();
                Button currentButton = newActionButton.GetComponent<Button>();
                currentButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    CharacterUi.CollapseCharacterButton.Select();
                    ActionStates.NavigateToState(a.Name());
                });

//                Helper.SetNavigation(newActionButton, CharacterUi.WeaponCard, Helper.NavigationDirections.Left);
                if (i == _availableActions.Count - 1)
                {
                    Helper.SetNavigation(newActionButton, CharacterUi.CollapseCharacterButton.gameObject,
                        Direction.Down);
                    Helper.SetNavigation(CharacterUi.CollapseCharacterButton.gameObject, newActionButton,
                        Direction.Up);
                }

                if (i > 0)
                {
                    GameObject previousActionButton = _availableActions[i - 1].ActionButtonGameObject;
                    Helper.SetNavigation(newActionButton, previousActionButton, Direction.Up);
                    Helper.SetNavigation(previousActionButton, newActionButton, Direction.Down);
                }
                else if (i == 0)
                {
//                    Helper.SetNavigation(CharacterUi.WeaponCard.gameObject, newActionButton,
//                        Helper.NavigationDirections.Right);
                }
            }
        }

        public string GetConditions()
        {
            string conditions = "";
            conditions += Attributes.GetThirstStatus() + "(" + Mathf.Round(Attributes.Thirst.Val / 1.2f) / 10f + " litres/hr)\n";
            conditions += Attributes.GetThirstStatus() + "(" + Mathf.Round(Attributes.Hunger.Val / 1.2f) / 10f + " meals/hr)";
            return conditions;
        }
        
        protected override void SetCharacterUi(GameObject g)
        {
            base.SetCharacterUi(g);

            CharacterUi.NameText.text = Name;
            CharacterUi.ClassTraitText.text = CharacterTrait.Name + " " + CharacterClass.Name;
            CharacterUi.DetailedClassText.text = CharacterClass.GetTraitDetails();
            CharacterUi.DetailedTraitText.text = CharacterTrait.GetTraitDetails();
            CharacterUi.WeightText.text = "Weight: " + Attributes.Weight + " (requires " + ((int) Attributes.Weight + 5) + " fuel)";

            WorldTime.Instance().MinuteEvent += delegate
            {
                string currentActionString = ActionStates.GetCurrentState().Name() + " " + ((BaseCharacterAction) ActionStates.GetCurrentState()).GetCostAsString();
                CharacterUi.CurrentActionText.text = currentActionString;
                CharacterUi.DetailedCurrentActionText.text = currentActionString;
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
            sleepAction.SetStateTransitionTarget(action.Name());
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