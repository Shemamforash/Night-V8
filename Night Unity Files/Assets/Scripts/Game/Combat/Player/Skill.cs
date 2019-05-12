using System.Collections.Generic;
using System.Xml;
using Extensions;
using Game.Combat.Misc;
using Game.Combat.Ui;


using SamsHelper;
using UnityEngine;

namespace Game.Combat.Player
{
	public abstract class Skill
	{
		private static readonly Dictionary<string, SkillValue> _skillValues = new Dictionary<string, SkillValue>();
		private static          bool                           _loaded;

		public readonly string       Name;
		private         PlayerCombat _player;
		private         Transform    _playerTransform;
		private         SkillValue   _skillValue;


		protected Skill(string name)
		{
			Name = name;
			ReadSkillValue(this);
		}

		private static void ReadSkillValue(Skill s)
		{
			LoadTemplates();
			string skillName = s.Name;
			if (!_skillValues.ContainsKey(skillName)) throw new Exceptions.SkillDoesNotExistException(skillName);

			SkillValue value = _skillValues[skillName];
			s._skillValue = value;
		}

		public string Description() => _skillValue.Description;

		private static void LoadTemplates()
		{
			if (_loaded) return;
			XmlNode root = Helper.OpenRootNode("Skills");
			foreach (XmlNode skillNode in root.GetNodesWithName("Skill"))
				new SkillValue(skillNode);

			_loaded = true;
		}

		protected PlayerCombat Player()          => _player;
		protected Transform    PlayerTransform() => _playerTransform;
		protected Vector2      PlayerPosition()  => _playerTransform.position;

		public bool Activate(bool freeSkill)
		{
			if (!freeSkill && !PlayerCombat.Instance.ConsumeAdrenaline(Cost())) return false;
			AchievementManager.Instance().IncreaseSkillsUsed();
			_player          = PlayerCombat.Instance;
			_playerTransform = _player.transform;
			InstantEffect();
			if (_skillValue.Duration != -1)
			{
				_player.SetPassiveSkill(PassiveEffect, _skillValue.Duration);
				ActiveSkillController.Play();
			}
			else
			{
				_player.WeaponAudio.PlayActiveSkill();
			}

			UIMagazineController.UpdateMagazineUi();
			PlayerCombat.Instance.Player.BrandManager.IncreaseSkillsUsed();
			return true;
		}

		public bool CanAfford() => PlayerCombat.Instance.CanAffordSkill(Cost());

		protected void Heal(float percent)
		{
			int healAmount = Mathf.CeilToInt(percent * PlayerCombat.Instance.HealthController.GetMaxHealth());
			PlayerCombat.Instance.HealthController.Heal(healAmount);
		}

		protected virtual void PassiveEffect(Shot s)
		{
		}

		protected virtual void InstantEffect()
		{
		}

		public int Cost() => _skillValue.Cost;

		private class SkillValue
		{
			public readonly int    Cost;
			public readonly string Description;
			public readonly float  Duration;

			public SkillValue(XmlNode skillNode)
			{
				string name = skillNode.ParseString("Name");
				Duration           = skillNode.ParseFloat("Duration");
				Cost               = skillNode.ParseInt("Cost");
				Description        = skillNode.ParseString("Description");
				_skillValues[name] = this;
			}
		}
	}
}