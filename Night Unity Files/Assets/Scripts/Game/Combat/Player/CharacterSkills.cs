using System;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Misc;
using Game.Combat.Generation;
using Game.Combat.Ui;

namespace Game.Combat.Player
{
    public static class CharacterSkills
    {
        public static Skill GetCharacterSkillOne(Characters.Player player)
        {
            switch (player.CharacterTemplate.CharacterClass)
            {
                case CharacterClass.Villain:
                    return new Staunch();
                case CharacterClass.Deserter:
                    return new Immolate();
                case CharacterClass.Beast:
                    return new Taunt();
                case CharacterClass.Watcher:
                    return new Terrify();
                case CharacterClass.Wanderer:
                    return new Shatter();
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

        public static Skill GetCharacterSkillTwo(Characters.Player player)
        {
            switch (player.CharacterTemplate.CharacterClass)
            {
                case CharacterClass.Villain:
                    return new Rejuvinate();
                case CharacterClass.Deserter:
                    return new Lacerate();
                case CharacterClass.Beast:
                    return new Crush();
                case CharacterClass.Watcher:
                    return new Afflict();
                case CharacterClass.Wanderer:
                    return new Brace();
                case CharacterClass.Protector:
                    return new Execute();
                case CharacterClass.Hunter:
                    return new Fortify();
                case CharacterClass.Ghost:
                    return new Unearth();
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
            CombatManager.Player().HealthController.Heal(Characters.Player.PlayerHealthChunkSize);
        }
    }

    public class Rejuvinate : Skill
    {
        public Rejuvinate() : base(nameof(Rejuvinate))
        {
        }

        protected override void OnFire()
        {
            PlayerCombat pCombat = CombatManager.Player();
//            pCombat.GetTarget().Bleeding.AddStacks(pCombat.Bleeding.Size());
//            pCombat.Bleeding.Clear();

//            pCombat.GetTarget().Burn.AddStacks(pCombat.Burn.Size());
//            pCombat.Burn.Clear();

//            pCombat.GetTarget().Sick.AddStacks(pCombat.Sick.Size());
//            pCombat.Sick.Clear();
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
            Grenade.Create(CombatManager.Player().transform.position, CombatManager.Player().GetTarget().transform.position);
        }
    }

    public class Lacerate : Skill
    {
        public Lacerate() : base(nameof(Lacerate))
        {
        }

        protected override void OnFire()
        {
            Grenade.Create(CombatManager.Player().transform.position, CombatManager.Player().GetTarget().transform.position);
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
//            CombatManager.Player().CurrentTarget.CurrentAction = CombatManager.Player().CurrentTarget.MoveToPlayer;
        }
    }

    public class Crush : Skill
    {
        public Crush() : base(nameof(Crush))
        {
        }

        protected override void OnFire()
        {
//            EnemyBehaviour nearestEnemy = CombatManager.NearestEnemy();
//            if (nearestEnemy == null || nearestEnemy.DistanceToTarget() > 5) return;
//            nearestEnemy.Knockback(5);
//            nearestEnemy.ArmourController.TakeDamage(ArmourPlate.PlateHealthUnit);
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
//            UIEnemyController.Enemies.ForEach(e => e.Knockback(0));
        }
    }

    public class Afflict : Skill
    {
        public Afflict() : base(nameof(Afflict))
        {
        }

        protected override void OnFire()
        {
//            DetailedEnemyCombat target = CombatManager.Player().CurrentTarget;
//            if (target.Sick.Size() != 0)
//            {
//                target.Sick.AddStacks(Sickness.MaxStacks - target.Sick.Size());
//            }
        }
    }

    //Wanderer

    public class Shatter : Skill
    {
        public Shatter() : base(nameof(Shatter))
        {
        }

        protected override void OnFire()
        {
//            UIGrenadeController.AddGrenade(GrenadeType.Pierce, CombatManager.Player().Position.CurrentValue(), CombatManager.Player().CurrentTarget.Position.CurrentValue());
        }
    }

    public class Brace : Skill
    {
        public Brace() : base(nameof(Brace))
        {
        }

        protected override void OnFire()
        {
//            if (CombatManager.Player().ArmourController.CurrentArmour() == 0) return;
//            CombatManager.Player().ArmourController.TakeDamage(ArmourPlate.PlateHealthUnit);
//            CombatManager.Player().HealthController.Heal(Characters.Player.Player.PlayerHealthChunkSize);
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
//            CombatManager.Player().HealthController.TakeDamage(Characters.Player.Player.PlayerHealthChunkSize);
//            DetailedEnemyCombat target = CombatManager.Player().CurrentTarget;
//            target.Burn.AddStacks(3);
//            target.Bleeding.AddStacks(3);
//            target.Sick.AddStacks(3);
        }
    }

    public class Execute : Skill
    {
        public Execute() : base(nameof(Execute))
        {
        }

        protected override void OnFire()
        {
//            foreach (EnemyBehaviour e in UIEnemyController.Enemies)
//            {
//                if (e.DistanceToPlayer > 5 || e.HealthController.GetCurrentHealth() > 100) continue;
//                e.HealthController.TakeDamage(101);
//                break;
//            }
        }
    }

    //Hunter

    public class Fortify : Skill
    {
        public Fortify() : base(nameof(Fortify))
        {
        }

        protected override void OnFire()
        {
//            CombatManager.Player().ArmourController.RepairArmour(2 * ArmourPlate.PlateHealthUnit);
        }
    }

    public class Restock : Skill
    {
        public Restock() : base(nameof(Restock))
        {
        }

        protected override void OnFire()
        {
//            Player().Inventory().IncrementResource(Player().Weapon.WeaponAttributes.AmmoType, 1);
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
//            CombatManager.Player().Position.SetCurrentValue(CombatManager.Player().CurrentTarget.Position.CurrentValue());
        }
    }

    public class Unearth : Skill
    {
        public Unearth() : base(nameof(Unearth))
        {
        }

        protected override void OnFire()
        {
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
//            CombatManager.Player().CurrentTarget.Bleeding.AddStacks(5);
        }
    }

    public class Absolve : Skill
    {
        public Absolve() : base(nameof(Absolve))
        {
        }

        protected override void OnFire()
        {
//            DetailedEnemyCombat target = CombatManager.Player().CurrentTarget;
//            int healAmount = 0;
//            if (target.Bleeding.Active())
//            {
//                target.Bleeding.Clear();
//                healAmount += Characters.Player.Player.PlayerHealthChunkSize;
//            }

//            if (target.Burn.Active())
//            {
//                target.Burn.Clear();
//                healAmount += Characters.Player.Player.PlayerHealthChunkSize;
//            }

//            if (target.Sick.Active())
//            {
//                target.Sick.Clear();
//                healAmount += Characters.Player.Player.PlayerHealthChunkSize;
//            }

//            CombatManager.Player().HealthController.Heal(healAmount);
        }
    }
}