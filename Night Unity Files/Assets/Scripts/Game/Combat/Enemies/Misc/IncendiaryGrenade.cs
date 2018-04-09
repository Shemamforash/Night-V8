using Game.Combat.Misc;

namespace Game.Combat.Enemies.Misc
{
    public class IncendiaryGrenade : Grenade
    {
        protected override void CreateExplosion()
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, 5, 20);
            explosion.SetBurning();
            explosion.Detonate();
        }
    }
}