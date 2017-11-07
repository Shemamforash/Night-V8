using System;
using System.Net;
using Game.Characters;
using Game.Combat.Enemies;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI;

namespace Game.Combat.Skills
{
    public abstract class Skill : Cooldown
    {
        public int RageCostInitial, RageCostOverTime;
        public readonly string Name;
        private bool _instant;
        private Character _character;
        
        private Skill(Character character, bool instant, string name) : base(CombatManager.CombatCooldowns)
        {
            Name = name;
            _instant = instant;
            _character = character;
        }

        public void Activate()
        {
            if (Running()) return;
            MyValue rage = _character.Rage;
            if(rage.ReachedMin() || rage.GetCurrentValue() < RageCostInitial) return;
            rage.Increment(-RageCostInitial);
            if (!_instant) return;
            OnFire();
            Deactivate();
        }

        private void Deactivate()
        {
            Start();
        }

        protected virtual void OnFire()
        {
            if (_character.Weapon().Empty())
            {
                Deactivate();
            }
        }

        public override void SetController(CooldownController controller)
        {
            base.SetController(controller);
            controller.Text(Name);
        }

        public class PiercingShot : Skill
        {
            public PiercingShot(Character character) : base(character, true, nameof(PiercingShot))
            {
                Duration = 2f;
            }

            protected override void OnFire()
            {
                base.OnFire();
                EnemyPlayerRelation relation = CombatManager.GetCurrentTarget();
                _character.FireWeapon();
                int damage = _character.Weapon().GetShotDamage(relation.Distance.GetCurrentValue(), _character.RageActivated(), true);
                foreach (EnemyPlayerRelation e in CombatManager.GetEnemyPlayerRelations())
                {
                    if (e.Distance > relation.Distance)
                    {
                        e.Enemy.TakeDamage(damage);
                    }
                }
            }
        }

        public class FullBlast : Skill
        {
            public FullBlast(Character character) : base(character, true, nameof(FullBlast))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                EnemyPlayerRelation relation = CombatManager.GetCurrentTarget();
                int rounds = _character.Weapon().GetRemainingAmmo();
                int damage = _character.Weapon().GetShotDamage(relation.Distance.GetCurrentValue(), true, true);
                damage *= rounds;
                relation.Enemy.TakeDamage(damage);
                _character.Weapon().AmmoInMagazine.SetCurrentValue(0);
            }
        }
    }
}