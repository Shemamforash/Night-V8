﻿using System;
using DG.Tweening;
using Game.Combat.Misc;
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
            s.AddOnHit(() =>
            {
                for (int i = 0; i < PlayerCombat.Instance._weaponBehaviour.AmmoInMagazine; ++i)
                {
                    Shot fragment = Shot.Create(s);
                    fragment.SetAccuracy(180f);
                    fragment.Fire(0.4f);
                }
            });
            s.Fire();
            PlayerCombat.Instance.ConsumeAmmo();
        }
    }

    //Shotgun

    public class Sweep : Skill
    {
        private ParticleSystem _pushParticles;
        private GameObject _pushPrefab;
        
        public Sweep() : base(nameof(Sweep))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            if (_pushPrefab == null) _pushPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Push Burst");
            GameObject pushObject = GameObject.Instantiate(_pushPrefab);
            pushObject.transform.SetParent(s._origin.transform);
            pushObject.transform.position = s._origin.transform.position;
            _pushParticles = pushObject.GetComponent<ParticleSystem>();
            float angle = AdvancedMaths.AngleFromUp(s._origin.transform.position, PlayerCombat.Instance.transform.position);
            _pushParticles.transform.rotation = Quaternion.Euler(0, 0, angle + 80f);
            _pushParticles.Emit(50);
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(2f);
            sequence.AppendCallback(() => GameObject.Destroy(pushObject));
        }
    }

    public class Swarm : Skill
    {
        public Swarm() : base(nameof(Swarm))
        {
        }

        protected override void InstantEffect()
        {
            for (int i = 0; i < 20; ++i)
            {
                Shot s = Shot.Create(PlayerCombat.Instance);
                s.Fire();
                s.Seek();
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
            s.SetKnockbackForce(100);
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
            PlayerCombat.Instance.FireWeapon();
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