using System;
using System.Collections.Generic;
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
using Game.Combat.Misc;
using Game.Gear.Weapons;
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

		private readonly Dictionary<WeaponType, int> _weaponKills = new Dictionary<WeaponType, int>();
		private          CharacterView               _characterView;
		private          int                         _daysSurvived;
		private          int                         _timeAlive;

		public readonly CharacterAttributes Attributes;
		public readonly BrandManager        BrandManager = new BrandManager();

		private bool _skill1Unlocked, _skill2Unlocked, _skill3Unlocked, _skill4Unlocked;

//		public readonly Skill               CharacterSkillOne, CharacterSkillTwo;
		public readonly CharacterTemplate CharacterTemplate;
		public readonly StateMachine      States = new StateMachine();

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

		public void UnlockSkill()
		{
			switch (EnvironmentManager.CurrentEnvironmentType)
			{
				case EnvironmentType.Desert:
					_skill1Unlocked = true;
					break;
				case EnvironmentType.Mountains:
					_skill2Unlocked = true;
					break;
				case EnvironmentType.Sea:
					_skill3Unlocked = true;
					break;
				case EnvironmentType.Ruins:
					_skill4Unlocked = true;
					break;
			}
		}

		public Skill SkillOne()   => !_skill1Unlocked ? null : CharacterSkills.GetCharacterSkillOne(this);
		public Skill SkillTwo()   => !_skill2Unlocked ? null : CharacterSkills.GetCharacterSkillTwo(this);
		public Skill SkillThree() => !_skill3Unlocked ? null : WeaponSkills.GetWeaponSkillOne(Weapon);
		public Skill SkillFour()  => !_skill4Unlocked ? null : WeaponSkills.GetWeaponSkillTwo(Weapon);

		public override XmlNode Save(XmlNode root)
		{
			root = base.Save(root);
			Attributes.Save(root);
			BrandManager.Save(root);
			root.CreateChild("TimeAlive",             _timeAlive);
			root.CreateChild("DaysSurvived",          _daysSurvived);
			root.CreateChild("CharacterClass",        CharacterTemplate.CharacterClass.ToString());
			root.CreateChild(nameof(_skill1Unlocked), _skill1Unlocked);
			root.CreateChild(nameof(_skill2Unlocked), _skill2Unlocked);
			root.CreateChild(nameof(_skill3Unlocked), _skill3Unlocked);
			root.CreateChild(nameof(_skill4Unlocked), _skill4Unlocked);
			((BaseCharacterAction) States.GetCurrentState()).Save(root);
			return root;
		}

		public override void Load(XmlNode root)
		{
			base.Load(root);
			Attributes.Load(root);
			BrandManager.Load(root);
			_timeAlive      = root.ParseInt("TimeAlive");
			_daysSurvived   = root.ParseInt("DaysSurvived");
			_skill1Unlocked = root.ParseBool(nameof(_skill1Unlocked));
			_skill2Unlocked = root.ParseBool(nameof(_skill1Unlocked));
			_skill3Unlocked = root.ParseBool(nameof(_skill1Unlocked));
			_skill4Unlocked = root.ParseBool(nameof(_skill1Unlocked));
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
			Debug.Log(lifeRemaining + " " + travelCost);
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

		public override void EquipWeapon(Weapon weapon)
		{
			base.EquipWeapon(weapon);
			if (_characterView        != null) _characterView.WeaponController.UpdateWeapon();
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.EquipWeapon();
		}

		public void ApplyModifier(AttributeType target, AttributeModifier modifier)
		{
			if (!target.IsCoreAttribute()) return;
			CharacterAttribute attribute = Attributes.Get(target);
			attribute.Max += modifier.RawBonus();
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		public void RemoveModifier(AttributeType target, AttributeModifier modifier)
		{
			if (!target.IsCoreAttribute()) return;
			CharacterAttribute attribute = Attributes.Get(target);
			attribute.Max -= modifier.RawBonus();
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		private bool CanRest() => Attributes.Life < Attributes.Life.Max || Attributes.Will < Attributes.Will.Max;

		public CharacterView CharacterView() => _characterView;
	}
}