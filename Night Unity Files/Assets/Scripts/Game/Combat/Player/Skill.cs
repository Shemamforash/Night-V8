using System.Collections.Generic;
using System.Xml;
using Game.Characters;
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

        protected Skill(string name)
        {
            Name = name;
            ReadSkillValue(this);
        }

        //todo read cost and cooldown from file

        private static void ReadSkillValue(Skill s)
        {
            LoadTemplates();
            string skillName = s.Name;
            if (!_skillValues.ContainsKey(skillName)) throw new Exceptions.SkillDoesNotExistException(skillName);

            SkillValue value = _skillValues[skillName];
            s._skillValue = value;
        }

        public int Cooldown()
        {
            return _skillValue.Cooldown;
        }

        public string Description()
        {
            return _skillValue.Description;
        }

        private static void LoadTemplates()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Skills");
            foreach (XmlNode skillNode in Helper.GetNodesWithName(root, "Skill"))
                new SkillValue(skillNode);

            _loaded = true;
        }

        protected static Characters.Player Player()
        {
            return PlayerCombat.Instance.Player;
        }

        protected static EnemyBehaviour Target()
        {
            return (EnemyBehaviour)PlayerCombat.Instance.GetTarget();
        }

        private static void KnockbackSingleTarget(Vector2 position, ITakeDamageInterface c, float force)
        {
            CharacterCombat character =  c as CharacterCombat;
            if (character == null) return;
            float distance = Vector2.Distance(character.transform.position, position);
            if (distance < 0f) distance = 1;
            float scaledForce = force / distance;
            character.MovementController.Knockback(position, scaledForce);
        }

        protected List<ITakeDamageInterface> KnockbackInRange(float range, float force)
        {
            Vector2 position = PlayerCombat.Instance.transform.position;
            List<ITakeDamageInterface> enemiesInRange = CombatManager.GetEnemiesInRange(position, range);
            enemiesInRange.ForEach(e => { KnockbackSingleTarget(position, e, force); });
            return enemiesInRange;
        }

        public bool Activate(bool freeSkill)
        {
            if (Target() == null && _skillValue.NeedsTarget) return false;
            if (!freeSkill && !PlayerCombat.Instance.ConsumeAdrenaline(Cooldown())) return false;
            if (_skillValue.AppliesToMagazine) PlayerCombat.Instance.OnFireActions.Add(MagazineEffect);
            else InstantEffect();
            UIMagazineController.UpdateMagazineUi();
            CombatManager.IncreaseSkillsUsed();
            return true;
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

        protected static Shot CreateShot()
        {
            return Shot.Create(PlayerCombat.Instance);
        }

        private class SkillValue
        {
            public readonly int Cooldown;
            public readonly string Description;
            public readonly bool NeedsTarget;
            public readonly bool AppliesToMagazine;

            public SkillValue(XmlNode skillNode)
            {
                string name = skillNode.StringFromNode("Name");
                Cooldown = skillNode.IntFromNode("Cooldown");
                Description = skillNode.StringFromNode("Description");
                NeedsTarget = skillNode.BoolFromNode("RequiresTarget");
                AppliesToMagazine = skillNode.BoolFromNode("AppliesToMagazine");
                _skillValues[name] = this;
            }
        }
    }
}