using System;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
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
            Shot s = Shot.Create(PlayerCombat.Instance);
            s.Attributes().AddOnHit(() => FireBurstBehaviour.Create(s.transform.position));
            s.Fire();
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
            s.Attributes().AddOnHit(() => { VortexBehaviour.Create(s.transform.position); });
            s.Fire();
        }
    }

    //Shotgun

    public class Sweep : Skill
    {
        public Sweep() : base(nameof(Sweep))
        {
        }

        protected override void InstantEffect()
        {
            PushController.Create(PlayerCombat.Instance.transform.position, 0, true, 360f);
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
                s.Attributes().SetDamageModifier(2);
                s.OverrideDirection(dir);
                s.Fire();
            }
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
            s.Attributes().Seek();
        }
    }

    public class Impact : Skill
    {
        public Impact() : base(nameof(Impact))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            s.Attributes().AddOnHit(() =>
            {
                Explosion e = Explosion.CreateExplosion(s.transform.position, s.Attributes().DamageDealt(), 0.25f);
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

        protected override void InstantEffect()
        {
            Transform playerTransform = PlayerCombat.Instance.transform;
            Vector2 startPos = playerTransform.position + playerTransform.up * 0.5f;
            Vector2 targetPos = playerTransform.position + playerTransform.up * 1f;
            int damage = ((int) PlayerCombat.Instance.Weapon().Quality() + 1) * 10;
            NeedleBehaviour.Create(startPos, targetPos, damage, true);
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
                s.Attributes().SetDamageModifier(5);
            }

            PlayerCombat.Instance.DamageTakenSinceLastShot = false;
        }
    }
}