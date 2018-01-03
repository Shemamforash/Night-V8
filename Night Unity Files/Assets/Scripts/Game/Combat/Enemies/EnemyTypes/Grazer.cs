﻿using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Grazer : Enemy
    {
        public Grazer() : base(nameof(Grazer), Random.Range(1000, 2500))
        {
            BaseAttributes.Perception.SetCurrentValue(0);
            BaseAttributes.Willpower.SetCurrentValue(0);
        }

        public void InitialiseBehaviour(Enemy enemy)
        {
        }

//        public override void InitialiseBehaviour(EnemyPlayerRelation relation)
//        {
//            base.InitialiseBehaviour(relation);
//            Wander wander = new Wander(relation);
//            Graze graze = new Graze(relation);
//            Herd herd = new Herd(relation);
//            Flee flee = new Flee(relation);
//            herd.SetOnDetectBehaviour(flee);
//            SetReciprocralBehaviour(wander, graze);
//            EnemyBehaviour.NavigateToState(wander.Name);
//        }
    }
}