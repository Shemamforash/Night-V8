using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation.Shrines;
using SamsHelper.Libraries;
using UnityEngine;
using Game.Characters.Brands;
using Game.Global;

namespace Game.Characters
{
    public abstract class Brand
    {
        private readonly string _riteName;
        protected readonly Player Player;

        private int _counterTarget;
        private string _successName, _failName, _successEffect, _failEffect, _requirementString;
        protected float SuccessModifier, FailModifier;

        private int _counter;
        public BrandStatus Status = BrandStatus.Locked;
        private bool _requiresSkillUnlock;
        private int _minLevel;

        protected Brand(Player player, string riteName)
        {
            Player = player;
            _riteName = riteName;
            SetStatus(BrandStatus.Locked);
        }

        public bool PlayerRequirementsMet(Player player)
        {
            if (_requiresSkillUnlock && player.CharacterSkillOne != null) return false;
            if (_minLevel > WorldState.CurrentLevel()) return false;
            return true;
        }

        public void ReadData(XmlNode root)
        {
            root = root.SelectSingleNode(_riteName);
            _requirementString = root.StringFromNode("Requirement");
            _counterTarget = root.IntFromNode("TargetValue");
            _requirementString = _requirementString.Replace("num", _counterTarget.ToString());
            _successName = root.StringFromNode("SuccessName");
            _successEffect = root.StringFromNode("SuccessEffect");
            SuccessModifier = root.FloatFromNode("SuccessValue");
            _failName = root.StringFromNode("FailName");
            _failEffect = root.StringFromNode("FailEffect");
            FailModifier = root.FloatFromNode("FailValue");
            _requiresSkillUnlock = root.BoolFromNode("RequiresSkill");
            _minLevel = root.IntFromNode("MinLevel");
        }

        protected string Progress()
        {
            return _counter + "/" + _counterTarget;
        }

        public void SetStatus(BrandStatus status)
        {
            Status = status;
            Player.BrandManager.UpdateBrandStatus(this);
        }

        public string GetName()
        {
            return "Rite of " + _riteName;
        }

        public void UpdateValue(int amount)
        {
            _counter += amount;
            if (_counter >= _counterTarget)
            {
                RiteStarter.Generate(this);
            }
        }

        public void Succeed()
        {
            SetStatus(BrandStatus.Succeeded);
//            UiBrandMenu.ShowBrand(this);
            OnSucceed();
        }

        public void Fail()
        {
            SetStatus(BrandStatus.Failed);
            UiBrandMenu.ShowBrand(this);
            OnFail();
        }

        protected abstract void OnSucceed();
        protected abstract void OnFail();

        public void PrintStatus()
        {
            Debug.Log(this + " " + Status + " " + _counter + "/" + _counterTarget);
        }

        public string GetProgressString()
        {
            return "Rite of " + _riteName + ": " + GetProgressSubstring();
        }

        protected abstract string GetProgressSubstring();

        public void Load(XmlNode doc)
        {
            _counter = doc.IntFromNode("TimeRemaining");
            Status = (BrandStatus) doc.IntFromNode("Status");
            if (this is FettleBrand || this is GritBrand || this is FocusBrand || this is WillBrand) return;
            if (Status == BrandStatus.Succeeded) OnSucceed();
            else if (Status == BrandStatus.Failed) OnFail();
        }

        public XmlNode Save(XmlNode doc)
        {
            doc.CreateChild("Name", _riteName);
            doc.CreateChild("TimeRemaining", _counter);
            doc.CreateChild("Status", (int) Status);
            return doc;
        }

        public string GetSuccessName()
        {
            return _successName;
        }

        public string GetFailName()
        {
            return _failName;
        }

        public string GetEffectString()
        {
            return Status == BrandStatus.Succeeded ? _successEffect : _failEffect;
        }

        public string GetRequirementText()
        {
            return _requirementString;
        }
    }
}