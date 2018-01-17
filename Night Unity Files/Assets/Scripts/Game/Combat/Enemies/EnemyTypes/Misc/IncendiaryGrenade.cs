namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class IncendiaryGrenade : Grenade
    {
        public IncendiaryGrenade(float distance, float targetPosition) : base(distance, targetPosition, "Fire Bomb")
        {
        }
        
        protected override void CreateExplosion()
        {
            Explosion explosion = new Explosion(Position.CurrentValue(), 5, 20);
            explosion.SetBurning();
            explosion.Fire();
        }
    }
}