using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;

namespace Game.Combat.Enemies.Humans
{
    public class Sentinel : ArmedBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<Push>().Initialise(3f);
        }

    }
}