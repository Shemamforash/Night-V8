using System;
using System.Net;
using Game.Characters;
using Game.Combat.Enemies;
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

        public virtual void OnFire()
        {
            if (_character.Weapon().Empty())
            {
                Deactivate();
            }
        }

        public class PiercingShot : Skill
        {
            public PiercingShot(Character character) : base(character, true, nameof(PiercingShot))
            {
                Duration = 2f;
            }

            public override void OnFire()
            {
                EnemyPlayerRelation relation = CombatManager.GetCurrentTarget();
                base.OnFire();
                int damage = _character.Weapon().Fire(relation.Distance.GetCurrentValue(), _character.RageActivated(), true);
                foreach (EnemyPlayerRelation e in CombatManager.GetEnemyPlayerRelations())
                {
                    if (e.Distance > relation.Distance)
                    {
                        e.Enemy.TakeDamage(damage);
                    }
                }
            }
        }
    }
}