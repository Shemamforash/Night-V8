using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Global;

namespace Game.Combat.Enemies.Nightmares
{
    public class Shadow : NightmareEnemyBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<ErraticDash>();
            gameObject.AddComponent<Push>().Initialise(4, 2);
            if (WorldState.Difficulty() > 25) gameObject.AddComponent<Needler>().Initialise(1);
        }
    }
}