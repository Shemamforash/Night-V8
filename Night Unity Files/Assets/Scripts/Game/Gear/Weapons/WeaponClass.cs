using Game.Combat.Player;

namespace Game.Gear.Weapons
{
    public class WeaponClass
    {
        public readonly int AmmoCost;
        public readonly bool Automatic;
        public readonly string Name;
        public readonly WeaponType Type;
        public readonly int Pellets, Capacity, Handling, Accuracy, Damage;
        public readonly float ReloadSpeed, FireRate;

        public WeaponClass(WeaponType type, string name, bool automatic, int ammoCost, int damage, float fireRate, float reloadSpeed, int accuracy, int handling, int capacity, int pellets)
        {
            Type = type;
            Automatic = automatic;
            AmmoCost = ammoCost;
            Name = name;
            Damage = damage;
            FireRate = fireRate;
            ReloadSpeed = reloadSpeed;
            Accuracy = accuracy;
            Handling = handling;
            Capacity = capacity;
            Pellets = pellets;
        }

        public Weapon CreateWeapon(ItemQuality quality)
        {
            Weapon w = new Weapon(Type.ToString(), 10, quality);
            WeaponAttributes weaponAttributes = w.WeaponAttributes;
            weaponAttributes.SetClass(this);
            w.WeaponSkillOne = WeaponSkills.GetWeaponSkillOne(w);
            w.WeaponSkillTwo = WeaponSkills.GetWeaponSkillTwo(w);
            return w;
        }
    }
}