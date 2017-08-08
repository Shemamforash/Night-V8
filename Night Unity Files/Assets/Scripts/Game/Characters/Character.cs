using System.Collections.Generic;
using Facilitating.UI.GameOnly;
using Game.Characters;
using Game.Misc;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace Characters
{
    public class Character
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
            private string _actionName;
            public GameObject ActionObject;

            public CharacterAction(string actionName)
            {
                _actionName = actionName;
            }

            public abstract void ExecuteAction();

            public string GetActionName()
            {
                return _actionName;
            }
        }

        public class FindResources : CharacterAction
        {
            public FindResources(string actionName) : base(actionName)
            {
            }

            public override void ExecuteAction()
            {
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
            _availableActions.Add(new FindResources("Find Resources"));
            actionButtonPrefab = Resources.Load("Prefabs/Action Button") as GameObject;
            GameObject characterUI = GameObject.Instantiate(Resources.Load("Prefabs/Character Template") as GameObject);
            characterUI.transform.SetParent(GameObject.Find("Characters").transform);
            SetCharacterUi(characterUI);
        }

        public void SetCharacterUi(GameObject g)
        {
            CharacterUi = new CharacterUI(g);
            CharacterUi.EatButton.onClick.AddListener(Eat);
            CharacterUi.DrinkButton.onClick.AddListener(Drink);
            for (int i = 0; i < _availableActions.Count; ++i)
            {
                CharacterAction a = _availableActions[i];
                GameObject newActionButton = GameObject.Instantiate(actionButtonPrefab);
                a.ActionObject = newActionButton;
                newActionButton.transform.SetParent(CharacterUi.actionScrollContent.transform);
                newActionButton.transform.Find("Text").GetComponent<Text>().text = a.GetActionName();
                Button currentButton = newActionButton.GetComponent<Button>();
                currentButton.GetComponent<Button>().onClick.AddListener(a.ExecuteAction);
                
                Helper.SetNavigation(newActionButton, CharacterUi.WeaponCard, Helper.NavigationDirections.Left);
                if (i == _availableActions.Count - 1)
                {
                    Helper.SetNavigation(newActionButton, CharacterUi.CollapseCharacterButton.gameObject, Helper.NavigationDirections.Down);
                }

                if (i > 0)
                {
                    GameObject previousActionButton = _availableActions[i - 1].ActionObject;
                    Helper.SetNavigation(newActionButton, previousActionButton, Helper.NavigationDirections.Up);
                    Helper.SetNavigation(previousActionButton, newActionButton, Helper.NavigationDirections.Down);
                }
                else if (i == 0)
                {
                    Helper.SetNavigation(CharacterUi.WeaponCard.gameObject, newActionButton, Helper.NavigationDirections.Right);
                }
            }
            CharacterUi.NameText.text = Name;
            CharacterUi.ClassTraitText.text = _characterClass.ClassName() + " " + SecondaryTrait.Name;
            CharacterUi.DetailedClassText.text = PrimaryTrait.GetTraitDetails();
            CharacterUi.DetailedTraitText.text = SecondaryTrait.GetTraitDetails();
        }

        public CharacterUI GetCharacterUi()
        {
            return CharacterUi;
        }

        public class CharacterUI
        {
            public GameObject GameObject, SimpleView, DetailedView;
            public Button EatButton;
            public Button DrinkButton;
            public Button CollapseCharacterButton;
            public GameObject actionScrollContent, WeaponCard;
            public Text ThirstText, HungerText, StrengthText, IntelligenceText, EnduranceText, StabilityText;
            public Text StrengthTextDetail, IntelligenceTextDetail, EnduranceTextDetail, StabilityTextDetail;
            public Text NameText, ClassTraitText, DetailedClassText, DetailedTraitText;
            public Text ConditionsText;

            public CharacterUI(GameObject gameObject)
            {
                GameObject = gameObject;
                SimpleView = GameObject.transform.Find("Simple").gameObject;
                DetailedView = GameObject.transform.Find("Detailed").gameObject;
                
                actionScrollContent = Helper.FindChildWithName(gameObject, "Content").gameObject;
                CollapseCharacterButton = FindInDetailedView<Button>("Back Button");
                CollapseCharacterButton.onClick.AddListener(
                    gameObject.transform.parent.GetComponent<CharacterSelect>().ExitCharacter);
                
                ThirstText = FindInSimpleView<Text>("Thirst");
                HungerText = FindInSimpleView<Text>("Hunger");
                StrengthText = FindInSimpleView<Text>("Strength");
                IntelligenceText = FindInSimpleView<Text>("Intelligence");
                EnduranceText = FindInSimpleView<Text>("Endurance");
                StabilityText = FindInSimpleView<Text>("Stability");

                NameText = FindInSimpleView<Text>("Simple Name");
                ClassTraitText = FindInSimpleView<Text>("ClassTrait");

                DetailedClassText = FindInDetailedView<Text>("Class");
                DetailedTraitText = FindInDetailedView<Text>("Trait");
                
                EatButton = FindInDetailedView<Button>("Eat Button");
                DrinkButton = FindInDetailedView<Button>("Drink Button");
                WeaponCard = Helper.FindChildWithName(DetailedView, "Weapon Card").gameObject;
                ConditionsText = FindInDetailedView<Text>("Conditions");
                
                StrengthTextDetail = FindInDetailedView<Text>("Strength");
                IntelligenceTextDetail = FindInDetailedView<Text>("Intelligence");
                EnduranceTextDetail = FindInDetailedView<Text>("Endurance");
                StabilityTextDetail = FindInDetailedView<Text>("Stability");
            }

            public T FindInSimpleView<T>(string name)
            {
                return Helper.FindChildWithName(SimpleView, name).GetComponent<T>();
            }

            public T FindInDetailedView<T>(string name)
            {
                return Helper.FindChildWithName(DetailedView, name).GetComponent<T>();
            }
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