using Game.Combat.Misc;

namespace Game.Combat.Enemies.Misc
{
    public class DecayGrenade : Grenade
    {
        protected override void CreateExplosion()
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, 20);
            explosion.Detonate();
        }
    }
}