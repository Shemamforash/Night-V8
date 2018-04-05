namespace Game.Combat.Enemies.EnemyTypes
{
    public class Watcher : Enemy
    {
//        public Watcher(float position) : base(nameof(Watcher), position)
//        {
//        }

//        public override void InitialiseBehaviour(EnemyPlayerRelation relation)
//        {
//            base.InitialiseBehaviour(relation);
//            EnemyBehaviour.EnableGrazing();
//            EnemyBehaviour.EnableWatching();
//        }
        
//        public override void InitialiseBehaviour(EnemyPlayerRelation relation)
//        {
//            base.InitialiseBehaviour(relation);
//            Wander wander = new Wander(relation);
//            Graze graze = new Graze(relation);
//            Watch watch = new Watch(relation);
//            Rush rush = new Rush(relation);
//            Retreat retreat = new Retreat(relation);
//            Melee melee = new Melee(relation);
//            Herd herd = new Herd(relation);
//            herd.SetOnDetectBehaviour(rush);
//            rush.AddExitTransition(melee);
//            melee.AddExitTransition(retreat);
//            retreat.AddExitTransition(rush);
//            SetReciprocralBehaviour(wander, graze);
//            SetReciprocralBehaviour(graze, watch);
//            SetReciprocralBehaviour(watch, wander);
//            EnemyBehaviour.NavigateToState(wander.Name);
//        }
        protected Watcher(EnemyType type) : base(type)
        {
        }
    }
}