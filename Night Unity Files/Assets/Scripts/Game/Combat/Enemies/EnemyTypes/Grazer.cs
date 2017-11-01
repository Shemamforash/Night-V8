using Game.Combat.Enemies.EnemyBehaviours;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Grazer : Enemy
    {
        public Grazer() : base(nameof(Grazer), Random.Range(1000, 2500))
        {
            
        }
        
        public override void InitialiseBehaviour(EnemyPlayerRelation relation)
        {
            base.InitialiseBehaviour(relation);
            Wander wander = new Wander(relation);
            Graze graze = new Graze(relation);
            Herd herd = new Herd(relation);
            Flee flee = new Flee(relation);
            herd.SetOnDetectBehaviour(flee);
            SetReciprocralBehaviour(wander, graze);
            BehaviourMachine.AddState(wander);
            BehaviourMachine.AddState(graze);
            BehaviourMachine.AddState(herd);
            BehaviourMachine.AddState(flee);
            BehaviourMachine.NavigateToState(wander.Name);
        }
    }
}