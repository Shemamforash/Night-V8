using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation.Shrines;
using SamsHelper.Libraries;
using UnityEngine;
using Game.Characters.Brands;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters
{
    public abstract class Brand
    {
        private readonly string _riteName;
        protected readonly Player Player;

        private int _counterTarget;
        private string _description, _effect, _requirementString;
        protected float SuccessModifier;

        private int _counter;
        public BrandStatus Status = BrandStatus.Locked;
        private bool _requiresSkillUnlock;
        private int _minLevel;
        private bool _ready;

        protected Brand(Player player, string riteName)
        {
            Player = player;
            _riteName = riteName;
            SetStatus(BrandStatus.Locked);
        }

        public void SetMinLevel(int minLevel)
        {
            _minLevel = minLevel;
        }

        public bool PlayerRequirementsMet(Player player)
        {
            if (_requiresSkillUnlock && player.CharacterSkillOne != null) return false;
            return _minLevel <= (int) EnvironmentManager.CurrentEnvironmentType();
        }

        public void ReadData(XmlNode root)
        {
            root = root.SelectSingleNode(_riteName);
            _description = root.StringFromNode("Description");
            _requirementString = root.StringFromNode("Requirement");
            _counterTarget = root.IntFromNode("TargetValue");
            _requirementString = _requirementString.Replace("num", _counterTarget.ToString());
            _effect = root.StringFromNode("Effect");
            SuccessModifier = root.FloatFromNode("Modifier");
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
            if (Status == BrandStatus.Active) CombatLogController.PostLog("The " + GetDisplayName() + " has begun");
        }

        public string GetDisplayName() => "Rite of " + _riteName;

        public string GetName() => _riteName;

        public void UpdateValue(int amount)
        {
            if (_ready) return;
            if (CombatManager.GetCurrentRegion().GetRegionType() == RegionType.Rite) return;
            _counter += amount;
            if (_counter < _counterTarget)
            {
                CombatLogController.PostLog(GetDisplayName() + " - " + GetProgressString());
                return;
            }

            CombatLogController.PostLog("Completed The " + GetDisplayName());
            _ready = true;
        }

        public bool Ready() => _ready;

        public void Succeed()
        {
            Player.BrandManager.SetBrandInactive(this);
            SetStatus(BrandStatus.Succeeded);
            UiBrandMenu.ShowBrand(this);
            OnSucceed();
            _ready = false;
        }

        public void Fail()
        {
            Player.BrandManager.SetBrandInactive(this);
            SetStatus(BrandStatus.Failed);
            UiBrandMenu.ShowBrand(this);
            LoseAttributes();
            _ready = false;
        }

        private void LoseAttributes()
        {
            CharacterAttribute fettle = Player.Attributes.Get(AttributeType.Fettle);
            CharacterAttribute grit = Player.Attributes.Get(AttributeType.Grit);
            CharacterAttribute will = Player.Attributes.Get(AttributeType.Will);
            CharacterAttribute focus = Player.Attributes.Get(AttributeType.Focus);
            if (fettle.CurrentValue() > 1) fettle.SetCurrentValue(1);
            if (grit.CurrentValue() > 1) grit.SetCurrentValue(1);
            if (will.CurrentValue() > 1) will.SetCurrentValue(1);
            if (focus.CurrentValue() > 1) focus.SetCurrentValue(1);
        }

        protected abstract void OnSucceed();

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
            _counter = doc.IntFromNode("Counter");
            Status = (BrandStatus) doc.IntFromNode("Status");
            if (Status == BrandStatus.Active && _counter >= _counterTarget) _ready = true;
            if (this is FettleBrand || this is GritBrand || this is FocusBrand || this is WillBrand) return;
            if (Status == BrandStatus.Succeeded) OnSucceed();
        }

        public void Save(XmlNode doc)
        {
            doc.CreateChild("Name", _riteName);
            doc.CreateChild("Counter", _counter);
            doc.CreateChild("Status", (int) Status);
        }

        public string GetRequirementText()
        {
            return _requirementString;
        }

        public string Description()
        {
            return _description;
        }

        public float NormalisedProgress()
        {
            return _counter / (float) _counterTarget;
        }

        public string GetEffectText() => _effect;
    }
}