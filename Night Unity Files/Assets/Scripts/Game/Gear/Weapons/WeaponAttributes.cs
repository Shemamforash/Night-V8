﻿using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : DesolationAttributes
    {
        private const int MaxDurability = 50;
        private readonly Number _durability;
        private float _dps;
        private readonly AttributeModifier _damageDurabilityModifier;
        private readonly AttributeModifier _fireRateDurabilityModifier;
        private readonly AttributeModifier _reloadSpeedDurabilityModifier;
        private readonly AttributeModifier _accuracyDurabilityModifier;
        private readonly Weapon _weapon;
        public bool Automatic = true;
        private WeaponClassType WeaponClassType;
        public WeaponType WeaponType;

        public WeaponAttributes(Weapon weapon, WeaponClass weaponClass)
        {
            _weapon = weapon;
            _damageDurabilityModifier = new AttributeModifier();
            _fireRateDurabilityModifier = new AttributeModifier();
            _reloadSpeedDurabilityModifier = new AttributeModifier();
            _accuracyDurabilityModifier = new AttributeModifier();
            AddMod(AttributeType.Damage, _damageDurabilityModifier);
            AddMod(AttributeType.FireRate, _fireRateDurabilityModifier);
            AddMod(AttributeType.ReloadSpeed, _reloadSpeedDurabilityModifier);
            AddMod(AttributeType.Accuracy, _accuracyDurabilityModifier);
            int maxDurability = ((int) weapon.Quality() + 1) * 10;
            _durability = new Number(maxDurability, 0, maxDurability);
            SetMax(AttributeType.Accuracy, 1);
            SetClass(weaponClass);
        }

        public override XmlNode Save(XmlNode root)
        {
            root.CreateChild("Class", (int) WeaponClassType);
            root.CreateChild("Durability", _durability.CurrentValue());
            root = base.Save(root);
            return root;
        }

        public override void Load(XmlNode root)
        {
            base.Load(root);
            _durability.SetCurrentValue(root.FloatFromNode("Durability"));
        }

        private void SetClass(WeaponClass weaponClass)
        {
            SetVal(AttributeType.FireRate, weaponClass.FireRate);
            SetVal(AttributeType.ReloadSpeed, weaponClass.ReloadSpeed);
            SetVal(AttributeType.Damage, weaponClass.Damage);
            SetVal(AttributeType.Handling, weaponClass.Handling);
            SetVal(AttributeType.Capacity, weaponClass.Capacity);
            SetVal(AttributeType.Pellets, weaponClass.Pellets);
            SetVal(AttributeType.Accuracy, weaponClass.Accuracy);
            WeaponType = weaponClass.Type;
            Automatic = weaponClass.Automatic;
            WeaponClassType = weaponClass.Name;
            RecalculateAttributeValues();
        }

        public WeaponClassType GetWeaponClass() => WeaponClassType;

        private void RecalculateAttributeValues()
        {
            float damageModifier = 0.08f * _durability.CurrentValue();
            float fireRateModifier = 0.02f * _durability.CurrentValue();
            float reloadModifier = -0.01f * _durability.CurrentValue();
            float accuracyModifier = 0.01f * _durability.CurrentValue();
            _damageDurabilityModifier.SetFinalBonus(damageModifier);
            _fireRateDurabilityModifier.SetFinalBonus(fireRateModifier);
            _reloadSpeedDurabilityModifier.SetFinalBonus(reloadModifier);
            _accuracyDurabilityModifier.SetFinalBonus(accuracyModifier);
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = Val(AttributeType.Damage) * (int) Val(AttributeType.Pellets);
            float magazineDamage = Val(AttributeType.Capacity) * averageShotDamage;
            float magazineDuration = Val(AttributeType.Capacity) / Val(AttributeType.FireRate) + Val(AttributeType.ReloadSpeed);
            _dps = magazineDamage / magazineDuration;
        }

        public float DPS() => _dps;

        public string GetPrintMessage() => WeaponType + " " + WeaponClassType + " " + _weapon.Quality()
                                           + "\nDurability: " + _durability.CurrentValue() + " (" + _durability.Max + ")"
                                           + "\nDPS: " + DPS()
                                           + "\nAutomatic: " + Automatic
                                           + "\nCapacity:   " + Val(AttributeType.Capacity)
                                           + "\nPellets:    " + Val(AttributeType.Pellets)
                                           + "\nDamage:     " + Val(AttributeType.Damage)
                                           + "\nFire Rate:  " + Val(AttributeType.FireRate)
                                           + "\nReload:     " + Val(AttributeType.ReloadSpeed)
                                           + "\nAccuracy: " + Val(AttributeType.Accuracy);

        public void DecreaseDurability(float modifier)
        {
            float durabilityLoss = Val(AttributeType.Damage) * Val(AttributeType.Pellets) / Val(AttributeType.ReloadSpeed);
            durabilityLoss /= 1000f;
            durabilityLoss += durabilityLoss * modifier;
            _durability.Decrement(durabilityLoss);
            RecalculateAttributeValues();
        }

        public void IncreaseDurability(int durabilityGain)
        {
            _durability.Increment(durabilityGain);
            RecalculateAttributeValues();
        }

        public Number GetDurability()
        {
            return _durability;
        }
    }
}