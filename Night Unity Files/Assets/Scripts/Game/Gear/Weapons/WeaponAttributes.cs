using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : DesolationAttributes
    {
        private readonly int MaxDurability;
        private const float MinDurabilityMod = 0.75f;
        private readonly Number _durability;
        private float _dps;
        private readonly AttributeModifier _durabilityModifier;
        private readonly Weapon _weapon;
        public bool Automatic = true;
        private WeaponClassType WeaponClassType;
        public WeaponType WeaponType;

        public WeaponAttributes(Weapon weapon, WeaponClass weaponClass)
        {
            _weapon = weapon;
            _durabilityModifier = new AttributeModifier();
            AddMod(AttributeType.Damage, _durabilityModifier);
            AddMod(AttributeType.FireRate, _durabilityModifier);
            AddMod(AttributeType.Accuracy, _durabilityModifier);
            MaxDurability = ((int) weapon.Quality() + 1) * 10;
            _durability = new Number(MaxDurability, 0, MaxDurability);
            SetMax(AttributeType.Accuracy, 100);
            SetClass(weaponClass);
        }

        public XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("WeaponType", root, WeaponType);
            SaveController.CreateNodeAndAppend("Class", root, WeaponClassType);
            SaveController.CreateNodeAndAppend("Durability", root, _durability.CurrentValue());
            SaveController.CreateNodeAndAppend("Quality", root, _weapon.Quality());
            return root;
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
            float normalisedDurability = _durability.CurrentValue() / MaxDurability;
            float qualityModifier = (int) _weapon.Quality() + 1 / 2f;
            float durabilityModifierValue = MinDurabilityMod + (1 - MinDurabilityMod) * normalisedDurability * qualityModifier;
            --durabilityModifierValue;
            _durabilityModifier.SetFinalBonus(durabilityModifierValue);
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
            durabilityLoss /= 200f;
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