using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : AttributeContainer
    {
        public readonly WeaponClass WeaponClass;
        public readonly GearModifier SubClass, SecondaryModifier;
        public CharacterAttribute Damage, Accuracy, CriticalChance, Handling, FireRate, ReloadSpeed, Capacity, Pellets;
        private float _dps;
        private AttributeModifier _durabilityModifier = new AttributeModifier();
        public readonly MyValue Durability;
        private const int MaxDurability = 20;

        public WeaponAttributes(WeaponClass weaponClass, GearModifier subClass, GearModifier secondaryModifier)
        {
            Durability = new MyValue(Random.Range(0, MaxDurability), 0, MaxDurability);
            WeaponClass = weaponClass;
            WeaponClass.ApplyToGear(this);
            SubClass = subClass;
            SubClass.ApplyToGear(this);
            SecondaryModifier = secondaryModifier;
            SecondaryModifier.ApplyToGear(this);
            RecalculateAttributeValues();
        }

        public void RecalculateAttributeValues()
        {
            float durabilityModifierValue = 1f / (MaxDurability * 2) * (Durability.CurrentValue() + MaxDurability);
            _durabilityModifier.SetMultiplicative(durabilityModifierValue);
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = CriticalChance.CurrentValue() / 100 * Damage.CurrentValue() * 2 + (1 - CriticalChance.CurrentValue() / 100) * Damage.CurrentValue();
            float magazineDamage = (int) Capacity.CurrentValue() * averageShotDamage * (int) Pellets.CurrentValue() * Accuracy.CurrentValue() / 100;
            float magazineDuration = (int) Capacity.CurrentValue() / FireRate.CurrentValue() + ReloadSpeed.CurrentValue();
            _dps = magazineDamage / magazineDuration;
        }

        public float DPS()
        {
            return _dps;
        }

        protected override void CacheAttributes()
        {
            Damage = new CharacterAttribute(AttributeType.Damage, 0);
            Accuracy = new CharacterAttribute(AttributeType.Accuracy, 0, 0, 100);
            CriticalChance = new CharacterAttribute(AttributeType.CriticalChance, 0, 0, 100);
            Handling = new CharacterAttribute(AttributeType.Handling, 0, 0, 100);
            FireRate = new CharacterAttribute(AttributeType.FireRate, 0);
            ReloadSpeed = new CharacterAttribute(AttributeType.ReloadSpeed, 0);
            Capacity = new CharacterAttribute(AttributeType.Capacity, 0);
            Pellets = new CharacterAttribute(AttributeType.Pellets, 0);
            AddAttribute(Damage);
            AddAttribute(Accuracy);
            AddAttribute(ReloadSpeed);
            AddAttribute(CriticalChance);
            AddAttribute(Handling);
            AddAttribute(FireRate);
            AddAttribute(Capacity);
            AddAttribute(Pellets);
            _durabilityModifier.AddTargetAttributes(new List<CharacterAttribute> {Damage, Accuracy, CriticalChance, Handling, FireRate, ReloadSpeed});
        }
    }
}