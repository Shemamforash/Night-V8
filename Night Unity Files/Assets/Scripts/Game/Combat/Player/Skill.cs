using System.Collections.Generic;
using System.Xml;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Player
{
    public abstract class Skill
    {
        private static readonly Dictionary<string, SkillValue> _skillValues = new Dictionary<string, SkillValue>();
        private static bool _loaded;

        public readonly string Name;
        private SkillValue _skillValue;
        private PlayerCombat _player;
        private Transform _playerTransform;


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

        public string Description()
        {
            return _skillValue.Description;
        }

        private static void LoadTemplates()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Skills");
            foreach (XmlNode skillNode in root.GetNodesWithName("Skill"))
                new SkillValue(skillNode);

            _loaded = true;
        }

        protected PlayerCombat Player() => _player;
        protected Transform PlayerTransform() => _playerTransform;
        protected Vector2 PlayerPosition() => _playerTransform.position;

        public bool Activate(bool freeSkill)
        {
            if (!freeSkill && !PlayerCombat.Instance.ConsumeAdrenaline(Cost())) return false;
            _player = PlayerCombat.Instance;
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

        public bool CanAfford()
        {
            return PlayerCombat.Instance.CanAffordSkill(Cost());
        }

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

        private class SkillValue
        {
            public readonly int Cost;
            public readonly string Description;
            public readonly float Duration;

            public SkillValue(XmlNode skillNode)
            {
                string name = skillNode.StringFromNode("Name");
                Duration = skillNode.FloatFromNode("Duration");
                Cost = skillNode.IntFromNode("Cost");
                Description = skillNode.StringFromNode("Description");
                _skillValues[name] = this;
            }
        }

        public int Cost()
        {
            return _skillValue.Cost;
        }
    }
}