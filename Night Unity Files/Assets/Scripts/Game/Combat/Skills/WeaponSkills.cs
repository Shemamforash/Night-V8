using System;
using Game.Combat.Enemies;
using Game.Gear.Weapons;

namespace Game.Combat.Skills
{
    public static class WeaponSkills
    {
        public static void GetWeaponSkills(Weapon weapon)
        {
            Skill skillOne, skillTwo;
            switch (weapon.WeaponType())
            {
                case WeaponType.Rifle:
                    skillOne = new Gouge();
                    skillTwo = new Blast();
                    break;
                case WeaponType.Shotgun:
                    skillOne = new Sweep();
                    skillTwo = new Swarm();
                    break;
                case WeaponType.LMG:
                    skillOne = new Refill();
                    skillTwo = new Compel();
                    break;
                case WeaponType.SMG:
                    skillOne = new Hairpin();
                    skillTwo = new Splinter();
                    break;
                case WeaponType.Pistol:
                    skillOne = new Retribution();
                    skillTwo = new Revenge();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            weapon.WeaponSkillOne= skillOne;
            weapon.WeaponSkillTwo = skillTwo;
        }
    }
    
    //Rifle
    
    public class Gouge : Skill
    {
        public Gouge() : base(true, nameof(Gouge))
        {
            Duration = 2f;
        }

        protected override void OnFire()
        {
            base.OnFire();
            Shot.SetPierceDepth(100);
            Shot.SetPierceChance(1);
            Shot.Fire();
        }
    }

    public class Blast : Skill
    {
        public Blast() : base(true, nameof(Blast))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Shot.UseRemainingShots();
            Shot.Fire();
        }
    }

    //Shotgun
    
    public class Sweep : Skill
    {
        public Sweep() : base(true, nameof(Sweep))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Shot.GuaranteeHit();
            Shot.Fire();
            Shot knockdownShot = new Shot(Shot.Target(), Shot.Origin());
            knockdownShot.SetKnockdownChance(1, 5);
            knockdownShot.Fire();
        }
    }

    public class Swarm : Skill
    {
        public Swarm() : base(true, nameof(Swarm))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            foreach (Enemy e in UIEnemyController.Enemies)
            {
                Shot s = new Shot(e, Shot.Origin());
                s.Fire();
            }
        }
    }

    //LMG
    
    public class Refill : Skill
    {
        public Refill() : base(true, nameof(Refill))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().Weapon().Reload(Player().Inventory());
        }
    }

    public class Compel : Skill
    {
        public Compel() : base(true, nameof(Compel))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().OnFireAction = s => { s.SetKnockdownChance(0.25f, 2); };
        }
    }
    
    //SMG

    public class Hairpin : Skill
    {
        public Hairpin() : base(true, nameof(Hairpin))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().OnFireAction = s => { s.SetNumberOfShots(2); };
        }
    }

    public class Splinter : Skill
    {
        public Splinter() : base(true, nameof(Splinter))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().OnFireAction = s => { s.AddOnHit(() => { Explosion.CreateAndDetonate(s.Target().Position.CurrentValue(), 5, s.DamageDealt()); }); };
        }
    }

    //Pistol
    
    public class Retribution : Skill
    {
        public Retribution() : base(true, nameof(Retribution))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().OnFireAction = s =>
            {
                Player().RageController.Increase(100);
                Player().RageController.TryStart();
            };
        }
    }

    public class Revenge : Skill
    {
        public Revenge() : base(true, nameof(Revenge))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().Retaliate = true;
        }
    }
}