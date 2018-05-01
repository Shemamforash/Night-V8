using Game.Combat.Misc;

namespace Game.Combat.Enemies.Misc
{
    public class IncendiaryGrenade : Grenade
    {
        protected override void CreateExplosion()
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, 10, 2);
            explosion.Detonate();
            FireBehaviour.Create(transform.position, 2);
        }
    }
}