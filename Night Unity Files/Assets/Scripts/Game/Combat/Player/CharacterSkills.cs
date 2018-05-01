using System;
using System.Diagnostics;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Misc;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Gear.Armour;
using UnityEngine;

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
            CombatManager.Player().ClearConditions();

        }
    }

    public class Rejuvinate : Skill
    {
        public Rejuvinate() : base(nameof(Rejuvinate))
        {
        }

        protected override void OnFire()
        {
            CombatManager.Player().HealthController.Heal((int) (Characters.Player.PlayerHealthChunkSize / 2f));
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
            IncendiaryGrenade.Create(CombatManager.Player().transform.position, CombatManager.Player().GetTarget().transform.position);
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
            //todo
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
            CombatManager.Player().Ram(CombatManager.Player().GetTarget(), 200);
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
            CombatManager.EnemiesOnScreen().ForEach(e => e.MoveToCover(null));
        }
    }

    public class Afflict : Skill
    {
        public Afflict() : base(nameof(Afflict))
        {
        }

        protected override void OnFire()
        {
            CombatManager.Player().GetTarget().Sicken(5);
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
            DecayGrenade.Create(CombatManager.Player().transform.position, CombatManager.Player().GetTarget().transform.position);
        }
    }

    public class Brace : Skill
    {
        public Brace() : base(nameof(Brace))
        {
        }

        protected override void OnFire()
        {
            ArmourController armour = CombatManager.Player().Player.ArmourController;
            if(armour.GetCurrentArmour() == 0) return;
            armour.TakeDamage(ArmourPlate.PlateHealthUnit);
            CombatManager.Player().HealthController.Heal(50);
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
            CombatManager.Player().HealthController.TakeDamage(Characters.Player.PlayerHealthChunkSize);
            CharacterCombat target = CombatManager.Player().GetTarget();
            target.Burn();
            target.Decay();
        }
    }

    public class Execute : Skill
    {
        public Execute() : base(nameof(Execute))
        {
        }

        protected override void OnFire()
        {
            CharacterCombat target = CombatManager.Player().GetTarget();
            if (target.DistanceToTarget() > 0.5f || target.HealthController.GetCurrentHealth() > 100) return;
            target.HealthController.TakeDamage(101);
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
            CombatManager.Player().Player.ArmourController.Repair(2 * ArmourPlate.PlateHealthUnit);
        }
    }

    public class Restock : Skill
    {
        public Restock() : base(nameof(Restock))
        {
        }

        protected override void OnFire()
        {
            //todo
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
            Cell c = PathingGrid.Instance().GetCellNearMe(CombatManager.Player().GetTarget().CurrentCell(), 1f);
            CombatManager.Player().transform.position = c.Position;
        }
    }

    public class Unearth : Skill
    {
        public Unearth() : base(nameof(Unearth))
        {
        }

        protected override void OnFire()
        {
            //todo
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
            CombatManager.Player().GetTarget().Decay();
        }
    }

    public class Absolve : Skill
    {
        public Absolve() : base(nameof(Absolve))
        {
        }

        protected override void OnFire()
        {
            CharacterCombat target = CombatManager.Player().GetTarget();
            int healAmount = 0;
            if (target.IsBurning()) healAmount += 10;
            if (target.IsDecaying()) healAmount += 10;
            if (target.IsSick()) healAmount += 10;
            target.ClearConditions();
            CombatManager.Player().HealthController.Heal(healAmount);
        }
    }
}