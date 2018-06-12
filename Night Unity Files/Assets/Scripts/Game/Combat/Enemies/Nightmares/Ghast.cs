using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghast : EnemyBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            ChooseNextAction();
            gameObject.AddComponent<Teleport>().Initialise(5);
            gameObject.AddComponent<Bombardment>().Initialise(3, 7);
        }

        public override void ChooseNextAction()
        {
            Reposition(PathingGrid.FindCellToAttackPlayer(CurrentCell(), 5f, 2f));
        }
    }
}