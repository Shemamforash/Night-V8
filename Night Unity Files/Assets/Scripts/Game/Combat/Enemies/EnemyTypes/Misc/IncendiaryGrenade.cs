namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class IncendiaryGrenade : Grenade
    {
        public IncendiaryGrenade(float position, float targetPosition) : base(position, targetPosition, "Fire Bomb")
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