using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : AttributeContainer
    {
        public CharacterAttribute Damage, Accuracy, CriticalChance, Handling, FireRate, ReloadSpeed;
        private readonly Weapon _weapon;
        private float _dps;
        
        public WeaponAttributes(Weapon weapon)
        {
            _weapon = weapon;
            RecalculateAttributeValues();
        }

        public void RecalculateAttributeValues()
        {
            Damage.SetCurrentValue((int) _weapon.WeaponClass.Damage.GetScaledValue(_weapon.Durability.GetCurrentValue()));
            Accuracy.SetCurrentValue((int) _weapon.WeaponClass.Accuracy.GetScaledValue(_weapon.Durability.GetCurrentValue()));
            CriticalChance.SetCurrentValue((int) _weapon.WeaponClass.CriticalChance.GetScaledValue(_weapon.Durability.GetCurrentValue()));
            Handling.SetCurrentValue((int) _weapon.WeaponClass.Handling.GetScaledValue(_weapon.Durability.GetCurrentValue()));
            
            FireRate.SetCurrentValue(_weapon.WeaponClass.FireRate.GetScaledValue(_weapon.Durability.GetCurrentValue()));
            ReloadSpeed.SetCurrentValue(_weapon.WeaponClass.ReloadSpeed.GetScaledValue(_weapon.Durability.GetCurrentValue()));
            
            _weapon.SubClass.Apply(this);
            _weapon.SecondaryModifier.Apply(this);
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = CriticalChance.GetCalculatedValue() / 100 * Damage.GetCalculatedValue() * 2 + (1 - CriticalChance.GetCalculatedValue() / 100) * Damage.GetCalculatedValue();
            float magazineDamage = _weapon.Capacity * averageShotDamage * _weapon.Pellets * Accuracy.GetCalculatedValue() / 100;
            float magazineDuration =  _weapon.Capacity / FireRate.GetCalculatedValue() + ReloadSpeed.GetCalculatedValue();
            _dps = magazineDamage / magazineDuration;
        }

        public float DPS()
        {
            return _dps;
        }

        protected override void CacheAttributes()
        {
            Damage = new CharacterAttribute(AttributeType.Damage, 0);
            Accuracy= new CharacterAttribute(AttributeType.Accuracy, 0, 0, 100);
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