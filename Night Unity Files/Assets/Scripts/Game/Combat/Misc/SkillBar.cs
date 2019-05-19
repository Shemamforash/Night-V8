using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Extensions;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
	public class SkillBar : MonoBehaviour
	{
		private const  int                      NoSlots = 4;
		private static bool                     _needsUpdate;
		private static SkillBar                 _instance;
		private        float                    _cooldownModifier;
		private static float                    _cooldownRemaining;
		private        float                    _duration;
		private        Characters.Player        _player;
		private        List<CooldownController> _skillControllers;
		private        bool                     _skillsReady;

		public void Awake()
		{
			_instance         = this;
			_skillControllers = new List<CooldownController>();

			for (int i = 0; i < NoSlots; ++i)
				_skillControllers.Add(gameObject.FindChildWithName<CooldownController>("Skill " + (i + 1)));

			_skillsReady = false;
		}

		private void OnDestroy() => _instance = null;

		private void UpdateCooldownControllers(float normalisedDuration)
		{
			_skillControllers.ForEach(c => c.UpdateCooldownFill(normalisedDuration));
		}

		private void UpdateCooldown()
		{
			//todo make this called when skill equipped
			TryUpdateSkills();
			if (_skillsReady) return;
			if (CooldownRemaining < 0)
			{
				CooldownRemaining = 0;
				UpdateCooldownControllers(1);
				_skillsReady = true;
				return;
			}

			float normalisedTime = 1 - CooldownRemaining / _duration;
			UpdateCooldownControllers(normalisedTime);
		}

		private void TryUpdateSkills()
		{
			if (!_needsUpdate) return;
			_needsUpdate = false;
			_player      = CharacterManager.SelectedCharacter;
			Weapon weapon = _player.Weapon;
			_cooldownModifier          = _player.Attributes.CooldownModifier();
			_skillControllers[0].Skill = weapon.SkillOne;
			_skillControllers[1].Skill = weapon.SkillTwo;
			_skillControllers[2].Skill = weapon.SkillThree;
			_skillControllers[3].Skill = weapon.SkillFour;
		}

		public static float CooldownRemaining
		{
			get => _cooldownRemaining;
			set
			{
				_cooldownRemaining = value;
				_instance.UpdateCooldown();
			}
		}

		public static void UpdateSkills()
		{
			_needsUpdate = true;
		}

		private bool IsSkillFree()
		{
			float freeSkillChance = PlayerCombat.Instance.Player.Attributes.FreeSkillChance;
			return Random.Range(0f, 1f) <= freeSkillChance;
		}

		public static SkillBar Instance() => _instance;

		public void ActivateSkill(int skillNo)
		{
			if (!_skillsReady) return;
			Skill skill = _skillControllers[skillNo].Skill;
			if (skill == null) return;
			bool freeSkill = IsSkillFree();
			if (!skill.Activate()) return;
			if (freeSkill) return;
			StartCooldown(skill.Cooldown);
		}

		private void StartCooldown(int duration)
		{
			_skillsReady      = false;
			_duration         = duration * _cooldownModifier;
			CooldownRemaining = _duration;
		}

		public static void DecreaseCooldown(WeaponAttributes weaponAttributes)
		{
			float decreaseAmount = weaponAttributes.DPS() / weaponAttributes.CurrentLevel;
			CooldownRemaining -= decreaseAmount / 100;
		}
	}
}