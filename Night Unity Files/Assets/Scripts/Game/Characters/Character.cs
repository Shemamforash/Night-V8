using System.Collections.Generic;
using Facilitating.UI.GameOnly;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace Characters
{
    public class Character
    {
        public CharacterUI CharacterUi;
        private string _name;
        private float _strength, _strengthMax, _intelligence, _intelligenceMax, _endurance, _stability;
        private float _starvationTolerance, _dehydrationTolerance, _starvation, _dehydration;
        private WeightCategory _weight;
        private float _sight;
        private Traits.Trait _primaryTrait, _secondaryTrait;

        private ClassCharacter _characterClass;
        public enum WeightCategory { Light, Medium, Heavy };
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

        public Character(string name, string className, float strength, float intelligence, float endurance, float stability, float starvationTolerance, float dehydrationTolerance,
        WeightCategory weight, float sight, string secondaryTrait)
        {
            _name = name;
            _characterClass = ClassCharacter.FindClass(className);
            _strength = strength;
            _strengthMax = strength;
            _intelligence = intelligence;
            _intelligenceMax = intelligence;
            _endurance = endurance;
            _stability = stability;
            _starvationTolerance = starvationTolerance;
            _dehydrationTolerance = dehydrationTolerance;
            _weight = weight;
            _sight = sight;
            _primaryTrait = Traits.FindTrait(_characterClass.ClassTrait());
            _secondaryTrait = Traits.FindTrait(secondaryTrait);
	        _availableActions.Add(new FindResources("Find Resources"));
	        actionButtonPrefab = Resources.Load("Prefabs/Action Button") as GameObject;
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
			
			public CharacterUI(GameObject gameObject){
				GameObject = gameObject;
				actionScrollContent = Helper.FindChildWithName(gameObject.transform, "Content").gameObject;
				Button backButton = Helper.FindChildWithName(gameObject.transform, "Back Button").GetComponent<Button>();
				backButton.onClick.AddListener(gameObject.transform.parent.GetComponent<CharacterSelect>().ExitCharacter);
				EatButton = Helper.FindChildWithName(gameObject.transform, "Eat Button").GetComponent<Button>();
				DrinkButton = Helper.FindChildWithName(gameObject.transform, "Drink Button").GetComponent<Button>();
				SimpleViewButton = gameObject.transform.Find("Simple").GetComponent<Button>();
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