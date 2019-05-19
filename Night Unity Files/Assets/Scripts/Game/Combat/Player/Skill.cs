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

		public string Description => _skillValue.Description;
		public int    Duration    => _skillValue.Duration;
		public int    Cooldown    => _skillValue.Cooldown;

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

		public bool Activate()
		{
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

		protected static void Heal(float percent)
		{
			HealthController healthController = PlayerCombat.Instance.HealthController;
			int              healAmount       = Mathf.CeilToInt(percent * healthController.GetMaxHealth());
			healthController.Heal(healAmount);
		}

		protected virtual void PassiveEffect(Shot s)
		{
		}

		protected virtual void InstantEffect()
		{
		}

		private class SkillValue
		{
			public readonly string Description;
			public readonly int    Cooldown;
			public readonly int    Duration;

			public SkillValue(XmlNode skillNode)
			{
				string name = skillNode.ParseString("Name");
				Duration           = skillNode.ParseInt("Duration");
				Cooldown           = skillNode.ParseInt("Cost");
				Description        = skillNode.ParseString("Description");
				_skillValues[name] = this;
			}
		}
	}
}