using System.Collections.Generic;
using Game.Gear;
using UnityEngine;

namespace Game.Combat.Weapons
{
    public class WeaponBase
    {
        public enum Attributes
        {
            Damage,
            Accuracy,
            FireRate,
            Handling,
            ReloadSpeed,
            CriticalChance,
            Capacity
        }

        private class WeaponAttribute
        {
            private float _min, _max;

            public WeaponAttribute(float min, float max)
            {
                _min = min;
                _max = max;
            }

            public float Min()
            {
                return _min;
            }

            public float Max()
            {
                return _max;
            }
        }

        private float _damageMin,
            _damageMax,
            _accuracyMin,
            _accuracyMax,
            _fireRateMin,
            _fireRateMax,
            _handlingMin,
            _handlingMax,
            _reloadTimeMin,
            _reloadTimeMax,
            _criticalChanceMin,
            _criticalChanceMax;

        private int _capacityMin, _ammoMax;
        private bool _automatic;
        public string Suffix;
        public WeaponGenerator.WeaponType Type;
        public WeaponGenerator.WeaponRarity Rarity;
        private WeaponClass _weaponClass;
        private AttributeModifier _primaryAttributeModifier, _secondaryAttributeModifier;

        private Dictionary<Attributes, WeaponAttribute> attributeDictionary =
            new Dictionary<Attributes, WeaponAttribute>();

        public WeaponBase(WeaponGenerator.WeaponType type, WeaponGenerator.WeaponRarity rarity, string suffix)
        {
            Type = type;
            Rarity = rarity;
            Suffix = suffix;
        }

        public float GetAttributeValue(Attributes attribute)
        {
            WeaponAttribute a = attributeDictionary[attribute];
            return Random.Range(a.Min(), a.Max());
        }

        public void SetAttribute(Attributes attribute, float min, float max)
        {
            WeaponAttribute damage = new WeaponAttribute(min, max);
            attributeDictionary[attribute] = damage;
        }
    }
}