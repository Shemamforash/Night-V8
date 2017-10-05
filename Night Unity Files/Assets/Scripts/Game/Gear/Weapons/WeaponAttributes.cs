using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : AttributeContainer
    {
        public readonly FloatAttribute Damage, Accuracy, ReloadSpeed, CriticalChance, Handling, FireRate;
        public readonly IntAttribute Capacity, Pellets;
        private readonly Weapon _weapon;
        private float _dps;
        
        public WeaponAttributes(Weapon weapon)
        {
            _weapon = weapon;
            Damage = new FloatAttribute(AttributeType.Damage, 0);
            Accuracy= new FloatAttribute(AttributeType.Accuracy, 0, 0, 100);
            ReloadSpeed = new FloatAttribute(AttributeType.ReloadSpeed, 0);
            CriticalChance = new FloatAttribute(AttributeType.CriticalChance, 0, 0, 100);
            Handling = new FloatAttribute(AttributeType.Handling, 0, 0, 100);
            FireRate = new FloatAttribute(AttributeType.FireRate, 0);
            Capacity = new IntAttribute(AttributeType.Capacity, 0);
            Pellets = new IntAttribute(AttributeType.Pellets, 0);
            AddAttribute(Damage);
            AddAttribute(Accuracy);
            AddAttribute(ReloadSpeed);
            AddAttribute(CriticalChance);
            AddAttribute(Handling);
            AddAttribute(FireRate);
            AddAttribute(Capacity);
            AddAttribute(Pellets);
            RecalculateAttributeValues();
        }

        public void RecalculateAttributeValues()
        {
            Damage.Val = _weapon.WeaponClass.Damage.GetScaledValue(_weapon.Durability.Val);
            Debug.Log("Before " + Damage.CalculatedValue() + " " + Damage.Val);
            Accuracy.Val = _weapon.WeaponClass.Accuracy.GetScaledValue(_weapon.Durability.Val);
            ReloadSpeed.Val = _weapon.WeaponClass.ReloadSpeed.GetScaledValue(_weapon.Durability.Val);
            CriticalChance.Val = _weapon.WeaponClass.CriticalChance.GetScaledValue(_weapon.Durability.Val);
            Handling.Val = _weapon.WeaponClass.Handling.GetScaledValue(_weapon.Durability.Val);
            FireRate.Val = _weapon.WeaponClass.FireRate.GetScaledValue(_weapon.Durability.Val);
            _weapon.SubClass.Modifier.Apply(this);
            Debug.Log("After " + Damage.CalculatedValue() + " " + Damage.Val);
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = CriticalChance.CalculatedValue() / 100 * Damage.CalculatedValue() * 2 + (1 - CriticalChance.CalculatedValue() / 100) * Damage.CalculatedValue();
            float magazineDamage = Capacity.CalculatedValue() * averageShotDamage;
            float magazineDuration = Capacity.CalculatedValue() / FireRate.CalculatedValue() + ReloadSpeed.CalculatedValue();
            _dps = magazineDamage / magazineDuration;
        }

        public float DPS()
        {
            return _dps;
        }
    }
}