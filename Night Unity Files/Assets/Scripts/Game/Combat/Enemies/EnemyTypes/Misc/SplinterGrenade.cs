namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class SplinterGrenade : Grenade
    {
        public SplinterGrenade(float position, float targetPosition) : base(position, targetPosition, "Splinter Bomb")
        {
        }
        
        protected override void CreateExplosion()
        {
            Explosion explosion = new Explosion(Position.CurrentValue(), 5, 10);
            explosion.SetBleeding();
            explosion.Fire();
        }
    }
}