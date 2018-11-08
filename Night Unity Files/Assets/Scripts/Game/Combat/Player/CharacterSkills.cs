using System;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Enemies.Misc;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Gear.Armour;
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
                    return new Absolve();
                case CharacterClass.Deserter:
                    return new Lacerate();
                case CharacterClass.Beast:
                    return new Terrify();
                case CharacterClass.Watcher:
                    return new Medicate();
                case CharacterClass.Survivor:
                    return new Aegis();
                case CharacterClass.Protector:
                    return new Relinquish();
                case CharacterClass.Hunter:
                    return new Nourish();
                case CharacterClass.Ghost:
                    return new Rejuvinate();
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
                    return new Unleash();
                case CharacterClass.Watcher:
                    return new Impel();
                case CharacterClass.Survivor:
                    return new Erupt();
                case CharacterClass.Protector:
                    return new Shatter();
                case CharacterClass.Hunter:
                    return new Mark();
                case CharacterClass.Ghost:
                    return new Sacrifice();
                case CharacterClass.Wanderer:
                    return new Afflict();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    //Villain

    public class Absolve : Skill
    {
        public Absolve() : base(nameof(Absolve))
        {
        }

        protected override void InstantEffect()
        {
            CanTakeDamage target = Target();
            float healPercent = 0;
            if (target.IsBurning()) healPercent += 0.1f;
            if (target.IsSick()) healPercent += 0.1f;
            target.ClearConditions();
            Heal(healPercent);
        }
    }

    public class Defile : Skill
    {
        public Defile() : base(nameof(Defile))
        {
        }

        protected override void InstantEffect()
        {
            CanTakeDamage target = Target();
            target.Sicken(10);
            if (target.HealthController.GetCurrentHealth() == 0) Explosion.CreateExplosion(target.transform.position, 20).InstantDetonate();
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
            Vector3 playerPosition = PlayerCombat.Instance.transform.position;
            Vector2 targetPosition = playerPosition + PlayerCombat.Instance.transform.up;
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
            Vector3 playerPosition = PlayerCombat.Instance.transform.position;
            Vector2 targetPosition = playerPosition + PlayerCombat.Instance.transform.up;
            Grenade.CreateIncendiary(playerPosition, targetPosition, true);
        }
    }

    //Beast

    public class Terrify : Skill
    {
        public Terrify() : base(nameof(Terrify))
        {
        }

        protected override void InstantEffect()
        {
            int hit = 0;
            KnockbackInRange(1f, 100f).ForEach(e => ++hit);
            Heal(hit * 0.05f);
        }
    }

    public class Unleash : Skill
    {
        public Unleash() : base(nameof(Unleash))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat.Instance.TakeArmourDamage(Armour.ArmourHealthUnit);
            KnockbackInRange(2f, 25).ForEach(e =>
            {
                CharacterCombat c = e as CharacterCombat;
                if (c == null) return;
                c.TakeArmourDamage(Armour.ArmourHealthUnit);
            });
        }
    }

    //Watcher

    public class Medicate : Skill
    {
        public Medicate() : base(nameof(Medicate))
        {
        }

        protected override void InstantEffect()
        {
            Heal(0.25f);
            PlayerCombat.Instance.ClearConditions();
        }
    }

    public class Impel : Skill
    {
        public Impel() : base(nameof(Impel))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat.Instance.TakeArmourDamage(Armour.ArmourHealthUnit);
            KnockbackInRange(4f, 100f);
        }
    }

    //Survivor

    public class Aegis : Skill
    {
        public Aegis() : base(nameof(Aegis))
        {
        }

        protected override void InstantEffect()
        {
            Transform transform = PlayerCombat.Instance.transform;
            DecayBlastBehaviour.Create(PlayerCombat.Instance, transform.position + transform.up);
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
            e.AddIgnoreTarget(PlayerCombat.Instance);
            e.InstantDetonate();
        }

        protected override void InstantEffect()
        {
            Vector2 position = PlayerCombat.Instance.transform.position;
            CreateExplosion(position, 50, 2f);
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

    public class Shatter : Skill
    {
        public Shatter() : base(nameof(Shatter))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat.Instance.TakeArmourDamage(-Armour.ArmourHealthUnit);
            Vector2 position = PlayerCombat.Instance.transform.position;
            FireBurstBehaviour.Create(position);
        }
    }

    public class Relinquish : Skill
    {
        public Relinquish() : base(nameof(Relinquish))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat.Instance.ConsumeAmmo(1);
            Heal(0.25f);
        }
    }

    //Hunter

    public class Nourish : Skill
    {
        public Nourish() : base(nameof(Nourish))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat.Instance.TakeArmourDamage(Armour.ArmourHealthUnit);
            Heal(0.5f);
        }
    }

    public class Mark : Skill
    {
        public Mark() : base(nameof(Mark))
        {
        }

        protected override void InstantEffect()
        {
            float duration = 5f;
            PlayerCombat.Instance.StartMark(Target());
        }
    }

    //Ghost

    public class Rejuvinate : Skill
    {
        public Rejuvinate() : base(nameof(Rejuvinate))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat.Instance.UpdateSkillActions.Add(() => { Heal(0.01f); });
        }
    }

    public class Sacrifice : Skill
    {
        public Sacrifice() : base(nameof(Sacrifice))
        {
        }

        protected override void InstantEffect()
        {
            PlayerCombat.Instance.HealthController.TakeDamage(PlayerCombat.Instance.HealthController.GetMaxHealth() * 0.1f);
            Target().Burn();
            Target().Sicken(2);
            Target().Decay();
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
            Vector3 playerPosition = PlayerCombat.Instance.transform.position;
            Vector2 targetPosition = playerPosition + PlayerCombat.Instance.transform.up;
            Grenade.CreateDecay(playerPosition, targetPosition, true);
        }
    }
}