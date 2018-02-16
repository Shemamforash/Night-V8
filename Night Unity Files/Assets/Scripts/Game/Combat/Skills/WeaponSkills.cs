using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
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
                    return new Blast();
                case WeaponType.Shotgun:
                    return new Swarm();
                case WeaponType.LMG:
                    return new Compel();
                case WeaponType.SMG:
                    return new Splinter();
                case WeaponType.Pistol:
                    return new Revenge();
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
            List<DetailedEnemyCombat> enemiesBehindTarget = CombatManager.GetEnemiesBehindTarget(CombatManager.Player.CurrentTarget);
            enemiesBehindTarget.Add(CombatManager.Player.CurrentTarget);
            foreach (DetailedEnemyCombat enemyCombat in enemiesBehindTarget)
            {
                Shot s = new Shot(enemyCombat, CombatManager.Player);
                s.GuaranteeHit();
                s.Fire();
            }
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
            for (int i = 0; i < CombatManager.Player.Weapon().GetRemainingAmmo(); ++i)
            {
                CreateShot().Fire();
            }
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
            foreach (DetailedEnemyCombat e in UIEnemyController.Enemies)
            {
                Shot s = new Shot(e, CombatManager.Player);
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
            CombatManager.Player.Weapon().Reload(CombatManager.Player.Player.Inventory());
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
            CombatManager.Player.OnFireAction += s => { s.SetKnockdownChance(0.25f, 2); };
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
            CombatManager.Player.OnFireAction += s => { CombatManager.Player.FireWeapon(s.Target()); };
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
            CombatManager.Player.OnFireAction += s => { s.AddOnHit(() => { Explosion.CreateAndDetonate(s.Target().Position.CurrentValue(), 5, s.DamageDealt()); }); };
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
            CombatManager.Player.OnFireAction += s =>
            {
                CombatManager.Player.RageController.Increase(1);
                CombatManager.Player.RageController.TryStart();
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
            CombatManager.Player.Retaliate = true;
        }
    }
}