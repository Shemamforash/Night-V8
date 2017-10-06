using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Gear.Weapons
{
    public class WeaponModifier : AttributesModifier
    {
        public readonly string Name;
        public readonly int Capacity, Pellets;
        public readonly float CapacityModifier;

        public WeaponModifier(string name, int capacity, int pellets, float capacityModifier = 1)
        {
            Name = name;
            Capacity = capacity;
            Pellets = pellets;
            CapacityModifier = capacityModifier;
        }

        public string GetDescription(bool includePellets = false)
        {
            string description = Name + ":";
            if (CapacityModifier != 1)
            {
                description += "\n" + AddPrefix(CapacityModifier) + "% Capacity";
            }
            if (includePellets && Pellets != 1)
            {
                description += "\nx" + Pellets + " rnds/shot";
            }
            foreach (AttributeType type in SummativeModifiers.Keys)
            {
                float modifierValue = SummativeModifiers[type];
                if (modifierValue != 0)
                {
                    description += "\n" + AddPrefix(modifierValue) + " " + type;
                }
            }
            foreach (AttributeType type in MultiplicativeModifiers.Keys)
            {
                float modifierValue = MultiplicativeModifiers[type];
                if (modifierValue != 0)
                {
                    description += "\n" + AddPrefix(modifierValue) + "% " + type;
                }
            }
            return description;
        }

        private string AddPrefix(float value)
        {
            if (value <= 0)
            {
                return value.ToString();
            }
            return "+" + value;
        }
    }
}