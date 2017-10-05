using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Gear.Weapons
{
    public class WeaponSubClass
    {
        public readonly string Name;
        public readonly int Capacity, Pellets;
        public readonly AttributesModifier Modifier;

        public WeaponSubClass(string name, int capacity, int pellets, AttributesModifier modifier)
        {
            Name = name;
            Capacity = capacity;
            Pellets = pellets;
            Modifier = modifier;
        }
    }
}