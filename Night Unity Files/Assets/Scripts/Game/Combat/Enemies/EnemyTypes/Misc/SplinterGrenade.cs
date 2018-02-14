namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class SplinterGrenade : Grenade
    {
        public override void Awake()
        {
            base.Awake();
            SetName("Splinter Bomb");
        }
        
        protected override void CreateExplosion()
        {
            Explosion explosion = new Explosion(CurrentPosition, 5, 10);
            explosion.SetBleeding();
            explosion.Fire();
        }
    }
}