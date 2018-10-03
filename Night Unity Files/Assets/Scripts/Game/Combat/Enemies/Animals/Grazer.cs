using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Misc;

namespace Game.Combat.Enemies.Animals
{
    public class Grazer : AnimalBehaviour
    {
        public override void Initialise(Enemy e)
        {
            base.Initialise(e);
            WanderDistance = 1f;
        }

        protected override void OnAlert()
        {
            Cell target = PathingGrid.GetCellOutOfRange(transform.position);
            Flee(target);
        }
    }
}