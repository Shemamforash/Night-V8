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
                    return new Implode();
                case WeaponType.Shotgun:
                    return new Shockwave();
                case WeaponType.SMG:
                    return new Seek();
                case WeaponType.Pistol:
                    return new Needle();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Skill GetWeaponSkillTwo(Weapon weapon)
        {
            switch (weapon.WeaponType())
            {
                case WeaponType.Rifle:
                    return new Ignite();
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

    public class Ignite : Skill
    {
        public Ignite() : base(nameof(Ignite))
        {
        }

        protected override void InstantEffect()
        {
            Shot s = ShotManager.Create(PlayerCombat.Instance);
            s.Attributes().AddOnHit(() => FireBurstBehaviour.Create(s.transform.position));
            s.Fire();
        }
    }

    public class Implode : Skill
    {
        public Implode() : base(nameof(Implode))
        {
        }

        protected override void InstantEffect()
        {
            Shot s = ShotManager.Create(PlayerCombat.Instance);
            s.Attributes().AddOnHit(() => { VortexBehaviour.Create(s.transform.position, 
                () => Explosion.CreateExplosion(s.transform.position, 0.5f).InstantDetonate()); });
            s.Fire();
        }
    }

    //Shotgun

    public class Shockwave : Skill
    {
        public Shockwave() : base(nameof(Shockwave))
        {
        }

        protected override void InstantEffect()
        {
            PushController.Create(PlayerCombat.Position(), 0, true, 360f);
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
                Shot s = ShotManager.Create(PlayerCombat.Instance);
                s.Attributes().NoAdrenaline();
                s.Attributes().SetDamageModifier(2);
                s.OverrideDirection(dir);
                s.Attributes().Seek();
                s.Fire();
            }
        }
    }

    //SMG

    public class Seek : Skill
    {
        public Seek() : base(nameof(Seek))
        {
        }

        protected override void PassiveEffect(Shot s)
        {
            s.Attributes().Seek();
        }
    }

    public class Impact : Skill
    {
        public Impact() : base(nameof(Impact))
        {
        }

        protected override void PassiveEffect(Shot s)
        {
            s.Attributes().AddOnHit(() =>
            {
                Explosion e = Explosion.CreateExplosion(s.transform.position, 0.25f);
                e.InstantDetonate();
            });
        }
    }

    //Pistol

    public class Needle : Skill
    {
        public Needle() : base(nameof(Needle))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat.Instance.WeaponAudio.PlayNeedleFire();
            Transform playerTransform = PlayerCombat.Instance.transform;

            Vector2 startPos = playerTransform.position + playerTransform.up * 0.5f;
            Vector2 targetPos = playerTransform.position + playerTransform.up * 1f;
            NeedleBehaviour.Create(startPos, targetPos, true);

            Vector2 leftStartPos = startPos + (Vector2) playerTransform.right * 0.1f;
            Vector2 leftEndPos = targetPos + (Vector2) playerTransform.right * 0.15f;
            NeedleBehaviour.Create(leftStartPos, leftEndPos, true);

            Vector2 rightStartPos = startPos + (Vector2) playerTransform.right * -0.1f;
            Vector2 rightEndPos = targetPos + (Vector2) playerTransform.right * -0.15f;
            NeedleBehaviour.Create(rightStartPos, rightEndPos, true);
        }
    }

    public class Revenge : Skill
    {
        public Revenge() : base(nameof(Revenge))
        {
        }

        protected override void PassiveEffect(Shot s)
        {
            float normalisedHealth = PlayerCombat.Instance.HealthController.GetNormalisedHealthValue();
            float damageModifier = 1 + 1 - normalisedHealth;
            damageModifier *= damageModifier;
            s.Attributes().SetDamageModifier(damageModifier);
        }
    }
}