﻿using System.Collections.Generic;
using System.Xml;
using Game.Combat.Misc;
using Game.Combat.Ui;
using SamsHelper;
using SamsHelper.Libraries;

namespace Game.Combat.Player
{
    public abstract class Skill
    {
//        protected Shot Shot;
        private static readonly Dictionary<string, SkillValue> _skillValues = new Dictionary<string, SkillValue>();
        private static bool _loaded;
        private readonly bool _waitForReload;
        public readonly string Name;
        private SkillValue _skillValue;

        protected Skill(string name, bool waitForReload = false)
        {
            Name = name;
            _waitForReload = waitForReload;
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

        public void Activate()
        {
            OnFire();
            UIMagazineController.UpdateMagazineUi();
            if (_waitForReload) PlayerCombat.Instance.OnReloadAction += StartOnReload;
        }

        private void StartOnReload()
        {
            PlayerCombat.Instance.OnReloadAction -= StartOnReload;
        }

        protected abstract void OnFire();

        protected static Shot CreateShot()
        {
            return Shot.Create(PlayerCombat.Instance);
        }

        private class SkillValue
        {
            public readonly int Cooldown;
            public readonly string Description;

            public SkillValue(XmlNode skillNode)
            {
                string name = Helper.GetNodeText(skillNode, "Name");
                Cooldown = Helper.IntFromNode(skillNode, "Cooldown");
                Description = Helper.GetNodeText(skillNode, "Description");
                _skillValues[name] = this;
            }
        }
    }
}