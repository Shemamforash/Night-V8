using System.Xml;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.WorldEvents;
using Game.Gear.Armour;
using Game.Global;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine;

namespace Game.Characters
{
	public sealed class Player : Character
	{
		private readonly string[] _restEvents =
		{
			"Awake again, or am I still dreaming?",
			"Rested, yet something is not right",
			"Dark day follows dark night, when will it end?"
		};

		private readonly string[] _tireEvents =
		{
			"I'm so tired, I need to rest",
			"If only I could close my eyes for a moment",
			"I can't remember when I last had some sleep"
		};

		private CharacterView _characterView;
		private int           _daysSurvived;
		private int           _timeAlive;

		public readonly CharacterAttributes Attributes;
		public readonly BrandManager        BrandManager = new BrandManager();
		public readonly CharacterTemplate   CharacterTemplate;
		public readonly StateMachine        States = new StateMachine();

		public Consume ConsumeAction;
		public Craft   CraftAction;
		public Rest    RestAction;
		public Travel  TravelAction;
		public bool    IsDead;


		//Create Character in code only- no view section, no references to objects in the scene
		public Player(CharacterTemplate characterTemplate) : base("The " + characterTemplate.CharacterClass)
		{
			Attributes        = new CharacterAttributes(this);
			CharacterTemplate = characterTemplate;
			AddStates();
			BrandManager.Initialise(this);
		}

		public override XmlNode Save(XmlNode root)
		{
			root = base.Save(root);
			Attributes.Save(root);
			BrandManager.Save(root);
			root.CreateChild("TimeAlive",      _timeAlive);
			root.CreateChild("DaysSurvived",   _daysSurvived);
			root.CreateChild("CharacterClass", CharacterTemplate.CharacterClass.ToString());
			((BaseCharacterAction) States.GetCurrentState()).Save(root);
			return root;
		}

		public override void Load(XmlNode root)
		{
			base.Load(root);
			Attributes.Load(root);
			BrandManager.Load(root);
			_timeAlive    = root.ParseInt("TimeAlive");
			_daysSurvived = root.ParseInt("DaysSurvived");
			LoadCurrentAction(root);
		}

		private void LoadCurrentAction(XmlNode root)
		{
			XmlNode             currentActionNode = root.SelectSingleNode("CurrentAction");
			string              currentActionName = currentActionNode.ParseString("Name");
			BaseCharacterAction currentAction;
			switch (currentActionName)
			{
				case "Craft":
					currentAction = CraftAction;
					break;
				case "Travel":
					currentAction = TravelAction;
					break;
				default:
					currentAction = RestAction;
					break;
			}

			if (currentAction != RestAction) currentAction.Load(root);
			currentAction.Enter();
		}

		public void Kill(DeathReason deathReason)
		{
			IsDead = true;
			if (Accessory != null)
			{
				Accessory.UnEquip();
				Inventory.Move(Accessory);
			}

			if (States.GetCurrentState() is Craft) CraftAction.AbortCraft();
			if (CharacterTemplate.CharacterClass == CharacterClass.Wanderer)
				SceneChanger.GoToGameOverScene(deathReason);
			else
				CharacterManager.RemoveCharacter(this);
		}

		private void AddStates()
		{
			RestAction    = new Rest(this);
			TravelAction  = new Travel(this);
			CraftAction   = new Craft(this);
			ConsumeAction = new Consume(this);
			States.SetDefaultState(RestAction);
		}

		public void Update()
		{
			if (MapMenuController.CharacterReturning != null) return;
			((BaseCharacterAction) States.GetCurrentState()).UpdateAction();
			UpdateTimeAlive();
		}

		private void UpdateTimeAlive()
		{
			++_timeAlive;
			if (_timeAlive != WorldState.MinutesPerHour * 24) return;
			IncreaseTimeSurvived();
			_timeAlive = 0;
		}

		private void IncreaseTimeSurvived()
		{
			if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Tutorial) return;
			++_daysSurvived;
		}

		public void SetCharacterView(CharacterView characterView)
		{
			_characterView = characterView;
		}

		public bool CanAffordTravel(int travelCost = 1)
		{
			int lifeRemaining = Mathf.CeilToInt(Attributes.Life.CurrentValue);
			return lifeRemaining >= travelCost;
		}

		public void Rest()
		{
			if (!CanRest()) return;
			Attributes.Will.Increment();
			Attributes.Life.Increment();
			if (CanRest()) return;
			WorldEventManager.GenerateEvent(new CharacterMessage(_restEvents.RandomElement(), this));
		}

		public override void EquipAccessory(Accessory accessory)
		{
			base.EquipAccessory(accessory);
			if (_characterView != null) _characterView.AccessoryController.UpdateAccessory();
		}

		private bool CanRest() => Attributes.Life < Attributes.Life.Max || Attributes.Will < Attributes.Will.Max;

		public CharacterView CharacterView() => _characterView;

		public void ApplyModifier(AttributeType attributeType, AttributeModifier modifier)
		{
			CharacterAttribute attribute = Attributes.Get(attributeType);
			if (attributeType.IsCoreAttribute() && attribute.Max == 20) return;
			attribute.Max += modifier.Value;
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		public void RemoveModifier(AttributeType attributeType, AttributeModifier modifier)
		{
			CharacterAttribute attribute = Attributes.Get(attributeType);
			attribute.Max -= modifier.Value;
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}
	}
}