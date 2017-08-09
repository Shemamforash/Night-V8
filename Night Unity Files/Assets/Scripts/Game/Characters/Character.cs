using System.Collections.Generic;
using Game.Characters;
using Game.Misc;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using World;

namespace Characters
{
    public partial class Character
    {
        public CharacterUI CharacterUi;
        public string Name;
        public MyFloat Strength;
        public MyFloat Intelligence;
        public MyFloat Endurance;
        public MyFloat Stability;
        public float StarvationTolerance, DehydrationTolerance;
        public MyFloat Starvation;
        public MyFloat Dehydration;
        public MyFloat Hunger, Thirst;
        public WeightCategory Weight;
        public float Sight;
        public Traits.Trait PrimaryTrait, SecondaryTrait;

        private ClassCharacter _characterClass;

        public enum WeightCategory
        {
            VeryLight,
            Light,
            Medium,
            Heavy,
            VeryHeavy
        };

        private List<CharacterAction> _availableActions = new List<CharacterAction>();
        private GameObject actionButtonPrefab;

        public abstract class CharacterAction
        {
            private readonly string _actionName;
            public GameObject ActionObject;
            private readonly bool _durationFixed;
            private int _duration;
            private TimeListener _timeListener = new TimeListener();
            private MyFloat _timeRemaining;
            private Character _parent;

            public int Duration
            {
                get { return _duration; }
                set
                {
                    if (!_durationFixed)
                    {
                        _duration = value;
                    }
                }
            }

            public Character Parent()
            {
                return _parent;
            }

            protected CharacterAction(string actionName, bool durationFixed, int duration, Character parent)
            {
                _actionName = actionName;
                _durationFixed = durationFixed;
                _duration = duration;
                _timeListener.OnHour(UpdateTime);
                _parent = parent;
                TextAssociation currentActionAssociation = new TextAssociation(_parent.CharacterUi.CurrentActionText,
                    f => _actionName + " (" + (int)f + "hrs)", true);
                _timeRemaining = new MyFloat(_duration, currentActionAssociation);
            }

            protected virtual void ExecuteAction()
            {
                _parent.CharacterUi.CurrentActionText.text = "Doing nothing";
            }

            public virtual void InitialiseAction()
            {
                _timeRemaining.Value = _duration;
            }

            protected void ImmobiliseParent()
            {
                //for preventing weapon switching when exploring
            }

            public string GetActionName()
            {
                return _actionName;
            }

            public bool IsDurationFixed()
            {
                return _durationFixed;
            }

            private void UpdateTime()
            {
                if (_timeRemaining > 0)
                {
                    --_timeRemaining.Value;
                    if (_timeRemaining == 0)
                    {
                        ExecuteAction();
                    }
                }
            }
        }

        public class FindResources : CharacterAction
        {
            public FindResources(Character c) : base("Find Resources", false, 1, c)
            {
            }

            protected override void ExecuteAction()
            {
                base.ExecuteAction();
                Home.IncrementResource(Resource.ResourceType.Water, 1);
                Home.IncrementResource(Resource.ResourceType.Food, 1);
            }
        }

        public Character(string name, ClassCharacter classCharacter, WeightCategory weight, Traits.Trait secondaryTrait)
        {
            Name = name;
            _characterClass = classCharacter;
            Weight = weight;
            PrimaryTrait = Traits.FindTrait(_characterClass.ClassTrait());
            SecondaryTrait = secondaryTrait;

            GameObject characterUi = GameObject.Instantiate(Resources.Load("Prefabs/Character Template") as GameObject);
            characterUi.transform.SetParent(GameObject.Find("Characters").transform);
            SetCharacterUi(characterUi);

            _availableActions.Add(new FindResources(this));
            actionButtonPrefab = Resources.Load("Prefabs/Action Button") as GameObject;

            UpdateActionUi();
        }

        private void UpdateActionUi()
        {
            for (int i = 0; i < _availableActions.Count; ++i)
            {
                CharacterAction a = _availableActions[i];
                GameObject newActionButton = GameObject.Instantiate(actionButtonPrefab);
                a.ActionObject = newActionButton;
                newActionButton.transform.SetParent(CharacterUi.actionScrollContent.transform);
                newActionButton.transform.Find("Text").GetComponent<Text>().text = a.GetActionName();
                Button currentButton = newActionButton.GetComponent<Button>();
                currentButton.GetComponent<Button>().onClick
                    .AddListener(() => WorldState.MenuNavigator.ShowActionDurationMenu(a));

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
                    GameObject previousActionButton = _availableActions[i - 1].ActionObject;
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

        private void SetCharacterUi(GameObject g)
        {
            CharacterUi = new CharacterUI(g);
            CharacterUi.EatButton.onClick.AddListener(Eat);
            CharacterUi.DrinkButton.onClick.AddListener(Drink);

            CharacterUi.NameText.text = Name;
            CharacterUi.ClassTraitText.text = _characterClass.ClassName() + " " + SecondaryTrait.Name;
            CharacterUi.DetailedClassText.text = PrimaryTrait.GetTraitDetails();
            CharacterUi.DetailedTraitText.text = SecondaryTrait.GetTraitDetails();
            CharacterUi.WeightText.text = "Weight: " + Weight + " (requires " + ((int) Weight + 5) + " fuel)";
        }

        public string GetConditions()
        {
            string conditions = "";
            conditions += GetThirstStatus() + "(" + Mathf.Round(Thirst.Value / 1.2f) / 10f + " litres/hr)\n";
            conditions += GetHungerStatus() + "(" + Mathf.Round(Hunger.Value / 1.2f) / 10f + " meals/hr)";
            return conditions;
        }

        public string GetHungerStatus(float f)
        {
            if (f == 0)
            {
                return "Full";
            }
            if (f < Hunger)
            {
                return "Sated";
            }
            if (f < Hunger * 2f)
            {
                return "Hungry";
            }
            if (f < Hunger * 3f)
            {
                Eat();
            }
            if (f >= StarvationTolerance)
            {
                Kill();
            }
            return "Starving";
        }

        public string GetHungerStatus()
        {
            return GetHungerStatus(Starvation.Value);
        }

        public void Kill()
        {
            CharacterManager.RemoveCharacter(this, Name == "Driver");
        }

        public string GetThirstStatus(float f)
        {
            if (f == 0)
            {
                return "Slaked";
            }
            if (f < Thirst)
            {
                return "Quenched";
            }
            if (f < Thirst * 2f)
            {
                return "Thirsty";
            }
            if (f < Thirst * 3f)
            {
                Drink();
            }
            if (f >= DehydrationTolerance)
            {
                Kill();
            }
            return "Parched";
        }

        public string GetThirstStatus()
        {
            return GetThirstStatus(Dehydration.Value);
        }

        public CharacterUI GetCharacterUi()
        {
            return CharacterUi;
        }

        public void Drink()
        {
            float consumed = Home.ConsumeResource(Resource.ResourceType.Water, 0.25f);
            Dehydration.Value -= consumed;
        }

        public void Eat()
        {
            float consumed = Home.ConsumeResource(Resource.ResourceType.Food, 1);
            Starvation.Value -= consumed;
        }
    }
}