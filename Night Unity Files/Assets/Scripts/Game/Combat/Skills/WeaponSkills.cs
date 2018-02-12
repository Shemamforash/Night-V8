using System;
using Game.Combat.Enemies;
using Game.Gear.Weapons;

namespace Game.Combat.Skills
{
    public static class WeaponSkills
    {
        public static Skill GetWeaponSkillOne(Weapon weapon)
        {
            switch (weapon.WeaponType())
            {
                case WeaponType.Rifle:
                    return new Gouge();
                case WeaponType.Shotgun:
                    return new Sweep();
                case WeaponType.LMG:
                    return new Refill();
                case WeaponType.SMG:
                    return new Hairpin();
                case WeaponType.Pistol:
                    return new Retribution();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static Skill GetWeaponSkillTwo(Weapon weapon)
        {
            switch (weapon.WeaponType())
            {
                case WeaponType.Rifle:
                    return  new Blast();
                case WeaponType.Shotgun:
                    return  new Swarm();
                case WeaponType.LMG:
                    return  new Compel();
                case WeaponType.SMG:
                    return  new Splinter();
                case WeaponType.Pistol:
                    return  new Revenge();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    //Rifle
    
    public class Gouge : Skill
    {
        public Gouge() : base(nameof(Gouge))
        {
            Duration = 2f;
        }

        protected override void OnFire()
        {
            base.OnFire();
            Shot shot = CreateShot();
            shot.SetPierceDepth(100);
            shot.SetPierceChance(1);
            shot.Fire();
        }
    }

    public class Blast : Skill
    {
        public Blast() : base(nameof(Blast))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Shot shot = CreateShot();
            shot.UseRemainingShots();
            shot.Fire();
        }
    }

    //Shotgun
    
    public class Sweep : Skill
    {
        public Sweep() : base(nameof(Sweep))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Shot shot = CreateShot();
            shot.GuaranteeHit();
            shot.SetKnockdownChance(1, 5);
            shot.Fire();
        }
    }

    public class Swarm : Skill
    {
        public Swarm() : base(nameof(Swarm))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            foreach (Enemy e in UIEnemyController.Enemies)
            {
                Shot s = new Shot(e, Player());
                s.Fire();
            }
        }
    }

    //LMG
    
    public class Refill : Skill
    {
        public Refill() : base(nameof(Refill))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().Weapon.Reload(Player().Inventory());
        }
    }

    public class Compel : Skill
    {
        public Compel() : base(nameof(Compel), true)
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().OnFireAction += s => { s.SetKnockdownChance(0.25f, 2); };
        }
    }
    
    //SMG

    public class Hairpin : Skill
    {
        public Hairpin() : base(nameof(Hairpin), true)
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().OnFireAction += s => { s.SetNumberOfShots(2); };
        }
    }

    public class Splinter : Skill
    {
        public Splinter() : base(nameof(Splinter), true)
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().OnFireAction += s => { s.AddOnHit(() => { Explosion.CreateAndDetonate(s.Target().Position.CurrentValue(), 5, s.DamageDealt()); }); };
        }
    }

    //Pistol
    
    public class Retribution : Skill
    {
        public Retribution() : base(nameof(Retribution), true)
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().OnFireAction += s =>
            {
                Player().RageController.Increase(1);
                Player().RageController.TryStart();
            };
        }
    }

    public class Revenge : Skill
    {
        public Revenge() : base(nameof(Revenge), true)
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().Retaliate = true;
        }
    }
}