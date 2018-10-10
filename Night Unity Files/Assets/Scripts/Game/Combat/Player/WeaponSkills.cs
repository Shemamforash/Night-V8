using System;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Player
{
    public static class WeaponSkills
    {
        public static Skill GetWeaponSkillOne(Weapon weapon)
        {
            switch (weapon.WeaponType())
            {
                case WeaponType.Rifle:
                    return new Splinter();
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
                    return new Gouge();
                case WeaponType.Shotgun:
                    return new Swarm();
                case WeaponType.LMG:
                    return new Compel();
                case WeaponType.SMG:
                    return new Impact();
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

        protected override void InstantEffect()
        {
            if (PlayerCombat.Instance._weaponBehaviour.Empty()) return;
            Shot s = Shot.Create(PlayerCombat.Instance);
            s.SetBurnChance(1);
            s.Fire();
            PlayerCombat.Instance.ConsumeAmmo(1);
        }
    }

    public class Splinter : Skill
    {
        public Splinter() : base(nameof(Splinter))
        {
        }

        protected override void InstantEffect()
        {
            Shot s = Shot.Create(PlayerCombat.Instance);
            s.AddOnHit(() => { VortexBehaviour.Create(s.transform.position); });
            s.Fire();
            PlayerCombat.Instance.ConsumeAmmo();
        }
    }

    //Shotgun

    public class Sweep : Skill
    {
        public Sweep() : base(nameof(Sweep))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            float angle = AdvancedMaths.AngleFromUp(s._origin.transform.position, PlayerCombat.Instance.transform.position) + 80f;
            PushController.Create(s._origin.transform.position, angle);
        }
    }

    public class Swarm : Skill
    {
        public Swarm() : base(nameof(Swarm))
        {
        }

        protected override void InstantEffect()
        {
            int shots = 50;
            float angleIncrement = 360f / shots;
            for (int i = 0; i < shots; ++i)
            {
                float angle = i * angleIncrement * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);
                Vector2 dir = new Vector2(x, y);
                Shot s = Shot.Create(PlayerCombat.Instance);
                s.OverrideDirection(dir);
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

        protected override void InstantEffect()
        {
            PlayerCombat.Instance._weaponBehaviour.Reload();
        }
    }

    public class Compel : Skill
    {
        public Compel() : base(nameof(Compel))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            s.AddKnockBackModifier(3);
        }
    }

    //SMG

    public class Hairpin : Skill
    {
        public Hairpin() : base(nameof(Hairpin))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            BaseWeaponBehaviour weaponBehaviour = PlayerCombat.Instance._weaponBehaviour;
            if (weaponBehaviour.Empty()) return;
            Shot shot = Shot.Create(s._origin);
            shot.Fire();
            weaponBehaviour.ConsumeAmmo(1);
            UIMagazineController.UpdateMagazineUi();
        }
    }

    public class Impact : Skill
    {
        public Impact() : base(nameof(Impact))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            s.AddOnHit(() =>
            {
                Explosion e = Explosion.CreateExplosion(s.transform.position, s.DamageDealt(), 0.25f);
                e.InstantDetonate();
            });
        }
    }

    //Pistol

    public class Passion : Skill
    {
        public Passion() : base(nameof(Passion))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            s.LeaveFireTrail();
        }
    }

    public class Revenge : Skill
    {
        public Revenge() : base(nameof(Revenge))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            if (PlayerCombat.Instance.DamageTakenSinceLastShot)
            {
                s.SetDamageModifier(2);
            }

            PlayerCombat.Instance.DamageTakenSinceLastShot = false;
        }
    }
}