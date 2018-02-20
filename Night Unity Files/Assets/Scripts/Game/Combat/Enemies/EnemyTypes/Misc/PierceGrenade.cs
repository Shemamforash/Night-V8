namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class PierceGrenade : Grenade
    {
        public override void Awake()
        {
            base.Awake();
            SetName("Pierce Grenade");
        }
        
        protected override void CreateExplosion()
        {
            Explosion explosion = new Explosion(CurrentPosition, 5, 20);
            explosion.SetPiercing();
            explosion.Fire();
        }
    }
}