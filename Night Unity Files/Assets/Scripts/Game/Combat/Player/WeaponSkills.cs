using System;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Gear.Weapons;

namespace Game.Combat.Player
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
                    return new Passion();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Skill GetWeaponSkillTwo(Weapon weapon)
        {
            switch (weapon.WeaponType())
            {
                case WeaponType.Rifle:
                    return new Impact();
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
        }

        protected override void OnFire()
        {
            Shot s = Shot.Create(PlayerCombat.Instance);
            s.AddOnHit(() =>
            {
                for (int i = 0; i < 10; ++i)
                {
                    Shot fragment = Shot.Create(s);
                    fragment.SetAccuracy(180f);
                    fragment.Fire(0.4f);
                }
            });
            s.Fire();
        }
    }

    public class Impact : Skill
    {
        public Impact() : base(nameof(Impact))
        {
        }

        protected override void OnFire()
        {
            Shot s = Shot.Create(PlayerCombat.Instance);
            s.SetBurnChance(1);
            s.Fire();
//            PlayerCombat.Instance.Weapon().ConsumeAmmo(PlayerCombat.Instance.Weapon().GetRemainingAmmo());
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
            Shot shot = CreateShot();
            shot.GuaranteeHit();
            shot.SetKnockbackForce(5);
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
            foreach (EnemyBehaviour e in CombatManager.EnemiesOnScreen())
            {
                Shot s = Shot.Create(PlayerCombat.Instance);
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
//            PlayerCombat.Instance.Weapon().Reload(PlayerCombat.Instance.Player.Inventory());
        }
    }

    public class Compel : Skill
    {
        public Compel() : base(nameof(Compel), true)
        {
        }

        protected override void OnFire()
        {
            PlayerCombat.Instance.OnFireAction += s => { s.SetKnockbackForce(50); };
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
            PlayerCombat.Instance.OnFireAction += s => { PlayerCombat.Instance.FireWeapon(); };
        }
    }

    public class Splinter : Skill
    {
        public Splinter() : base(nameof(Splinter), true)
        {
        }

        protected override void OnFire()
        {
            PlayerCombat.Instance.OnFireAction += s =>
            {
                s.AddOnHit(() =>
                {
                    Explosion e = Explosion.CreateExplosion(s.transform.position, s.DamageDealt(), 0.25f);
                    e.InstantDetonate();
                });
            };
        }
    }

    //Pistol

    public class Passion : Skill
    {
        public Passion() : base(nameof(Passion), true)
        {
        }

        protected override void OnFire()
        {
            PlayerCombat.Instance.OnFireAction += s => { s.SetBurnChance(1); };
        }
    }

    public class Revenge : Skill
    {
        public Revenge() : base(nameof(Revenge), true)
        {
        }

        protected override void OnFire()
        {
        }
    }
}