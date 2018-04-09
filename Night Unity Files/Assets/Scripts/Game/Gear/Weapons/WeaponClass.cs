using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;

namespace Game.Gear.Weapons
{
    public class WeaponClass
    {
        public readonly int AmmoCost;
        public readonly bool Automatic;
        public readonly List<AttributeModifier> Modifiers = new List<AttributeModifier>();
        public readonly string Name;
        public readonly WeaponType Type;

        public WeaponClass(WeaponType type, string name, bool automatic, int ammoCost)
        {
            Type = type;
            Automatic = automatic;
            AmmoCost = ammoCost;
            Name = name;
        }

        public Weapon CreateWeapon(ItemQuality quality, int durability = -1)
        {
            Weapon w = new Weapon(Type.ToString(), 10, quality, durability);
            WeaponAttributes weaponAttributes = w.WeaponAttributes;
            weaponAttributes.SetClass(this);
            w.WeaponSkillOne = WeaponSkills.GetWeaponSkillOne(w);
            w.WeaponSkillTwo = WeaponSkills.GetWeaponSkillTwo(w);
            return w;
        }

        public void AddAttributeModifier(AttributeModifier modifier)
        {
            Modifiers.Add(modifier);
        }
    }
}