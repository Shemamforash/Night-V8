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

		private bool IsCharacterSkillOneUnlocked() => _player.Attributes.SkillOneUnlocked;
		private bool IsCharacterSkillTwoUnlocked() => _player.Attributes.SkillTwoUnlocked;
		private bool IsWeaponSkillOneUnlocked()    => _player.Attributes.WeaponSkillOneUnlocks.Contains(_player.Weapon.WeaponType());
		private bool IsWeaponSkillTwoUnlocked()    => _player.Attributes.WeaponSkillTwoUnlocks.Contains(_player.Weapon.WeaponType());

		private void TryUpdateSkills()
		{
			if (!_needsUpdate) return;
			_needsUpdate = false;
			_cooldownModifier          = _player.Attributes.CooldownModifier();
			Skill characterSkillOne = _player.CharacterSkillOne;
			Skill characterSkillTwo = _player.CharacterSkillTwo;
			Skill weaponSkillOne    = _player.Weapon.WeaponSkillOne;
			Skill weaponSkillTwo    = _player.Weapon.WeaponSkillTwo;

			BindSkill(0, characterSkillOne, IsCharacterSkillOneUnlocked, _player.GetCharacterSkillOneProgress);
			BindSkill(1, characterSkillTwo, IsCharacterSkillTwoUnlocked, _player.GetCharacterSkillTwoProgress);
			BindSkill(2, weaponSkillOne,    IsWeaponSkillOneUnlocked,    _player.GetWeaponSkillOneProgress);
			BindSkill(3, weaponSkillTwo,    IsWeaponSkillTwoUnlocked,    _player.GetWeaponSkillTwoProgress);
		}

		private void BindSkill(int slot, Skill skill, Func<bool> isSkillUnlocked, Func<Tuple<string, float>> getProgress)
		{
			_skillControllers[slot].SetSkill(skill, isSkillUnlocked, getProgress);
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

		public static void DecreaseCooldown(Weapon weapon)
		{
			float decreaseAmount = weapon.WeaponAttributes.DPS() / weapon.WeaponAttributes.GetDurability().CurrentValue;
			CooldownRemaining -= decreaseAmount / 100;
		}
	}
}