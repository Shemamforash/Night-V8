using Game.Combat.Enemies.EnemyBehaviours;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Watcher : Enemy
    {
        public Watcher() : base(nameof(Watcher), Random.Range(500, 1000))
        {
        }

        public override void InitialiseBehaviour(EnemyPlayerRelation relation)
        {
            base.InitialiseBehaviour(relation);
            Wander wander = new Wander(relation);
            Graze graze = new Graze(relation);
            Watch watch = new Watch(relation);
            SetReciprocralBehaviour(wander, graze);
            SetReciprocralBehaviour(graze, watch);
            SetReciprocralBehaviour(watch, wander);
            BehaviourMachine.AddState(wander);
            BehaviourMachine.AddState(graze);
            BehaviourMachine.AddState(watch);
            BehaviourMachine.AddState(new Herd(relation));
            BehaviourMachine.NavigateToState(wander.Name);
        }
    }
}