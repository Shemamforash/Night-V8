using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Global;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Armour
{
	public class ArmourController
	{
		public const     int         MaxLevel             = 5;
		private const    int         ProtectionPerLevel   = 50;
		private const    float       BaseRechargeDuration = 7.5f;
		private readonly string[]    _names               = {"No Armour", "Leather Armour", "Makeshift Armour", "Metal Armour", "Iridescent Armour", "Celestial Armour"};
		private readonly Number      _protection          = new Number();
		private          float       _currentRechargeDuration;
		private          bool        _justTookDamage;
		private          ItemQuality _targetQuality;

		public ArmourController()
		{
			CurrentLevel = 0;
			CalculateMaxHealth();
		}

		public  int    CurrentLevel     { get; private set; }
		public  bool   Recharging       { get; private set; }
		public  float  TotalProtection  => (float) CurrentLevel / MaxLevel;
		public  string Name             => _names[CurrentLevel];
		public  float  FillLevel        => _protection.Normalised;
		private float  RechargeDuration => BaseRechargeDuration;
		public  bool   CanAbsorbDamage  => !Recharging && CurrentLevel > 0;

		public void Load(XmlNode doc)
		{
			CurrentLevel = doc.ParseInt("CurrentLevel");
			if (doc.OwnerDocument.DocumentElement.SelectSingleNode("Campfire") != null)
				CurrentLevel = Mathf.CeilToInt(CurrentLevel / 2f);
			CalculateMaxHealth();
		}

		public void Save(XmlNode doc)
		{
			doc.CreateChild("CurrentLevel", CurrentLevel);
		}

		private void CalculateMaxHealth()
		{
			float maxHealth = CurrentLevel * ProtectionPerLevel;
			_protection.Max          = maxHealth;
			_protection.CurrentValue = maxHealth;
			_targetQuality           = (ItemQuality) CurrentLevel;
		}

		public void TakeDamage(int damage)
		{
			Assert.IsFalse(CurrentLevel == 0);
			_protection.CurrentValue -= damage;
			if (!_protection.ReachedMin) return;
			Recharging = true;
		}

		public void Repair(int amount) => _protection.CurrentValue += amount;


		public bool DidJustTakeDamage()
		{
			bool didTakeDamage = _justTookDamage;
			_justTookDamage = false;
			return didTakeDamage;
		}

		public bool CanUpgrade() => Inventory.GetResourceQuantity(Armour.QualityToName(_targetQuality)) != 0;

		public void Upgrade()
		{
			if (!CanUpgrade()) return;
			Inventory.DecrementResource(Armour.QualityToName(_targetQuality), 1);
			++CurrentLevel;
			CalculateMaxHealth();
			if (CurrentLevel != MaxLevel) return;
			AchievementManager.Instance().MaxOutArmour();
		}

		public void AutoGenerateArmour()
		{
			int difficulty = Mathf.FloorToInt(WorldState.Difficulty() / 10f);
			int armourMin  = difficulty - 1;
			armourMin = Mathf.Clamp(armourMin, 0, MaxLevel);
			int armourMax = difficulty + 1;
			armourMax = Mathf.Clamp(armourMax, 0, MaxLevel);
			AutoFillSlots(Random.Range(armourMin, armourMax));
		}

		public void AutoFillSlots(int level)
		{
			CurrentLevel = level;
			CalculateMaxHealth();
		}

		public void Update()
		{
			if (!Recharging) return;
			_currentRechargeDuration += Time.deltaTime;
			float normalisedTime = _currentRechargeDuration / RechargeDuration;
			if (normalisedTime > 1)
			{
				normalisedTime           = 1;
				Recharging               = false;
				_currentRechargeDuration = 0f;
			}

			float newHealth = CurrentLevel * ProtectionPerLevel * normalisedTime;
			_protection.CurrentValue = newHealth;
		}

		public string GetBonus()
		{
			if (CurrentLevel == 0) return "-";
			return "Absorbs " + _protection.Max + " damage";
		}

		public string GetNextLevelBonus()
		{
			if (CurrentLevel == MaxLevel) return "Fully Upgraded";
			int nextLevelHealth = (CurrentLevel + 1) * ProtectionPerLevel;
			return "Next Level:\nAbsorbs " + nextLevelHealth + " Damage";
		}

		public string GetUpgradeRequirements()
		{
			if (CurrentLevel == MaxLevel) return "Fully Upgraded";
			if (!CanUpgrade()) return "Need " + Armour.QualityToName(_targetQuality) + " to Upgrade";
			return "Upgrade - Consumes " + Armour.QualityToName(_targetQuality);
		}

		public void Reset()
		{
			CalculateMaxHealth();
			Recharging = false;
		}
	}
}