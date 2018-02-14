using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Grazer : Enemy
    {
//        public Grazer(float position) : base(nameof(Grazer), position)
//        {
//        }
//
//        public void InitialiseBehaviour(Enemy enemy)
//        {
//        }

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
        protected Grazer(EnemyType type) : base(type)
        {
        }
    }
}