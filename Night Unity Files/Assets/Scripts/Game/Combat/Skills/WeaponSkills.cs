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
            Player().EquipmentController.Weapon().Reload(Player().Inventory());
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