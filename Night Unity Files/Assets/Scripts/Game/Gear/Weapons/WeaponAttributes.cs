using System;
using System.Xml;
using Extensions;
using Game.Characters;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
	public class WeaponAttributes : DesolationAttributes
	{
		private const    float           MinRange = 2f;
		private const    float           MaxRange = 6f;
		private readonly Number          _durability;
		private          float           _dps;
		private readonly Weapon          _weapon;
		private readonly string          Description;
		public           string          FireType, FireMode;
		public           bool            Automatic = true;
		private          WeaponClassType WeaponClassType;
		public           WeaponType      WeaponType;

		public WeaponAttributes(Weapon weapon, WeaponClass weaponClass)
		{
			_weapon = weapon;
			int maxDurability = ((int) weapon.Quality() + 1) * 10;
			_durability = new Number(maxDurability, 0, maxDurability);
			SetMax(AttributeType.Accuracy, 1);
			SetMax(AttributeType.Range,    1);
			SetClass(weaponClass);
			Description = weaponClass.Description;
		}

		public override XmlNode Save(XmlNode root)
		{
			root.CreateChild("Class",      (int) WeaponClassType);
			root.CreateChild("Durability", _durability.CurrentValue);
			root = base.Save(root);
			return root;
		}

		public override void Load(XmlNode root)
		{
			base.Load(root);
			_durability.CurrentValue = root.ParseFloat("Durability");
		}

		public float Damage()      => CalculateAttribute(AttributeType.Damage, 0.05f);
		public float Accuracy()    => Mathf.Clamp(CalculateAttribute(AttributeType.Accuracy, 0.01f), 0f, 1f);
		public float FireRate()    => CalculateAttribute(AttributeType.FireRate,    0.015f);
		public float ReloadSpeed() => CalculateAttribute(AttributeType.ReloadSpeed, -0.01f);
		public float Recoil()      => CalculateAttribute(AttributeType.Recoil,      0f);
		public int   Pellets()     => Mathf.CeilToInt(CalculateAttribute(AttributeType.Pellets,  0f));
		public int   Capacity()    => Mathf.CeilToInt(CalculateAttribute(AttributeType.Capacity, 0f));

		public float Range()   => Mathf.Lerp(MinRange, MaxRange, CalculateAttribute(AttributeType.Range, 0f));
		public float Void()    => CalculateConditionChance(AttributeType.Void);
		public float Shatter() => CalculateConditionChance(AttributeType.Shatter);
		public float Burn()    => CalculateConditionChance(AttributeType.Burn);

		private float CalculateAttribute(AttributeType attributeType, float durabilityModifier)
		{
			float attributeValue = Val(attributeType);
			float modifier       = GetAccessoryValue(attributeType, 1);
			modifier       += durabilityModifier * _durability.CurrentValue;
			attributeValue *= modifier;
			return attributeValue;
		}

		private float GetAccessoryValue(AttributeType attributeType, int defaultValue = 0)
		{
			Character character = _weapon.EquippedCharacter ?? CharacterManager.SelectedCharacter;
			if (character == null) return defaultValue;
			Accessory accessory = character.Accessory;
			if (accessory                 == null) return defaultValue;
			if (accessory.TargetAttribute != attributeType) return defaultValue;
			return accessory.ModifierValue + defaultValue;
		}

		private float GetPlayerValue(AttributeType attributeType, int defaultValue = 0)
		{
			if (!(_weapon.EquippedCharacter is Player player)) return defaultValue;
			return player.Attributes.Val(attributeType) + defaultValue;
		}

		private float CalculateConditionChance(AttributeType attributeType)
		{
			float conditionChance = Val(attributeType);
			float modifier        = GetAccessoryValue(attributeType, 0);
			modifier        += GetPlayerValue(attributeType, 0);
			conditionChance += modifier;
			return conditionChance;
		}

		private void SetClass(WeaponClass weaponClass)
		{
			SetVal(AttributeType.FireRate,    weaponClass.FireRate);
			SetVal(AttributeType.ReloadSpeed, weaponClass.ReloadSpeed);
			SetVal(AttributeType.Damage,      weaponClass.Damage);
			SetVal(AttributeType.Recoil,      weaponClass.Recoil);
			SetVal(AttributeType.Capacity,    weaponClass.Capacity);
			SetVal(AttributeType.Pellets,     weaponClass.Pellets);
			SetVal(AttributeType.Accuracy,    weaponClass.Accuracy);
			SetVal(AttributeType.Range,       weaponClass.Range);
			WeaponType      = weaponClass.Type;
			Automatic       = weaponClass.Automatic;
			WeaponClassType = weaponClass.Name;
			FireType        = weaponClass.FireType;
			FireMode        = weaponClass.FireMode;
		}

		public WeaponClassType GetWeaponClass() => WeaponClassType;

		public void CalculateDPS()
		{
			float averageShotDamage = Damage()   * Pellets();
			float magazineDamage    = Capacity() * averageShotDamage;
			float magazineDuration  = Capacity() / FireRate() + ReloadSpeed();
			_dps = magazineDamage / magazineDuration;
		}

		public float DPS() => _dps;

		public void RandomiseDurability()
		{
			float newDurability = Random.Range(_durability.Min, _durability.Max);
			_durability.CurrentValue = newDurability;
			CalculateDPS();
		}

		public string GetPrintMessage() => WeaponType       + " "                      + WeaponClassType + " "             + _weapon.Quality()
		                                 + "\nDurability: " + _durability.CurrentValue + " ("            + _durability.Max + ")"
		                                 + "\nDPS: "        + DPS()
		                                 + "\nAutomatic: "  + Automatic
		                                 + "\nCapacity:   " + Capacity()
		                                 + "\nPellets:    " + Pellets()
		                                 + "\nDamage:     " + Damage()
		                                 + "\nFire Rate:  " + FireRate()
		                                 + "\nReload:     " + ReloadSpeed()
		                                 + "\nAccuracy: "   + Accuracy();


		public Number GetDurability() => _durability;

		public string GetWeaponTypeDescription() => Description;

		private static float CalculateConditionChance(Func<float> conditionChanceFunc)
		{
			float conditionChance = conditionChanceFunc();
			conditionChance = conditionChance.Round(2);
			return conditionChance;
		}

		public float CalculateShatterChance() => CalculateConditionChance(Shatter);

		public float CalculateBurnChance() => CalculateConditionChance(Burn);

		public float CalculateVoidChance() => CalculateConditionChance(Void);
	}
}