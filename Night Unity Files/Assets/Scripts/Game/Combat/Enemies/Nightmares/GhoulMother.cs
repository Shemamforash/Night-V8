using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;

namespace Game.Combat.Enemies.Nightmares
{
    public class GhoulMother : NightmareEnemyBehaviour
    {
        private const int MinGhoulsReleased = 3;
        private const int MaxGhoulsReleased = 6;
        private const float GhoulCooldownMax = 10f;
        private float _ghoulCooldown;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<Spawn>().Initialise(EnemyType.Ghoul, GhoulCooldownMax, MinGhoulsReleased, MaxGhoulsReleased);
        }
    }
}