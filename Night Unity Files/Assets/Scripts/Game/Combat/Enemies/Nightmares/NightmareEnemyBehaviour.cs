namespace Game.Combat.Enemies.Nightmares
{
    public class NightmareEnemyBehaviour : EnemyBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.layer = 24;
        }
    }
}