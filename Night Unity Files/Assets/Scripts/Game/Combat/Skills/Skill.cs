using System.Collections.Generic;
using System.Xml;
using Game.Characters.Player;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Skills
{
    public abstract class Skill : Cooldown
    {
        public int Cost;

        public readonly string Name;
        public string Description;

//        protected Shot Shot;
        private static readonly Dictionary<string, SkillValue> _skillValues = new Dictionary<string, SkillValue>();
        private static bool _loaded;
        private bool _waitForReload;
        private bool _fired;

        private class SkillValue
        {
            public readonly int Cooldown, Cost;
            public readonly string Description;

            public SkillValue(int cooldown, int cost, string description)
            {
                Cooldown = cooldown;
                Cost = cost;
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
            s.Duration = value.Cooldown;
            s.Cost = value.Cost;
            s.Description = value.Description;
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
                int cost = int.Parse(skillNode.SelectSingleNode("Cost").InnerText);
                string description = skillNode.SelectSingleNode("Description").InnerText;
                SkillValue t = new SkillValue(cooldown, cost, description);
                _skillValues[name] = t;
            }

            _loaded = true;
        }


        protected static Player Player()
        {
            return CombatManager.Player;
        }

        protected Skill(string name, bool waitForReload = false) : base(CombatManager.CombatCooldowns)
        {
            Name = name;
            _waitForReload = waitForReload;
            ReadSkillValue(this);
            SetEndAction(() => _fired = false);
        }

        public void Activate()
        {
            if (Running()) return;
            if (!CombatManager.Player.RageController.Spend(Cost)) return;
            OnFire();
            Player().UpdateMagazineUi();
            if (!_waitForReload) Start();
            else
            {
                Player().OnReloadAction += StartOnReload;
            }
        }

        private void StartOnReload()
        {
            if (Running() || !_fired) return;
            Start();
            Player().OnReloadAction -= StartOnReload;
        }

        protected virtual void OnFire()
        {
            _fired = true;
            Controller.UpdateCooldownFill(0);
        }

        protected static Shot CreateShot()
        {
            return new Shot(CombatManager.GetCurrentTarget(), Player());
        }

        public override void SetController(CooldownController controller)
        {
            base.SetController(controller);
            controller.Text(Name);
        }
    }
}