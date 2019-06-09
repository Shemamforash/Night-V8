using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;
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
			_player           = CharacterManager.SelectedCharacter;

			for (int i = 0; i < NoSlots; ++i)
				_skillControllers.Add(gameObject.FindChildWithName<CooldownController>("Skill " + (i + 1)));

			_skillsReady = true;
			_needsUpdate = true;
			TryUpdateSkills();
		}

		private void OnDestroy() => _instance = null;

		private void UpdateCooldownControllers(float normalisedDuration)
		{
			_skillControllers.ForEach(c => c.UpdateCooldownFill(normalisedDuration));
		}

		private void UpdateCooldown()
		{
			TryUpdateSkills();
			if (_skillsReady) return;
			if (_cooldownRemaining <= 0)
			{
				_cooldownRemaining = 0;
				UpdateCooldownControllers(1);
				_skillsReady = true;
				return;
			}

			float normalisedTime = 1 - _cooldownRemaining / _duration;
			UpdateCooldownControllers(normalisedTime);
		}

		private void TryUpdateSkills()
		{
			if (!_needsUpdate) return;
			_needsUpdate      = false;
			_cooldownModifier = _player.Attributes.CooldownModifier();
			_skillControllers[0].SetSkill(_player.SkillOne());
			_skillControllers[1].SetSkill(_player.SkillTwo());
			_skillControllers[2].SetSkill(_player.SkillThree());
			_skillControllers[3].SetSkill(_player.SkillFour());
		}

		private static float CooldownRemaining
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

		public static void UnlockSkill(int skillNo)
		{
			UpdateSkills();
			_instance.TryUpdateSkills();
			_instance._skillControllers[skillNo].Unlock();
		}

		private bool IsSkillFree()
		{
			float freeSkillChance = PlayerCombat.Instance.Player.Attributes.FreeSkillChance;
			return Random.Range(0f, 1f) <= freeSkillChance;
		}

		public static SkillBar Instance() => _instance;

		private const float SkillHoldTimeTarget = 0.5f;
		private       float _skillHoldTime;
		private       int   _slotPressed;

		public void PressSkillBarSlot(int skillNo)
		{
			Skill target = _skillControllers[_slotPressed].Skill;
			if (target == null)
			{
				_skillHoldTime = 0;
				return;
			}

			float timeBefore = _skillHoldTime;
			_skillHoldTime += Time.deltaTime;
			_slotPressed   =  skillNo;
			if (timeBefore     > SkillHoldTimeTarget) return;
			if (_skillHoldTime < SkillHoldTimeTarget) return;
			UiBrandMenu.ShowCharacterSkill(target, _slotPressed);
			_skillHoldTime = 0;
		}

		private void StartCooldown(int duration)
		{
			_skillsReady      = false;
			_duration         = duration * _cooldownModifier;
			CooldownRemaining = _duration;
		}

		public static void DecreaseCooldown(Weapon weapon, int damageDealt)
		{
			float decreaseAmount = damageDealt / weapon.WeaponAttributes.GetDurability().CurrentValue;
			decreaseAmount    /= 25f;
			CooldownRemaining -= decreaseAmount;
		}

		public void ReleaseSkillBar()
		{
			if (_skillHoldTime < SkillHoldTimeTarget) ActivateSkill();
			_skillHoldTime = 0;
		}

		private void ActivateSkill()
		{
			Debug.Log(_skillsReady);
			if (!_skillsReady) return;
			Skill skill = _skillControllers[_slotPressed].Skill;
			if (skill == null) return;
			skill.Activate();
			_skillControllers[_slotPressed].Unlock();
			if (IsSkillFree()) return;
			StartCooldown(skill.Cooldown);
		}
	}
}