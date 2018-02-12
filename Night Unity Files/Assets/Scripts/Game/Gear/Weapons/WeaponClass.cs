using Game.Combat.Skills;

namespace Game.Gear.Weapons
{
    public class WeaponClass : GearModifier
    {
        public readonly bool Automatic;
        public readonly WeaponType Type;
        public readonly int AmmoCost;
        public readonly string Name;
        
        public WeaponClass(WeaponType type,string name, bool automatic, int ammoCost) : base(type.ToString())
        {
            Type = type;
            Automatic = automatic;
            AmmoCost = ammoCost;
            Name = name;
        }

        public Weapon CreateWeapon(int durability)
        {
            Weapon w = new Weapon(Type.ToString(), 10, durability);
            WeaponAttributes weaponAttributes = w.WeaponAttributes;
            weaponAttributes.SetClass(this);
            w.WeaponSkillOne = WeaponSkills.GetWeaponSkillOne(w);
            w.WeaponSkillTwo = WeaponSkills.GetWeaponSkillTwo(w);
            return w;
        }
    }
}