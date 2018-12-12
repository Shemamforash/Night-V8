using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Enemies.Misc;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Player
{
    public static class CharacterSkills
    {
        public static Skill GetCharacterSkillOne(Characters.Player player)
        {
            switch (player.CharacterTemplate.CharacterClass)
            {
                case CharacterClass.Villain:
                    return new Drain();
                case CharacterClass.Deserter:
                    return new Lacerate();
                case CharacterClass.Beast:
                    return new Cleanse();
                case CharacterClass.Watcher:
                    return new Escape();
                case CharacterClass.Survivor:
                    return new Tremor();
                case CharacterClass.Protector:
                    return new Relinquish();
                case CharacterClass.Hunter:
                    return new Sacrifice();
                case CharacterClass.Ghost:
                    return new Regenerate();
                case CharacterClass.Wanderer:
                    return new Staunch();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Skill GetCharacterSkillTwo(Characters.Player player)
        {
            switch (player.CharacterTemplate.CharacterClass)
            {
                case CharacterClass.Villain:
                    return new Defile();
                case CharacterClass.Deserter:
                    return new Immolate();
                case CharacterClass.Beast:
                    return new Leach();
                case CharacterClass.Watcher:
                    return new Discharge();
                case CharacterClass.Survivor:
                    return new Erupt();
                case CharacterClass.Protector:
                    return new Wake();
                case CharacterClass.Hunter:
                    return new Mark();
                case CharacterClass.Ghost:
                    return new Refill();
                case CharacterClass.Wanderer:
                    return new Afflict();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    //Villain

    public class Drain : Skill
    {
        public Drain() : base(nameof(Drain))
        {
        }

        protected override void InstantEffect()
        {
            Heal(0.15f);
            SickenBehaviour.Create(PlayerPosition(), new List<CanTakeDamage>());
        }
    }

    public class Defile : Skill
    {
        public Defile() : base(nameof(Defile))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat player = Player();
            List<CanTakeDamage> sickened = SickenBehaviour.Create(player.transform.position, new List<CanTakeDamage> {player}, 5);
            int explosionDamage = (int) (10 + WorldState.Difficulty() / 2.5f);
            sickened.ForEach(e =>
            {
                if (e.HealthController.GetHealth().CurrentValue() != 0) return;
                Explosion explosion = Explosion.CreateExplosion(e.transform.position, explosionDamage, 0.5f);
                explosion.AddIgnoreTarget(player);
                explosion.InstantDetonate();
            });
        }
    }

    //Deserter

    public class Lacerate : Skill
    {
        public Lacerate() : base(nameof(Lacerate))
        {
        }

        protected override void InstantEffect()
        {
            Vector3 playerPosition = PlayerPosition();
            Vector2 targetPosition = playerPosition + PlayerTransform().up;
            Grenade g = Grenade.CreateBasic(playerPosition, targetPosition, true);
            g.AddOnDetonate(enemies => { Heal(enemies.Count * 0.05f); });
        }
    }

    public class Immolate : Skill
    {
        public Immolate() : base(nameof(Immolate))
        {
        }

        protected override void InstantEffect()
        {
            Vector3 playerPosition = PlayerPosition();
            Vector2 targetPosition = playerPosition + PlayerTransform().up;
            Grenade.CreateIncendiary(playerPosition, targetPosition, true);
        }
    }

    //Beast

    public class Cleanse : Skill
    {
        public Cleanse() : base(nameof(Cleanse))
        {
        }

        protected override void InstantEffect()
        {
            Vector2 position = PlayerPosition();
            VortexBehaviour.Create(position, () => { FireBurstBehaviour.Create(position).AddIgnoreTarget(Player()); });
        }
    }

    public class Leach : Skill
    {
        public Leach() : base(nameof(Leach))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            s.Attributes().AddOnHit(() =>
            {
                int damageDealt = s.Attributes().DamageDealt();
                damageDealt = (int) (damageDealt / 4f);
                if (damageDealt <= 0) damageDealt = 1;
                Player().HealthController.Heal(damageDealt);
            });
        }
    }

    //Watcher

    public class Escape : Skill
    {
        public Escape() : base(nameof(Escape))
        {
        }

        protected override void InstantEffect()
        {
            Heal(0.25f);
            PlayerCombat player = Player();
            player.MovementController.AddForce(player.transform.up * 1000);
        }
    }

    public class Discharge : Skill
    {
        public Discharge() : base(nameof(Discharge))
        {
        }

        protected override void InstantEffect()
        {
            Vector2 playerPosition = PlayerPosition();
            DecayBehaviour.Create(playerPosition).AddIgnoreTarget(Player());
            FireBurstBehaviour.Create(playerPosition).AddIgnoreTarget(Player());
        }
    }

    //Survivor

    public class Tremor : Skill
    {
        public Tremor() : base(nameof(Tremor))
        {
        }

        protected override void InstantEffect()
        {
            Transform transform = PlayerTransform();
            DecayBlastBehaviour.Create(Player(), transform.position + transform.up);
        }
    }

    public class Erupt : Skill
    {
        public Erupt() : base(nameof(Erupt))
        {
        }

        private void CreateExplosion(Vector2 position, int damage, float radius)
        {
            Explosion e = Explosion.CreateExplosion(position, damage, radius);
            e.AddIgnoreTarget(Player());
            e.InstantDetonate();
        }

        protected override void InstantEffect()
        {
            Vector2 position = PlayerPosition();
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.5f);
            sequence.AppendCallback(() =>
            {
                int explosionCount = 8;
                float angleInterval = 360f / explosionCount;
                for (int i = 0; i < explosionCount; ++i)
                {
                    Vector2 pos = AdvancedMaths.CalculatePointOnCircle(angleInterval * i, Random.Range(1.5f, 2f), position);
                    Cell cell = PathingGrid.WorldToCellPosition(pos, false);
                    if (cell == null || !cell.Reachable) continue;
                    Sequence subSequence = DOTween.Sequence();
                    subSequence.AppendInterval(Random.Range(0.1f, 0.2f));
                    subSequence.AppendCallback(() => CreateExplosion(pos, 25, 0.5f));
                }
            });
        }
    }

    //Protector

    public class Wake : Skill
    {
        private Sequence _sequence;

        public Wake() : base(nameof(Wake))
        {
        }

        protected override void InstantEffect()
        {
            _sequence?.Kill();
            LeaveFireTrail fireTrail = Player().GetComponent<LeaveFireTrail>();
            if (fireTrail == null) fireTrail = Player().gameObject.AddComponent<LeaveFireTrail>();
            _sequence = DOTween.Sequence();
            _sequence.AppendInterval(5f);
            _sequence.AppendCallback(() =>
            {
                if (Player() == null) return;
                GameObject.Destroy(fireTrail);
            });
        }
    }

    public class Relinquish : Skill
    {
        public Relinquish() : base(nameof(Relinquish))
        {
        }

        protected override void InstantEffect()
        {
            Player().ConsumeAmmo();
            Heal(0.25f);
        }
    }

    //Hunter

    public class Sacrifice : Skill
    {
        public Sacrifice() : base(nameof(Sacrifice))
        {
        }

        protected override void InstantEffect()
        {
            DecayBehaviour.Create(PlayerPosition());
            Heal(0.1f);
        }
    }

    public class Mark : Skill
    {
        public Mark() : base(nameof(Mark))
        {
        }

        protected override void InstantEffect()
        {
            MarkController.Create(PlayerPosition());
        }
    }

    //Ghost

    public class Regenerate : Skill
    {
        public Regenerate() : base(nameof(Regenerate))
        {
        }

        protected override void InstantEffect()
        {
            float t = 0f;
            Player().UpdateSkillActions.Add(() =>
            {
                t -= Time.deltaTime;
                if (t > 0) return;
                Heal(0.01f);
                t = 0.5f;
            });
        }
    }

    public class Refill : Skill
    {
        public Refill() : base(nameof(Refill))
        {
        }

        protected override void MagazineEffect(Shot s)
        {
            s.Attributes().AddOnHit(() =>
            {
                if (Helper.RollDie(0, 2)) Player()._weaponBehaviour.IncreaseAmmo(1);
            });
        }
    }

    //Wanderer

    public class Staunch : Skill
    {
        public Staunch() : base(nameof(Staunch))
        {
        }

        protected override void InstantEffect()
        {
            Heal(0.5f);
        }
    }

    public class Afflict : Skill
    {
        public Afflict() : base(nameof(Afflict))
        {
        }

        protected override void InstantEffect()
        {
            Vector3 playerPosition = PlayerPosition();
            Vector2 targetPosition = playerPosition + PlayerTransform().up;
            Grenade.CreateDecay(playerPosition, targetPosition, false);
        }
    }
}