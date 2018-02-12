using System;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Random = UnityEngine.Random;

namespace Game.Combat.Skills
{
    public static class CharacterSkills
    {
        public static Skill GetCharacterSkillOne(Player player)
        {
            switch (player.CharacterTemplate.CharacterClass)
            {
                case CharacterClass.Villain:
                    return new Staunch();
                case CharacterClass.Father:
                    return new Fortify();
                case CharacterClass.Deserter:
                    return new Immolate();
                case CharacterClass.Beast:
                    return new Taunt();
                case CharacterClass.Watcher:
                    return new Terrify();
                case CharacterClass.Wanderer:
                    return new Lob();
                case CharacterClass.Protector:
                    return new Sacrifice();
                case CharacterClass.Hunter:
                    return new Restock();
                case CharacterClass.Ghost:
                    return new Blink();
                case CharacterClass.Driver:
                    return new Defile();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Skill GetCharacterSkillTwo(Player player)
        {
            switch (player.CharacterTemplate.CharacterClass)
            {
                case CharacterClass.Villain:
                    return new Rejuvinate();
                case CharacterClass.Father:
                    return new Endure();
                case CharacterClass.Deserter:
                    return new Lacerate();
                case CharacterClass.Beast:
                    return new Headbutt();
                case CharacterClass.Watcher:
                    return new Bellow();
                case CharacterClass.Wanderer:
                    return new Unearth();
                case CharacterClass.Protector:
                    return new Execute();
                case CharacterClass.Hunter:
                    return new Curse();
                case CharacterClass.Ghost:
                    return new Fade();
                case CharacterClass.Driver:
                    return new Absolve();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    //Villain

    public class Staunch : Skill
    {
        public Staunch() : base(nameof(Staunch))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().HealthController.Heal(10);
        }
    }

    public class Rejuvinate : Skill
    {
        public Rejuvinate() : base(nameof(Rejuvinate))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().HealthController.Heal(50);
            SkillBar.ResetSkillTimers();
        }
    }

    //Father

    public class Fortify : Skill
    {
        public Fortify() : base(nameof(Fortify))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().ArmourLevel.Increment(2);
        }
    }

    public class Endure : Skill
    {
        public Endure() : base(nameof(Endure))
        {
            //todo
        }
    }

    //Deserter

    public class Immolate : Skill
    {
        public Immolate() : base(nameof(Immolate))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            IncendiaryGrenade g = new IncendiaryGrenade(Player().Position.CurrentValue(), CombatManager.CurrentTarget.Position.CurrentValue());
            UIGrenadeController.AddGrenade(g);
        }
    }

    public class Lacerate : Skill
    {
        public Lacerate() : base(nameof(Lacerate))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            SplinterGrenade g = new SplinterGrenade(Player().Position.CurrentValue(), CombatManager.CurrentTarget.Position.CurrentValue());
            UIGrenadeController.AddGrenade(g);
        }
    }

    //Beast

    public class Taunt : Skill
    {
        public Taunt() : base(nameof(Taunt))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            CombatManager.CurrentTarget.CurrentAction = CombatManager.CurrentTarget.MoveToTargetDistance(0);
        }
    }

    public class Headbutt : Skill
    {
        public Headbutt() : base(nameof(Headbutt))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Enemy nearestEnemy = UIEnemyController.NearestEnemy();
            if (nearestEnemy?.DistanceToPlayer <= 5)
            {
                nearestEnemy.Knockback(5);
            }
        }
    }

    //Watcher

    public class Terrify : Skill
    {
        public Terrify() : base(nameof(Terrify))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            float distance = Random.Range(CombatManager.VisibilityRange / 2, CombatManager.VisibilityRange);
            CombatManager.CurrentTarget.CurrentAction = CombatManager.CurrentTarget.MoveToTargetDistance(distance);
        }
    }

    public class Bellow : Skill
    {
        public Bellow() : base(nameof(Bellow))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            foreach (Enemy e in UIEnemyController.Enemies)
            {
                e.Knockback(0);
            }
        }
    }

    //Wanderer

    public class Lob : Skill
    {
        public Lob() : base(nameof(Lob))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            foreach (Grenade g in UIGrenadeController.Grenades)
            {
                if (g.DistanceToPlayer < 5)
                {
                    g.SetTargetPosition(CombatManager.CurrentTarget.Position.CurrentValue());
                    break;
                }
            }
        }
    }

    public class Unearth : Skill
    {
        public Unearth() : base(nameof(Unearth))
        {
            //todo
        }
    }

    //Protector

    public class Sacrifice : Skill
    {
        public Sacrifice() : base(nameof(Sacrifice))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();

            Player().RageController.Increase(Player().RageController.CurrentValue());
            Player().HealthController.TakeDamage((int) (Player().HealthController.GetCurrentHealth() / 2f));
        }
    }

    public class Execute : Skill
    {
        public Execute() : base(nameof(Execute))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            foreach (Enemy e in UIEnemyController.Enemies)
            {
                if (e.DistanceToPlayer > 5 || !e.IsKnockedDown || e.HealthController.GetCurrentHealth() > 100) continue;
                e.HealthController.TakeDamage(101);
                break;
            }
        }
    }

    //Hunter

    public class Restock : Skill
    {
        public Restock() : base(nameof(Restock))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().Inventory().IncrementResource(Player().Weapon.WeaponAttributes.AmmoType, 1);
        }
    }

    public class Curse : Skill
    {
        public Curse() : base(nameof(Curse))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            //todo
        }
    }

    //Ghost

    public class Blink : Skill
    {
        public Blink() : base(nameof(Blink))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().Position.SetCurrentValue(CombatManager.CurrentTarget.Position.CurrentValue() - 5);
        }
    }

    public class Fade : Skill
    {
        public Fade() : base(nameof(Fade))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().Position.SetCurrentValue(UIEnemyController.NearestEnemy().Position.CurrentValue() - 10);
            foreach (Enemy e in UIEnemyController.Enemies)
            {
                e.Reset();
            }
        }
    }

    //Driver

    public class Defile : Skill
    {
        public Defile() : base(nameof(Defile))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            if (CombatManager.CurrentTarget.DistanceToPlayer > Enemy.MeleeDistance) return;
            CombatManager.CurrentTarget.OnHit(25, false);
            for (int i = 0; i < 5; ++i) CombatManager.CurrentTarget.Bleeding.AddStack();
        }
    }

    public class Absolve : Skill
    {
        public Absolve() : base(nameof(Absolve))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Enemy target = CombatManager.CurrentTarget;
            int healAmount = 0;
            if (target.Bleeding.Active())
            {
                target.Bleeding.Clear();
                healAmount += 10;
            }

            if (target.Burn.Active())
            {
                target.Burn.Clear();
                healAmount += 10;
            }

            if (target.Sick.Active())
            {
                target.Sick.Clear();
                healAmount += 10;
            }

            Player().HealthController.Heal(healAmount);
        }
    }
}