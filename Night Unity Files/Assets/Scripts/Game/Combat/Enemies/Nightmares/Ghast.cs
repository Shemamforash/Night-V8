using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Global;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghast : NightmareEnemyBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            if (WorldState.Difficulty() > 15) gameObject.AddComponent<Teleport>().Initialise(5);
            int projectiles = (int) (WorldState.Difficulty() / 5f + 1);
            gameObject.AddComponent<Bombardment>().Initialise(projectiles, 4, 3);
            CurrentAction = Move;
        }
    }
}