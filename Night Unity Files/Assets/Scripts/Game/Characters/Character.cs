using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World;
using UI.GameOnly;

namespace Characters
{
    public class Character
    {
        public CharacterUI characterUI;
        private string name;
        private float strength, strengthMax, intelligence, intelligenceMax, endurance, stability;
        private float starvationTolerance, dehydrationTolerance, starvation, dehydration;
        private WeightCategory weight;
        private float sight;
        private Traits.Trait primaryTrait, secondaryTrait;

        private ClassCharacter characterClass;
        public enum WeightCategory { LIGHT, MEDIUM, HEAVY };
		private List<CharacterAction> availableActions = new List<CharacterAction>();

		public abstract class CharacterAction {
			private string actionName;

			public CharacterAction(string actionName){
				this.actionName = actionName;
			}

			public abstract void ExecuteAction();
		}

		public class FindResources : CharacterAction {
			public FindResources(string actionName) : base(actionName) {
			}

			public override void ExecuteAction(){
				//find some food, water, and fuel
			}
		}

        public Character(string name, string className, float strength, float intelligence, float endurance, float stability, float starvationTolerance, float dehydrationTolerance,
        WeightCategory weight, float sight, string secondaryTrait)
        {
            this.name = name;
            this.characterClass = ClassCharacter.FindClass(className);
            this.strength = strength;
            this.strengthMax = strength;
            this.intelligence = intelligence;
            this.intelligenceMax = intelligence;
            this.endurance = endurance;
            this.stability = stability;
            this.starvationTolerance = starvationTolerance;
            this.dehydrationTolerance = dehydrationTolerance;
            this.weight = weight;
            this.sight = sight;
            this.primaryTrait = Traits.FindTrait(characterClass.ClassTrait());
            this.secondaryTrait = Traits.FindTrait(secondaryTrait);
        }

		public void SetCharacterUI(GameObject g){
			characterUI  = new CharacterUI(g);
		}

		public CharacterUI GetCharacterUI(){
			return characterUI;
		}

		public class CharacterUI {
			public GameObject gameObject;
			public Button simpleViewButton;
			public Button eatButton;
			
			public CharacterUI(GameObject gameObject){
				this.gameObject = gameObject;
				Button backButton = Helper.FindChildWithName(gameObject.transform, "Back Button").GetComponent<Button>();
				backButton.onClick.AddListener(gameObject.transform.parent.GetComponent<CharacterSelect>().ExitCharacter);
				eatButton = Helper.FindChildWithName(gameObject.transform, "Eat Button").GetComponent<Button>();
				simpleViewButton = gameObject.transform.Find("Simple").GetComponent<Button>();
			}
		}
    }
}