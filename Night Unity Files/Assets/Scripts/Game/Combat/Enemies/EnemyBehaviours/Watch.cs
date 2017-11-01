using UnityEngine;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Watch : EnemyBehaviour
    {
        private float _previousVisionRange, _previousDetectionRange;

        public Watch(EnemyPlayerRelation relation) : base(nameof(Watch), relation)
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
            Relation.Enemy.VisionRange.AddModifier(1f);
            Relation.Enemy.DetectionRange.AddModifier(1f);
            SetStatusText("Watching");
        }

        public override void Exit()
        {
            Relation.Enemy.VisionRange.RemoveModifier(1f);
            Relation.Enemy.DetectionRange.RemoveModifier(1f);
            base.Exit();
        }
    }
}