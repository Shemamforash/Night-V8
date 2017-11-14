using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : AttributeContainer
    {
        public CharacterAttribute Damage, Accuracy, CriticalChance, Handling, FireRate, ReloadSpeed;
        private readonly Weapon _weapon;
        private float _dps;
        private const float MaxDurability = 20;

        public WeaponAttributes(Weapon weapon)
        {
            _weapon = weapon;
            RecalculateAttributeValues();
        }

        public void RecalculateAttributeValues()
        {
            float durabilityModifier = 1f / (MaxDurability * 2) * (_weapon.Durability.GetCurrentValue() + MaxDurability);

            Damage.SetCurrentValue(_weapon.WeaponClass.Damage * durabilityModifier);
            Accuracy.SetCurrentValue(_weapon.WeaponClass.Accuracy * durabilityModifier);
            CriticalChance.SetCurrentValue(_weapon.WeaponClass.CriticalChance * durabilityModifier);
            Handling.SetCurrentValue(_weapon.WeaponClass.Handling * durabilityModifier);
            FireRate.SetCurrentValue(_weapon.WeaponClass.FireRate * durabilityModifier);
            ReloadSpeed.SetCurrentValue(_weapon.WeaponClass.ReloadSpeed * durabilityModifier);

            _weapon.SubClass.Apply(this);
            _weapon.SecondaryModifier.Apply(this);
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = CriticalChance.GetCalculatedValue() / 100 * Damage.GetCalculatedValue() * 2 + (1 - CriticalChance.GetCalculatedValue() / 100) * Damage.GetCalculatedValue();
            float magazineDamage = _weapon.Capacity * averageShotDamage * _weapon.Pellets * Accuracy.GetCalculatedValue() / 100;
            float magazineDuration = _weapon.Capacity / FireRate.GetCalculatedValue() + ReloadSpeed.GetCalculatedValue();
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
            AddAttribute(Damage);
            AddAttribute(Accuracy);
            AddAttribute(ReloadSpeed);
            AddAttribute(CriticalChance);
            AddAttribute(Handling);
            AddAttribute(FireRate);
        }
    }
}