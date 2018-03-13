using System.Collections.Generic;
using System.Xml;
using Game.Characters.Player;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Skills
{
    public abstract class Skill
    {
        public readonly string Name;
        private SkillValue _skillValue;

//        protected Shot Shot;
        private static readonly Dictionary<string, SkillValue> _skillValues = new Dictionary<string, SkillValue>();
        private static bool _loaded;
        private readonly bool _waitForReload;

        private class SkillValue
        {
            public readonly int Cooldown;
            public readonly string Description;

            public SkillValue(int cooldown, string description)
            {
                Cooldown = cooldown;
                Description = description;
            }
        }

        //todo read cost and cooldown from file

        private static void ReadSkillValue(Skill s)
        {
            LoadTemplates();
            string skillName = s.Name;
            if (!_skillValues.ContainsKey(skillName))
            {
                throw new Exceptions.SkillDoesNotExistException(skillName);
            }

            SkillValue value = _skillValues[skillName];
            s._skillValue = value;
        }

        public float Cooldown()
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
            TextAsset enemyFile = Resources.Load<TextAsset>("XML/Skills");
            XmlDocument enemyXml = new XmlDocument();
            enemyXml.LoadXml(enemyFile.text);
            XmlNode root = enemyXml.SelectSingleNode("Skills");
            foreach (XmlNode skillNode in root.SelectNodes("Skill"))
            {
                string name = skillNode.SelectSingleNode("Name").InnerText;
                int cooldown = int.Parse(skillNode.SelectSingleNode("Cooldown").InnerText);
                string description = skillNode.SelectSingleNode("Description").InnerText;
                SkillValue t = new SkillValue(cooldown, description);
                _skillValues[name] = t;
            }

            _loaded = true;
        }


        protected static Player Player()
        {
            return CombatManager.Player.Player;
        }

        protected Skill(string name, bool waitForReload = false)
        {
            Name = name;
            _waitForReload = waitForReload;
            ReadSkillValue(this);
        }

        public void Activate()
        {
            OnFire();
            CombatManager.Player.UpdateMagazineUi();
            if (_waitForReload)
            {
                CombatManager.Player.OnReloadAction += StartOnReload;
            }
        }

        private void StartOnReload()
        {
            CombatManager.Player.OnReloadAction -= StartOnReload;
        }

        protected abstract void OnFire();

        protected static Shot CreateShot()
        {
            return Shot.CreateShot(CombatManager.Player);
        }
    }
}