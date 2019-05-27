using System.Collections.Generic;
using System.Xml;
using Extensions;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;

namespace Game.Gear.Weapons
{
	public class Weapon : DesolationAttributes
	{
		private const    int                               AbsoluteMaxUpgradeLevel = 50;
		private          string                            Description;
		private readonly Dictionary<AttributeType, Number> _attributeValues = new Dictionary<AttributeType, Number>();
		private          float                             _dps;
		private          int                               _currentLevel           = 0;
		private          int                               _currentMaxUpgradeLevel = 10;
		private const    float                             RangeMin                = 1.5f;
		private const    float                             RangeMax                = 5.5f;
		public readonly  Character                         EquippedCharacter;
		public           Skill                             SkillOne, SkillTwo, SkillThree, SkillFour;

		public Weapon(Character equippedCharacter)
		{
			EquippedCharacter = equippedCharacter;
			InitialiseAttributes();
		}

		private Weapon(ItemQuality quality, WeaponType weaponType)
		{
		}

		public void InitialiseAttributes()
		{
			SetMax(AttributeType.Accuracy, 1);
			Description = "A gun";
			_attributeValues.Add(AttributeType.Damage,      new Number());
			_attributeValues.Add(AttributeType.Accuracy,    new Number(0f, 0f, 1f));
			_attributeValues.Add(AttributeType.FireRate,    new Number());
			_attributeValues.Add(AttributeType.ReloadSpeed, new Number());
			_attributeValues.Add(AttributeType.Recoil,      new Number(0f, 0f, 1f));
			_attributeValues.Add(AttributeType.Range,       new Number());
			_attributeValues.Add(AttributeType.Pellets,     new Number(1f, 1f));
			_attributeValues.Add(AttributeType.Capacity,    new Number(1f, 1f));
			_attributeValues.Add(AttributeType.Void,        new Number(0f, 0f, 1f));
			_attributeValues.Add(AttributeType.Shatter,     new Number(0f, 0f, 1f));
			_attributeValues.Add(AttributeType.Burn,        new Number(0f, 0f, 1f));
			_attributeValues.Add(AttributeType.Pierce,      new Number(0f, 0f, 1f));
		}

		public static Weapon Generate(ItemQuality quality, WeaponType weaponType) => new Weapon(quality, weaponType);

		public WeaponType WeaponType() => Weapons.WeaponType.Pistol;

		public float CalculateMinimumDistance()
		{
			float range = Val(AttributeType.Accuracy);
			range *= range;
			float minimumDistance = (RangeMax - RangeMin) * range + RangeMin * 0.5f;
			return minimumDistance;
		}

		public void ApplyInscription(Inscription inscription)
		{
			Inventory.Destroy(inscription);
			ApplyInscriptionModifier(inscription);
			RecalculateAttributeValues();
		}

		public string GetDisplayName()
		{
			return "Gun";
		}

		private void ApplyInscriptionModifier(Inscription inscription)
		{
			Assert.IsNotNull(inscription);
			ApplyModifier(inscription.Target(), inscription.Modifier());
		}

		public void ApplyModifier(AttributeType target, AttributeModifier modifier)
		{
			Get(target).AddModifier(modifier);
			RecalculateAttributeValues();
		}

		public string GetSummary() => DPS().Round(1) + "DPS";

		public int MaxLevel     => _currentMaxUpgradeLevel;
		public int CurrentLevel => _currentLevel;

		public bool CanIncreaseUpgradeLevel() => _currentMaxUpgradeLevel < AbsoluteMaxUpgradeLevel;

		public void IncreaseUpgradeLevel()
		{
			if (_currentMaxUpgradeLevel >= AbsoluteMaxUpgradeLevel) return;
			_currentMaxUpgradeLevel += 10;
		}

		public bool CanUpgrade() => _currentLevel < _currentMaxUpgradeLevel;

		public bool CanIncreaseAttribute(AttributeType attribute) => !_attributeValues[attribute].ReachedMax;

		public void IncreaseAttribute(AttributeType attribute, float amount) => _attributeValues[attribute].Increment(amount);

		public override XmlNode Save(XmlNode root)
		{
			root = root.CreateChild("Weapon");
			root = base.Save(root);
			return root;
		}

		public override void Load(XmlNode root)
		{
			base.Load(root);
		}

		public void RecalculateAttributeValues()
		{
			CalculateDPS();
		}

		private void CalculateDPS()
		{
			float averageShotDamage = Val(AttributeType.Damage)   * (int) Val(AttributeType.Pellets);
			float magazineDamage    = Val(AttributeType.Capacity) * averageShotDamage;
			float magazineDuration  = Val(AttributeType.Capacity) / Val(AttributeType.FireRate) + Val(AttributeType.ReloadSpeed);
			_dps = magazineDamage / magazineDuration;
		}

		public float DPS() => _dps;

		public string GetPrintMessage() => "A Gun : "                                       + _currentMaxUpgradeLevel
		                                                                 + "\nDPS: "        + DPS()
		                                                                 + "\nCapacity:   " + Val(AttributeType.Capacity)
		                                                                 + "\nPellets:    " + Val(AttributeType.Pellets)
		                                                                 + "\nDamage:     " + Val(AttributeType.Damage)
		                                                                 + "\nFire Rate:  " + Val(AttributeType.FireRate)
		                                                                 + "\nReload:     " + Val(AttributeType.ReloadSpeed)
		                                                                 + "\nAccuracy: "   + Val(AttributeType.Accuracy);

		public string GetWeaponTypeDescription() => Description;

		private float CalculateConditionChance(AttributeType condition)
		{
			float weaponChance                                      = Val(condition);
			float characterChance                                   = 0;
			if (EquippedCharacter is Player player) characterChance += player.Attributes.Val(condition);
			float totalChance                                       = weaponChance + characterChance;
			totalChance = totalChance.Round(2);
			return totalChance;
		}

		public float CalculateShatterChance() => CalculateConditionChance(AttributeType.Shatter);

		public float CalculateBurnChance() => CalculateConditionChance(AttributeType.Burn);

		public float CalculateVoidChance() => CalculateConditionChance(AttributeType.Void);

		public float PierceChance() => Val(AttributeType.Pierce) / 100f;
	}
}