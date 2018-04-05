using System;
using System.Collections.Generic;

namespace Game.Characters
{
    public class BrandManager
    {
        private Dictionary<BrandType, bool> _activeBrands = new Dictionary<BrandType, bool>();
        private Player.Player _character;
        private int _hoursAtHighWeight, _hoursAtLowWeight;
        private int _hoursIdling, _hoursTravelling;

        public BrandManager(Player.Player character)
        {
            _character = character;
            foreach (BrandType brandType in Enum.GetValues(typeof(BrandType)))
            {
                _activeBrands.Add(brandType, false);
            }
        }

        private void CheckBrandUnlock(bool condition, BrandType brand, Action unlockAction)
        {
            if (!condition || _activeBrands[brand]) return;
            _activeBrands[brand] = true;
            unlockAction();
        }

        public void IncreaseTimeSpentHighCapacity()
        {
            ++_hoursAtHighWeight;
            CheckBrandUnlock(_hoursAtHighWeight >= 100, BrandType.Powerful, ()=> _character.Attributes.Strength.Max++);
            CheckBrandUnlock(_hoursAtHighWeight >= 200, BrandType.Monolithic, ()=> _character.Attributes.Strength.Max++);
            CheckBrandUnlock(_hoursAtHighWeight >= 400, BrandType.Godlike, ()=> _character.Attributes.Strength.Max++);
        }

        public void IncreaseTimeSpentLowCapacity()
        {
            ++_hoursAtLowWeight;
            CheckBrandUnlock(_hoursAtLowWeight >= 200, BrandType.Pathetic, ()=> _character.Attributes.Strength.Max--);
            CheckBrandUnlock(_hoursAtLowWeight >= 400, BrandType.Feeble, ()=> _character.Attributes.Strength.Max--);
        }

        public void IncreaseIdleTime()
        {
            ++_hoursIdling;
            CheckBrandUnlock(_hoursIdling >= 10, BrandType.Lazy, ()=> _character.Attributes.Endurance.Max--);
            CheckBrandUnlock(_hoursIdling >= 300, BrandType.Sloth, ()=> _character.Attributes.Endurance.Max--);
        }

        public void IncreaseTravelTime()
        {
            ++_hoursTravelling;
            CheckBrandUnlock(_hoursTravelling >= 100, BrandType.Nomadic, ()=> _character.Attributes.Endurance.Max++);
            CheckBrandUnlock(_hoursTravelling >= 200, BrandType.Enduring, ()=> _character.Attributes.Endurance.Max++);
            CheckBrandUnlock(_hoursTravelling >= 400, BrandType.Tireless, ()=> _character.Attributes.Endurance.Max++);
        }
    }

    public enum BrandType
    {
        Pathetic,
        Feeble,
        Powerful,
        Monolithic,
        Godlike,
        Sloth,
        Lazy,
        Nomadic,
        Enduring,
        Tireless
    }
}