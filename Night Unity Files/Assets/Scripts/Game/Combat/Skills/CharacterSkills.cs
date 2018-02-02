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
        public static void GetCharacterSkills(Player player)
        {
            Skill skillOne = null, skillTwo = null;
            switch (player.CharacterTemplate.CharacterClass)
            {
                case CharacterClass.Villain:
                    skillOne = new Staunch();
                    skillTwo = new Rejuvinate();
                    break;
                case CharacterClass.Father:
                    skillOne = new Fortify();
                    skillTwo = new Endure();
                    break;
                case CharacterClass.Deserter:
                    skillOne = new Inferno();
                    skillTwo = new Lacerate();
                    break;
                case CharacterClass.Beast:
                    skillOne = new Taunt();
                    skillTwo = new Headbutt();
                    break;
                case CharacterClass.Watcher:
                    skillOne = new Terrify();
                    break;
                case CharacterClass.Wanderer:
                    skillOne = new Lob();
                    skillTwo = new Unearth();
                    break;
                case CharacterClass.Protector:
                    skillOne = new Sacrifice();
                    skillTwo = new Execute();
                    break;
                case CharacterClass.Hunter:
                    skillOne = new Restock();
                    skillTwo = new Curse();
                    break;
                case CharacterClass.Ghost:
                    skillOne = new Blink();
                    skillTwo = new Fade();
                    break;
                case CharacterClass.Driver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            player.CharacterSkillOne = skillOne;
            player.CharacterSkillTwo = skillTwo;
        }
    }

    //Villain

    public class Staunch : Skill
    {
        public Staunch() : base(true, nameof(Staunch))
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
        public Rejuvinate() : base(true, nameof(Rejuvinate))
        {
            Player().HealthController.Heal(50);
            CombatManager.SkillBar.ResetSkillTimers();
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().HealthController.Heal(50);
            CombatManager.SkillBar.ResetSkillTimers();
        }
    }

    //Father

    public class Fortify : Skill
    {
        public Fortify() : base(true, nameof(Fortify))
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
        public Endure() : base(true, nameof(Endure))
        {
            //todo
        }
    }

    //Deserter

    public class Inferno : Skill
    {
        public Inferno() : base(true, nameof(Inferno))
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
        public Lacerate() : base(true, nameof(Lacerate))
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
        public Taunt() : base(true, nameof(Taunt))
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
        public Headbutt() : base(true, nameof(Headbutt))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Enemy nearestEnemy = UIEnemyController.NearestEnemy();
            if (nearestEnemy.DistanceToPlayer <= 5)
            {
                nearestEnemy.Knockback(5);
            }
        }
    }

    //Watcher

    public class Terrify : Skill
    {
        public Terrify() : base(true, nameof(Terrify))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            foreach (Enemy e in UIEnemyController.Enemies)
            {
                float distance = Random.Range(CombatManager.VisibilityRange / 2, CombatManager.VisibilityRange);
                e.CurrentAction = e.MoveToTargetDistance(distance);
            }
        }
    }

//    public class Staunch : Skill
//    {
//        public Staunch() : base(true, nameof(Staunch))
//        {
//        }
//    }

    //Wanderer

    public class Lob : Skill
    {
        public Lob() : base(true, nameof(Lob))
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
        public Unearth() : base(true, nameof(Unearth))
        {
            //todo
        }
    }

    //Protector

    public class Sacrifice : Skill
    {
        public Sacrifice() : base(true, nameof(Sacrifice))
        {
        }
    }

    public class Execute : Skill
    {
        public Execute() : base(true, nameof(Execute))
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
        public Restock() : base(true, nameof(Restock))
        {
        }

        protected override void OnFire()
        {
            base.OnFire();
            Player().Inventory().IncrementResource(Player().Weapon().WeaponAttributes.AmmoType, 1);
        }
    }

    public class Curse : Skill
    {
        public Curse() : base(true, nameof(Curse))
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
        public Blink() : base(true, nameof(Blink))
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
        public Fade() : base(true, nameof(Fade))
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

//    public class Staunch : Skill
//    {
//        public Staunch() : base(true, nameof(Staunch))
//        {
//        }
//    protected override void OnFire()
//    {
//    base.OnFire();
//}
//    }

//    public class Staunch : Skill
//    {
//        public Staunch() : base(true, nameof(Staunch))
//        {
//        }
//protected override void OnFire()
//{
//base.OnFire();
//}
//    }
}