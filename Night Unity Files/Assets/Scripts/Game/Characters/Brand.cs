using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Characters.Brands;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Characters
{
	public abstract class Brand
	{
		private readonly   string _riteName;
		protected readonly Player Player;

		private int _counter;

		private   int         _counterTarget;
		private   string      _effect, _requirementString;
		private   int         _minLevel;
		private   bool        _ready;
		private   bool        _requiresSkillUnlock;
		public    BrandStatus Status = BrandStatus.Locked;
		protected float       SuccessModifier;

		protected Brand(Player player, string riteName)
		{
			Player    = player;
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
			return _minLevel <= (int) EnvironmentManager.CurrentEnvironmentType;
		}

		public void ReadData(XmlNode root)
		{
			root                 = root.SelectSingleNode(_riteName);
			_requirementString   = root.ParseString("Requirement");
			_counterTarget       = root.ParseInt("TargetValue");
			_requirementString   = _requirementString.Replace("num", _counterTarget.ToString());
			_effect              = root.ParseString("Effect");
			SuccessModifier      = root.ParseFloat("Modifier");
			_requiresSkillUnlock = root.ParseBool("RequiresSkill");
			if (_minLevel != 0) return;
			_minLevel = root.ParseInt("MinLevel");
		}

		protected string Progress() => _counter + "/" + _counterTarget;

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
			if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Rite) return;
			_counter += amount;
			if (_counter < _counterTarget) return;
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
			CharacterAttribute life                      = Player.Attributes.Get(AttributeType.Life);
			CharacterAttribute will                      = Player.Attributes.Get(AttributeType.Will);
			if (life.CurrentValue > 1) life.CurrentValue = 1;
			if (will.CurrentValue > 1) will.CurrentValue = 1;
		}

		protected abstract void OnSucceed();

		public void PrintStatus()
		{
			Debug.Log(this + " " + Status + " " + _counter + "/" + _counterTarget);
		}

		public string GetProgressString()
		{
			if (_counter < _counterTarget) return "Rite of " + _riteName + ": " + GetProgressSubstring();
			return "Completed The " + GetDisplayName();
		}

		protected abstract string GetProgressSubstring();

		public void Load(XmlNode doc)
		{
			_counter = doc.ParseInt("Counter");
			Status   = (BrandStatus) doc.ParseInt("Status");
			if (Status == BrandStatus.Active && _counter >= _counterTarget) _ready = true;
			if (this is LifeBrand || this is GritBrand || this is FocusBrand || this is WillBrand) return;
			if (Status == BrandStatus.Succeeded) OnSucceed();
		}

		public void Save(XmlNode doc)
		{
			doc.CreateChild("Name",    _riteName);
			doc.CreateChild("Counter", _counter);
			doc.CreateChild("Status",  (int) Status);
		}

		public string GetRequirementText() => _requirementString;

		public float NormalisedProgress() => _counter / (float) _counterTarget;

		public string GetEffectText() => _effect;
	}
}