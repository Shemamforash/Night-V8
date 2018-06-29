using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : DesolationAttributes
    {
        private readonly int MaxDurability;
        private const float MinDurabilityMod = 0.75f;
        public readonly Number Durability;
        private float _dps;
        private readonly AttributeModifier _durabilityModifier;
        private readonly Weapon _weapon;
        public bool Automatic = true;
        public float DurabilityModifier;
        public string ModifierName, ModifierDescription;

        public WeaponClassType WeaponClassName;
        public string WeaponClassDescription;
        public WeaponType WeaponType;

        public WeaponAttributes(Weapon weapon)
        {
            _weapon = weapon;
            _durabilityModifier = new AttributeModifier();
            AddMod(AttributeType.Damage, _durabilityModifier);
            AddMod(AttributeType.FireRate, _durabilityModifier);
            AddMod(AttributeType.Accuracy, _durabilityModifier);
            MaxDurability = ((int) weapon.Quality() + 1) * 10;
            Durability = new Number(MaxDurability, 0, MaxDurability);
            SetMax(AttributeType.Accuracy, 100);
        }

        public XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("WeaponType", root, WeaponType);
            SaveController.CreateNodeAndAppend("Class", root, WeaponClassName);
            SaveController.CreateNodeAndAppend("Durability", root, Durability.CurrentValue());
            SaveController.CreateNodeAndAppend("Quality", root, _weapon.Quality());
            return root;
        }

        public void SetClass(WeaponClass weaponClass)
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
            WeaponClassName = weaponClass.Name;
            RecalculateAttributeValues();
        }

        public WeaponClassType GetWeaponClass() => WeaponClassName;

        public void RecalculateAttributeValues()
        {
            float normalisedDurability = Durability.CurrentValue() / MaxDurability;
            float qualityModifier = (int) _weapon.Quality() + 1 / 2f;
            DurabilityModifier = MinDurabilityMod + (1 - MinDurabilityMod) * normalisedDurability * qualityModifier;
            --DurabilityModifier;
            _durabilityModifier.SetFinalBonus(DurabilityModifier);
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = Val(AttributeType.Damage);
            float magazineDamage = (int) Val(AttributeType.Capacity) * averageShotDamage * (int) Val(AttributeType.Pellets);
            float magazineDuration = (int) Val(AttributeType.Capacity) / Val(AttributeType.FireRate) + Val(AttributeType.ReloadSpeed);
            _dps = magazineDamage / magazineDuration;
        }

        public float DPS() => _dps;

        public string Print() => WeaponType + " " + WeaponClassName + " " + ModifierName
                                 + "\nDurability: " + Durability.CurrentValue() + " (" + DurabilityModifier + ")"
                                 + "\nDPS: " + DPS()
                                 + "\nAutomatic: " + Automatic
                                 + "\nCapacity:   " + Val(AttributeType.Capacity)
                                 + "\nPellets:    " + Val(AttributeType.Pellets)
                                 + "\nDamage:     " + Val(AttributeType.Damage)
                                 + "\nFire Rate:  " + Val(AttributeType.FireRate)
                                 + "\nReload:     " + Val(AttributeType.ReloadSpeed)
                                 + "\nAccuracy: " + Val(AttributeType.Accuracy)
                                 + "\n" + WeaponClassDescription?.Replace("\n", " ")
                                 + "\n" + ModifierDescription?.Replace("\n", " ") + "\n\n";

        public void DecreaseDurability()
        {
            float durabilityLoss = Val(AttributeType.Damage) * Val(AttributeType.Pellets) / Val(AttributeType.ReloadSpeed);
            durabilityLoss /= 200f;
            Durability.Decrement(durabilityLoss);
            RecalculateAttributeValues();
        }

        public void IncreaseDurability()
        {
        }
    }
}