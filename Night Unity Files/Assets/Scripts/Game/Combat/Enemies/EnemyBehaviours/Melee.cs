using Game.Combat.CombatStates;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Melee : EnemyBehaviour
    {
        public Melee(EnemyPlayerRelation relation) : base(nameof(Melee), relation)
        {
        }

        public override void Enter()
        {
            SetStatusText("Meleeing");
            Duration = Random.Range(0.5f, 1f);
        }

        public override void Update()
        {
            Duration -= Time.deltaTime;
            if (Duration > 0) return;
            SelectRandomTransition();
            if (Relation.PlayerCover.ReachedMax()) return;
            Relation.Player.CombatController.KnockDown();
            Relation.Player.TakeDamage(10);
        }
    }
}