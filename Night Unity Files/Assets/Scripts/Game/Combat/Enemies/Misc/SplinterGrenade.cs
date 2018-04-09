using Game.Combat.Misc;

namespace Game.Combat.Enemies.Misc
{
    public class SplinterGrenade : Grenade
    {
        protected override void CreateExplosion()
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, 5, 10);
            explosion.SetBleeding();
            explosion.Detonate();
        }
    }
}