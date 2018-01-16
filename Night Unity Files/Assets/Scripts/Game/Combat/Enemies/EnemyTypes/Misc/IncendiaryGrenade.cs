namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class IncendiaryGrenade : Grenade
    {
        public IncendiaryGrenade(int distance, int targetDistance) : base(distance, targetDistance, "Fire Bomb")
        {
        }
        
        protected override void ReachTarget()
        {
            Shot s = new Shot(null, null);
            s.SetDamage(10);
            s.SetBurnChance(1);
            s.Fire();
        }
    }
}