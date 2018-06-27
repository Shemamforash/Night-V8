using System;
using System.Collections.Generic;

namespace Game.Characters
{
    public class BrandManager
    {
        private readonly Dictionary<BrandType, bool> _activeBrands = new Dictionary<BrandType, bool>();
        private readonly Player _character;
        private int _hoursAtHighWeight, _hoursAtLowWeight;
        private int _hoursIdling, _hoursTravelling;

        public BrandManager(Player character)
        {
            _character = character;
            foreach (BrandType brandType in Enum.GetValues(typeof(BrandType))) _activeBrands.Add(brandType, false);
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
        }

        public void IncreaseTimeSpentLowCapacity()
        {
            ++_hoursAtLowWeight;
        }

        public void IncreaseIdleTime()
        {
            ++_hoursIdling;
        }

        public void IncreaseTravelTime()
        {
            ++_hoursTravelling;
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