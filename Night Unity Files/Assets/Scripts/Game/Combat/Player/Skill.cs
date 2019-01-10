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

        protected static CanTakeDamage Target()
        {
            return PlayerCombat.Instance.GetTarget();
        }

        private static void KnockbackSingleTarget(Vector2 position, CanTakeDamage c, float force)
        {
            CharacterCombat character = c as CharacterCombat;
            if (character == null) return;
            float distance = Vector2.Distance(character.transform.position, position);
            if (distance < 0f) distance = 1;
            float scaledForce = force / distance;
            Vector2 direction = (position - (Vector2) character.transform.position).normalized;
            character.MovementController.KnockBack(direction, scaledForce);
        }

        protected List<CanTakeDamage> KnockbackInRange(float range, float force)
        {
            Vector2 position = PlayerCombat.Position();
            List<CanTakeDamage> enemiesInRange = CombatManager.GetEnemiesInRange(position, range);
            enemiesInRange.ForEach(e => { KnockbackSingleTarget(position, e, force); });
            return enemiesInRange;
        }

        protected PlayerCombat Player() => _player;
        protected Transform PlayerTransform() => _playerTransform;
        protected Vector2 PlayerPosition() => _playerTransform.position;
        
        public bool Activate(bool freeSkill)
        {
            if (Target() == null && _skillValue.NeedsTarget) return false;
            if (!freeSkill && !PlayerCombat.Instance.ConsumeAdrenaline(AdrenalineCost())) return false;
            _player = PlayerCombat.Instance;
            _playerTransform = _player.transform;
            InstantEffect();
            if (_skillValue.AppliesToMagazine)
            {
                PlayerCombat.Instance.OnFireActions.Add(MagazineEffect);
                ActiveSkillController.Play();
            }

            UIMagazineController.UpdateMagazineUi();
            PlayerCombat.Instance.Player.BrandManager.IncreaseSkillsUsed();
            return true;
        }

        public bool CanAfford()
        {
            return PlayerCombat.Instance.CanAffordSkill(AdrenalineCost());
        }

        protected void Heal(float percent)
        {
            int healAmount = Mathf.FloorToInt(percent * PlayerCombat.Instance.HealthController.GetMaxHealth());
            PlayerCombat.Instance.HealthController.Heal(healAmount);
        }

        protected virtual void MagazineEffect(Shot s)
        {
        }

        protected virtual void InstantEffect()
        {
        }

        private class SkillValue
        {
            public readonly int AdrenalineCost;
            public readonly string Description;
            public readonly bool NeedsTarget;
            public readonly bool AppliesToMagazine;

            public SkillValue(XmlNode skillNode)
            {
                string name = skillNode.StringFromNode("Name");
                AdrenalineCost = skillNode.IntFromNode("Cooldown");
                Description = skillNode.StringFromNode("Description");
                NeedsTarget = skillNode.BoolFromNode("RequiresTarget");
                AppliesToMagazine = skillNode.BoolFromNode("AppliesToMagazine");
                _skillValues[name] = this;
            }
        }

        public int AdrenalineCost()
        {
            return _skillValue.AdrenalineCost;
        }
    }
}