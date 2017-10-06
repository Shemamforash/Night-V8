using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Gear.Weapons
{
    public class WeaponAttributes : AttributeContainer
    {
        public readonly IntAttribute Damage, Accuracy, CriticalChance, Handling;
        public readonly FloatAttribute FireRate, ReloadSpeed;
        private readonly Weapon _weapon;
        private float _dps;
        
        public WeaponAttributes(Weapon weapon)
        {
            _weapon = weapon;
            Damage = new IntAttribute(AttributeType.Damage, 0);
            Accuracy= new IntAttribute(AttributeType.Accuracy, 0, 0, 100);
            CriticalChance = new IntAttribute(AttributeType.CriticalChance, 0, 0, 100);
            Handling = new IntAttribute(AttributeType.Handling, 0, 0, 100);
            FireRate = new FloatAttribute(AttributeType.FireRate, 0);
            ReloadSpeed = new FloatAttribute(AttributeType.ReloadSpeed, 0);
            AddAttribute(Damage);
            AddAttribute(Accuracy);
            AddAttribute(ReloadSpeed);
            AddAttribute(CriticalChance);
            AddAttribute(Handling);
            AddAttribute(FireRate);
            RecalculateAttributeValues();
        }

        public void RecalculateAttributeValues()
        {
            Damage.Val = (int) _weapon.WeaponClass.Damage.GetScaledValue(_weapon.Durability.Val);
            Accuracy.Val = (int) _weapon.WeaponClass.Accuracy.GetScaledValue(_weapon.Durability.Val);
            CriticalChance.Val = (int) _weapon.WeaponClass.CriticalChance.GetScaledValue(_weapon.Durability.Val);
            Handling.Val = (int) _weapon.WeaponClass.Handling.GetScaledValue(_weapon.Durability.Val);
            
            FireRate.Val = _weapon.WeaponClass.FireRate.GetScaledValue(_weapon.Durability.Val);
            ReloadSpeed.Val = _weapon.WeaponClass.ReloadSpeed.GetScaledValue(_weapon.Durability.Val);
            
            _weapon.SubClass.Apply(this);
            _weapon.SecondaryModifier.Apply(this);
            CalculateDPS();
        }

        private void CalculateDPS()
        {
            float averageShotDamage = CriticalChance.CalculatedValue() / 100 * Damage.CalculatedValue() * 2 + (1 - CriticalChance.CalculatedValue() / 100) * Damage.CalculatedValue();
            float magazineDamage = _weapon.Capacity * averageShotDamage * _weapon.Pellets * Accuracy.CalculatedValue() / 100;
            float magazineDuration =  _weapon.Capacity / FireRate.CalculatedValue() + ReloadSpeed.CalculatedValue();
            _dps = magazineDamage / magazineDuration;
        }

        public float DPS()
        {
            return _dps;
        }
    }
}