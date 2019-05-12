using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.WorldEvents;
using Game.Gear.Armour;
using Game.Gear.Weapons;
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
		private const int CharacterSkillOneTarget = 2, CharacterSkillTwoTarget = 4, WeaponSkillOneTarget = 75, WeaponSkillTwoTarget = 200;

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
		public readonly  CharacterAttributes         Attributes;
		public readonly  BrandManager                BrandManager = new BrandManager();
		public readonly  Skill                       CharacterSkillOne, CharacterSkillTwo;
		public readonly  CharacterTemplate           CharacterTemplate;
		public readonly  StateMachine                States = new StateMachine();
		private          CharacterView               _characterView;
		private          int                         _daysSurvived;
		private          bool                        _showJournal;
		private          int                         _timeAlive;
		public           Consume                     ConsumeAction;
		public           Craft                       CraftAction;

		public bool   IsDead;
		public Rest   RestAction;
		public Travel TravelAction;

		//Create Character in code only- no view section, no references to objects in the scene
		public Player(CharacterTemplate characterTemplate) : base("The " + characterTemplate.CharacterClass)
		{
			Attributes        = new CharacterAttributes(this);
			CharacterTemplate = characterTemplate;
			CharacterSkillOne = CharacterSkills.GetCharacterSkillOne(this);
			CharacterSkillTwo = CharacterSkills.GetCharacterSkillTwo(this);

			AddStates();
			BrandManager.Initialise(this);
			WeaponGenerator.GetWeaponTypes().ForEach(t => { _weaponKills.Add(t, 0); });
		}

		public override XmlNode Save(XmlNode doc)
		{
			doc = base.Save(doc);
			Attributes.Save(doc);
			BrandManager.Save(doc);
			doc.CreateChild("TimeAlive",      _timeAlive);
			doc.CreateChild("DaysSurvived",   _daysSurvived);
			doc.CreateChild("CharacterClass", CharacterTemplate.CharacterClass.ToString());
			XmlNode weaponKillNode = doc.CreateChild("WeaponKills");
			foreach (KeyValuePair<WeaponType, int> weaponKills in _weaponKills)
				weaponKillNode.CreateChild(weaponKills.Key.ToString(), weaponKills.Value);

			((BaseCharacterAction) States.GetCurrentState()).Save(doc);
			return doc;
		}

		public override void Load(XmlNode root)
		{
			base.Load(root);
			Attributes.Load(root);
			BrandManager.Load(root);
			_timeAlive    = root.ParseInt("TimeAlive");
			_daysSurvived = root.ParseInt("DaysSurvived");

			XmlNode          weaponKillNode = root.SelectSingleNode("WeaponKills");
			List<WeaponType> weaponTypes    = WeaponGenerator.GetWeaponTypes();
			weaponTypes.ForEach(t =>
			{
				_weaponKills[t] = weaponKillNode.ParseInt(t.ToString());
				TryUnlockWeaponSkills(t, false);
			});

			TryUnlockCharacterSkill(false);
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

		public bool CanShowJournal()
		{
			if (!_showJournal) return false;
			_showJournal = false;
			return true;
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
			_showJournal = true;
		}

		public void TryUnlockCharacterSkill(bool showScreen)
		{
			if (_daysSurvived >= CharacterSkillOneTarget)
			{
				Attributes.UnlockCharacterSkillOne(showScreen);
			}

			if (_daysSurvived >= CharacterSkillTwoTarget)
			{
				Attributes.UnlockCharacterSkillTwo(showScreen);
			}
		}

		public Tuple<string, float> GetCharacterSkillOneProgress() => GetCharacterSkillProgress(CharacterSkillOneTarget);

		public Tuple<string, float> GetCharacterSkillTwoProgress() => GetCharacterSkillProgress(CharacterSkillTwoTarget);

		private Tuple<string, float> GetCharacterSkillProgress(int target)
		{
			int    progress           = target                - _daysSurvived;
			string progressString     = "Survive " + progress + " day".Pluralise(progress);
			float  normalisedProgress = (float) _daysSurvived / target;
			return Tuple.Create(progressString, normalisedProgress);
		}

		public Tuple<string, float> GetWeaponSkillOneProgress() => GetWeaponProgress(WeaponSkillOneTarget);

		public Tuple<string, float> GetWeaponSkillTwoProgress() => GetWeaponProgress(WeaponSkillTwoTarget);

		private Tuple<string, float> GetWeaponProgress(int target)
		{
			WeaponType weaponType         = Weapon.WeaponType();
			int        progress           = target - _weaponKills[weaponType];
			string     pluralisedEnemy    = progress <= 1 ? " enemy" : " enemies";
			string     progressString     = "Kill " + progress + pluralisedEnemy;
			float      normalisedProgress = (float) _weaponKills[weaponType] / target;
			return Tuple.Create(progressString, normalisedProgress);
		}

		public void SetCharacterView(CharacterView characterView)
		{
			_characterView = characterView;
		}

		public bool CanAffordTravel(int travelCost = 1)
		{
			int lifeRemaining = Mathf.CeilToInt(Attributes.Val(AttributeType.Life));
			return lifeRemaining >= travelCost;
		}

		public void Rest()
		{
			if (!CanRest()) return;
			CharacterAttribute life = Attributes.Get(AttributeType.Life);
			CharacterAttribute will = Attributes.Get(AttributeType.Will);
			will.Increment();
			life.Increment();
			if (CanRest()) return;
			WorldEventManager.GenerateEvent(new CharacterMessage(_restEvents.RandomElement(), this));
		}

		public void UpdateWeapon()
		{
			if (_characterView        != null) _characterView.WeaponController.UpdateWeapon();
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.EquipWeapon();
		}

		public override void EquipAccessory(Accessory accessory)
		{
			base.EquipAccessory(accessory);
			if (_characterView != null) _characterView.AccessoryController.UpdateAccessory();
		}

		public void IncreaseKills()
		{
			if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Tutorial) return;
			BrandManager.IncreaseEnemiesKilled();
			WeaponType weaponType = Weapon.WeaponType();
			_weaponKills[weaponType] = _weaponKills[weaponType] + 1;
			TryUnlockWeaponSkills(weaponType, true);
		}

		private void TryUnlockWeaponSkills(WeaponType weaponType, bool showScreen)
		{
			if (_weaponKills[weaponType] >= WeaponSkillOneTarget)
			{
				Attributes.UnlockWeaponSkillOne(weaponType, showScreen);
			}

			if (_weaponKills[weaponType] >= WeaponSkillTwoTarget)
			{
				Attributes.UnlockWeaponSkillTwo(weaponType, showScreen);
			}
		}

		private bool CanRest() =>
			Attributes.Val(AttributeType.Life) < Attributes.Max(AttributeType.Life) ||
			Attributes.Val(AttributeType.Will) < Attributes.Max(AttributeType.Will);

		public CharacterView CharacterView() => _characterView;

		public void ApplyModifier(AttributeType target, AttributeModifier modifier)
		{
			if (!CharacterAttribute.IsCharacterAttribute(target)) return;
			CharacterAttribute attribute = Attributes.Get(target);
			attribute.Max += modifier.RawBonus();
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		public void RemoveModifier(AttributeType target, AttributeModifier modifier)
		{
			if (!CharacterAttribute.IsCharacterAttribute(target)) return;
			CharacterAttribute attribute = Attributes.Get(target);
			attribute.Max -= modifier.RawBonus();
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}
	}
}