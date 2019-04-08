using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Exploration.WorldEvents;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : DesolationAttributes
    {
        private readonly Number _durability;
        private float _dps;
        private readonly AttributeModifier _damageDurabilityModifier;
        private readonly AttributeModifier _fireRateDurabilityModifier;
        private readonly AttributeModifier _reloadSpeedDurabilityModifier;
        private readonly AttributeModifier _accuracyDurabilityModifier;
        private readonly Weapon _weapon;
        private readonly string Description;
        public string FireType, FireMode;
        public bool Automatic = true;
        private WeaponClassType WeaponClassType;
        public WeaponType WeaponType;

        private readonly string[] _durabilityEvents =
        {
            "My weapon feels weak",
            "My weapon feels too light",
            "The power of my weapon is waning"
        };

        public WeaponAttributes(Weapon weapon, WeaponClass weaponClass)
        {
            _weapon = weapon;
            _damageDurabilityModifier = new AttributeModifier(-1);
            _fireRateDurabilityModifier = new AttributeModifier(-1);
            _reloadSpeedDurabilityModifier = new AttributeModifier(-1);
            _accuracyDurabilityModifier = new AttributeModifier(-1);
            AddMod(AttributeType.Damage, _damageDurabilityModifier);
            AddMod(AttributeType.FireRate, _fireRateDurabilityModifier);
            AddMod(AttributeType.ReloadSpeed, _reloadSpeedDurabilityModifier);
            AddMod(AttributeType.Accuracy, _accuracyDurabilityModifier);
            int maxDurability = ((int) weapon.Quality() + 1) * 10;
            _durability = new Number(maxDurability, 0, maxDurability);
            SetMax(AttributeType.Accuracy, 1);
            SetClass(weaponClass);
            Description = weaponClass.Description;
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
            SetVal(AttributeType.Recoil, weaponClass.Recoil);
            SetVal(AttributeType.Capacity, weaponClass.Capacity);
            SetVal(AttributeType.Pellets, weaponClass.Pellets);
            SetVal(AttributeType.Accuracy, weaponClass.Accuracy);
            WeaponType = weaponClass.Type;
            Automatic = weaponClass.Automatic;
            WeaponClassType = weaponClass.Name;
            FireType = weaponClass.FireType;
            FireMode = weaponClass.FireMode;
            RecalculateAttributeValues();
        }

        public WeaponClassType GetWeaponClass() => WeaponClassType;

        public void RecalculateAttributeValues()
        {
            float damageModifier = 0.08f * _durability.CurrentValue();
            float fireRateModifier = 0.015f * _durability.CurrentValue();
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

        public void DecreaseDurability(float shots, float durabilityModifier)
        {
            float durabilityLossPerShot = 0.01f / Val(AttributeType.Pellets);
            float durabilityLoss = durabilityLossPerShot * shots * durabilityModifier;
            float durabilityBefore = _durability.Normalised();
            _durability.Decrement(durabilityLoss);
            float durabilityAfter = _durability.Normalised();
            if (durabilityBefore >= 0.25 && durabilityAfter < 0.25f) GenerateDurabilityEvent();
            RecalculateAttributeValues();
        }

        private void GenerateDurabilityEvent()
        {
            WorldEventManager.GenerateEvent(new WorldEvent(_durabilityEvents.RandomElement()));
        }

        public void IncreaseDurability(int durabilityGain)
        {
            _durability.Increment(durabilityGain * 10);
            RecalculateAttributeValues();
        }

        public Number GetDurability()
        {
            return _durability;
        }

        public string GetWeaponTypeDescription()
        {
            return Description;
        }

        private float CalculateConditionChance(AttributeType condition)
        {
            float weaponChance = Val(condition);
            float characterChance = 0;
            if (_weapon.EquippedCharacter is Player player) characterChance += player.Attributes.Val(condition);
            float totalChance = weaponChance + characterChance;
            totalChance = totalChance.Round(2);
            return totalChance;
        }

        public float CalculateShatterChance() => CalculateConditionChance(AttributeType.Shatter);

        public float CalculateBurnChance() => CalculateConditionChance(AttributeType.Burn);

        public float CalculateVoidChance() => CalculateConditionChance(AttributeType.Void);
    }
}