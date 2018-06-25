using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : AttributeContainer
    {
        private readonly int MaxDurability;
        private const float MinDurabilityMod = 0.75f;
        public readonly Number Durability;
        private float _dps;
        private readonly AttributeModifier _durabilityModifier;
        private readonly Weapon _weapon;
        public bool Automatic = true;
        public float DurabilityModifier;
        public CharacterAttribute FireRate, ReloadSpeed, Damage, Handling, Capacity, Pellets, Accuracy;
        public string ModifierName, ModifierDescription;
        public CharacterAttribute BurnChance, BleedChance, SicknessChance;

        public WeaponClassType WeaponClassName;
        public string WeaponClassDescription;
        public WeaponType WeaponType;

        public WeaponAttributes(Weapon weapon)
        {
            _weapon = weapon;
            _durabilityModifier = new AttributeModifier();
            Damage.AddModifier(_durabilityModifier);
            FireRate.AddModifier(_durabilityModifier);
            Accuracy.AddModifier(_durabilityModifier);
            MaxDurability = ((int) weapon.Quality() + 1) * 10;
            Durability = new Number(MaxDurability, 0, MaxDurability);
        }

        public XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("WeaponType", root, WeaponType);
            SaveController.CreateNodeAndAppend("Class", root, WeaponClassName);
            SaveController.CreateNodeAndAppend("Durability", root, Durability.CurrentValue());
            SaveController.CreateNodeAndAppend("Quality", root, _weapon.Quality());
            return root;
        }

        public void SetDurability(int value)
        {
            Durability.SetCurrentValue(value);
            RecalculateAttributeValues();
        }

        public void SetClass(WeaponClass weaponClass)
        {
            FireRate.SetCurrentValue(weaponClass.FireRate);
            ReloadSpeed.SetCurrentValue(weaponClass.ReloadSpeed);
            Damage.SetCurrentValue(weaponClass.Damage);
            Handling.SetCurrentValue(weaponClass.Handling);
            Capacity.SetCurrentValue(weaponClass.Capacity);
            Pellets.SetCurrentValue(weaponClass.Pellets);
            Accuracy.SetCurrentValue(weaponClass.Accuracy);
            WeaponType = weaponClass.Type;
            Automatic = weaponClass.Automatic;
            WeaponClassName = weaponClass.Name;
            RecalculateAttributeValues();
        }


        public void SetInscription(Inscription inscription)
        {
//            inscription.ApplyToGear(this);
//            ModifierName = inscription.Name;
//            ModifierDescription = inscription.GetDescription();
//            RecalculateAttributeValues();
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
            float averageShotDamage = Damage.CurrentValue();
            float magazineDamage = (int) Capacity.CurrentValue() * averageShotDamage * (int) Pellets.CurrentValue();
            float magazineDuration = (int) Capacity.CurrentValue() / FireRate.CurrentValue() + ReloadSpeed.CurrentValue();
            _dps = magazineDamage / magazineDuration;
        }

        public float DPS() => _dps;

        protected override void CacheAttributes()
        {
            Damage = new CharacterAttribute(this, AttributeType.Damage, 0);
            Accuracy = new CharacterAttribute(this, AttributeType.Accuracy, 0, 0, 100);
            FireRate = new CharacterAttribute(this, AttributeType.FireRate, 0);
            ReloadSpeed = new CharacterAttribute(this, AttributeType.ReloadSpeed, 0);
            Handling = new CharacterAttribute(this, AttributeType.Handling, 0);

            Capacity = new CharacterAttribute(this, AttributeType.Capacity, 0);
            Pellets = new CharacterAttribute(this, AttributeType.Pellets, 0);

            BurnChance = new CharacterAttribute(this, AttributeType.BurnChance, 0);
            BleedChance = new CharacterAttribute(this, AttributeType.DecayChance, 0);
            SicknessChance = new CharacterAttribute(this, AttributeType.SicknessChance, 0);
        }

        public string Print() => WeaponType + " " + WeaponClassName + " " + ModifierName
                                 + "\nDurability: " + Durability.CurrentValue() + " (" + DurabilityModifier + ")"
                                 + "\nDPS: " + DPS()
                                 + "\nAutomatic: " + Automatic
                                 + "\nCapacity:   " + Capacity.CurrentValue()
                                 + "\nPellets:    " + Pellets.CurrentValue()
                                 + "\nDamage:     " + Damage.CurrentValue()
                                 + "\nFire Rate:  " + FireRate.CurrentValue()
                                 + "\nReload:     " + ReloadSpeed.CurrentValue()
                                 + "\nAccuracy: " + Accuracy.CurrentValue()
                                 + "\n" + WeaponClassDescription?.Replace("\n", " ")
                                 + "\n" + ModifierDescription?.Replace("\n", " ") + "\n\n";

        public void DecreaseDurability()
        {
            float durabilityLoss = Damage.CurrentValue() * Pellets.CurrentValue() / ReloadSpeed.CurrentValue();
            durabilityLoss /= 200f;
            Durability.Decrement(durabilityLoss);
            RecalculateAttributeValues();
        }

        public void IncreaseDurability()
        {
        }
    }
}