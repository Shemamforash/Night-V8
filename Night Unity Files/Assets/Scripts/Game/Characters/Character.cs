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
	    public MyFloat StarvationTolerance;
	    public MyFloat DehydrationTolerance;
	    public MyFloat Starvation;
	    public MyFloat Dehydration;
	    public MyFloat Hunger, Thirst;
	    public WeightCategory Weight;
	    public float Sight;
	    public Traits.Trait PrimaryTrait, SecondaryTrait;

        private ClassCharacter _characterClass;
        public enum WeightCategory { VeryLight, Light, Medium, Heavy, VeryHeavy };
		private List<CharacterAction> _availableActions = new List<CharacterAction>();
	    private GameObject actionButtonPrefab;

		public abstract class CharacterAction {
			private string _actionName;

			public CharacterAction(string actionName){
				_actionName = actionName;
			}

			public abstract void ExecuteAction();

			public string GetActionName()
			{
				return _actionName;
			}
		}

		public class FindResources : CharacterAction {
			public FindResources(string actionName) : base(actionName) {
			}

			public override void ExecuteAction(){
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

		public void SetCharacterUi(GameObject g){
			CharacterUi = new CharacterUI(g);
			CharacterUi.EatButton.onClick.AddListener(Eat);
			CharacterUi.DrinkButton.onClick.AddListener(Drink);
			foreach (CharacterAction a in _availableActions)
			{
				GameObject newActionButton = GameObject.Instantiate(actionButtonPrefab);
				newActionButton.transform.SetParent(CharacterUi.actionScrollContent.transform);
				newActionButton.transform.Find("Text").GetComponent<Text>().text = a.GetActionName();
				newActionButton.GetComponent<Button>().onClick.AddListener(a.ExecuteAction);
			}
		}

		public CharacterUI GetCharacterUi(){
			return CharacterUi;
		}

		public class CharacterUI {
			public GameObject GameObject;
			public Button SimpleViewButton;
			public Button EatButton;
			public Button DrinkButton;
			public GameObject actionScrollContent;
			public Text ThirstText, HungerText, StrengthText, IntelligenceText, EnduranceText, StabilityText;
			
			public CharacterUI(GameObject gameObject){
				GameObject = gameObject;
				actionScrollContent = Helper.FindChildWithName(gameObject, "Content").gameObject;
				Button backButton = Helper.FindChildWithName(gameObject, "Back Button").GetComponent<Button>();
				backButton.onClick.AddListener(gameObject.transform.parent.GetComponent<CharacterSelect>().ExitCharacter);
				EatButton = Helper.FindChildWithName(gameObject, "Eat Button").GetComponent<Button>();
				DrinkButton = Helper.FindChildWithName(gameObject, "Drink Button").GetComponent<Button>();
				SimpleViewButton = GameObject.Find("Simple").GetComponent<Button>();
				ThirstText = Helper.FindChildWithName(gameObject, "Thirst").GetComponent<Text>();
				HungerText = Helper.FindChildWithName(gameObject, "Hunger").GetComponent<Text>();
				StrengthText = Helper.FindChildWithName(gameObject, "Strength").GetComponent<Text>();
				IntelligenceText = Helper.FindChildWithName(gameObject, "Intelligence").GetComponent<Text>();
				EnduranceText = Helper.FindChildWithName(gameObject, "Endurance").GetComponent<Text>();
				StabilityText = Helper.FindChildWithName(gameObject, "Stability").GetComponent<Text>();
			}
		}

	    public void Drink()
	    {
		    Home.ConsumeResource(Resource.ResourceType.Water, 10);
	    }

	    public void Eat()
	    {
		    Home.ConsumeResource(Resource.ResourceType.Food, 10);
	    }
    }
}