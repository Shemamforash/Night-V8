using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Graze : EnemyBehaviour
    {
        public Graze(EnemyPlayerRelation relation) : base(nameof(Graze), relation)
        {
            
        }
        
        public override void Update()
        {
            Duration -= Time.deltaTime;
            if (Duration < 0)
            {
                SelectRandomTransition();
            }
        }

        public override void Enter()
        {
            Duration = Random.Range(5, 10);
            Relation.Enemy.VisionRange.AddModifier(-0.5f);
            Relation.Enemy.DetectionRange.AddModifier(-0.5f);
            SetStatusText("Grazing");
        }

        public override void Exit()
        {
            Relation.Enemy.VisionRange.RemoveModifier(-0.5f);
            Relation.Enemy.DetectionRange.RemoveModifier(-0.5f);
            base.Exit();
        }
    }
}