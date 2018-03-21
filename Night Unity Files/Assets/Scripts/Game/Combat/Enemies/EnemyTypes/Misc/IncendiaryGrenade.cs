namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class IncendiaryGrenade : Grenade
    {
        public override void Awake()
        {
            base.Awake();
            SetName("Fire Bomb");
        }
        
        protected override void CreateExplosion()
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, 5, 20);
            explosion.SetBurning();
            explosion.Detonate();
        }
    }
}