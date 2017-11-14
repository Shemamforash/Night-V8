using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Grazer : Enemy
    {
        public Grazer() : base(nameof(Grazer), Random.Range(1000, 2500))
        {
            BaseAttributes.Intelligence.SetCurrentValue(0);
            BaseAttributes.Stability.SetCurrentValue(0);
        }

        public void InitialiseBehaviour(Enemy enemy)
        {
            EnemyBehaviour.EnableGrazing();
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