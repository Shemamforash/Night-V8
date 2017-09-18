using System.Collections.Generic;
using System.Linq;
using Game.Characters.CharacterActions;
using Game.World;
using Game.World.Region;
using Game.World.Time;
using SamsHelper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class DesolationCharacter : Character
    {
        public Region CurrentRegion;
        public Traits.Trait CharacterClass, CharacterTrait;

        protected override void Awake()
        {
            Attributes = new CharacterAttributes(this);
            AddState(new CollectResources(this));
            AddState(new CharacterActions.Combat(this));
            AddState(new Sleep(this));
            AddState(new Idle(this));
            AddState(new PrepareTravel(this));
            AddState(new Travel(this));
            AddState(new Return(this));
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

        public void Initialise(string characterName, Traits.Trait classCharacter, Traits.Trait characterTrait)
        {
            CharacterInventory = new DesolationInventory(characterName);
            CharacterClass = classCharacter;
            CharacterTrait = characterTrait;
            Initialise(characterName);
            UpdateActionUi();
            SetDefaultState("Idle");
        }

        public override void Kill()
        {
            CharacterManager.RemoveCharacter(this, CharacterName == "Driver");
        }

        public void Drink()
        {
            int consumed = WorldState.Inventory().DecrementResource("Water", 1);
            Attributes.Dehydration.Val -= consumed;
        }

        public void Eat()
        {
            int consumed = WorldState.Inventory().DecrementResource("Food", 1);
            Attributes.Starvation.Val -= consumed;
        }

        public float RemainingCarryCapacity()
        {
            return Attributes.Strength.Val;
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
                    NavigateToState(a.Name());
                });

                Helper.SetNavigation(newActionButton, CharacterUi.WeaponCard, Helper.NavigationDirections.Left);
                if (i == _availableActions.Count - 1)
                {
                    Helper.SetNavigation(newActionButton, CharacterUi.CollapseCharacterButton.gameObject,
                        Helper.NavigationDirections.Down);
                    Helper.SetNavigation(CharacterUi.CollapseCharacterButton.gameObject, newActionButton,
                        Helper.NavigationDirections.Up);
                }

                if (i > 0)
                {
                    GameObject previousActionButton = _availableActions[i - 1].ActionButtonGameObject;
                    Helper.SetNavigation(newActionButton, previousActionButton, Helper.NavigationDirections.Up);
                    Helper.SetNavigation(previousActionButton, newActionButton, Helper.NavigationDirections.Down);
                }
                else if (i == 0)
                {
                    Helper.SetNavigation(CharacterUi.WeaponCard.gameObject, newActionButton,
                        Helper.NavigationDirections.Right);
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
            CharacterUi.EatButton.onClick.AddListener(Eat);
            CharacterUi.DrinkButton.onClick.AddListener(Drink);

            CharacterUi.NameText.text = CharacterName;
            CharacterUi.ClassTraitText.text = CharacterTrait.Name + " " + CharacterClass.Name;
            CharacterUi.DetailedClassText.text = CharacterClass.GetTraitDetails();
            CharacterUi.DetailedTraitText.text = CharacterTrait.GetTraitDetails();
            CharacterUi.WeightText.text = "Weight: " + Attributes.Weight + " (requires " + ((int) Attributes.Weight + 5) + " fuel)";

            WorldTime.Instance().MinuteEvent += delegate
            {
                string currentActionString = GetCurrentState().Name() + " " + ((BaseCharacterAction) GetCurrentState()).GetCostAsString();
                CharacterUi.CurrentActionText.text = currentActionString;
                CharacterUi.DetailedCurrentActionText.text = currentActionString;
            };

            Attributes.Strength.AddOnValueChange(delegate(int f)
            {
                CharacterUi.StrengthText.text = f + " <sprite name=\"Strength\">";
                CharacterUi.StrengthTextDetail.text = f + "/" + Attributes.Strength.Max + " <sprite name=\"Strength\">";
            });
            Attributes.Endurance.AddOnValueChange(delegate(int f)
            {
                CharacterUi.EnduranceText.text = f + " <sprite name=\"Endurance\">";
                CharacterUi.EnduranceTextDetail.text = f + "/" + Attributes.Endurance.Max + " <sprite name=\"Endurance\">";
            });
            Attributes.Stability.AddOnValueChange(delegate(int f)
            {
                CharacterUi.StabilityText.text = f + " <sprite name=\"Stability\">";
                CharacterUi.StabilityTextDetail.text = f + "/" + Attributes.Stability.Max + " <sprite name=\"Stability\">";
            });
            Attributes.Intelligence.AddOnValueChange(delegate(int f)
            {
                CharacterUi.IntelligenceText.text = f + " <sprite name=\"Intelligence\">";
                CharacterUi.IntelligenceTextDetail.text = f + "/" + Attributes.Intelligence.Max + " <sprite name=\"Intelligence\">";
            });
            Attributes.Hunger.AddOnValueChange(f => CharacterUi.HungerText.text = Attributes.GetHungerStatus());
            Attributes.Thirst.AddOnValueChange(f => CharacterUi.ThirstText.text = Attributes.GetThirstStatus());
        }
        
        protected override bool IsOverburdened()
        {
            return CharacterInventory.GetInventoryWeight() > Attributes.Strength.Val;
        }
        
        private void Tire(int amount)
        {
            Attributes.Endurance.Val -= IsOverburdened() ? amount * 2 : amount;
            CheckEnduranceZero();
        }

        private void CheckEnduranceZero()
        {
            if (!Attributes.Endurance.ReachedMin()) return;
            BaseCharacterAction action = GetCurrentState() as BaseCharacterAction;
            action.Interrupt();
            Sleep sleepAction = NavigateToState("Sleep") as Sleep;
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
                NavigateToState("Idle");
            }
        }
        
        public void Travel()
        {
            Tire(CalculateEnduranceCostForDistance(1));
        }

        public int CalculateTotalWeight()
        {
            int characterWeight = 5 + (int) Attributes.Weight;
            int inventoryWeight = (int)(CharacterInventory.GetInventoryWeight() / 10);
            return characterWeight + inventoryWeight;
        }
        
        public int CalculateEnduranceCostForDistance(int distance)
        {
            return distance * CalculateTotalWeight();
        }
    }
}