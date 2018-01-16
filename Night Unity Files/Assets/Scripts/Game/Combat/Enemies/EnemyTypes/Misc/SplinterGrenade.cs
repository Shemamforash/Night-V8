namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class SplinterGrenade : Grenade
    {
        public SplinterGrenade(int distance, int targetDistance) : base(distance, targetDistance, "Splinter Bomb")
        {
        }
        
        protected override void ReachTarget()
        {
            Shot s = new Shot(null, null);
            s.SetDamage(10);
            s.SetBleedChance(1);
            s.SetSplinterRange(10);
            s.SetSplinterFalloff(0.5f);
            s.Fire();
        }
    }
}